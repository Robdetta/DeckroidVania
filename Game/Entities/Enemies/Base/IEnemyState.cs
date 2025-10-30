namespace DeckroidVania.Game.Entities.Enemies.Interfaces
{
    public interface IEnemyState
    {
        void Enter();
        void Exit();
        void HandleInput(double delta);
        void UpdateState(double delta);
    }
}