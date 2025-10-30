using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;
using Godot;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class JumpingState : IPlayerState
{
    [Export, ExportGroup("Player Attributes")]
    private readonly float _doubleJumpAcceleration;
    private MovementController _controller;

    public JumpingState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _controller._curNumOfJumps++;

        // Start the grace period to avoid premature IsOnFloor() detection
        _controller.StartJumpGracePeriod();

        // Reset vertical velocity to avoid stacking impulses
        _controller._velocity.Y = 0;
        GD.Print($"[JumpingState] Enter: Reset vertical velocity. Current Velocity: {_controller._velocity}");

        if (!_controller._isWallJump)
        {
            // Normal jump: apply vertical impulse.
            _controller._velocity.Y = _controller.JumpVelocity;
            //GD.Print($"[JumpingState] Normal Jump: Applied JumpVelocity: {_controller.JumpVelocity}");
        }
        else
        {
            // Wall jump: Do not override the horizontal push already applied.
            _controller._velocity.Y = _controller.JumpVelocity;
            _controller._isWallJump = false; // Clear the flag for future jumps.
            //GD.Print($"[JumpingState] Wall Jump: Applied JumpVelocity: {_controller.JumpVelocity}");
        }

        // Check if coming from a dash
        if (_controller._isDashJumping)
        {
            float inputX = _controller.GetHorizontalInput();

            // If player presses a direction, override previous direction
            if (Mathf.Abs(inputX) > 0)
            {
                _controller._faceRight = (inputX > 0);
                _controller.SnapRotationToCurrentFacing();
            }

            // Apply dash jump speed
            _controller._velocity.X = _controller._faceRight ? _controller.DashJumpSpeed : -_controller.DashJumpSpeed;
            _controller._dashJumpTimer = _controller.DashJumpTime;

            //GD.Print($"[JumpingState] Dash Jump: Applied DashJumpSpeed: {_controller.DashJumpSpeed}, Direction: {(_controller._faceRight ? "Right" : "Left")}");
        }
    }

    public void Exit()
    {
        _controller._isDashJumping = false;
    }

    public void HandleInput(double delta)
    {
        // Land check
        if (_controller.IsOnFloor())
        {
            _controller._curNumOfJumps = 0;
            _controller._isDashJumping = false;
            _controller.ChangeState(PlayerState.Normal);
            GD.Print("[JumpingState] Landed: Reset jumps and changed to Normal state.");
            return;
        }

        // Check for wall stick conditions
        if (!_controller.IsOnFloor() 
            && _controller.CheckIfCanWallStick() 
            && _controller.IsPressingIntoWall()
            && _controller._wallJumpLockTime <= 0f) // <-- Add this check
        {
            GD.Print("[JumpingState] Wall stick conditions met. Transitioning to WallStickState.");
            _controller.ChangeState(PlayerState.WallStick);
            return;
        }
        
        // Double jump
        if (Input.IsActionJustPressed(ControlsSchema.UI_JUMP) && _controller._curNumOfJumps < _controller._maxJumps)
        {
            _controller._curNumOfJumps++;
            _controller._velocity.Y =
                (_controller._curNumOfJumps == _controller._maxJumps && _doubleJumpAcceleration > 0)
                ? _doubleJumpAcceleration
                : _controller.JumpVelocity;

            GD.Print($"[JumpingState] Double Jump: Current Jumps: {_controller._curNumOfJumps}, Velocity.Y: {_controller._velocity.Y}");
        }

        // Air dash
        if (Input.IsActionJustPressed(ControlsSchema.UI_DASH) && _controller.AllowAirDash)
        {
            _controller.ChangeState(PlayerState.Dashing);
            GD.Print("[JumpingState] Air Dash: Transitioning to Dashing state.");
            return;
        }
    }
    public void UpdateState(double delta)
    {
        if (_controller._isDashJumping)
        {
            _controller._dashJumpTimer -= (float)delta;
            if (_controller._dashJumpTimer > 0)
            {
                // "Floaty" gravity
                _controller._velocity.Y += _controller._characterBody.GetGravity().Y
                    * (float)delta
                    * _controller._gravityStrength
                    * _controller.DashJumpGravityMultiplier;

                GD.Print($"[JumpingState] Dash Jump Timer Active: Timer: {_controller._dashJumpTimer}, Velocity.Y: {_controller._velocity.Y}");
                return;
            }
            else
            {
                // Dash jump time ended
                _controller._isDashJumping = false;
                GD.Print("[JumpingState] Dash Jump Timer Ended.");
            }
        }

        // Decrease wall jump lock timer
        if (_controller._wallJumpLockTime > 0)
        {
            _controller._wallJumpLockTime -= (float)delta;
            GD.Print($"[JumpingState] WallJumpLockTime Active: {_controller._wallJumpLockTime}");
        }
        else
        {
            // Normal horizontal air control resumes ONLY after lock expires
            _controller.ApplyHorizontalMovement(delta);
        }

        // Always apply gravity
        _controller._velocity.Y += _controller._characterBody.GetGravity().Y * (float)delta * _controller._gravityStrength;
        //GD.Print($"[JumpingState] Gravity Applied: Velocity.Y: {_controller._velocity.Y}");

        _controller.ApplyFacingRotation(delta);
    }
}
