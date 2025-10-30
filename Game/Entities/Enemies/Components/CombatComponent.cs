using Godot;
using System.Collections.Generic;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Data;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles all combat-related functionality for enemies
    /// Responsibilities: attack selection, execution, cooldowns, damage dealing
    /// </summary>
    public partial class CombatComponent : Node, ICombatComponent
    {
        private AttackRanges _attackRanges;
        private Dictionary<string, int> _attackRotationIndex = new Dictionary<string, int>();
        private float _timeSinceLastAttack = 999f; // Start ready to attack
        
        public float AttackCooldown { get; private set; }
        public bool CanAttack => _timeSinceLastAttack >= AttackCooldown;
        public EnemyAttackData CurrentAttack { get; private set; }

        public void Initialize(AttackRanges attackRanges, float cooldown)
        {
            _attackRanges = attackRanges;
            AttackCooldown = cooldown;
            
            GD.Print("[CombatComponent] ═════════════════════════════════════");
            GD.Print("[CombatComponent] Initialized with AttackRanges:");
            if (attackRanges?.Melee != null && attackRanges.Melee.Attacks.Count > 0)
            {
                GD.Print($"[CombatComponent]   Melee ({attackRanges.Melee.MinDistance}-{attackRanges.Melee.MaxDistance}m):");
                foreach (var attackId in attackRanges.Melee.Attacks)
                    GD.Print($"[CombatComponent]     - {attackId}");
            }
            if (attackRanges?.Ranged != null && attackRanges.Ranged.Attacks.Count > 0)
            {
                GD.Print($"[CombatComponent]   Ranged ({attackRanges.Ranged.MinDistance}-{attackRanges.Ranged.MaxDistance}m):");
                foreach (var attackId in attackRanges.Ranged.Attacks)
                    GD.Print($"[CombatComponent]     - {attackId}");
            }
            GD.Print("[CombatComponent] ═════════════════════════════════════");
        }

        public EnemyAttackData SelectAttack(float distanceToTarget)
        {
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
            }
            else if (_attackRanges.Ranged != null && 
                distanceToTarget >= _attackRanges.Ranged.MinDistance && 
                distanceToTarget <= _attackRanges.Ranged.MaxDistance)
            {
                applicableRange = _attackRanges.Ranged;
                rangeName = "ranged";
            }

            if (applicableRange == null || applicableRange.Attacks.Count == 0)
            {
                // No attack available at this distance - this is normal during chase
                return null;
            }

            // Get or create rotation index for this range
            if (!_attackRotationIndex.ContainsKey(rangeName))
                _attackRotationIndex[rangeName] = 0;

            // Get current attack and rotate to next
            int currentIndex = _attackRotationIndex[rangeName];
            string attackId = applicableRange.Attacks[currentIndex];
            
            // Advance to next attack (cycle through)
            _attackRotationIndex[rangeName] = (currentIndex + 1) % applicableRange.Attacks.Count;

            CurrentAttack = AttackDatabase.GetAttack(attackId);
            
            if (CurrentAttack != null)
            {
                GD.Print($"[CombatComponent] Selected {rangeName} attack: {CurrentAttack.Name} (distance: {distanceToTarget:F1}m)");
            }
            
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
            GD.Print($"[CombatComponent]   Damage: {CurrentAttack.Damage}, Type: {CurrentAttack.Type}");
            GD.Print($"[CombatComponent]   Animation: {CurrentAttack.AnimationName}");
            
            // Reset cooldown
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
    }
}
