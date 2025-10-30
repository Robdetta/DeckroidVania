using Godot;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Types;
using DeckroidVania.Game.Entities.Enemies.Data;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    public class AttackState : IEnemyState
    {
        private Enemy _enemy; // Direct reference to Enemy (component orchestrator)
        private EnemyAttackData _currentAttack; 
        private float _attackTimer;
        
        public AttackState(Enemy enemy)
        {
            _enemy = enemy;
        }
        
        public void Enter()
        {
            // NEW: Use CombatComponent to select attack based on distance to target
            if (_enemy?.CombatComponent != null && _enemy?.AIComponent != null)
            {
                float distanceToTarget = _enemy.AIComponent.GetDistanceToTarget();
                _currentAttack = _enemy.CombatComponent.SelectAttack(distanceToTarget);
                
                if (_currentAttack == null)
                {
                    GD.PushError("[AttackState] No suitable attack found for distance!");
                    return;
                }
            }
            else
            {
                GD.PushError("[AttackState] Components not initialized!");
                return;
            }
            
            // NEW: Use MovementComponent to stop movement
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0f);
            }
            
            _attackTimer = 0f;
            
            // NEW: Use AnimationComponent
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Attack);
            }
        }
        
        public void Exit()
        {
        }
        
        public void HandleInput(double delta)
        {
        }

        public void UpdateState(double delta)
        {
            if (_currentAttack == null) 
                return;

            // NEW: Use AIComponent to check for target
            if (_enemy?.AIComponent == null || !_enemy.AIComponent.HasTarget())
            {
                ReturnToDefaultState();
                return;
            }

            Node3D target = _enemy.AIComponent.CurrentTarget;
            float distanceToTarget = _enemy.AIComponent.GetDistanceToTarget();

            // Don't re-select attacks during attack - stick with the one we entered with
            // This prevents issues where rotating to a shorter-range attack kicks us out
            
            // Check if target is STILL in range for our current attack
            // Add a small buffer (1.5x range) to prevent immediate chase-attack loop
            float effectiveRange = _currentAttack.Range * 1.5f;
            if (distanceToTarget > effectiveRange)
            {
                // Mages return to idle, melee enemies chase
                EnemyState nextState = (_enemy is MageEnemy) ? EnemyState.Idle : EnemyState.Chase;
                
                // Use legacy controller for actual state change
                if (_enemy?._movementController != null)
                {
                    _enemy._movementController.ChangeState(nextState);
                }
                
                return;
            }

            // NEW: Use MovementComponent to stop
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0f);
                
                // Update facing direction
                Vector3 targetPos = target.GlobalPosition;
                Vector3 enemyPos = _enemy.GlobalPosition;
                float directionToTarget = Mathf.Sign(targetPos.X - enemyPos.X);
                _enemy.MovementComponent.FaceRight = (directionToTarget > 0);
            }

            // Attack cooldown timer
            if (_attackTimer > 0f)
            {
                _attackTimer -= (float)delta;
            }
            else
            {
                // NEW: Check if CombatComponent says we can attack
                if (_enemy?.CombatComponent?.CanAttack ?? false)
                {
                    PerformAttack();
                    _attackTimer = _currentAttack.Cooldown;
                }
            }
        }

        private void PerformAttack()
        {
            // NEW: Use CombatComponent to execute attack
            if (_enemy?.CombatComponent != null)
            {
                _enemy.CombatComponent.ExecuteAttack();
            }
            
            // NEW: Use AnimationComponent to play animation
            if (_enemy?.AnimationComponent != null && !string.IsNullOrEmpty(_currentAttack.AnimationName))
            {
                _enemy.AnimationComponent.PlayAttackAnimation(_currentAttack.AnimationName);
            }
        }
        
        private void ReturnToDefaultState()
        {
            // NEW: Use AIComponent to change state
            // Default to Idle for now (can be made configurable later)
            EnemyState defaultState = EnemyState.Idle;
            
            // Check if enemy is a mage or knight to determine default state
            if (_enemy is MageEnemy)
            {
                defaultState = EnemyState.Idle; // Mages idle by default
            }
            else
            {
                defaultState = EnemyState.Patrol; // Knights patrol by default
            }
            
            // Use legacy controller for actual state change
            if (_enemy?._movementController != null)
            {
                _enemy._movementController.ChangeState(defaultState);
            }
        }
    }
}