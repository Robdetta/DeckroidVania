using DeckroidVania.Game.Entities.Enemies.Data;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface ICombatComponent
    {
        float AttackCooldown { get; }
        bool CanAttack { get; }
        EnemyAttackData CurrentAttack { get; }
        
        void Initialize(Enemy enemy, CombatData combatData);
        EnemyAttackData SelectAttack(float distanceToTarget);
        void ExecuteAttack();
        void Update(double delta);
        void ResetCooldown();
    }
}
