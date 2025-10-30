using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Data;
using DeckroidVania.Game.Components.Health;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using System.Text.Json;

namespace DeckroidVania.Game.Entities.Enemies.Types
{
    public partial class KnightEnemy : Enemy
    {
        [Export]
        public string _enemyDefinitionPath = "res://Game/Entities/Enemies/Definitions/knight.json";
        private EnemyData _enemyData;
        private Vector3 _startPosition;
        private int _direction = -1; // -1 for left, 1 for right

        public override void _Ready()
        {
            AttackDatabase.Initialize();
            LoadEnemyDefinition();
            base._Ready();
            //KnockbackImmune = true;
        }

        protected override void InitializeHealth()
        {

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

            _movementController = new EnemyMovementController();
            AddChild(_movementController);
            _movementController.Initialize(
                this,
                _enemyData.Movement.PatrolSpeed,
                _enemyData.Movement.PatrolRange,
                _enemyData.Combat.ChaseSpeed,
                _enemyData.Combat.AttackRange,
                _enemyData.Vision.DetectionRange,
                _enemyData.Vision.LoseTargetRange,
                EnemyState.Patrol,  // Set Patrol as default state for Knight
                _enemyData.Combat.DetectionBehavior,
                _enemyData.Combat.AttackCooldown,
                defaultAttack,
                _enemyData.Combat.DefaultIdleState 



            );
        }

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
                    GD.Print($"✅ Loaded Knight definition: {_enemyData.Name}, MaxHealth: {_enemyData.Health?.MaxHealth}");
                    
                    // NEW: Initialize ECS components after loading JSON
                    InitializeComponents(_enemyData);
                }
            }
            catch (System.Exception e)
            {
                GD.PushError($"Failed to load enemy definition: {e.Message}");
            }
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
            if (_enemyData?.Combat != null)
            {
                return _enemyData.Combat.KnockbackResistance;
            }
            return 1f; // Default if data not loaded
        }

    }



}
