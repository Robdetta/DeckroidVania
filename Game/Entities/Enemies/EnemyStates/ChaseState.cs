using Godot;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    public class ChaseState : IEnemyState
    {
        private Enemy _enemy; // Direct reference to Enemy
        private Node3D _target;
        private float _chaseSpeed;
        
        public bool CanBeKnockedBack => true;  // Chasing can be knocked back
        public bool CanTakeDamage => true;     // Takes full damage
        
        public ChaseState(Enemy enemy, float chaseSpeed)
        {
            _enemy = enemy;
            _chaseSpeed = chaseSpeed;
        }
        
        public void Enter()
        {
            // NEW: Use AnimationComponent
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Running);
            }
        }
        
        public void Exit()
        {
        }
        
        public void HandleInput(double delta)
        {
            // Enemies don't have input
        }

        public void UpdateState(double delta)
        {
            // NEW: Use AIComponent to check target
            if (_enemy?.AIComponent == null || !_enemy.AIComponent.HasTarget())
            {
                if (_enemy?.AIComponent != null)
                {
                    _enemy.AIComponent.ChangeState(EnemyState.Patrol);
                }
                
                return;
            }

            _target = _enemy.AIComponent.CurrentTarget;
            float distanceToTarget = _enemy.AIComponent.GetDistanceToTarget();

            // Only check for attacks when we're reasonably close (within 10 meters)
            // This prevents spamming warnings when enemy is far from player
            const float ATTACK_CHECK_DISTANCE = 10f;
            
            if (distanceToTarget <= ATTACK_CHECK_DISTANCE)
            {
                // Use CombatComponent to determine attack range
                if (_enemy?.CombatComponent != null)
                {
                    var attack = _enemy.CombatComponent.SelectAttack(distanceToTarget);
                    if (attack != null)
                    {
                        // We found an attack that works at this distance - switch to attack state
                        if (_enemy?.AIComponent != null)
                        {
                            _enemy.AIComponent.ChangeState(EnemyState.Attack);
                        }
                        return;
                    }
                    // If no attack found but we're within check distance, just keep chasing
                }
            }



            // Calculate direction to target (on X-axis for 2.5D movement)
            Vector3 enemyPos = _enemy.GlobalPosition;
            Vector3 targetPos = _target.GlobalPosition;
            float xDifference = targetPos.X - enemyPos.X;
            
            // ═══════════════════════════════════════════════════════════
            // OPTION D: Stop chasing when target is directly overhead
            // ═══════════════════════════════════════════════════════════
            // If target is roughly above us (small X difference), stop chasing
            // This prevents flipping when player is directly above
            if (Mathf.Abs(xDifference) < 0.5f)
            {
                GD.Print($"[ChaseState] 📍 Target directly above us (X diff: {xDifference:F2}), stopping horizontal chase");
                _enemy.MovementComponent.SetHorizontalVelocity(0f);
                
                // Switch to Idle animation while waiting for player to move
                if (_enemy?.AnimationComponent != null)
                {
                    _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Idle);
                }
                
                return;
            }
            
            // Target is clearly to the left or right - chase normally
            float directionToTarget = Mathf.Sign(xDifference);
            
            // NEW: Use MovementComponent
            if (_enemy?.MovementComponent != null)
            {
                float velocityToSet = directionToTarget * _chaseSpeed;
                _enemy.MovementComponent.SetHorizontalVelocity(velocityToSet);
                _enemy.MovementComponent.FaceRight = (directionToTarget > 0);
                
                // Switch back to Running animation when chasing
                if (_enemy?.AnimationComponent != null)
                {
                    _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Running);
                }
                
                GD.Print($"[ChaseState] 🔄 Chasing in direction: {(directionToTarget > 0 ? "RIGHT" : "LEFT")} (X diff: {xDifference:F2})");
            }

            // NEW: Update animation blend
            if (_enemy?.AnimationComponent != null && _enemy?.MovementComponent != null)
            {
                float currentSpeed = Mathf.Abs(_enemy.MovementComponent.Velocity.X);
                _enemy.AnimationComponent.SetMovementBlend(currentSpeed);
            }
        }
    }
}