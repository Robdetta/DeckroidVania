using DeckroidVania.Game.Entities.Enemies.Data;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface ICombatComponent
    {
        float AttackCooldown { get; }
        bool CanAttack { get; }
        EnemyAttackData CurrentAttack { get; }
        
        void Initialize(AttackRanges attackRanges, float cooldown);
        EnemyAttackData SelectAttack(float distanceToTarget);
        void ExecuteAttack();
        void Update(double delta);
        void ResetCooldown();
    }
}
