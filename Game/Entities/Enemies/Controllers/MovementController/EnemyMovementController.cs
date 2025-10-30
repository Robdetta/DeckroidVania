using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Interfaces;
using DeckroidVania.Game.Entities.Enemies.States;
using DeckroidVania.Game.Entities.Enemies.Data;
using System.Collections.Generic;
using DeckroidVania.Game.Entities.Enemies.Types;
using System;

namespace DeckroidVania.Game.Entities.Enemies.Controllers
{
    public partial class EnemyMovementController : Node
    {
        [Export] public float Speed { get; set; } = 10f;
        [Export] public float PatrolRange { get; set; } = 10f;
        private RayCast3D _wallDetector;
        private RayCast3D _edgeDetector;
        private Area3D _visionArea;
        public EnemyAnimationTree _animationTree;
        
        public Enemy _enemy;
        public Vector3 _velocity;
        private Node3D _visualNode;
        private Node3D _currentTarget;
        
        public bool _faceRight = true;
        public Vector3 _startPosition;
        
        public Dictionary<EnemyState, IEnemyState> _states;
        private EnemyState _currentStateEnum;
        private IEnemyState _currentStateInstance;

        // Combat properties
        private float _chaseSpeed;
        private float _attackRange;
        private float _detectionRange;
        private float _loseTargetRange;
        private float _attackCooldown;
        private EnemyAttackData _defaultAttack;
        private EnemyState _defaultIdleState;
        public EnemyState CurrentState => _currentStateEnum;

        private DetectionBehavior _detectionBehavior;

        public enum DetectionBehavior
        {
            ChaseOnDetect,    // Knight behavior
            AttackOnDetect    // Mage behavior
        }

        public void Initialize(Enemy enemy, float speed, float patrolRange, float chaseSpeed, float attackRange, float detectionRange, float loseTargetRange, EnemyState defaultState, string detectionBehaviorString, float attackCooldown, EnemyAttackData defaultAttack, string defaultIdleStateString = "Idle")
        {
            _enemy = enemy;
            Speed = speed;
            PatrolRange = patrolRange;
            _chaseSpeed = chaseSpeed;
            _attackRange = attackRange;
            _detectionRange = detectionRange;
            _loseTargetRange = loseTargetRange;
            _attackCooldown = attackCooldown;
            _defaultAttack = defaultAttack;

            // NEW: Parse the default idle state from string
            _defaultIdleState = defaultIdleStateString.ToLower() switch
            {
                "patrol" => EnemyState.Patrol,
                "idle" => EnemyState.Idle,
                _ => EnemyState.Idle
            };

            _detectionBehavior = detectionBehaviorString.ToLower() switch
            {
                "chase" => DetectionBehavior.ChaseOnDetect,
                "attack" => DetectionBehavior.AttackOnDetect,
                _ => DetectionBehavior.ChaseOnDetect // Default fallback
            };

            _animationTree = _enemy.GetNodeOrNull<EnemyAnimationTree>("Visual/AnimationTree");

            // Find animation tree in child nodes
            //_animationTree = _enemy.GetNode<EnemyAnimationTree>("AnimationTree");
            if (_animationTree == null)
            {
                // Fallback to direct child
                _animationTree = _enemy.GetNodeOrNull<EnemyAnimationTree>("AnimationTree");
            }

            if (_animationTree == null)
            {
                GD.PushWarning($"[EnemyMovementController] AnimationTree not found on {_enemy.Name}!");
            }

            // Try to get visual node, but don't fail if it doesn't exist
            _visualNode = enemy.GetNodeOrNull<Node3D>("Visual");
            if (_visualNode == null)
            {
                GD.PushWarning("Visual node not found. Enemy will not rotate to face direction.");
            }

            // Get raycasts
            _wallDetector = enemy.GetNodeOrNull<RayCast3D>("Visual/WallDetector");
            _edgeDetector = enemy.GetNodeOrNull<RayCast3D>("Visual/EdgeDetector");

            if (_wallDetector == null)
                GD.PushWarning("WallDetector not found. Enemy won't detect walls.");
            if (_edgeDetector == null)
                GD.PushWarning("EdgeDetector not found. Enemy won't detect ledges.");

            // Setup vision area
            _visionArea = enemy.GetNodeOrNull<Area3D>("VisionArea");
            if (_visionArea != null)
            {
                _visionArea.BodyEntered += OnBodyEnteredVision; // Remove this line
                _visionArea.BodyExited += OnBodyExitedVision;

                // Just verify the collision shape exists and log its radius
                var collisionShape = _visionArea.GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
                if (collisionShape == null)
                {
                    GD.PushWarning("[Vision] CollisionShape3D not found under VisionArea!");
                }
                else if (collisionShape.Shape is not SphereShape3D)
                {
                    GD.PushWarning($"[Vision] CollisionShape3D should be a SphereShape3D!");
                }
            }
            else
            {
                GD.PushWarning("VisionArea not found. Enemy won't detect player.");
            }

            _startPosition = enemy.GlobalPosition;

            // Create and cache each state - all use Enemy reference for components
            _states = new Dictionary<EnemyState, IEnemyState>
            {
                { EnemyState.Idle, new IdleState(_enemy) },
                { EnemyState.Patrol, new PatrolState(_enemy, Speed, PatrolRange) },
                { EnemyState.Chase, new ChaseState(_enemy, _chaseSpeed) },
                { EnemyState.Attack, new AttackState(_enemy) },
                { EnemyState.Knockback, new KnockBackState(_enemy) },
                { EnemyState.Falling, new FallingState(_enemy) }
            };

            // Start in the specified default state
            _currentStateEnum = defaultState;
            _currentStateInstance = _states[_currentStateEnum];
            _currentStateInstance.Enter();
        }
        
