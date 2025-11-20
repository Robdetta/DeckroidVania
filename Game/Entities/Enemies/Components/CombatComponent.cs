using Godot;
using System.Collections.Generic;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Data;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles all combat-related functionality for enemies
    /// Now uses AIBehaviorComponent for intelligent attack selection
    /// </summary>
    public partial class CombatComponent : Node, ICombatComponent
    {
        private AttackRanges _attackRanges;
        private float _attackCooldown;
        private float _timeSinceLastAttack = 999f;
        private Enemy _enemy;
        
        // NEW: AI Behavior Component
        private IAIBehaviorComponent _aiBehavior;
        
        public float AttackCooldown { get; private set; }
        public bool CanAttack => _timeSinceLastAttack >= AttackCooldown;
        public EnemyAttackData CurrentAttack { get; private set; }

        public void Initialize(Enemy enemy, CombatData combatData)
        {
            _enemy = enemy;
            _attackRanges = combatData.AttackRanges;
            _attackCooldown = combatData.AttackCooldown;
            AttackCooldown = _attackCooldown;
            
            GD.Print("[CombatComponent] Starting initialization...");

            // Initialize AI Behavior Component
            _aiBehavior = new AIBehaviorComponent();
            AddChild((Node)_aiBehavior);
            
            // Get melee attack pattern and pass to AI behavior
            if (combatData.AttackRanges?.Melee != null && combatData.AttackRanges.Melee.Attacks.Count > 0)
            {
                GD.Print($"[CombatComponent] Initializing AIBehavior with {combatData.AttackRanges.Melee.Attacks.Count} attacks");
                GD.Print($"[CombatComponent] AIBehavior config: {combatData.AIBehavior != null}");
                
                _aiBehavior.Initialize(
                    enemy,
                    combatData.AttackRanges.Melee.Attacks,
                    combatData.AIBehavior?.ToDictionary()
                );
            }
            else
            {
                GD.PushError("[CombatComponent] No melee attacks found!");
            }
    
            
            GD.Print("[CombatComponent] ═════════════════════════════════════");
            GD.Print("[CombatComponent] Initialized with AttackRanges:");
            if (_attackRanges?.Melee != null && _attackRanges.Melee.Attacks.Count > 0)
            {
                GD.Print($"[CombatComponent]   Melee ({_attackRanges.Melee.MinDistance}-{_attackRanges.Melee.MaxDistance}m):");
                foreach (var attackId in _attackRanges.Melee.Attacks)
                    GD.Print($"[CombatComponent]     - {attackId}");
            }
            if (_attackRanges?.Ranged != null && _attackRanges.Ranged.Attacks.Count > 0)
            {
                GD.Print($"[CombatComponent]   Ranged ({_attackRanges.Ranged.MinDistance}-{_attackRanges.Ranged.MaxDistance}m):");
                foreach (var attackId in _attackRanges.Ranged.Attacks)
                    GD.Print($"[CombatComponent]     - {attackId}");
            }
            GD.Print("[CombatComponent] ═════════════════════════════════════");
        }

        public EnemyAttackData SelectAttack(float distanceToTarget)
        {
            GD.Print($"[CombatComponent] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            GD.Print($"[CombatComponent] 📏 Distance to target: {distanceToTarget:F2}m");
            
            if (_attackRanges == null)
            {
                GD.PushError("[CombatComponent] AttackRanges is NULL!");
                return null;
            }

            AttackRange applicableRange = null;
            string rangeName = "";

            // Determine which range the target is in
            if (_attackRanges.Melee != null && 
                distanceToTarget >= _attackRanges.Melee.MinDistance && 
                distanceToTarget <= _attackRanges.Melee.MaxDistance)
            {
                applicableRange = _attackRanges.Melee;
                rangeName = "melee";
                GD.Print($"[CombatComponent] ✅ IN MELEE RANGE ({_attackRanges.Melee.MinDistance}-{_attackRanges.Melee.MaxDistance}m)");
            }
            else if (_attackRanges.Ranged != null && 
                distanceToTarget >= _attackRanges.Ranged.MinDistance && 
                distanceToTarget <= _attackRanges.Ranged.MaxDistance)
            {
                applicableRange = _attackRanges.Ranged;
                rangeName = "ranged";
                GD.Print($"[CombatComponent] 🎯 IN RANGED ({_attackRanges.Ranged.MinDistance}-{_attackRanges.Ranged.MaxDistance}m)");
            }
            else
            {
                GD.Print($"[CombatComponent] ❌ OUT OF RANGE");
                return null;
            }

            if (applicableRange == null || applicableRange.Attacks.Count == 0)
            {
                return null;
            }

            // Get player health (TODO: implement actual player reference)
            int playerHealth = 100;
            int enemyHealth = _enemy?.HealthComponent?.CurrentHealth ?? 40;

            // Use AI Behavior to decide which attack
            string chosenAttackId = _aiBehavior.DecideAttack(
                distanceToTarget,
                playerHealth,
                enemyHealth
            );

            if (string.IsNullOrEmpty(chosenAttackId))
            {
                GD.PushError("[CombatComponent] AI Behavior returned null attack!");
                return null;
            }

            CurrentAttack = AttackDatabase.GetAttack(chosenAttackId);
            
            if (CurrentAttack != null)
            {
                GD.Print($"[CombatComponent] 🎲 Selected {rangeName} attack:");
                GD.Print($"[CombatComponent]    Name: {CurrentAttack.Name}");
                GD.Print($"[CombatComponent]    ID: {chosenAttackId}");
                GD.Print($"[CombatComponent]    Animation: {CurrentAttack.AnimationName}");
            }
            else
            {
                GD.PushError($"[CombatComponent] ❌ Attack '{chosenAttackId}' not found in database!");
            }
            
            GD.Print($"[CombatComponent] ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            
            return CurrentAttack;
        }

        public void ExecuteAttack()
        {
            if (CurrentAttack == null)
            {
                GD.PushWarning("[CombatComponent] Cannot execute - no attack selected!");
                return;
            }

            if (!CanAttack)
            {
                GD.PushWarning($"[CombatComponent] Attack on cooldown ({_timeSinceLastAttack:F1}s / {AttackCooldown}s)");
                return;
            }

            GD.Print($"[CombatComponent] ⚔️ Executing: {CurrentAttack.Name}");
            _timeSinceLastAttack = 0f;
        }

        public void Update(double delta)
        {
            _timeSinceLastAttack += (float)delta;
        }

        public void ResetCooldown()
        {
            _timeSinceLastAttack = 0f;
        }
        
        /// <summary>
        /// Reset AI behavior when target is lost
        /// </summary>
        public void ResetAIBehavior()
        {
            _aiBehavior?.Reset();
        }
    }
}