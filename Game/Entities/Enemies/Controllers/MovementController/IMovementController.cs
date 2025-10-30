using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Controllers
{

    public interface IMovementController
    {
        void Initialize(Enemy enemy);
        void StartMovement(Vector2 direction);
        void StopMovement();
        void Update(float deltaTime, Enemy enemy);
    }
}