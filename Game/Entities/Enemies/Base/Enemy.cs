using Godot;
using DeckroidVania.Game.Components.Health;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.States;
using DeckroidVania.Game.Entities.Enemies.Components;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;

namespace DeckroidVania.Game.Entities.Enemies.Base

{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Hurt,
        Dead,
        Knockback,
        Falling
    }

    public abstract partial class Enemy : CharacterBody3D, IEnemy
    {
        [Export]
        public float Gravity { get; set; } = 800f; // Gravity strength
        
        // ===== ECS COMPONENTS =====
        public Components.Interfaces.IHealthComponent HealthComponent { get; protected set; }
        public IMovementComponent MovementComponent { get; protected set; }
        public ICombatComponent CombatComponent { get; protected set; }
        public IAIComponent AIComponent { get; protected set; }
        public IAnimationComponent AnimationComponent { get; protected set; }
        
        // Legacy - keeping for now during transition (public for state access)
        public EnemyMovementController _movementController;     

        public bool IsDead => HealthComponent?.IsDead ?? false;

        public override void _Ready()
        {
            InitializeHealth();
            InitializeMovementController();
            // Components will be initialized by derived classes after loading JSON data
        }
        
        /// <summary>
        /// Initialize all ECS components - call this after loading enemy data from JSON
        /// </summary>
        protected virtual void InitializeComponents(Data.EnemyData enemyData)
        {
            if (enemyData == null)
            {
                GD.PushError("[Enemy] Cannot initialize components - enemyData is null!");
                return;
            }
            
            GD.Print("[Enemy] ═══════════════════════════════════════");
            GD.Print($"[Enemy] Initializing ECS Components for {enemyData.Name}");
            GD.Print("[Enemy] ═══════════════════════════════════════");
            
            // 1. Create HealthComponent
            var healthComp = new Components.HealthComponent();
            AddChild(healthComp);
            healthComp.Initialize(enemyData.Health);
            healthComp.OnHealthChanged += OnHealthChanged;
            healthComp.OnDeath += OnDeath;
            HealthComponent = healthComp;
            
            // 2. Create MovementComponent
            var movementComp = new Components.MovementComponent();
            AddChild(movementComp);
            movementComp.Initialize(this, enemyData.Movement.PatrolSpeed, enemyData.Movement.PatrolRange);
            MovementComponent = movementComp;
            
            // 3. Create CombatComponent
            var combatComp = new Components.CombatComponent();
            AddChild(combatComp);
            if (enemyData.Combat.AttackRanges != null)
            {
                combatComp.Initialize(enemyData.Combat.AttackRanges, enemyData.Combat.AttackCooldown);
            }
            else
            {
                GD.PushWarning($"[Enemy] {enemyData.Name} has no AttackRanges defined in JSON!");
            }
            CombatComponent = combatComp;
            
            // 4. Create AIComponent
            var aiComp = new Components.AIComponent();
            AddChild(aiComp);
            
            // Parse initial state from JSON (case-insensitive)
            EnemyState initialState = EnemyState.Idle; // Default
            if (!string.IsNullOrEmpty(enemyData.Combat.DefaultIdleState))
            {
                string stateStr = enemyData.Combat.DefaultIdleState.ToLower();
                if (stateStr == "patrol")
                {
                    initialState = EnemyState.Patrol;
                }
                else if (stateStr == "idle")
                {
                    initialState = EnemyState.Idle;
                }
            }
            
            aiComp.Initialize(initialState, enemyData.Combat.DetectionBehavior);
            AIComponent = aiComp;
            
            GD.Print($"[Enemy] AIComponent initialized - Initial State: {initialState}");
            
            // 5. Create AnimationComponent
            var animComp = new Components.AnimationComponent();
            AddChild(animComp);
            animComp.Initialize(); // Will try to auto-find AnimationTree
            AnimationComponent = animComp;
            
            GD.Print("[Enemy] ✅ All components initialized!");
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            
            // Legacy movement controller (still handles state machine and state updates)
            _movementController?.HandleMovement(delta);
            
            // NEW: Apply gravity through MovementComponent if not in Knockback
            if (AIComponent?.CurrentState != EnemyState.Knockback && !IsOnFloor())
            {
                if (MovementComponent != null)
                {
                    var currentVel = MovementComponent.Velocity;
                    MovementComponent.Velocity = new Vector3(currentVel.X, currentVel.Y - Gravity * (float)delta, currentVel.Z);
                }
            }
            
            // NEW: Apply movement through MovementComponent
            if (MovementComponent != null)
            {
                MovementComponent.ApplyMovement(delta);
            }
            
            // Update combat component cooldown
            if (CombatComponent != null)
            {
                CombatComponent.Update(delta);
            }

            // **Lock enemy to Z = 0 (or whatever fixed Z you want)**
            if (GlobalPosition.Z != 0f)
            {
                GlobalPosition = new Vector3(GlobalPosition.X, GlobalPosition.Y, 0f);
            }

            
            // Prevent wall sliding
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                var collision = GetSlideCollision(i);
                if (collision.GetCollider() is StaticBody3D)
                {
                    Velocity = new Vector3(0, Velocity.Y, 0);
                    break;
                }
            }    

            // Check for collision with player - ONLY physical collisions, NOT vision area
            for (int i = 0; i < GetSlideCollisionCount(); i++)
            {
                KinematicCollision3D collision = GetSlideCollision(i);
                Node3D collider = collision.GetCollider() as Node3D;
                
                // Make sure it's the player AND it's NOT the vision area
                if (collider != null && collider.IsInGroup("Player"))
                {
                    // Extra safety check - ensure it's not an Area3D (vision area)
                    if (!(collider is Area3D))
                    {
                        DealDamage(collider);
                    }
                }
            }
        }

        protected virtual void InitializeHealth()
        {
            // Note: Health initialization moved to InitializeComponents()
            // This method kept for legacy compatibility during transition
        }

        protected virtual void InitializeMovementController()
        {
            // Override in derived classes
        }

        public virtual void TakeDamage(int amount, float knockbackForce, float knockbackDuration, Vector3 attackerPosition)
        {
            GD.Print($"{Name} taking damage: {amount}");
            HealthComponent?.TakeDamage(amount);

            // Check if the enemy is immune to knockback
            // if (KnockbackImmune)
            // {
            //     GD.Print($"{Name} is immune to knockback!");
            //     return; // Skip the knockback logic
            // }

            // **Apply knockback resistance multiplier**
            float resistance = GetKnockbackResistance();
            
            // **If resistance is 0 or less, skip knockback entirely (immunity)**
            if (resistance <= 0f)
            {
                GD.Print($"[{Name}] Knockback immunity! Resistance: {resistance}");
                return;
            }
            
            knockbackForce *= resistance;
            if (resistance < 1f)
            {
                GD.Print($"[{Name}] Knockback reduced by resistance: {resistance}. New force: {knockbackForce}");
            }

            // Calculate knockback direction in global space
            Vector3 knockbackDirectionGlobal = (GlobalPosition - attackerPosition).Normalized();
            GD.Print($"Enemy Position: {GlobalPosition}, Attacker Position: {attackerPosition}");
            GD.Print($"Raw knockback direction: {knockbackDirectionGlobal}");

            // Zero out the Y component to keep the knockback horizontal
            knockbackDirectionGlobal.Y = 0;
            knockbackDirectionGlobal = knockbackDirectionGlobal.Normalized();
            
            GD.Print($"Final knockback direction: {knockbackDirectionGlobal}, Force: {knockbackForce}");
            GD.Print($"Final knockback velocity: {knockbackDirectionGlobal * knockbackForce}");

            // Apply knockback
            ApplyKnockback(knockbackDirectionGlobal * knockbackForce, knockbackDuration);
        }

        public void ApplyKnockback(Vector3 knockbackVelocity, float knockbackDuration)
        {
            if (_movementController != null)
            {
                // Get the KnockBackState instance from the controller's state dictionary
                if (_movementController._states.TryGetValue(EnemyState.Knockback, out var knockBackStateBase) && knockBackStateBase is KnockBackState knockBackState)
                {
                    // Apply the knockback velocity and duration to the KnockBackState
                    knockBackState.ApplyKnockback(knockbackVelocity, knockbackDuration);

                    // Change the enemy's state to KnockBack
                    _movementController.ChangeState(EnemyState.Knockback);
                }
                else
                {
                    GD.PrintErr("KnockBackState not found or is not of the correct type in the state dictionary.");
                }
            }
            else
            {
                GD.PrintErr("Movement controller is null.");
            }
        }

        protected virtual void OnHealthChanged(int newHealth)
        {
            GD.Print($"{Name} health changed to: {newHealth}");
        }

        protected virtual void OnDeath()
        {
            GD.Print($"{Name} died!");
            QueueFree();
        }

        public virtual void DealDamage(Node3D target)
        {
            // Check if the target is an enemy
            if (target is Enemy)
            {
                return; // Don't damage other enemies
            }

            // SAFETY: Make absolutely sure this isn't coming from the vision area
            if (target is Area3D)
            {
                GD.Print($"[Enemy] Attempted damage from Area3D (vision area?), blocking!");
                return;
            }

            // Check if the target has a health component

            Game.Components.Health.IHealthComponent targetHealth = target as Game.Components.Health.IHealthComponent;
            if (targetHealth != null)
            {
                GD.Print($"[Enemy] Dealing 10 damage to {target.Name} (IHealthComponent)!");
                targetHealth.TakeDamage(10); // Deal 10 damage
            }
            else if (target.IsInGroup("Player")) // Check if it's the player
            {
                GD.Print($"[Enemy] Dealing 10 damage to Player (HealthSystem.Instance)!");
                HealthSystem.Instance.TakeDamage(10); // Deal 10 damage using the player's health system
            }
            else
            {
                GD.PrintErr($"[Enemy] {target.Name} has no health component and is not the Player!");
            }
        }

        protected virtual float GetKnockbackResistance()
        {
            return 1f; // Default: no resistance
        }
    }
}