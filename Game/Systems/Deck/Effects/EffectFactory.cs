using Godot;
using DeckroidVania2.Game.Systems.Deck.Effects.Implementations;

namespace DeckroidVania2.Game.Systems.Deck.Effects;

/// <summary>
/// Responsible for taking a string from the JSON (e.g., "Heal") 
/// and returning the correct C# logic class.
/// </summary>
public static class EffectFactory
{
    public static ICardEffect Create(string effectType)
    {
        switch (effectType)
        {
            case "Heal":
                return new HealEffect();
            case "Melee":
                return new MeleeEffect();
            case "Projectile":
                return new ProjectileEffect();
            case "ApplyBuff":
                return new ApplyBuffEffect();
            case "AttackManager":
                return new AttackManagerEffect();                      
            default:
                GD.PrintErr($"EffectFactory: Unknown effect type '{effectType}'");
                return null;
        }
    }
}