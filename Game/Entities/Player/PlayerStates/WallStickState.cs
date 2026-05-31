using Godot;
using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;
using System;
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

        // Determine wall direction
        float wallDirection = _controller.GetWallPushDirection();

        // Fallback: if GetWallPushDirection returns 0, use input direction
        if (wallDirection == 0f)
        {
            float inputRight = Input.GetActionStrength(ControlsSchema.UI_RIGHT);
            float inputLeft = Input.GetActionStrength(ControlsSchema.UI_LEFT);

            if (inputRight > 0.1f)
                wallDirection = -1f;  // Moving right = wall on right
            else if (inputLeft > 0.1f)
                wallDirection = 1f;   // Moving left = wall on left
        }

        // Set facing direction based on wall
        if (wallDirection != 0f)
        {
            // Wall on left (push right, so face right)
            if (wallDirection > 0f)
                _controller._faceRight = true;
            // Wall on right (push left, so face left)
            else if (wallDirection < 0f)
                _controller._faceRight = false;
        }

        // Immediately apply the rotation instead of waiting for next frame
        Node3D rootNode = _controller._characterBody.GetNode<Node3D>("Visual/RootNode");
        if (rootNode != null)
        {
            float targetRotY = _controller._faceRight ? 0 : -MathF.PI;
            rootNode.Rotation = new Vector3(0, targetRotY, 0);
        }
    }

    public void Exit()
    {
        GD.Print("Wall State Exited");
    }

    public void HandleInput(double delta)
    {
        if (_controller.IsMovementLocked) return;
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
        if (_controller.IsMovementLocked) return;
        if (_controller.IsPressingIntoWall())
        {
            _controller.ClampWallSlideVelocity();
        }

        _controller.ApplyHorizontalMovement(delta);
    }
}