        public EnemyAttackData GetDefaultAttack()
        {
            return _defaultAttack;
        }

                
        public EnemyState GetDefaultIdleState()  // NEW: Getter for default idle state
        {
            return _defaultIdleState;
        }

        public void SetAnimationCondition(string conditionName, bool value)
        {
            // Now we can use the EnemyAnimationTree enum instead of raw strings
            // This method is kept for backwards compatibility, but EnemyAnimationTree.ChangeState() is preferred
            if (_animationTree != null)
            {
                _animationTree.Set($"parameters/conditions/{conditionName}", value);
            }
        }                   


        private void OnBodyEnteredVision(Node3D body)
        {
            if (body.IsInGroup("Player"))
            {
                _currentTarget = body;
                
                // NEW: Update AIComponent with target
                if (_enemy?.AIComponent != null)
                {
                    _enemy.AIComponent.SetTarget(body);
                }

                switch (_detectionBehavior)
                {
                    case DetectionBehavior.ChaseOnDetect:
                        if (_currentStateEnum == EnemyState.Patrol || _currentStateEnum == EnemyState.Idle)
                        {
                            ChangeState(EnemyState.Chase);
                        }
                        break;

                    case DetectionBehavior.AttackOnDetect:
                        if (_currentStateEnum == EnemyState.Idle)
                        {
                            ChangeState(EnemyState.Attack);
                        }
                        break;
                }
            }
        }
        
        private void OnBodyExitedVision(Node3D body)
        {
            if (body == _currentTarget)
            {
                float distance = _enemy.GlobalPosition.DistanceTo(body.GlobalPosition);
                if (distance > _loseTargetRange)
                {
                    _currentTarget = null;
                    
                    // NEW: Clear AIComponent target
                    if (_enemy?.AIComponent != null)
                    {
                        _enemy.AIComponent.ClearTarget();
                    }
                    
                    if (_currentStateEnum == EnemyState.Chase)
                    {
                        ChangeState(EnemyState.Patrol);
                    }
                }
            }
        }
        
        public void ChangeState(EnemyState newState)
        {
            if (_currentStateEnum == newState)
                return;
                
            _currentStateInstance.Exit();
            
            _currentStateEnum = newState;
            _currentStateInstance = _states[newState];
            
            _currentStateInstance.Enter();
        }

        public void HandleMovement(double delta)
        {
            _velocity = _enemy.Velocity;

            _currentStateInstance.HandleInput(delta);
            _currentStateInstance.UpdateState(delta);

            // Update enemy velocity from controller velocity
            _enemy.Velocity = _velocity;

            // Apply facing rotation before MoveAndSlide
            ApplyFacingRotation();

            // NOTE: MoveAndSlide now handled by MovementComponent in Enemy._PhysicsProcess

            if (_animationTree != null)
            {
                // ...existing animation logic...
            }
        }

        

        private void ApplyFacingRotation()
        {
            // Set rotation directly, don't compound it
            if (_visualNode != null)
            {
                _visualNode.Rotation = new Vector3(0, _faceRight ? 0 : Mathf.Pi, 0);
            }
        }

        public void SetHorizontalVelocity(float speed)
        {
            _velocity.X = speed;
        }
        
         public bool IsWallAhead()
        {
            return _wallDetector?.IsColliding() ?? false;
        }

        public bool IsEdgeAhead()
        {
            // If the edge detector is NOT colliding, there's no ground ahead
            return !(_edgeDetector?.IsColliding() ?? true);
        }

        public Node3D GetTarget()
        {
            return _currentTarget;
        }
        
        public float GetAttackRange()
        {
            return _attackRange;
        }
    }
}