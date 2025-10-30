using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Player.PlayerStates;
using DeckroidVania2.Game.Scripts.Inputs;
using Godot;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace DeckroidVania2.Game.Player;

public partial class MovementController : Node
{
	#region Export Params
	[Export, ExportGroup("Player Attributes")]
	public float Speed { get; set; }

	[Export, ExportGroup("Player Attributes")]
	public float JumpVelocity { get; set; }

	[Export, ExportGroup("Player Attributes")]
	public float RotationSpeed { get; set; }

	[Export, ExportGroup("Player Attributes")]
	public short _maxJumps = 2;
	public short _curNumOfJumps;

	[Export, ExportGroup("Player Environment")]
	public float _gravityStrength;


	//This can be moved back into Dashingstate.cs, left it here for the exports to Godot
	[Export, ExportGroup("Dash Settings")]
	public float DashSpeed { get; set; } = 30f;
	[Export, ExportGroup("Dash Settings")]
	public float DashLength { get; set; } = 10f;
	[Export, ExportGroup("Dash Settings")]

	public bool AllowAirDash { get; set; } = false;

	[Export, ExportGroup("DashJump Settings")]
	public float DashJumpSpeed { get; set; }

	[Export, ExportGroup("DashJump Settings")]
	public float DashJumpGravityMultiplier { get; set; }

	[Export, ExportGroup("DashJump Settings")]
	public float DashJumpTime { get; set; } = 0.4f;  // Adjust in inspector, e.g. 0.4s
	#endregion

	[Export, ExportGroup("Wall Jump Settings")]
	public float WalljumpHorizontalPush { get; set; } = 10f;
	[Export, ExportGroup("Wall Jump Settings")]
	public float WalljumpVerticalPush { get; set; } = 20f;

	public float WallJumpLockDuration { get; set; } = 0.25f; // seconds
	private float _jumpGraceTimer = 0f; // Timer to ignore IsOnFloor() after jumping
	private const float JumpGracePeriod = 0.1f; // Adjust this value as needed (e.g., 0.1 seconds)
	public float _wallStickTimer { get; set; }
	public bool _isWallJump = false;
	public float _wallJumpLockTime = 0f;


	public Player _characterBody;
	public Vector3 _velocity;
	private Node3D _rootNode;

	public bool _faceRight;
	public bool _isDashJumping = false;
	public float _dashJumpTimer;


	public bool IsOnFloor()
	{
		// Ignore IsOnFloor() if within the grace period
		if (_jumpGraceTimer > 0)
		{
			GD.Print($"[MovementController] Ignoring IsOnFloor() due to grace period. Timer: {_jumpGraceTimer}");
			return false;
		}
		return _characterBody.IsOnFloor();
	}

	public bool CanDoubleJump()
	{
		return _curNumOfJumps < _maxJumps;
	}
	//public PlayerState CurrentState => _currentState;
	private const string WALLS_GROUP = "WallStick";
	private Dictionary<PlayerState, IPlayerState> _states;
	private PlayerState _currentStateEnum;
	private IPlayerState _currentStateInstance;
	public PlayerState CurrentState => _currentStateEnum;

	public void Initialize(Player characterBody3D)
	{
		_characterBody = characterBody3D;
		_rootNode = GetNode<Node3D>("../Visual/RootNode");

		// Create + cache each state
		_states = new Dictionary<PlayerState, IPlayerState>
		{
			{ PlayerState.Normal, new NormalState(this) },
			{ PlayerState.Falling, new FallingState(this) },
			{ PlayerState.Dashing, new DashingState(this) },
			{ PlayerState.Jumping, new JumpingState(this) },
			{ PlayerState.Tumble, new TumbleState(this) },
			{ PlayerState.WallStick, new WallStickState(this) },
			{ PlayerState.WallJump, new WallJumpState(this) },
			{ PlayerState.Locked, new LockedState(this) }
		};

		// Start in Normal
		_currentStateEnum = PlayerState.Normal;
		_currentStateInstance = _states[_currentStateEnum];
		_currentStateInstance.Enter();

		_faceRight = true;
	}
	public void ChangeState(PlayerState newState)
	{
		if (_currentStateEnum == newState)
			return;

		// Exit old
		_currentStateInstance.Exit();

		// Switch to new
		_currentStateEnum = newState;
		_currentStateInstance = _states[newState];

		// Enter new
		_currentStateInstance.Enter();
	}

	public void HandleMovement(double delta)
	{
		// Update the grace timer
		if (_jumpGraceTimer > 0)
		{
			_jumpGraceTimer -= (float)delta;
		}
	

		// We can still do any universal steps first:
		_velocity = _characterBody.Velocity;

		// Let the current state handle input & transitions
		_currentStateInstance.HandleInput(delta);

		// Apply universal gravity logic
		ApplyUniversalGravity(delta);

		// Then let the current state update logic
		_currentStateInstance.UpdateState(delta);

		// Then apply final velocity
		_characterBody.Velocity = _velocity;
		_characterBody.MoveAndSlide();
	}

