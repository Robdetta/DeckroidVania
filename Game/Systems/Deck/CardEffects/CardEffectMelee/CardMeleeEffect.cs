using Godot;
using System;
using DeckroidVania.Game.Attacks;
using DeckroidVania.Game.Combat.Hitbox; 
using DeckroidVania2.Game.Systems.Deck.CardEffects;
using PlayerClass = DeckroidVania2.Game.Player.Player; // Needed to cast _attacker to Player for IsFacingRight()

namespace DeckroidVania2.Game.Systems.Deck.CardEffects;

public partial class CardMeleeEffect : Node3D
{
    private AttackData _attackData;
    private Node3D _attacker; // This will be the Plaer node

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

        // --- FIX 1: Orient this CardMeleeEffect node itself based on player facing ---
        if (_attacker is PlayerClass player)
        {
            // Set the global position of this CardMeleeEffect to match the player's.
            GlobalPosition = player.GlobalPosition;

            // Explicitly set rotation based on player's facing direction.
            // Assuming IsFacingRight() controls a visual flip or Y-rotation on the player.
            // If player faces right, RotationDegrees.Y = 0 (or original model orientation).
            // If player faces left, RotationDegrees.Y = 180 degrees.
            if (!player.IsFacingRight())
            {
                RotationDegrees = new Vector3(RotationDegrees.X, 180, RotationDegrees.Z); // Rotate 180 degrees around Y-axis
            }
            else
            {
                RotationDegrees = new Vector3(RotationDegrees.X, 0, RotationDegrees.Z); // Ensure no extra Y-rotation if facing right
            }
        }


        PackedScene visualEffectPackedScene = GD.Load<PackedScene>(visualEffectScenePath);

        if (visualEffectPackedScene == null)
        {
            GD.PrintErr($"CardMeleeEffect: Failed to load visual effect scene at path: {visualEffectScenePath}. Deleting CardMeleeEffect.");
            QueueFree(); // Self-destruct if the visual can't be loaded
            return;
        }

        _visualEffectInstance = visualEffectPackedScene.Instantiate<Node3D>();
        
        // Add the specific visual (e.g., dagger) as a child of this controller node.
        // This means _visualEffectInstance's position is local to CardMeleeEffect.
        AddChild(_visualEffectInstance);

        // --- FIX 2: Apply the offset LOCALLY to _visualEffectInstance ---
        // Get the base offset from attacks.json
        // This offset is now relative to the CardMeleeEffect's (player-aligned) local space.
        Vector3 localOffset = _attackData.HitboxOffsetVec;
        
        // Apply this local offset to the visual effect instance.
        _visualEffectInstance.Position = localOffset;
        
        GD.Print($"CardMeleeEffect DEBUG: Visual spawned with local offset {localOffset}.");


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

        GD.Print($"CardMeleeEffect: Started '{animationName}' animation from '{visualEffectScenePath}'.");
    }

    /// <summary>
    /// Called by an Animation Event in the visual effect's AnimationPlayer.
    /// This is the moment to spawn the actual hitbox.
    /// </summary>
    public void SpawnHitbox()
    {
        GD.Print($"CardMeleeEffect: SpawnHitbox called by animation event for attack '{_attackData?.Name}'.");

        // 1. Validate essential data (_attackData and _attacker should not be null)
        if (_attackData == null || _attacker == null)
        {
            GD.PrintErr("CardMeleeEffect: Cannot spawn hitbox; attack data or attacker is null. Aborting hitbox spawn.");
            return;
        }

        // 2. Create HitboxData from _attackData.
        var hitboxData = new HitboxData
        {
            Damage = _attackData.Damage,
            Size = _attackData.HitboxSizeVec,
            Offset = _attackData.HitboxOffsetVec,
            Lifetime = _attackData.HitBoxLifetime ?? 0.2f, // Use defined lifetime or a default
            KnockbackForce = _attackData.KnockbackForce,
            KnockbackDuration = _attackData.KnockbackDuration
        };

        // 3. Instantiate a new HitboxComponent.
        var cardHitbox = new HitboxComponent();

        // 4. Add the HitboxComponent as a child of this CardMeleeEffect node.
        //    This ensures the hitbox moves and rotates with the visual effect.
        AddChild(cardHitbox);

        // 5. Initialize the HitboxComponent.
        //    The target group should be "Enemy" for player-originated attacks.
        //    The HitboxComponent itself handles its position (relative to its parent),
        //    duration, and damage logic after initialization.
        cardHitbox.Initialize(hitboxData, "Enemy");

        GD.Print($"CardMeleeEffect: Spawned HitboxComponent for '{_attackData.Name}'. Damage: {hitboxData.Damage}, Lifetime: {hitboxData.Lifetime}, Target: 'Enemy'.");
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
        QueueFree(); // Remove this CardMeleeEffect node (and its children, including the visual)
    }
}