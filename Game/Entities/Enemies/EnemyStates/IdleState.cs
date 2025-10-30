using Godot;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    public class IdleState : IEnemyState
    {
        private Enemy _enemy; // Direct reference to Enemy
        
        public IdleState(Enemy enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            // NEW: Use MovementComponent to stop
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0);
            }

            // NEW: Use AnimationComponent
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Idle);
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
            // NEW: Use MovementComponent to stay stopped
            if (_enemy?.MovementComponent != null)
            {
                _enemy.MovementComponent.SetHorizontalVelocity(0);
            }
        }
    }
}