	public void StartJumpGracePeriod()
	{
		_jumpGraceTimer = JumpGracePeriod;
	}

	public void ApplyFacingRotation(double delta)
	{
		// If we are dashing, maybe we skip or do partial logic
		if (_currentStateEnum == PlayerState.Dashing)
			return;

		// Else run your existing smooth rotation
		if (_faceRight && _rootNode.Rotation.Y < 0)
		{
			_rootNode.Rotation = new Vector3(
				0,
				Mathf.Lerp(_rootNode.Rotation.Y, 0, 1 / RotationSpeed * (float)delta),
				0
			);
		}
		else if (!_faceRight && _rootNode.Rotation.Y > -MathF.PI)
		{
			_rootNode.Rotation = new Vector3(
				0,
				Mathf.Lerp(_rootNode.Rotation.Y, -MathF.PI, 1 / RotationSpeed * (float)delta),
				0
			);
		}
	}

	public void ApplyHorizontalMovement(double delta)
	{
		float horizontalInput = GetHorizontalInput();

		if (Mathf.Abs(horizontalInput) > 0)
			_faceRight = (horizontalInput > 0);

		_velocity.X = horizontalInput * Speed;

		// Optionally apply friction if horizontalInput == 0
		if (Mathf.Abs(horizontalInput) < 0.01f)
			_velocity.X = Mathf.MoveToward(_velocity.X, 0, Speed * 0.1f);
	}

	public float GetHorizontalInput()
	{
		return Input.GetActionStrength(ControlsSchema.UI_RIGHT) - Input.GetActionStrength(ControlsSchema.UI_LEFT);
	}

	public void SnapRotationToCurrentFacing()
	{
		_rootNode.Rotation = new Vector3(0, _faceRight ? 0 : -Mathf.Pi, 0);
	}

	private void ApplyUniversalGravity(double delta)
	{
		if (_isDashJumping)
		{
			return;
		}
		_velocity.Y += _characterBody.GetGravity().Y * (float)delta * _gravityStrength;
	}

	public bool CheckIfCanWallStick()
	{
		// Short Horizontal Raycast
		Vector3 start = _characterBody.GlobalPosition;
		//var end = start + new Vector3(0, 1, 0);
		float horizontalDir = _faceRight ? 1 : -1;
		float rayDistance = 1.5f;
		Vector3 end = start + new Vector3(horizontalDir, 0, 0) * rayDistance;

		var query = PhysicsRayQueryParameters3D.Create(start, end, _characterBody.CollisionMask, new Godot.Collections.Array<Rid> { _characterBody.GetRid() });

		var result = _characterBody.GetWorld3D().DirectSpaceState.IntersectRay(query);

		if(result.TryGetValue("collider", out Variant value))
		{
			var hitObject = value.As<Node3D>();
			//GD.Print($"[Debug] Raycast hit: {hitObject.Name}");
			return hitObject.IsInGroup(WALLS_GROUP);
		}
		//GD.Print("[Debug] Raycast found nothing");
		return false;
	}

	public bool IsPressingIntoWall()
	{
		// Check input strengths.
		float inputRight = Input.GetActionStrength(ControlsSchema.UI_RIGHT);
		float inputLeft  = Input.GetActionStrength(ControlsSchema.UI_LEFT);
		
		// If facing right, require significant right input.
		if (_faceRight && inputRight > 0.1f)
			return true;
		// If facing left, require significant left input.
		if (!_faceRight && inputLeft > 0.1f)
			return true;
			
		return false;
	}

	public bool ShouldEnterWallStick()
	{
		// Only consider entering wall stick if we're not on the floor,
		// the raycast detects a wall, and the player is actively pressing toward it.
		return !IsOnFloor() && CheckIfCanWallStick() && IsPressingIntoWall();
	}

	public void ClampWallSlideVelocity(float maxSlideSpeed = -2f)
	{
		// Only clamp if the player is pressing into the wall and falling.
		if (IsPressingIntoWall() && _velocity.Y < 0)
		{
			if (_velocity.Y < maxSlideSpeed)
				_velocity.Y = maxSlideSpeed;
		}
	}

	public float GetWallPushDirection()
	{
		// returns -1 is a wall is detected on the right
		// returns 1 is a wall is detected on the left
		// returns 0 if non is clearly detected
		int collisionCount = _characterBody.GetSlideCollisionCount();
		for(int i = 0; i < collisionCount; i++)
		{
			var collision = _characterBody.GetSlideCollision(i);
			Vector3 normal = collision.GetNormal();
			//GD.Print($"[Debug] Collision Normal: {normal}");
            if (Mathf.Abs(normal.Y) < 0.3f)
            {
                if (normal.X < -0.6f)
                    return -1f; // Wall is on the right, so push left.
                if (normal.X > 0.6f)
                    return 1f;  // Wall is on the left, so push right.
            }
		}
		return 0f;
	}

}
