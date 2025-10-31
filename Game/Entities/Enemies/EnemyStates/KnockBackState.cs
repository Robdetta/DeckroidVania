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
        
        public bool CanBeKnockedBack => false; // Already in knockback, can't stack!
        public bool CanTakeDamage => true;     // Can still take damage while flying back

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
                // Return to appropriate state based on whether we have a target
                if (_enemy?.AIComponent != null)
                {
                    // If we have a target, return to combat (Chase or Attack)
                    if (_enemy.AIComponent.HasTarget())
                    {
                        float distanceToTarget = _enemy.AIComponent.GetDistanceToTarget();
                        
                        // Check if ANY attack is available at current distance
                        // This is the proper way - don't assume CurrentAttack is still valid
                        if (_enemy.CombatComponent != null)
                        {
                            var availableAttack = _enemy.CombatComponent.SelectAttack(distanceToTarget);
                            if (availableAttack != null)
                            {
                                // Attack is available at this distance - go to Attack state
                                GD.Print($"[KnockBackState] Knockback recovery → Attack (attack available at distance {distanceToTarget})");
                                _enemy.AIComponent.ChangeState(EnemyState.Attack);
                            }
                            else
                            {
                                // No attack available, need to chase to get closer
                                GD.Print($"[KnockBackState] Knockback recovery → Chase (no attack at distance {distanceToTarget})");
                                _enemy.AIComponent.ChangeState(EnemyState.Chase);
                            }
                        }
                        else
                        {
                            // No combat component, default to chase
                            _enemy.AIComponent.ChangeState(EnemyState.Chase);
                        }
                    }
                    else
                    {
                        // No target, return to idle/patrol
                        _enemy.AIComponent.ChangeState(EnemyState.Idle);
                    }
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