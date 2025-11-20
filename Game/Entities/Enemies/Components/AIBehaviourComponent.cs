using Godot;
using System.Collections.Generic;
using System.Linq;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// AI Behavior Component - Makes intelligent decisions based on JSON-defined patterns
    /// All logic is configurable via JSON, no hardcoded values
    public partial class AIBehaviorComponent : Node, IAIBehaviorComponent
    {
        private Enemy _enemy;
        private List<string> _attackPattern = new();
        private int _patternIndex = 0;
        
        // Configuration from JSON
        private int _maxConsecutiveAttacks;
        private float _blockCooldown;
        private float _defensiveHealthThreshold;
        private float _defensiveBlockCooldownMultiplier;
        private bool _allowRandomAttackSelection = true;
        
        // Runtime state
        private float _blockCooldownTimer = 0f;
        private int _consecutiveAttacks = 0;
        
        // Pattern analysis
        private float _attackFrequency;
        private float _blockFrequency;
        
        public void Initialize(Enemy enemy, List<string> attackPattern, Dictionary<string, dynamic> behaviorConfig = null)
        {
            _enemy = enemy;
            _attackPattern = new List<string>(attackPattern);
            _patternIndex = 0;
            
            // Load configuration from JSON (or use defaults)
            LoadConfiguration(behaviorConfig);
            
            // Analyze the pattern
            AnalyzeAttackPattern();
            
            GD.Print($"[AIBehavior] Initialized with pattern: {string.Join(", ", attackPattern)}");
            GD.Print($"[AIBehavior] Config: MaxConsecutive={_maxConsecutiveAttacks}, BlockCD={_blockCooldown}s, DefensiveThreshold={_defensiveHealthThreshold:P0}");
            GD.Print($"[AIBehavior] Attack Frequency: {_attackFrequency:P0}, Block Frequency: {_blockFrequency:P0}");
        }
        

        /// Load AI behavior configuration from JSON
        private void LoadConfiguration(Dictionary<string, dynamic> config)
        {
            if (config == null)
                return;
            
            if (config.TryGetValue("maxConsecutiveAttacks", out var maxAttacks))
                _maxConsecutiveAttacks = (int)maxAttacks;
            
            if (config.TryGetValue("blockCooldown", out var blockCD))
                _blockCooldown = (float)(double)blockCD;
            
            if (config.TryGetValue("defensiveHealthThreshold", out var healthThresh))
                _defensiveHealthThreshold = (float)(double)healthThresh;
            
            if (config.TryGetValue("defensiveBlockCooldownMultiplier", out var cdMult))
                _defensiveBlockCooldownMultiplier = (float)(double)cdMult;
            
            if (config.TryGetValue("allowRandomAttackSelection", out var allowRandom))
                _allowRandomAttackSelection = (bool)allowRandom;
        }
        

        /// Analyze the attack pattern to determine frequencies

        private void AnalyzeAttackPattern()
        {
            if (_attackPattern.Count == 0)
                return;
            
            int attackCount = 0;
            int blockCount = 0;
            
            foreach (var attack in _attackPattern)
            {
                if (attack == "002_block")
                    blockCount++;
                else
                    attackCount++;
            }
            
            _attackFrequency = (float)attackCount / _attackPattern.Count;
            _blockFrequency = (float)blockCount / _attackPattern.Count;
        }
        
        public override void _Process(double delta)
        {
            if (_blockCooldownTimer > 0)
            {
                _blockCooldownTimer -= (float)delta;
            }
        }
        
        
        /// Decide what attack to use based on JSON-configured pattern and game state
        public string DecideAttack(float distanceToPlayer, int playerHealth, int enemyHealth)
        {
            if (_attackPattern.Count == 0)
                return null;
            
            // Separate attacks and blocks from pattern
            var realAttacks = _attackPattern.Where(a => a != "002_block").ToList();
            var hasBlockOption = _attackPattern.Contains("002_block");
            
            // Use AI decision logic
            string decision = EvaluateDecision(realAttacks, hasBlockOption, playerHealth, enemyHealth);
            
            if (decision != null)
            {
                GD.Print($"[AIBehavior] Decision: {decision} | Consecutive: {_consecutiveAttacks}/{_maxConsecutiveAttacks} | BlockCD: {_blockCooldownTimer:F1}s");
                return decision;
            }
            
            // Fallback: cycle through pattern
            return CyclePattern();
        }
        
        /// Evaluate decision based on configurable thresholds and game state
        private string EvaluateDecision(List<string> realAttacks, bool hasBlockOption, int playerHealth, int enemyHealth)
        {
            // Rule 1: If block cooldown active, must attack
            if (_blockCooldownTimer > 0)
            {
                if (realAttacks.Count > 0)
                {
                    _consecutiveAttacks++;
                    return _allowRandomAttackSelection 
                        ? realAttacks[(int)GD.Randi() % realAttacks.Count]
                        : CyclePattern();
                }
            }
            
            // Rule 2: Block after reaching consecutive attack limit
            if (hasBlockOption && _consecutiveAttacks >= _maxConsecutiveAttacks && _blockCooldownTimer <= 0)
            {
                _blockCooldownTimer = _blockCooldown;
                _consecutiveAttacks = 0;
                return "002_block";
            }
            
            // Rule 3: Defensive block when low health
            if (hasBlockOption && enemyHealth < CalculateHealthThreshold(100) && _blockCooldownTimer <= 0)
            {
                _blockCooldownTimer = _blockCooldown * _defensiveBlockCooldownMultiplier;
                _consecutiveAttacks = 0;
                return "002_block";
            }
            
            // Rule 4: Attack (default behavior)
            if (realAttacks.Count > 0)
            {
                _consecutiveAttacks++;
                // FIX: Check bounds before accessing
                int randomIndex = (int)(GD.Randi() % (uint)realAttacks.Count);
                return _allowRandomAttackSelection 
                    ? realAttacks[randomIndex]
                    : CyclePattern();
            }
            
            return null;
        }
        
        /// Calculate absolute health value from threshold percentage
        private int CalculateHealthThreshold(int maxHealth)
        {
            return (int)(maxHealth * _defensiveHealthThreshold);
        }
        
        /// Cycle through pattern sequentially (used when randomness is disabled)
        private string CyclePattern()
        {
            string attack = _attackPattern[_patternIndex];
            _patternIndex = (_patternIndex + 1) % _attackPattern.Count;
            return attack;
        }
        
        /// Reset behavior when losing target or changing states
        public void Reset()
        {
            _blockCooldownTimer = 0;
            _consecutiveAttacks = 0;
            _patternIndex = 0;
        }
        
        public (float attackFreq, float blockFreq) GetPatternFrequencies()
        {
            return (_attackFrequency, _blockFrequency);
        }
    }
}