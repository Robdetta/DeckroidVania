using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;
using Godot;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class DashingState : IPlayerState
{
    [Export, ExportGroup("Dash Settings")]
    public float DashAcceleration { get; set; } = 0f;

    private bool _isDashing;
    private float _dashTimer;
    private float _dashSpeedPerFrame = 0f;
    private MovementController _controller;

    public DashingState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // StartDash() logic
        _isDashing = true;
        _dashTimer = _controller.DashLength / _controller.DashSpeed;

        // read input, decide facing
        float inputX = _controller.GetHorizontalInput();
        if (Mathf.Abs(inputX) > 0)
        {
            _controller._faceRight = (inputX > 0);
        }

        _controller.SnapRotationToCurrentFacing();
        _dashSpeedPerFrame = _controller._faceRight ? _controller.DashSpeed : -_controller.DashSpeed;

        // IMMEDIATELY APPLY DASH SPEED
        _controller._velocity.X = _dashSpeedPerFrame;
    }

    public void Exit()
    {
        // exit dash
        _isDashing = false;
    }

    public void HandleInput(double delta)
    {
        _dashTimer -= (float)delta;

        // If jump is pressed, transfer momentum correctly
        if (Input.IsActionJustPressed(ControlsSchema.UI_JUMP))
        {
            float inputX = _controller.GetHorizontalInput();

            // If input direction exists, override dash direction
            if (Mathf.Abs(inputX) > 0)
            {
                _controller._faceRight = (inputX > 0);
                _controller.SnapRotationToCurrentFacing();
            }

            // Apply horizontal dash jump speed in the correct direction
            _controller._velocity.X = _controller._faceRight ? _controller.DashJumpSpeed : -_controller.DashJumpSpeed;

            // Mark dash jumping
            _controller._isDashJumping = true;
            _controller._dashJumpTimer = _controller.DashJumpTime;

            // Apply upward velocity for the jump
            _controller._velocity.Y = _controller.JumpVelocity;

            // Switch to Jumping
            _controller.ChangeState(PlayerState.Jumping);
            return;
        }

        // End dash if timer is up or dash button is released
        if (_dashTimer <= 0 || !Input.IsActionPressed(ControlsSchema.UI_DASH))
        {
            _controller.ChangeState(PlayerState.Normal);
            return;
        }

        // Handle reversing direction mid-dash
        float inputXDir = _controller.GetHorizontalInput();
        if (Mathf.Abs(inputXDir) > 0)
        {
            bool newFacing = inputXDir > 0;
            if (newFacing != _controller._faceRight)
            {
                _controller._faceRight = newFacing;
                _dashSpeedPerFrame = newFacing ? _controller.DashSpeed : -_controller.DashSpeed;
                _controller._velocity.X = _dashSpeedPerFrame;
                _controller.SnapRotationToCurrentFacing();
            }
        }
    }

    public void UpdateState(double delta)
    {
        // accelerate toward dash speed
        _controller._velocity.X = Mathf.MoveToward(
            _controller._velocity.X,
            _dashSpeedPerFrame,
            DashAcceleration * (float)delta
        );
    }
}