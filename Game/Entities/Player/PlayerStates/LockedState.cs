using Godot;
using DeckroidVania2.Game.Player.Interfaces;

namespace DeckroidVania2.Game.Player.PlayerStates;

public class LockedState : IPlayerState
{
    private readonly MovementController _controller;

    public LockedState(MovementController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // Optionally zero velocity or play a lock animation
        _controller._velocity = Vector3.Zero;
    }

    public void HandleInput(double delta)
    {
        // Ignore all movement input
    }

    public void UpdateState(double delta)
    {
        // Optionally, transition back to Normal after a timer (handled by Player or MovementController)
    }

    public void Exit()
    {
        // Cleanup if needed
    }
}