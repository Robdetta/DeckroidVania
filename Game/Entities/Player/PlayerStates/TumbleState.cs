using DeckroidVania2.Game.Player.Interfaces;
using Godot;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class TumbleState : IPlayerState
{

    private MovementController _controller;

    public TumbleState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        TurnOffCollision();
        return;
    }

    public void Exit()
    {
        TurnOnCollision();
        return;
    }

    public void HandleInput(double delta)
    {
        TurnOnCollision();
        // Check if turning collision back on will immediately collide with something (like the floor, collision must be on)
        var isColliding = _controller._characterBody.TestMove(_controller._characterBody.GlobalTransform, new Vector3(0, -0.1f, 0));
        if (isColliding)
        {
            TurnOffCollision();
            return;
        }

        if (!_controller.IsOnFloor())
        {
            _controller.ChangeState(PlayerState.Falling);
            return;
        }

        _controller.ChangeState(PlayerState.Normal);
        return;
    }

    public void UpdateState(double delta)
    {
        _controller.ApplyHorizontalMovement(delta);
        return;
    }

    private void TurnOffCollision()
    {
        _controller._characterBody.SetCollisionMaskValue(1, false);
        _controller._characterBody.SetCollisionLayerValue(1, false);
    }

    private void TurnOnCollision()
    {
        _controller._characterBody.SetCollisionMaskValue(1, true);
        _controller._characterBody.SetCollisionLayerValue(1, true);
    }
}
