namespace DeckroidVania2.Game.Player.Interfaces
{
    public interface IPlayerState
    {
        // Called once when we enter the state
        void Enter();

        // Called once when we exit the state
        void Exit();

        // Called every frame (or sub-step) to handle user input, transitions, etc.
        void HandleInput(double delta);

        // Called every frame for the state’s main logic
        void UpdateState(double delta);
    }
}
