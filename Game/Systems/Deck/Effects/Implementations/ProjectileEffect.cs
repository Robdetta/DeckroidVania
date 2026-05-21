using Godot;
using System.Collections.Generic;

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

public class ProjectileEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        float damage = System.Convert.ToSingle(effectParams.GetValueOrDefault("damage", 15.0f));
        float speed = System.Convert.ToSingle(effectParams.GetValueOrDefault("speed", 15.0f));
        string scenePath = effectParams.GetValueOrDefault("scenePath", "").ToString();

        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PrintErr("ProjectileEffect: No 'scenePath' specified in JSON params.");
            return false;
        }

        // Load and spawn the projectile scene
        PackedScene projectileScene = GD.Load<PackedScene>(scenePath);
        if (projectileScene == null) return false;

        var projInstance = projectileScene.Instantiate<Node3D>();
        context.Tree.CurrentScene.AddChild(projInstance);

        // Position it slightly in front of the source
        Vector3 spawnPoint = context.Source.GlobalPosition + (-context.Source.GlobalTransform.Basis.Z * 1.5f);
        projInstance.GlobalPosition = spawnPoint;

        // If you have a custom Projectile script attached, configure it here:
        // (Assuming you have a script on the scene with a Launch method)
        if (projInstance.HasMethod("Launch"))
        {
            projInstance.Call("Launch", context.Source, context.TargetPosition, speed, damage);
        }

        GD.Print($"[Projectile Effect] Fired {scenePath} towards target position!");
        return true;
    }
}