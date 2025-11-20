using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Interfaces;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    /// <summary>
    /// Block/Parry state - enemy raises shield and blocks incoming damage
    /// Used by knights and other shield-bearing enemies
    /// </summary>
    public class BlockState : IEnemyState
    {
        private Enemy _enemy;
        private float _blockTimer;
        private float _blockDuration = 1.5f; // How long to hold block
        
        // Block state properties - THIS IS THE KEY!
        public bool CanBeKnockedBack => false; // 🛡️ IMMUNE to knockback while blocking!
        public bool CanTakeDamage => false;    // 🛡️ BLOCKS all damage!
        
        public BlockState(Enemy enemy)
        {
            _enemy = enemy;
        }
        
        public void Enter()
        {
            GD.Print($"[BlockState] 🛡️ {_enemy.Name} raises shield!");
            
            _blockTimer = _blockDuration;
            
            // Stop movement
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0f);
            }
            
            // Play shield block animation
            if (_enemy?.AnimationComponent != null)
            {
                // Play the shield animation directly instead of changing to Idle
                _enemy.AnimationComponent.PlayAttackAnimation("Shield");
            }
        }
        
        public void Exit()
        {
            GD.Print($"[BlockState] {_enemy.Name} lowers shield");
        }
        
        public void HandleInput(double delta)
        {
            // Enemies don't have input
        }
        
        public void UpdateState(double delta)
        {
            _blockTimer -= (float)delta;
            
            // Stop movement while blocking
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0f);
            }
            
            // Block duration expired - return to appropriate state
            if (_blockTimer <= 0f && _enemy?.StateManagerComponent != null)
            {
                if (_enemy.StateManagerComponent.HasTarget())
                {
                    // Return to attack if still have target
                    _enemy.StateManagerComponent.ChangeState(EnemyState.Attack);
                }
                else
                {
                    // Return to patrol if no target
                    _enemy.StateManagerComponent.ChangeState(EnemyState.Patrol);
                }
            }
        }
    }
}
