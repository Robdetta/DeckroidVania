using Godot;
using System;
using System.Collections.Generic;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.States;


namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles all AI state machine logic for enemies
    /// Responsibilities: state transitions, target tracking, decision-making, state machine management
    /// </summary>
    public partial class AIComponent : Node, IAIComponent
    {
        private Node3D _target;
        private EnemyState _currentState = EnemyState.Idle;
        private IEnemyState _currentStateInstance;
        private string _detectionBehavior;
        private Dictionary<EnemyState, IEnemyState> _states;
        private Enemy _enemy;

        public EnemyState CurrentState => _currentState;
        public Node3D CurrentTarget => _target; // Changed from Target to CurrentTarget to match interface
        
        public event Action<EnemyState, EnemyState> OnStateChanged;

        public void Initialize(EnemyState initialState, string detectionBehavior)
        {
            _currentState = initialState;
            _detectionBehavior = detectionBehavior;
            
            // Get Enemy reference
            _enemy = GetParent<Enemy>();
            if (_enemy == null)
            {
                GD.PushError("[AIComponent] Must be child of Enemy!");
                return;
            }
            
            GD.Print($"[AIComponent] Initialized - State: {initialState}, Detection: {detectionBehavior}");
        }

        /// <summary>
        /// Creates the state machine dictionary with all enemy states.
        /// Should be called after all components are initialized.
        /// </summary>
        public void CreateStates(float speed, float patrolRange, float chaseSpeed)
        {
            if (_enemy == null)
            {
                GD.PushError("[AIComponent] Cannot create states - Enemy reference is null!");
                return;
            }

            _states = new Dictionary<EnemyState, IEnemyState>
            {
                { EnemyState.Idle, new IdleState(_enemy) },
                { EnemyState.Patrol, new PatrolState(_enemy, speed, patrolRange) },
                { EnemyState.Chase, new ChaseState(_enemy, chaseSpeed) },
                { EnemyState.Attack, new AttackState(_enemy) },
                { EnemyState.Block, new BlockState(_enemy) },
                { EnemyState.Knockback, new KnockBackState(_enemy) },
                { EnemyState.Falling, new FallingState(_enemy) }
            };

            // Enter initial state
            _currentStateInstance = _states[_currentState];
            _currentStateInstance.Enter();

            GD.Print($"[AIComponent] Created {_states.Count} states, starting in {_currentState}");
        }

        public void ChangeState(EnemyState newState)
        {
            if (_currentState == newState)
                return;

            // Exit current state
            _currentStateInstance?.Exit();

            var oldState = _currentState;
            _currentState = newState;
            
            // Enter new state
            _currentStateInstance = _states[newState];
            _currentStateInstance.Enter();
            
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
            // Execute current state's logic
            if (_currentStateInstance != null)
            {
                _currentStateInstance.HandleInput(delta);
                _currentStateInstance.UpdateState(delta);
            }
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

        /// <summary>
        /// Gets a specific state instance from the state dictionary.
        /// Useful for applying knockback or other state-specific operations.
        /// </summary>
        public IEnemyState GetState(EnemyState state)
        {
            if (_states != null && _states.TryGetValue(state, out var stateInstance))
            {
                return stateInstance;
            }
            return null;
        }
    }
}
