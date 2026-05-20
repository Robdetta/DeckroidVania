using Godot;
using System.Collections.Generic;
using DeckroidVania2.Game.Systems.GameSystems; // Where HealthComponent live

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

/// <summary>
/// An effect that heals the entity that played the card (the source).
/// </summary>
public class HealEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        // 1. Validate parameters
        if (!effectParams.TryGetValue("amount", out object amountObj))
        {
            GD.PrintErr("HealEffect missing 'amount' parameter.");
            return false;
        }

        // Parse the amount (JSON numbers usually come in as floats/doubles or ints)
        int healAmount = System.Convert.ToInt32(amountObj);

        // 2. Ensure we have a valid source (the Player)
        if (context.Source == null) return false;

        // 3. OBJECT COMPOSITION IN ACTION: 
        // We don't care *what* class the source is, we just ask: "Do you have a HealthComponent?"
        var healthComp = context.Source.GetNode<HealthSystem>("HealthSystem");
        
        if (healthComp != null)
        {
            // 4. Apply the effect
            healthComp.Heal(healAmount);
            GD.Print($"HealEffect executed: Healed {context.Source.Name} for {healAmount} HP.");
            return true;
        }

        GD.PrintErr($"HealEffect failed: {context.Source.Name} does not have a HealthComponent.");
        return false;
    }
}