using Godot;

namespace DeckroidVania.Game.Components.Health;

public interface IHealthComponent
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }
    
    event System.Action<int> OnHealthChanged;
    event System.Action OnDeath;
    
    void Initialize(Node owner);
    void TakeDamage(int amount);
    void Heal(int amount);
    void SetMaxHealth(int amount);
}
