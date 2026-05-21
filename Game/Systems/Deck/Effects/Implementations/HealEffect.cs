using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using DeckroidVania2.Game.Systems.GameSystems; // Where HealthComponent/HealthSystem lives

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

/// <summary>
/// An effect that heals the entity that played the card (the source).
/// </summary>
public class HealEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        // 1. Validate parameters
        if (effectParams == null || !effectParams.TryGetValue("amount", out object amountObj))
        {
            GD.PrintErr("HealEffect missing 'amount' parameter.");
            return false;
        }

        // 2. Safely extract the primitive int from System.Text.Json's JsonElement structure
        int healAmount = 0;
        
        if (amountObj is JsonElement jsonElement)
        {
            // Safely grab the integer directly out of the JSON element
            healAmount = jsonElement.GetInt32();
        }
        else
        {
            // Fallback for standard C# types if parsed manually elsewhere
            healAmount = Convert.ToInt32(amountObj);
        }

        // 3. Ensure we have a valid source (the Player)
        if (context.Source == null) 
        {
            GD.PrintErr("HealEffect failed: Context Source node is null.");
            return false;
        }

        // 4. OBJECT COMPOSITION IN ACTION:
        // First, check if the global singleton HealthSystem.Instance exists (since your damage system uses it).
        // If not, fall back to searching for a local "HealthSystem" node attached directly to the source.
        HealthSystem activeHealthSystem = HealthSystem.Instance;

        if (activeHealthSystem == null)
        {
            activeHealthSystem = context.Source.GetNodeOrNull<HealthSystem>("HealthSystem");
        }
        
        if (activeHealthSystem != null)
        {
            // 5. Apply the effect
            activeHealthSystem.Heal(healAmount);
            GD.Print($"HealEffect executed: Healed {context.Source.Name} for {healAmount} HP via HealthSystem.");
            return true;
        }

        GD.PrintErr($"HealEffect failed: HealthSystem.Instance is null and {context.Source.Name} does not have a local HealthSystem child node.");
        return false;
    }
}