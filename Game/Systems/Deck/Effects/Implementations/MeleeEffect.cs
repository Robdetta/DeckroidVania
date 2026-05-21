using Godot;
using System.Collections.Generic;
using DeckroidVania2.Game.Systems.GameSystems; // To access HealthSystem

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

public class MeleeEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        // Safely extract parameters from the JSON dictionary
        float damage = System.Convert.ToSingle(effectParams.GetValueOrDefault("damage", 10.0f));
        float range = System.Convert.ToSingle(effectParams.GetValueOrDefault("range", 2.5f));

        if (context.Source == null) return false;

        // Perform a programmatic physics query to find enemies in range
        var spaceState = context.Source.GetWorld3D().DirectSpaceState;
        var query = new PhysicsShapeQueryParameters3D();
        
        var sphereShape = new SphereShape3D { Radius = range };
        query.ShapeRid = sphereShape.GetRid();
        query.Transform = context.Source.GlobalTransform;
        query.CollisionMask = 2; // Assuming Layer 2 represents your Enemies

        var results = spaceState.IntersectShape(query);
        Vector3 playerForward = -context.Source.GlobalTransform.Basis.Z.Normalized();
        int hitCount = 0;

        foreach (var result in results)
        {
            if (result.TryGetValue("collider", out Variant colliderVar))
            {
                var enemyNode = colliderVar.AsGodotObject() as Node3D;
                if (enemyNode == null || enemyNode == context.Source) continue;

                // Simple cone/angle check (90 degrees total, 45 degrees left & right)
                Vector3 toEnemy = (enemyNode.GlobalPosition - context.Source.GlobalPosition).Normalized();
                float angle = Mathf.RadToDeg(playerForward.AngleTo(toEnemy));

                if (angle <= 45.0f)
                {
                    // Access HealthSystem directly as a child of the hit enemy
                    var enemyHealth = enemyNode.GetNodeOrNull<HealthSystem>("HealthSystem");
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage((int)damage);
                        hitCount++;
                    }
                }
            }
        }

        GD.Print($"[Melee Effect] Swung! Hit {hitCount} enemies for {damage} damage.");
        return true;
    }
}