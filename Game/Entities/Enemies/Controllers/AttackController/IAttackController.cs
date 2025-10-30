using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Controllers
{
    public interface IAttackController
    {
        void Update(float deltaTime, Enemy enemy);
    }
}