using Godot;
using System;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Base;


namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles all AI state machine logic for enemies
    /// Responsibilities: state transitions, target tracking, decision-making
    /// </summary>
    public partial class AIComponent : Node, IAIComponent
    {
        private Node3D _target;
        private EnemyState _currentState = EnemyState.Idle;
        private string _detectionBehavior;

        public EnemyState CurrentState => _currentState;
        public Node3D CurrentTarget => _target; // Changed from Target to CurrentTarget to match interface
        
        public event Action<EnemyState, EnemyState> OnStateChanged;

        public void Initialize(EnemyState initialState, string detectionBehavior)
        {
            _currentState = initialState;
            _detectionBehavior = detectionBehavior;
            
            GD.Print($"[AIComponent] Initialized - State: {initialState}, Detection: {detectionBehavior}");
        }

        public void ChangeState(EnemyState newState)
        {
            if (_currentState == newState)
                return;

            var oldState = _currentState;
            _currentState = newState;
            
            GD.Print($"[AIComponent] State: {oldState} → {newState}");
            OnStateChanged?.Invoke(oldState, newState);
        }

        public void SetTarget(Node3D target)
        {
            _target = target;
            
            if (target != null)
                GD.Print($"[AIComponent] Target set: {target.Name}");
            else
                GD.Print("[AIComponent] Target cleared");
        }

        public void ClearTarget()
        {
            _target = null;
            GD.Print("[AIComponent] Target cleared");
        }

        public void Update(double delta)
        {
            // This is where state-specific update logic would go
            // For now, state updates are handled by separate state classes
            // In a full ECS, we'd move state logic here too
        }

        public bool HasTarget()
        {
            return _target != null && GodotObject.IsInstanceValid(_target);
        }

        public float GetDistanceToTarget()
        {
            if (!HasTarget())
                return float.MaxValue;
            
            var owner = GetParent<Node3D>();
            return owner.GlobalPosition.DistanceTo(_target.GlobalPosition);
        }
    }
}
