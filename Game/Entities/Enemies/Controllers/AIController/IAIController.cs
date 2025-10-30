using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Controllers
{
    public interface IAIController
    {
        void Initialize(Enemy enemy); // Add this line
        void Update(float deltaTime, Enemy enemy);
    }
}