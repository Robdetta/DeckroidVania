using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Data;
using DeckroidVania.Game.Components.Health;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using System.Text.Json;



namespace DeckroidVania.Game.Entities.Enemies.Types
{
    public partial class MageEnemy : Enemy
    {
        //protected override int GetStartingHealth() => 40; // Mages have more health
        [Export] 
        private string _enemyDefinitionPath = "res://Game/Entities/Enemies/Definitions/mage.json";
        //Data json has no UID
        private EnemyData _enemyData;
        private Vector3 _startPosition;
        private int _direction = -1; // -1 for left, 1 for right89

        //private List<AttackData> _attacks = new List<AttackData>();


        public override void _Ready()
        {
            AttackDatabase.Initialize();
            LoadEnemyDefinition();
            base._Ready();
        }

        protected override void InitializeMovementController()
        {
            if (_enemyData == null || _enemyData.Movement == null || _enemyData.Combat == null || _enemyData.Vision == null)
            {
                GD.PushError("Cannot initialize movement: Enemy data not fully loaded");
                return;
            }
            
            // Load the default attack from database
            EnemyAttackData defaultAttack = AttackDatabase.GetAttack(_enemyData.Combat.DefaultAttackId);

            if (defaultAttack == null)
            {
                GD.PushError($"Failed to load default attack: {_enemyData.Combat.DefaultAttackId}");
                return;
            }
            
            // Create AI state machine
            if (AIComponent != null)
            {
                AIComponent.CreateStates(
                    _enemyData.Movement.PatrolSpeed,
                    _enemyData.Movement.PatrolRange,
                    _enemyData.Combat.ChaseSpeed
                );
            }
        }

        /// <summary>
        /// Loads enemy data from a JSON file located at _enemyDefinitionPath
        /// </summary>
        /// <remarks>
        /// This method will attempt to load enemy data from a JSON file. If the file is empty or not found,
        /// an error will be pushed to the Godot console. If the enemy data cannot be deserialized
        /// from the JSON, an error will also be pushed to the Godot console.
        /// </remarks>
        private void LoadEnemyDefinition()
        {
            try
            {
                string jsonText = Godot.FileAccess.GetFileAsString(_enemyDefinitionPath);

                if (string.IsNullOrEmpty(jsonText))
                {
                    GD.PushError($"JSON file is empty or not found at: {_enemyDefinitionPath}");
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _enemyData = JsonSerializer.Deserialize<EnemyData>(jsonText, options);

                if (_enemyData == null)
                {
                    GD.PushError("Failed to deserialize enemy definition from JSON.");
                }
                else
                {
                    GD.Print($"Loaded enemy data: {_enemyData.Name}, MaxHealth: {_enemyData.Health?.MaxHealth}");
                    
                    // NEW: Initialize ECS components after loading JSON
                    InitializeComponents(_enemyData);
                }
            }
            catch (System.Exception e)
            {
                GD.PushError($"Error loading enemy definition: {e.Message}");
            }
        }

        protected override void InitializeHealth()
        {
            // Health component now initialized in InitializeComponents() called from LoadEnemyDefinition()
            // This method kept for legacy compatibility during transition
        }

        protected override void OnHealthChanged(int newHealth)
        {
            base.OnHealthChanged(newHealth);
            // Add any mage-specific health change behavior here
        }

        protected override void OnDeath()
        {
            base.OnDeath();
            // Add any mage-specific death behavior here
        }

        protected override float GetKnockbackResistance()
        {
            // FIRST check if blocking - complete immunity takes priority!
            float baseResistance = base.GetKnockbackResistance();
            if (baseResistance <= 0f)
            {
                return 0f; // 🛡️ Blocking = complete immunity
            }
            
            // Otherwise use JSON-defined resistance
            if (_enemyData?.Combat != null)
            {
                return _enemyData.Combat.KnockbackResistance;
            }
            return 1f; // Default if data not loaded
        }
        
        protected override string GetDetectionBehavior()
        {
            if (_enemyData?.Combat != null && !string.IsNullOrEmpty(_enemyData.Combat.DetectionBehavior))
            {
                return _enemyData.Combat.DetectionBehavior;
            }
            return "attack"; // Mage defaults to attack
        }

    }


    // // In a hypothetical CastingState for mages
    // if (_controller._animationTree is MageAnimationTree mageTree)
    // {
    //     mageTree.ChangeState((EnemyAnimationState)MageAnimationTree.MageAnimationState.Casting);
    // }
    // else
    // {
    //     _controller._animationTree?.ChangeState(EnemyAnimationTree.EnemyAnimationState.Idle);
    // }

    
}