using Godot;
using System;

namespace DeckroidVania2.Game.Systems.Deck.CardEffects.CardEffectMelee.Axe; // Adjust namespace as needed

public partial class AxeChopEffect : Node3D
{
    // This method will be called by the AnimationPlayer's "Call Method Track"
    public void CallParentSpawnHitbox()
    {
        // Try to cast the parent to CardMeleeEffect and call its SpawnHitbox method
        if (GetParent() is CardMeleeEffect cardMeleeEffect)
        {
            cardMeleeEffect.SpawnHitbox();
        }
        else
        {
            GD.PrintErr("AxeChopEffect: Parent is not CardMeleeEffect. Cannot call SpawnHitbox.");
        }
    }
}