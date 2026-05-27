using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json; // Needed for JsonElement
using PlayerClass = DeckroidVania2.Game.Player.Player;
using DeckroidVania.Game.Combat.Hitbox; // Needed for Projectile (if it's in this namespace or similar)


namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

public partial class ProjectileEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        if (context.Source == null)
        {
            GD.PrintErr("ProjectileEffect: Context Source is null. Effect aborted.");
            return false;
        }

        string scenePath = GetStringParam(effectParams, "scenePath", "res://Game/Entities/Player/Attacks/Projectiles/Projectile.tscn");
        int damage = GetIntParam(effectParams, "damage", 15);
        float speed = GetFloatParam(effectParams, "speed", 15.0f);
        float lifetime = GetFloatParam(effectParams, "lifetime", 2.0f);
        float knockbackForce = GetFloatParam(effectParams, "knockbackForce", 0f);
        float knockbackDuration = GetFloatParam(effectParams, "knockbackDuration", 0f);
        string colorHex = GetStringParam(effectParams, "color", "#FFFFFF");
        // --- NEW: Extract spawnOffset ---
        Vector3 spawnOffset = GetVector3Param(effectParams, "spawnOffset", Vector3.Zero); 


        PackedScene projectileScene = GD.Load<PackedScene>(scenePath);
        if (projectileScene == null)
        {
            GD.PrintErr($"ProjectileEffect: Failed to load projectile scene at {scenePath}. Effect aborted.");
            return false;
        }

        Projectile projectileInstance = projectileScene.Instantiate<Projectile>();
        if (projectileInstance == null)
        {
            GD.PrintErr($"ProjectileEffect: Instantiated scene at {scenePath} is not of type Projectile. Effect aborted.");
            return false;
        }

        Vector3 finalSpawnPosition = context.Source.GlobalPosition; // Start at player's global position
        Vector3 direction = -context.Source.GlobalTransform.Basis.Z.Normalized(); // Default forward

        // --- NEW: Apply spawnOffset based on player facing ---
        if (context.Source is PlayerClass player)
        {
            bool facingRight = player.IsFacingRight();
            float facing = facingRight ? 1f : -1f;
            direction = new Vector3(facing, 0, 0); // Direction for projectile movement

            // First, try to spawn from a specific HandMarker if it exists
            var handNode = player.GetNodeOrNull<Node3D>("Visual/RootNode/Skeleton/HandAttachment/HandContainer");
            if (handNode != null)
            {
                finalSpawnPosition = handNode.GlobalPosition;
            }
            // THEN, apply the configurable spawnOffset relative to the player's facing.
            // If the player is facing left, flip the X component of the offset.
            finalSpawnPosition += new Vector3(spawnOffset.X * facing, spawnOffset.Y, spawnOffset.Z);
        }
        else
        {
            // For non-player sources, apply offset directly (or implement specific logic)
            finalSpawnPosition += spawnOffset;
        }

        context.Tree.CurrentScene.AddChild(projectileInstance);
        projectileInstance.GlobalPosition = finalSpawnPosition; // Set the projectile's global position

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

        GD.Print($"[Projectile Card Effect] Successfully spawned '{scenePath}' with {damage} damage at {finalSpawnPosition}.");
        return true;
    }

    // --- NEW HELPER METHOD: GetVector3Param ---
    private Vector3 GetVector3Param(Dictionary<string, object> dict, string key, Vector3 defaultValue)
    {
        if (dict != null && dict.TryGetValue(key, out object val) && val != null)
        {
            if (val is JsonElement element && element.ValueKind == JsonValueKind.Array)
            {
                if (element.GetArrayLength() == 3)
                {
                    float x = (float)element[0].GetDouble();
                    float y = (float)element[1].GetDouble();
                    float z = (float)element[2].GetDouble();
                    return new Vector3(x, y, z);
                }
                GD.PrintErr($"GetVector3Param: Array for key '{key}' does not have 3 elements. Using default.");
            }
            else if (val is float[] floatArray && floatArray.Length == 3)
            {
                return new Vector3(floatArray[0], floatArray[1], floatArray[2]);
            }
            GD.PrintErr($"GetVector3Param: Value for key '{key}' is not a valid Vector3 array. Using default.");
        }
        return defaultValue;
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