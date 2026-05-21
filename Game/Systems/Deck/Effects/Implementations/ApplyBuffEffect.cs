using Godot;
using System.Collections.Generic;

namespace DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

public class ApplyBuffEffect : ICardEffect
{
    public bool Execute(EffectContext context, Dictionary<string, object> effectParams)
    {
        string statType = effectParams.GetValueOrDefault("statType", "Defense").ToString();
        float modifier = System.Convert.ToSingle(effectParams.GetValueOrDefault("modifier", 0.5f));
        float duration = System.Convert.ToSingle(effectParams.GetValueOrDefault("duration", 5.0f));

        // We can query custom systems or state controllers on our source node 
        // Example: Looking for a status manager on the Player
        var statusManager = context.Source.GetNodeOrNull("StatusManager");
        if (statusManager != null)
        {
            statusManager.Call("ApplyBuff", statType, modifier, duration);
        }

        GD.Print($"[Buff Effect] Applied {statType} buff (+{modifier * 100}%) to {context.Source.Name} for {duration} seconds.");
        return true;
    }
}