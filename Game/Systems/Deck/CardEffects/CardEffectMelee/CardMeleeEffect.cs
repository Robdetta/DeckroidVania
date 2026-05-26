using Godot;
using System;
using DeckroidVania.Game.Combat.Hitbox;

namespace DeckroidVania2.Game.Systems.Deck.CardEffects;

public partial class CardMeleeEffect : Node3D
{
    private AttackData _attackData;
    private Node3D _attacker;

    private Node3D _visualEffectInstance;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        // This will be called when the node is added to the scene.
        // You can initialize any necessary components here.
    }

    public void StartAttack(AttackData attackData, string visualEffectScenePath, Node3D attacker, string animationName)
    {
     _attackData = attackData;
     _attacker = attacker; 

    PackedScene visualEffectPackedScene = GD.Load<PackedScene>(visualEffectScenePath);

    if (visualEffectPackedScene == null)
    {
        GD.PrintErr($"Failed to load visual effect scene at path: {visualEffectScenePath}");
        QueueFree();
        return;
        
    }

    _visualEffectInstance = visualEffectPackedScene.Instantiate<Node3D>();

    AddChild(_visualEffectInstance);
    _animationPlayer = _visualEffectInstance.GetNode<AnimationPlayer>("AnimationPlayer");

    if (_animationPlayer == null)
    {
        GD.PrintErr($"AnimationPlayer not found in visual effect instance. : {_visualEffectInstance.Name}");
        QueueFree();
        return;
    }
    _animationPlayer.Play(animationName);

    _animationPlayer.AnimationFinished += OnAnimationFinished; 

    }

    public void SpawnHitbox()
    {
        GD.Print($"CardMeleeEffect: SpawnHitbox called by animation event for attack '{_attackData?.Name}'.");
    }

    private void OnAnimationFinished(StringName animName)
    {
        if (_animationPlayer != null)
        {
            _animationPlayer.AnimationFinished -= OnAnimationFinished; 
        }
    }
}
