namespace DeckroidVania.Game.Entities.Enemies.Interfaces
{
    public interface IEnemyState
    {
        void Enter();
        void Exit();
        void HandleInput(double delta);
        void UpdateState(double delta);
        
        /// <summary>
        /// Can this state be interrupted by knockback?
        /// false = immune to knockback (blocking, special moves)
        /// true = can be knocked back (default)
        /// </summary>
        bool CanBeKnockedBack { get; }
        
        /// <summary>
        /// Can damage be dealt to this enemy while in this state?
        /// Allows for blocking/parrying mechanics
        /// </summary>
        bool CanTakeDamage { get; }
    }
}