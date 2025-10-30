using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;
using Godot;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class NormalState : IPlayerState
{
    private MovementController _controller;
    private const string CAN_DROP_THROUGH = "CanDropThrough";

    public NormalState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // e.g. reset some flags
        // controller._curNumOfJumps = 0 if you want
        if (_controller.IsOnFloor())
        {
            _controller._curNumOfJumps = 0; // reset jumps
            _controller.ChangeState(PlayerState.Normal);
            return;
        }
    }

    public void Exit()
    {
        // Cleanup if needed
    }

    public void HandleInput(double delta)
    {
        // If not on floor, switch to falling
        if (!_controller.IsOnFloor())
        {
            _controller.ChangeState(PlayerState.Falling);
            return;
        }

        // if dash pressed
        if (Input.IsActionJustPressed(ControlsSchema.UI_DASH) && (_controller.AllowAirDash || _controller.IsOnFloor()))
        {
            _controller.ChangeState(PlayerState.Dashing);
            return;
        }

        // if jump pressed
        if (Input.IsActionJustPressed(ControlsSchema.UI_JUMP))
        {
            if (_controller.IsOnFloor() && _controller.CanDoubleJump())
            {
                _controller.ChangeState(PlayerState.Jumping);
                return;
            }
        }

        // if drop through platform attempted
        if (Input.IsActionJustPressed(ControlsSchema.DROP_DOWN))
        {
            if (_controller.IsOnFloor() && _controller._velocity.X < 0.5f)
            {
                if (CheckIfCanDropThrough())
                    _controller.ChangeState(PlayerState.Tumble);

                return;
            }
        }
    }

    public void UpdateState(double delta)
    {
        // apply normal horizontal movement, gravity, facing direction, etc.
        // e.g. controller.ApplyMovementAndGravity(delta);
        // or your own version. 
        // Possibly do:

        _controller.ApplyFacingRotation(delta);
        _controller.ApplyHorizontalMovement(delta);
    }

    private bool CheckIfCanDropThrough()
    {
        // start and end of ray-cast (-1 unit directly beneath player)
        var start = _controller._characterBody.GlobalPosition;
        //var end = start + new Vector3(0, -1, 0);
        float horizontalDir = _controller._faceRight ? 1 : -1;
        var end = start + new Vector3(horizontalDir, 0, 0);

        var query = PhysicsRayQueryParameters3D.Create(start, end, _controller._characterBody.CollisionMask, new Godot.Collections.Array<Rid> { _controller._characterBody.GetRid() });
        var result = _controller._characterBody.GetWorld3D().DirectSpaceState.IntersectRay(query);

        // Check if any objects hit by ray-cast
        if (result.TryGetValue("collider", out Variant value))
        {
            var hitObject = value.As<Node3D>();
            return hitObject.IsInGroup(CAN_DROP_THROUGH);
        }
        return false;
    }
}
