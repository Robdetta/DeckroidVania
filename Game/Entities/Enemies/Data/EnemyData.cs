using System;
using System.Collections.Generic;

namespace DeckroidVania.Game.Entities.Enemies.Data
{
    [Serializable]
    public class EnemyData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public HealthData Health { get; set; }
        public MovementData Movement { get; set; }
        public VisionData Vision { get; set; }
        public CombatData Combat { get; set; }
    }

    [Serializable]
    public class HealthData
    {
        public int MaxHealth { get; set; }
        public int HealthRegen { get; set; }
    }

    [Serializable]
    public class MovementData
    {
        public float PatrolSpeed { get; set; }
        public float PatrolRange { get; set; }
    }

    [Serializable]
    public class VisionData
    {
        public float DetectionRange { get; set; }
        public float LoseTargetRange { get; set; }
    }

    [Serializable]
    public class CombatData
    {
        public float ChaseSpeed { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public float KnockbackResistance { get; set; }
        public string DetectionBehavior { get; set; }
        public string DefaultAttackId { get; set; }  // Changed from AttackData to string ID
        public string DefaultIdleState {get; set; }
        public AttackRanges AttackRanges { get; set; } // NEW: Range-based attack selection
        public SpatialBehaviorData SpatialBehavior { get; set; } // NEW: Three-zone behavior system
    }
    
    /// <summary>
    /// Defines spatial behavior zones for intelligent enemy positioning
    /// Used by ChaseState to decide: back away, hold position, or chase closer
    /// </summary>
    [Serializable]
    public class SpatialBehaviorData
    {
        public float OptimalRangeMin { get; set; }      // Too close - back away
        public float OptimalRangeMax { get; set; }      // Ideal attack range
        public float LoseTargetRange { get; set; }      // Give up chase beyond this
        public bool RepositioningEnabled { get; set; }  // Enable movement to maintain range
    }
}