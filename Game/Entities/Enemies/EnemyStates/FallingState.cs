using Godot;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    public partial class FallingState : IEnemyState
    {
        private Enemy _enemy; // Direct reference to Enemy

        public FallingState(Enemy enemy)
        {
            _enemy = enemy;
        }

        public void Enter()
        {
            // NEW: Use AnimationComponent
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Falling);
            }
        }

        public void Exit()
        {
        }

        public void HandleInput(double delta)
        {
            // No input during falling
        }

        public void UpdateState(double delta)
        {
            if (_enemy == null)
                return;

            // Apply gravity (still use Enemy directly for physics)
            _enemy.Velocity = new Vector3(
                _enemy.Velocity.X, 
                _enemy.Velocity.Y - _enemy.Gravity * (float)delta, 
                _enemy.Velocity.Z
            );
            _enemy.MoveAndSlide();

            // If we land on floor => return to default state
            if (_enemy.IsOnFloor())
            {
                // Use legacy controller for actual state change
                if (_enemy?._movementController != null)
                {
                    _enemy._movementController.ChangeState(EnemyState.Patrol);
                }
            }
        }
    }
}