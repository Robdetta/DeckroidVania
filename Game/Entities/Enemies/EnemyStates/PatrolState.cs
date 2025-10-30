using Godot;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.States
{
    public class PatrolState : IEnemyState
    {
        private Enemy _enemy; // Direct reference to Enemy
        private int _direction = 1; // 1 = right, -1 = left
        private Vector3 _startPosition;
        private float _patrolSpeed;
        private float _patrolRange;

        public PatrolState(Enemy enemy, float speed, float range)
        {
            _enemy = enemy;
            _patrolSpeed = speed;
            _patrolRange = range;
        }

        public void Enter()
        {
            // Debug component availability
            if (_enemy?.MovementComponent == null)
            {
                GD.PushError("[PatrolState] MovementComponent is NULL!");
            }
            
            // NEW: Use MovementComponent to get start position
            if (_enemy?.MovementComponent != null)
            {
                _startPosition = _enemy.GlobalPosition;
            }
            
            // NEW: Use AnimationComponent
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.ChangeState((int)EnemyAnimationTree.EnemyAnimationState.Walking);
            }
            else
            {
                GD.PushWarning("[PatrolState] AnimationComponent is NULL!");
            }
        }
        
        public void Exit()
        {
        }        public void HandleInput(double delta)
        {
            // Enemies don't have input, but we keep this for consistency
        }

        public void UpdateState(double delta)
        {
            if (_enemy?.MovementComponent == null)
            {
                GD.PushError("[PatrolState] UpdateState - MovementComponent is NULL!");
                return;
            }

            // NEW: Use MovementComponent for obstacle detection
            bool wallAhead = _enemy.MovementComponent.IsWallAhead();
            bool edgeAhead = _enemy.MovementComponent.IsEdgeAhead();
            
            if (wallAhead || edgeAhead)
            {
                _direction *= -1;
                _startPosition = _enemy.GlobalPosition;
            }

            // NEW: Use MovementComponent to set velocity and facing
            _enemy.MovementComponent.SetHorizontalVelocity(_direction * _patrolSpeed);
            _enemy.MovementComponent.FaceRight = (_direction == 1);

            // NEW: Update animation blend
            if (_enemy?.AnimationComponent != null)
            {
                _enemy.AnimationComponent.SetMovementBlend(_patrolSpeed);
            }

            // Check if we've reached patrol range limit
            float distanceFromStart = _enemy.GlobalPosition.X - _startPosition.X;
            
            if (distanceFromStart >= _patrolRange && _direction == 1)
            {
                _direction = -1;
                _startPosition = _enemy.GlobalPosition;
            }
            else if (distanceFromStart <= -_patrolRange && _direction == -1)
            {
                _direction = 1;
                _startPosition = _enemy.GlobalPosition;
            }
        }
    }
}
