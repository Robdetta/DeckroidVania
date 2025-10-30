using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;
using Godot;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class FallingState : IPlayerState
{
    private MovementController _controller;

    public FallingState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // Nothing special needed, unless we want to do something
        // when we first start falling.
    }

    public void Exit()
    {
        // No cleanup needed unless you want some effect to stop, etc.
    }

    public void HandleInput(double delta)
    {   

        // If we land on floor => Normal
        if (_controller.IsOnFloor())
        {
            _controller.ChangeState(PlayerState.Normal);
            return;
        }

        // If dash pressed and allowAirDash => Dashing
        if (Input.IsActionJustPressed(ControlsSchema.UI_DASH) && _controller.AllowAirDash)
        {
            _controller.ChangeState(PlayerState.Dashing);
            return;
        }

        // If jump pressed and we still have jumps => Jumping
        if (Input.IsActionJustPressed(ControlsSchema.UI_JUMP) && _controller.CanDoubleJump())
        {
            _controller.ChangeState(PlayerState.Jumping);
            return;
        }

        // Check for wall-stick: in air, near a wall, and pressing into the wall.
        if (_controller.ShouldEnterWallStick())
        {
            GD.Print("[FallingState] Wall stick conditions met. Switching to WallStickState.");
            _controller.ChangeState(PlayerState.WallStick);
            return;
        }

        // In WallStickState.HandleInput:
        _controller._wallStickTimer += (float)delta;
        if (_controller._wallStickTimer < 0.1f) {
            // Remain in WallStickState for at least 0.1 seconds, regardless of transient raycast failures.
        } else if (!_controller.CheckIfCanWallStick() || !_controller.IsPressingIntoWall()) {
            //DEBUG
            //GD.Print("[WallStickState] Conditions lost. Switching to FallingState.");
            _controller.ChangeState(PlayerState.Falling);
            return;
        }
    }

    public void UpdateState(double delta)
    {
        // controller.ApplyFacingRotation(delta);
        // // We can apply normal gravity or call a helper like:
        // // Then do normal air input horizontally:
        _controller.ApplyHorizontalMovement(delta);
        // e.g. face direction is called in your air input or we can do:
        _controller.ApplyFacingRotation(delta);
    }

}
