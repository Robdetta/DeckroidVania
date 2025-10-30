using Godot;
using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class WallStickState : IPlayerState
{
    private MovementController _controller;

    private const float MIN_WALL_STICK_TIME = 0.1f;

    public WallStickState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        GD.Print("Wall State Entered");
        _controller._wallStickTimer = 0f;
    }

    public void Exit()
    {
        GD.Print("Wall State Exited");
    }

    public void HandleInput(double delta)
    {
        if (_controller.IsOnFloor())
        {
            _controller.ChangeState(PlayerState.Normal);
            return;
        }

        // If the player releases the directional input, exit to FallingState.
        if (!_controller.IsPressingIntoWall())
        {
            //DEBUG
            //GD.Print("[WallStickState] No wall input. Switching to FallingState.");
            _controller.ChangeState(PlayerState.Falling);
            return;
        }

        // Now, require that the player is pressing into the wall.
        if (!_controller.IsPressingIntoWall())
        {
            _controller._wallStickTimer += (float)delta;
            if (_controller._wallStickTimer > MIN_WALL_STICK_TIME)
            {
                //DEBUG
                //GD.Print("[WallStickState] No input detected. Exiting to FallingState.");
                _controller.ChangeState(PlayerState.Falling);
                return;
            }
        }
        else
        {
            // Reset the timer if input is maintained.
            _controller._wallStickTimer = 0f;
        }

        if (Input.IsActionJustPressed(ControlsSchema.UI_JUMP))
        {
            GD.Print("[WallStickState] Jump pressed. Transitioning to WallJumpState.");
            _controller.ChangeState(PlayerState.WallJump);
            return;
        }

        if (!_controller.IsOnFloor() &&
            _controller.CheckIfCanWallStick() &&
            _controller.IsPressingIntoWall() &&
            _controller._wallJumpLockTime <= 0f) // cooldown must expire
        {
            _controller.ChangeState(PlayerState.WallStick);
            return;
        }
    }

    public void UpdateState(double delta)
    {
        if (_controller.IsPressingIntoWall())
        {
            _controller.ClampWallSlideVelocity();
        }

        _controller.ApplyHorizontalMovement(delta);
    }
}