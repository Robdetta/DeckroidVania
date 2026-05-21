using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using PlayerClass = DeckroidVania2.Game.Player.Player;

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

public class ProjectileEffect : ICardEffect
{
public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
{
if (context.Source == null)
{
GD.PrintErr("ProjectileEffect: Context Source is null.");
return false;
}

    string scenePath = GetStringParam(effectParams, "scenePath", "res://Game/Entities/Player/Attacks/Projectiles/Projectile.tscn");
    int damage = GetIntParam(effectParams, "damage", 15);
    float speed = GetFloatParam(effectParams, "speed", 15.0f);
    float lifetime = GetFloatParam(effectParams, "lifetime", 2.0f);
    float knockbackForce = GetFloatParam(effectParams, "knockbackForce", 0f);
    float knockbackDuration = GetFloatParam(effectParams, "knockbackDuration", 0f);
    string colorHex = GetStringParam(effectParams, "color", "#FFFFFF");

    PackedScene projectileScene = GD.Load<PackedScene>(scenePath);
    if (projectileScene == null)
    {
        GD.PrintErr($"ProjectileEffect: Failed to load projectile scene at {scenePath}");
        return false;
    }

    Projectile projectileInstance = projectileScene.Instantiate<Projectile>();
    if (projectileInstance == null)
    {
        GD.PrintErr($"ProjectileEffect: Instantiated scene at {scenePath} is not of type Projectile.");
        return false;
    }

    Vector3 spawnPosition = context.Source.GlobalPosition;
    Vector3 direction = -context.Source.GlobalTransform.Basis.Z.Normalized();

    if (context.Source is PlayerClass player)
    {
        bool facingRight = player.IsFacingRight();
        float facing = facingRight ? 1f : -1f;
        direction = new Vector3(facing, 0, 0);

        var handNode = player.GetNodeOrNull<Node3D>("Visual/RootNode/HandMarker");
        if (handNode != null)
        {
            spawnPosition = handNode.GlobalPosition;
        }
    }

    context.Tree.CurrentScene.AddChild(projectileInstance);
    projectileInstance.GlobalPosition = spawnPosition;

    Color projectileColor = new Color(colorHex);
    projectileInstance.Initialize(
        direction: direction,
        damage: damage,
        speed: speed,
        color: projectileColor,
        knockbackForce: knockbackForce,
        knockbackDuration: knockbackDuration,
        owner: context.Source
    );
    
    projectileInstance.Lifetime = lifetime;

    GD.Print($"[Projectile Card Effect] Successfully spawned '{scenePath}' with {damage} damage.");
    return true;
}

private string GetStringParam(Dictionary<string, object> dict, string key, string defaultValue)
{
    if (dict != null && dict.TryGetValue(key, out object val) && val != null)
    {
        return val is JsonElement element ? element.GetString() : val.ToString();
    }
    return defaultValue;
}

private int GetIntParam(Dictionary<string, object> dict, string key, int defaultValue)
{
    if (dict != null && dict.TryGetValue(key, out object val) && val != null)
    {
        if (val is JsonElement element && element.ValueKind == JsonValueKind.Number)
        {
            return element.TryGetInt32(out int result) ? result : (int)element.GetDouble();
        }
        return Convert.ToInt32(val);
    }
    return defaultValue;
}

private float GetFloatParam(Dictionary<string, object> dict, string key, float defaultValue)
{
    if (dict != null && dict.TryGetValue(key, out object val) && val != null)
    {
        if (val is JsonElement element && element.ValueKind == JsonValueKind.Number)
        {
            return (float)element.GetDouble();
        }
        return Convert.ToSingle(val);
    }
    return defaultValue;
}

}