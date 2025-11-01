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
        
        // Decision timer to prevent re-rolling every frame
        private float _decisionTimer = 0f;
        private float _decisionCooldown = 0.5f; // Re-decide every 0.5 seconds
        private bool _currentDecisionIsMelee = false;
        private bool _currentDecisionIsBackAway = false;
        
        public bool CanBeKnockedBack => true;  // Chasing can be knocked back
        public bool CanTakeDamage => true;     // Takes full damage
        
        public ChaseState(Enemy enemy, float chaseSpeed)
        {
            _enemy = enemy;
            _chaseSpeed = chaseSpeed;
        }
        
        public void Enter()
        {
            // Don't set animation here - let UpdateState handle it based on what we're actually doing
            // This prevents the "run in place" stutter when transitioning from Attack
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
            // Update decision timer
            if (_decisionTimer > 0f)
            {
                _decisionTimer -= (float)delta;
            }
            
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

            // Check spatial behavior configuration
            var spatialBehavior = _enemy?.EnemyData?.Combat?.SpatialBehavior;
            
            // ═══════════════════════════════════════════════════════════════════
            // ZONE LOGIC: Defensive positioning (if repositioning enabled)
            // ═══════════════════════════════════════════════════════════════════
            if (spatialBehavior != null && spatialBehavior.RepositioningEnabled)
            {
                // ZONE 1: TOO CLOSE - Melee or back away based on aggression
                if (distanceToTarget < spatialBehavior.OptimalRangeMin)
                {
                    GD.Print($"[ChaseState] ❌ TOO CLOSE ({distanceToTarget:F1}m < {spatialBehavior.OptimalRangeMin}m)");
                    
                    // Make a decision once per cooldown (not every frame!)
                    if (_decisionTimer <= 0f)
                    {
                        float meleeRoll = GD.Randf();
                        _currentDecisionIsMelee = meleeRoll < spatialBehavior.MeleeAggressiveness;
                        
                        if (!_currentDecisionIsMelee)
                        {
                            float backAwayRoll = GD.Randf();
                            _currentDecisionIsBackAway = backAwayRoll < spatialBehavior.BackAwayChance;
                        }
                        
                        _decisionTimer = _decisionCooldown;
                        
                        GD.Print($"[ChaseState] 🎲 NEW DECISION: Melee={_currentDecisionIsMelee}, BackAway={_currentDecisionIsBackAway}");
                    }
                    
                    // Execute the committed decision
                    if (_currentDecisionIsMelee && _enemy?.CombatComponent != null)
                    {
                        var attack = _enemy.CombatComponent.SelectAttack(distanceToTarget);
                        if (attack != null && _enemy.CombatComponent.CanAttack)
                        {
                            GD.Print($"[ChaseState] ⚡ EXECUTING MELEE");
                            _enemy.MovementComponent.SetHorizontalVelocity(0f);
                            _enemy.AIComponent.ChangeState(EnemyState.Attack);
                            return;
                        }
                        else
                        {
                            // Wanted to melee but can't (cooldown or out of range)
                            // Default to backing away instead of standing still
                            GD.Print($"[ChaseState] ⚠️ Can't melee (cooldown), backing away instead");
                            BackAwayFromTarget();
                            return;
                        }
                    }
                    else if (_currentDecisionIsBackAway)
                    {
                        GD.Print($"[ChaseState] 🏃 EXECUTING BACK AWAY");
                        BackAwayFromTarget();
                        return;
                    }
                    else
                    {
                        // Decided to stand ground - hold position
                        GD.Print($"[ChaseState] 🚫 STAND GROUND");
                        _enemy.MovementComponent.SetHorizontalVelocity(0f);
                        return;
                    }
                }
                
                // ZONE 2: OPTIMAL RANGE - Try to attack
                if (distanceToTarget >= spatialBehavior.OptimalRangeMin && 
                    distanceToTarget <= spatialBehavior.OptimalRangeMax)
                {
                    // Reset zone 1 decision when we move away
                    _decisionTimer = 0f;
                    
                    GD.Print($"[ChaseState] ✅ OPTIMAL RANGE ({spatialBehavior.OptimalRangeMin}m < {distanceToTarget:F1}m < {spatialBehavior.OptimalRangeMax}m)");
                    
                    if (_enemy?.CombatComponent != null && _enemy.CombatComponent.CanAttack)
                    {
                        var attack = _enemy.CombatComponent.SelectAttack(distanceToTarget);
                        if (attack != null)
                        {
                            _enemy.MovementComponent.SetHorizontalVelocity(0f);
                            _enemy.AIComponent.ChangeState(EnemyState.Attack);
                            return;
                        }
                    }
                    else if (_enemy?.CombatComponent != null && !_enemy.CombatComponent.CanAttack)
                    {
                        // On cooldown - maintain distance but don't lock up
                        // Let it continue moving to maintain optimal range
                        GD.Print($"[ChaseState] ⏳ On cooldown, maintaining range");
                        return;
                    }
                    
                    // Can't attack and not on cooldown - just hold position
                    _enemy.MovementComponent.SetHorizontalVelocity(0f);
                    return;
                }
                
                // ZONE 3: TOO FAR - Chase closer
                GD.Print($"[ChaseState] 🏃 TOO FAR ({distanceToTarget:F1}m > {spatialBehavior.OptimalRangeMax}m) - Chasing");
                ChaseTowardTarget();
                return;
            }
            
            // ═══════════════════════════════════════════════════════════════════
            // SIMPLE LOGIC: Just chase and attack (if repositioning disabled)
            // ═══════════════════════════════════════════════════════════════════
            if (_enemy?.CombatComponent != null)
            {
                var attack = _enemy.CombatComponent.SelectAttack(distanceToTarget);
                if (attack != null)
                {
                    GD.Print($"[ChaseState] ✅ Attack found at {distanceToTarget:F1}m");
                    _enemy.MovementComponent.SetHorizontalVelocity(0f);
                    _enemy.AIComponent.ChangeState(EnemyState.Attack);
                    return;
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
        
        /// <summary>
        /// Chase toward target (used for zone 3: too far)
        /// </summary>
        private void ChaseTowardTarget()
        {
            Vector3 enemyPos = _enemy.GlobalPosition;
            Vector3 targetPos = _target.GlobalPosition;
            float xDifference = targetPos.X - enemyPos.X;
            
            if (Mathf.Abs(xDifference) < 0.5f)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0f);
                return;
            }
            
            float directionToTarget = Mathf.Sign(xDifference);
            float velocityToSet = directionToTarget * _chaseSpeed;
            
            _enemy.MovementComponent.SetHorizontalVelocity(velocityToSet);
            _enemy.MovementComponent.FaceRight = (directionToTarget > 0);
            
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Running);
            }
        }
        
        /// <summary>
        /// Back away from target (used for zone 1: too close)
        /// </summary>
        private void BackAwayFromTarget()
        {
            Vector3 enemyPos = _enemy.GlobalPosition;
            Vector3 targetPos = _target.GlobalPosition;
            float xDifference = targetPos.X - enemyPos.X;
            
            // Move in opposite direction
            float directionAwayFromTarget = -Mathf.Sign(xDifference);
            float velocityToSet = directionAwayFromTarget * _chaseSpeed;
            
            _enemy.MovementComponent.SetHorizontalVelocity(velocityToSet);
            _enemy.MovementComponent.FaceRight = (directionAwayFromTarget > 0);
            
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Running);
            }
        }
    }
}