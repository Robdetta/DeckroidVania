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
        
        // State properties
        public bool CanBeKnockedBack => true;  // Attacking can be interrupted
        public bool CanTakeDamage => true;     // Takes full damage while attacking
        
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
                    // No attack available at this distance - should chase instead
                    GD.Print($"[AttackState] No attack available at distance {distanceToTarget} - falling back to Chase");
                    _enemy.AIComponent.ChangeState(EnemyState.Chase);
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

            // Check if target is STILL in range for our current attack
            // Add a small buffer (1.5x range) to prevent immediate chase-attack loop
            float effectiveRange = _currentAttack.Range * 1.5f;
            if (distanceToTarget > effectiveRange)
            {
                // Always return to Chase to re-evaluate attacks at new distance
                // Once in Chase state, it will re-enter Attack when in range again
                if (_enemy?.AIComponent != null)
                {
                    _enemy.AIComponent.ChangeState(EnemyState.Chase);
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
            // Re-evaluate which attack to use based on current distance
            // This allows dynamic switching between melee/ranged attacks
            if (_enemy?.AIComponent != null && _enemy?.CombatComponent != null)
            {
                float currentDistance = _enemy.AIComponent.GetDistanceToTarget();
                EnemyAttackData newAttack = _enemy.CombatComponent.SelectAttack(currentDistance);
                
                if (newAttack != null)
                {
                    // Found a better attack for this distance
                    if (_currentAttack?.Id != newAttack.Id)
                    {
                        GD.Print($"[AttackState] Switching attack: {_currentAttack?.Name} → {newAttack.Name}");
                    }
                    _currentAttack = newAttack;
                }
                // If no attack available, silently keep current attack
                // This is normal - player might have moved slightly out of range
            }
            
            // ═══════════════════════════════════════════════════════════
            // CHECK FOR STATE TRANSITION (Defensive Moves, Special States)
            // ═══════════════════════════════════════════════════════════
            if (!string.IsNullOrEmpty(_currentAttack?.StateTransition))
            {
                GD.Print($"[AttackState] 🔄 State Transition: {_currentAttack.Name} → {_currentAttack.StateTransition}State");
                
                // Parse the state transition string to EnemyState enum
                if (System.Enum.TryParse(_currentAttack.StateTransition, out EnemyState targetState))
                {
                    _enemy.AIComponent.ChangeState(targetState);
                    return; // Don't execute normal attack - state transition handles it
                }
                else
                {
                    GD.PushWarning($"[AttackState] Unknown state transition: {_currentAttack.StateTransition}");
                    // Fall through to normal attack if state doesn't exist
                }
            }
            
            // ═══════════════════════════════════════════════════════════
            // NORMAL ATTACK EXECUTION
            // ═══════════════════════════════════════════════════════════
            
            // Use CombatComponent to execute attack
            if (_enemy?.CombatComponent != null)
            {
                _enemy.CombatComponent.ExecuteAttack();
            }
            
            // Use AnimationComponent to play animation
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
            
            if (_enemy?.AIComponent != null)
            {
                _enemy.AIComponent.ChangeState(defaultState);
            }
        }
    }
}