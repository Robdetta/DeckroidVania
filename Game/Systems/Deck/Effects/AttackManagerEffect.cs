using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

/// <summary>
/// A card effect that acts as a bridge to trigger attacks defined in attacks.json 
/// using your existing AttackManager!
/// </summary>
public class AttackManagerEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        if (context.Source == null) return false;

        // 1. Get the Attack Name or Attack ID from the JSON parameters
        string attackName = GetStringParam(effectParams, "attackName", "");
        int attackId = GetIntParam(effectParams, "attackId", -1);

        // 2. Locate the AttackManager on your Player node
        // In your project structure, your AttackManager is likely stored as a property/field on the Player class,
        // or instantiated dynamically. We can query it if you have exposed it on your Player class,
        // or call a method on the Player node to handle it.
        
        // If your Player script exposes a method to perform an attack:
        if (context.Source.HasMethod("ExecuteCardAttack"))
        {
            if (attackId != -1)
            {
                context.Source.Call("ExecuteCardAttack", attackId);
            }
            else if (!string.IsNullOrEmpty(attackName))
            {
                context.Source.Call("ExecuteCardAttack", attackName);
            }
            return true;
        }

        // ALTERNATIVE: Access the AttackManager dynamically if instantiated inside the Player script
        // Let's print a diagnostic message if we can't find the interface
        GD.PrintErr($"AttackManagerEffect: {context.Source.Name} does not implement 'ExecuteCardAttack' method. Please add this helper method to your Player.cs!");
        return false;
    }

    private string GetStringParam(Dictionary<string, object> dict, string key, string defaultValue)
    {
        if (dict.TryGetValue(key, out object val))
        {
            return val is JsonElement element ? element.GetString() : val.ToString();
        }
        return defaultValue;
    }

    private int GetIntParam(Dictionary<string, object> dict, string key, int defaultValue)
    {
        if (dict.TryGetValue(key, out object val))
        {
            if (val is JsonElement element) return element.GetInt32();
            return Convert.ToInt32(val);
        }
        return defaultValue;
    }
}