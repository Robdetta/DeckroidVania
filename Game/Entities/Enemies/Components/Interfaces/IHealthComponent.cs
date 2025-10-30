using System;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface IHealthComponent
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }
        bool IsDead { get; }
        
        event Action<int> OnHealthChanged;
        event Action OnDeath;
        
        void TakeDamage(int damage);
        void Heal(int amount);
        void Initialize(int maxHealth);
    }
}
