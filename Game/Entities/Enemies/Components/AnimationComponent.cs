using Godot;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using static EnemyAnimationTree;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Wrapper around EnemyAnimationTree to provide component interface
    /// Responsibilities: animation state changes, attack animations, movement blending
    /// </summary>
    public partial class AnimationComponent : Node, IAnimationComponent
    {
        private EnemyAnimationTree _animationTree;

        public override void _Ready()
        {
            // AnimationTree is typically a child of the enemy's visual node
            // We'll need to find it when the component is initialized
        }

        // Interface method - parameterless
        public void Initialize()
        {
            // Try to find AnimationTree automatically
            var owner = GetParent<Node3D>();
            if (owner != null)
            {
                _animationTree = owner.GetNodeOrNull<EnemyAnimationTree>("Visual/AnimationTree");
                if (_animationTree == null)
                {
                    GD.PushWarning("[AnimationComponent] Could not find AnimationTree automatically");
                }
                else
                {
                    GD.Print($"[AnimationComponent] Auto-initialized with {_animationTree.GetType().Name}");
                }
            }
        }

        // Overload for explicit initialization
        public void Initialize(EnemyAnimationTree animationTree)
        {
            _animationTree = animationTree;
            
            if (_animationTree != null)
                GD.Print($"[AnimationComponent] Initialized with {_animationTree.GetType().Name}");
            else
                GD.PushError("[AnimationComponent] AnimationTree is NULL!");
        }

        public void ChangeState(int stateId)
        {
            if (_animationTree == null)
            {
                GD.PushWarning("[AnimationComponent] Cannot change state - AnimationTree not initialized");
                return;
            }

            // EnemyAnimationTree uses EnemyAnimationState enum internally
            // We convert the int to enum for the actual call
            _animationTree.ChangeState((EnemyAnimationState)stateId);
            GD.Print($"[AnimationComponent] Animation state changed to: {(EnemyAnimationState)stateId}");
        }

        public void PlayAttackAnimation(string attackName)
        {
            if (_animationTree == null)
            {
                GD.PushWarning("[AnimationComponent] Cannot play attack - AnimationTree not initialized");
                return;
            }

            _animationTree.PlayAttackAnimation(attackName);
            GD.Print($"[AnimationComponent] Playing attack animation: {attackName}");
        }

        public void SetMovementBlend(float speed)
        {
            if (_animationTree == null)
            {
                GD.PushWarning("[AnimationComponent] Cannot set blend - AnimationTree not initialized");
                return;
            }

            // Convert speed to blend value (0 = idle, 1 = full run)
            // Assuming max speed is around 5-8 units/sec
            float blend = Mathf.Clamp(speed / 5f, 0f, 1f);
            
            // EnemyAnimationTree should expose a method for this
            // For now, we'll call a generic method that subclasses can override
            if (_animationTree.HasMethod("SetMovementBlend"))
            {
                _animationTree.Call("SetMovementBlend", blend);
            }
        }

        public void Update(double delta)
        {
            // Animation updates are typically handled by AnimationTree itself
            // This is here if we need frame-by-frame animation logic
        }
    }
}
