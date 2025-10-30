using Godot;
using DeckroidVania.Game.Entities.Components;

namespace DeckroidVania.Game.Components.Health;

public class HealthComponent : ComponentBase, IHealthComponent
{
    public int CurrentHealth { get; private set; }
    public int MaxHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;

    // Needs check
    public event System.Action<int> OnHealthChanged;
    public event System.Action OnDeath;

    private readonly int _startingHealth;

    public HealthComponent(int maxHealth)
    {
        _startingHealth = maxHealth;
        MaxHealth = maxHealth;
    }

    public override void Initialize(Node owner)
    {
        base.Initialize(owner);
        CurrentHealth = _startingHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        OnHealthChanged?.Invoke(CurrentHealth);

        if (IsDead)
        {
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        OnHealthChanged?.Invoke(CurrentHealth);
    }

    public void SetMaxHealth(int amount)
    {
        MaxHealth = amount;
        CurrentHealth = Mathf.Min(CurrentHealth, MaxHealth);
        OnHealthChanged?.Invoke(CurrentHealth);
    }
}
