using Godot;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Interfaces;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    public partial class KnockBackState : IEnemyState
    {
        private Enemy _enemy; // Direct reference to Enemy
        public Vector3 _knockbackVelocity;
        private float _knockbackDuration;
        public float _knockbackTimer;
        private float _gravity = 800f;

        public KnockBackState(Enemy enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            _knockbackTimer = _knockbackDuration;

            // NEW: Use AnimationComponent
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Knockback);
            }
        }

        public void Exit()
        {
            _knockbackVelocity = Vector3.Zero;
        }

        public void HandleInput(double delta)
        {
            // No input during knockback
        }

        public void UpdateState(double delta)
        {
            _knockbackTimer -= (float)delta;

            if (_knockbackTimer <= 0)
            {
                // Use legacy controller for actual state change
                if (_enemy?._movementController != null)
                {
                    _enemy._movementController.ChangeState(EnemyState.Falling);
                }
                return;
            }

            // Apply gravity to knockback velocity
            _knockbackVelocity.Y -= _gravity * (float)delta;

            // Force Z to 0 to prevent off-axis drift
            _knockbackVelocity.Z = 0f;

            // NEW: Use MovementComponent to set velocity
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.Velocity = _knockbackVelocity;
            }
        }


        public void ApplyKnockback(Vector3 knockbackVelocity, float knockbackDuration)
        {
            _knockbackVelocity = knockbackVelocity;
            _knockbackDuration = knockbackDuration;
        }
    }
}