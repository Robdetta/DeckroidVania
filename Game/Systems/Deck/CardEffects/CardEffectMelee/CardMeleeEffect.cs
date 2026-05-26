using Godot;
using System;
using DeckroidVania.Game.Combat.Hitbox; // Needed for AttackData
using DeckroidVania.Game.Attacks;
using DeckroidVania2.Game.Systems.Deck.CardEffects;
using PlayerClass = DeckroidVania2.Game.Player.Player; // Needed to cast _attacker to Player for IsFacingRight()

namespace DeckroidVania2.Game.Systems.Deck.CardEffects;

public partial class CardMeleeEffect : Node3D
{
    private AttackData _attackData;
    private Node3D _attacker; // This will be the Player node

    private Node3D _visualEffectInstance;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        // _Ready is not strictly needed here since StartAttack handles dynamic setup.
        // But if you had child nodes that were part of CardMeleeEffect.tscn itself (not dynamically loaded),
        // you would get references to them here.
    }

    /// <summary>
    /// Initializes and starts the card melee attack effect.
    /// </summary>
    /// <param name="attackData">The data defining the attack (damage, knockback, hitbox, etc.).</param>
    /// <param name="visualEffectScenePath">The path to the PackedScene containing the specific visual model and its animation (e.g., dagger, axe).</param>
    /// <param name="attacker">The Node3D that is initiating this attack (expected to be the Player).</param>
    /// <param name="animationName">The name of the animation to play within the visualEffectScene.</param>
    public void StartAttack(AttackData attackData, string visualEffectScenePath, Node3D attacker, string animationName)
    {
        _attackData = attackData;
        _attacker = attacker; 

        PackedScene visualEffectPackedScene = GD.Load<PackedScene>(visualEffectScenePath);

        if (visualEffectPackedScene == null)
        {
            GD.PrintErr($"CardMeleeEffect: Failed to load visual effect scene at path: {visualEffectScenePath}. Deleting CardMeleeEffect.");
            QueueFree(); // Self-destruct if the visual can't be loaded
            return;
        }

        _visualEffectInstance = visualEffectPackedScene.Instantiate<Node3D>();
        
        // Add the specific visual (e.g., dagger) as a child of this controller node.
        // This means _visualEffectInstance's position is relative to CardMeleeEffect.
        AddChild(_visualEffectInstance);

        // --- FIX: Position the visual effect using the AttackData's offset ---
        // Get the base offset from attacks.json
        Vector3 finalOffset = _attackData.HitboxOffsetVec;

        // Adjust X-offset based on player's facing direction
        if (_attacker is PlayerClass player) // Check if the attacker is indeed your Player class
        {
            // Assuming IsFacingRight() returns true for right, false for left.
            // And that a positive X offset means 'right' in attacks.json.
            if (!player.IsFacingRight())
            {
                finalOffset.X *= -1; // Flip X-component of the offset if facing left
            }
        }
        // Apply this final offset to the visual effect instance.
        // Its position is local to its parent (this CardMeleeEffect node).
        _visualEffectInstance.Position = finalOffset;

        // --- Find and play the animation ---
        _animationPlayer = _visualEffectInstance.GetNode<AnimationPlayer>("AnimationPlayer"); // Assuming "AnimationPlayer" is the correct name in your visual effect scene

        if (_animationPlayer == null)
        {
            GD.PrintErr($"CardMeleeEffect: AnimationPlayer not found in visual effect instance '{_visualEffectInstance.Name}'. Deleting CardMeleeEffect.");
            QueueFree();
            return;
        }

        _animationPlayer.Play(animationName);
        _animationPlayer.AnimationFinished += OnAnimationFinished; // Connect the cleanup signal

        GD.Print($"CardMeleeEffect: Started '{animationName}' animation from '{visualEffectScenePath}'. Visual spawned at local offset {finalOffset}.");
    }

    /// <summary>
    /// Called by an Animation Event in the visual effect's AnimationPlayer.
    /// This is the moment to spawn the actual hitbox.
    /// </summary>
    public void SpawnHitbox()
    {
        GD.Print($"CardMeleeEffect: SpawnHitbox called by animation event for attack '{_attackData?.Name}'.");

        // --- TODO: Implement hitbox spawning here, similar to AttackManager.ActivateHitbox() ---
        // This will be our next step after confirming animation/despawn/position.

        // For now, if you want to see if the event fires:
        // GD.Print("Hitbox spawning event triggered!");
    }

    /// <summary>
    /// Called when the animation played by _animationPlayer finishes.
    /// Used for cleaning up this effect node.
    /// </summary>
    /// <param name="animName">The name of the animation that finished.</param>
    private void OnAnimationFinished(StringName animName)
    {
        GD.Print($"CardMeleeEffect: Animation '{animName}' finished. QueueFree()ing this node.");
        if (_animationPlayer != null)
        {
            _animationPlayer.AnimationFinished -= OnAnimationFinished; // Disconnect the signal
        }
        // --- FIX: Add QueueFree() to make the effect disappear ---
        QueueFree(); // Remove this CardMeleeEffect node (and its children, including the visual)
    }
}