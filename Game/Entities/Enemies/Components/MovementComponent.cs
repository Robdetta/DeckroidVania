using Godot;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles all movement-related functionality for enemies
    /// Responsibilities: velocity, pathfinding, facing direction, collision detection
    /// </summary>
    public partial class MovementComponent : Node, IMovementComponent
    {
        private CharacterBody3D _owner;
        private Node3D _visualNode;
        private RayCast3D _wallDetector;
        private RayCast3D _edgeDetector;
        
        public Vector3 Velocity { get; set; }
        public bool FaceRight { get; set; } = true;
        public Vector3 StartPosition { get; private set; }
        
        private float _speed;
        private float _patrolRange;

        public void Initialize(CharacterBody3D owner, float speed, float patrolRange)
        {
            _owner = owner;
            _speed = speed;
            _patrolRange = patrolRange;
            StartPosition = owner.GlobalPosition;
            
            // Get visual node for rotation
            _visualNode = owner.GetNodeOrNull<Node3D>("Visual");
            if (_visualNode == null)
            {
                GD.PushWarning($"[MovementComponent] Visual node not found on {owner.Name}");
            }
            
            // Get raycasts for collision detection
            _wallDetector = owner.GetNodeOrNull<RayCast3D>("Visual/WallDetector");
            _edgeDetector = owner.GetNodeOrNull<RayCast3D>("Visual/EdgeDetector");
            
            if (_wallDetector == null)
                GD.PushWarning($"[MovementComponent] WallDetector not found");
            if (_edgeDetector == null)
                GD.PushWarning($"[MovementComponent] EdgeDetector not found");
                
            GD.Print($"[MovementComponent] Initialized - Speed: {speed}, PatrolRange: {patrolRange}");
        }

        public void SetHorizontalVelocity(float speed)
        {
            Velocity = new Vector3(speed, Velocity.Y, Velocity.Z);
        }

        public void ApplyMovement(double delta)
        {
            // Update owner's velocity
            _owner.Velocity = Velocity;
            
            // Apply facing rotation
            ApplyFacingRotation();
            
            // Execute movement
            _owner.MoveAndSlide();
            
            // Lock Z position to 0 (2.5D constraint)
            if (_owner.GlobalPosition.Z != 0f)
            {
                _owner.GlobalPosition = new Vector3(_owner.GlobalPosition.X, _owner.GlobalPosition.Y, 0f);
            }
        }

        private void ApplyFacingRotation()
        {
            if (_visualNode != null)
            {
                _visualNode.Rotation = new Vector3(0, FaceRight ? 0 : Mathf.Pi, 0);
            }
        }

        public bool IsWallAhead()
        {
            return _wallDetector?.IsColliding() ?? false;
        }

        public bool IsEdgeAhead()
        {
            // If edge detector is NOT colliding, there's no ground ahead
            return !(_edgeDetector?.IsColliding() ?? true);
        }
    }
}
