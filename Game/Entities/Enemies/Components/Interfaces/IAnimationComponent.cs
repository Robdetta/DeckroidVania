namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface IAnimationComponent
    {
        void Initialize();
        void ChangeState(int stateId); // Use int instead of enum to avoid coupling
        void PlayAttackAnimation(string animationName);
        void SetMovementBlend(float speed);
    }
}
