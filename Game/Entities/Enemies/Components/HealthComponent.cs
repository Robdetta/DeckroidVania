using Godot;
using System;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Data;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles all health-related functionality for enemies
    /// Responsibilities: health tracking, damage, healing, death
    /// </summary>
    public partial class HealthComponent : Node, IHealthComponent
    {
        private int _currentHealth;
        private int _maxHealth;
        private bool _isDead = false;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;
        public bool IsDead => _isDead;
        
        public event Action<int> OnHealthChanged;
        public event Action<int> OnDamageTaken;
        public event Action OnDeath;

        // Interface method - takes int directly
        public void Initialize(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
            _isDead = false;

            GD.Print($"[HealthComponent] Initialized with {_currentHealth}/{_maxHealth} HP");
        }

        // Overload for convenience - takes HealthData
        public void Initialize(HealthData healthData)
        {
            if (healthData == null)
            {
                GD.PushError("[HealthComponent] HealthData is NULL!");
                return;
            }

            Initialize(healthData.MaxHealth);
        }

        public void TakeDamage(int amount)
        {
            if (_isDead)
            {
                GD.Print("[HealthComponent] Already dead, ignoring damage");
                return;
            }

            if (amount <= 0)
            {
                GD.PushWarning($"[HealthComponent] Invalid damage amount: {amount}");
                return;
            }

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            
            GD.Print($"[HealthComponent] 💔 Took {amount} damage! HP: {_currentHealth}/{_maxHealth}");
            
            OnDamageTaken?.Invoke(amount);
            OnHealthChanged?.Invoke(_currentHealth);

            if (_currentHealth <= 0 && !_isDead)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (_isDead)
            {
                GD.Print("[HealthComponent] Cannot heal - already dead");
                return;
            }

            if (amount <= 0)
            {
                GD.PushWarning($"[HealthComponent] Invalid heal amount: {amount}");
                return;
            }

            int oldHealth = _currentHealth;
            _currentHealth = Mathf.Min(_maxHealth, _currentHealth + amount);
            int actualHealed = _currentHealth - oldHealth;

            if (actualHealed > 0)
            {
                GD.Print($"[HealthComponent] 💚 Healed {actualHealed} HP! HP: {_currentHealth}/{_maxHealth}");
                OnHealthChanged?.Invoke(_currentHealth);
            }
        }

        public void Die()
        {
            if (_isDead)
                return;

            _isDead = true;
            _currentHealth = 0;
            
            GD.Print("[HealthComponent] ☠️ DEATH");
            
            OnHealthChanged?.Invoke(_currentHealth);
            OnDeath?.Invoke();
        }

        public void Reset()
        {
            _currentHealth = _maxHealth;
            _isDead = false;
            
            GD.Print($"[HealthComponent] Reset to {_currentHealth}/{_maxHealth} HP");
            OnHealthChanged?.Invoke(_currentHealth);
        }
    }
}
