using Godot;
using System.Collections.Generic;

namespace DeckroidVania2.Game.Systems.Deck.Effects;


/// The base interface for all card effects.
/// Any logic that happens when a card is activated or sacrificed must implement this.
public interface ICardEffect
{

    /// Executes the card effect logic.
    /// <param name="context">The context containing the player, world data, etc.</param>
    /// <param name="effectParams">A dictionary of parameters loaded from JSON (e.g., {"amount": 30})</param>
    /// <returns>True if the effect executed successfully, false otherwise.</returns>
    bool Execute(EffectContext context, Dictionary<string, object> effectParams);
}