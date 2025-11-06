using Godot;
using DeckroidVania.Game.Components.Health;
using DeckroidVania.Game.Entities.Enemies.Controllers;
using DeckroidVania.Game.Entities.Enemies.States;
using DeckroidVania.Game.Entities.Enemies.Components;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania.Game.Combat.Hitbox;

namespace DeckroidVania.Game.Entities.Enemies.Base

{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Block,      // Shield blocking state
        Hurt,
        Dead,
        Knockback,
        Falling
    }

    public abstract partial class Enemy : CharacterBody3D, IEnemy
    {
        [Export]
        public float Gravity { get; set; } = 800f; // Gravity strength
        
        // Store the enemy data for access by states
        public Data.EnemyData EnemyData { get; protected set; }
        
        // ===== ECS COMPONENTS =====
        public Components.Interfaces.IHealthComponent HealthComponent { get; protected set; }
        public IMovementComponent MovementComponent { get; protected set; }
        public ICombatComponent CombatComponent { get; protected set; }
        public IAIComponent AIComponent { get; protected set; }
        public IAnimationComponent AnimationComponent { get; protected set; }
        public IVisionComponent VisionComponent { get; protected set; }

        public bool IsDead => HealthComponent?.IsDead ?? false;

        public override void _Ready()
        {
            InitializeHealth();
            InitializeMovementController();
            // Components will be initialized by derived classes after loading JSON data
        }
        
        public void SpawnHitbox(string configId)
        {
            GD.Print($"[Enemy] SpawnHitbox called with configId: '{configId}'");
            
            var hitboxData = HitboxConfigLoader.LoadHitboxConfig(configId);
            
            // Find the Visual/Knight node (or just Visual if Knight doesn't exist)
            Node3D visualNode = GetNodeOrNull<Node3D>("Visual/Knight");
            if (visualNode == null)
            {
                visualNode = GetNodeOrNull<Node3D>("Visual");
                GD.Print("[Enemy] Using Visual node as parent");
            }
            else
            {
                GD.Print("[Enemy] Using Visual/Knight node as parent");
            }
            
            if (visualNode == null)
            {
                GD.PrintErr("[Enemy] ✗ Could not find Visual or Visual/Knight node!");
                return;
            }
            
            var hitboxComponent = new HitboxComponent();
            visualNode.AddChild(hitboxComponent);  // Add to Visual/Knight instead of Enemy root
            hitboxComponent.Initialize(hitboxData, "Player");
            
            GD.Print($"[Enemy] ✓ Spawned hitbox for '{configId}' under {visualNode.Name}");
        }
        
        public void EnableMeleeHitbox()
        {
            GD.Print("[Enemy] EnableMeleeHitbox - spawning sword_slash hitbox");
            SpawnHitbox("sword_slash");
        }

        public void DisableMeleeHitbox()
        {
            GD.Print("[Enemy] DisableMeleeHitbox called (hitbox auto-destroys)");
        }

        public void EnableBlockHitbox()
        {
            GD.Print("[Enemy] EnableBlockHitbox - spawning block_counter hitbox");
            SpawnHitbox("block_counter");
        }

        public void DisableBlockHitbox()
        {
            GD.Print("[Enemy] DisableBlockHitbox called (hitbox auto-destroys)");
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
            
            // Store the enemy data for states to access
            EnemyData = enemyData;
            
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
            
            // 6. Create VisionComponent
            var visionComp = new Components.VisionComponent();
            AddChild(visionComp);
            
            // Find the VisionArea node (should be a child of the enemy in the scene)
            var visionArea = GetNodeOrNull<Area3D>("VisionArea");
            if (visionArea != null)
            {
                // Pass detection range from JSON to VisionComponent
                float detectionRange = enemyData.Vision.DetectionRange;
                visionComp.Initialize(visionArea, detectionRange, "Player");
                
                // Subscribe to vision events - when VisionComponent detects/loses targets
                // This is the "glue" connecting VisionComponent to AIComponent
                visionComp.OnTargetDetected += OnTargetDetectedByVision;
                visionComp.OnTargetLost += OnTargetLostFromVision;
            }
            else
            {
                GD.PushWarning("[Enemy] VisionArea not found - enemy won't detect player!");
            }
            
            VisionComponent = visionComp;
            
            GD.Print("[Enemy] ✅ All components initialized!");
        }
        
        /// <summary>
        /// Event handler: Called when VisionComponent detects a target
        /// This is the "callback" - VisionComponent announces "I found something!" and we respond
        /// </summary>
        /// <param name="target">The target that was detected (the player)</param>
        private void OnTargetDetectedByVision(Node3D target)
        {
            if (AIComponent == null)
                return;
            
            GD.Print($"[Enemy] 👁️ Target detected by vision! Current state: {AIComponent.CurrentState}");
            
            // Tell AIComponent about the target
            AIComponent.SetTarget(target);
            
            // Only change state if we're passively patrolling/idling
            // (Don't interrupt attack/knockback/etc)
            if (AIComponent.CurrentState == EnemyState.Idle || AIComponent.CurrentState == EnemyState.Patrol)
            {
                // Read detection behavior from JSON
                string detectionBehavior = GetDetectionBehavior();
                
                if (detectionBehavior.ToLower() == "attack")
                {
                    // Mage behavior: Go straight to attacking (ranged)
                    GD.Print("[Enemy] 🎯 Detection behavior: ATTACK - engaging immediately!");
                    AIComponent.ChangeState(EnemyState.Attack);
                }
                else // "chase" or default
                {
                    // Knight behavior: Chase first, attack when close
                    GD.Print("[Enemy] 🏃 Detection behavior: CHASE - pursuing target!");
                    AIComponent.ChangeState(EnemyState.Chase);
                }
            }
        }
        
        /// <summary>
        /// Event handler: Called when VisionComponent loses sight of target
        /// </summary>
        /// <param name="target">The target that was lost</param>
        private void OnTargetLostFromVision(Node3D target)
        {
            if (AIComponent == null)
                return;
            
            GD.Print($"[Enemy] 🚫 Target lost from vision");
            
            // Clear the target from AIComponent
            AIComponent.ClearTarget();
            
            // Only return to passive state if we were actively chasing
            if (AIComponent.CurrentState == EnemyState.Chase)
            {
                // Return to default idle behavior
                // Mage idles in place, Knight patrols
                if (this is Types.MageEnemy)
                {
                    GD.Print("[Enemy] Returning to Idle (Mage)");
                    AIComponent.ChangeState(EnemyState.Idle);
                }
                else
                {
                    GD.Print("[Enemy] Returning to Patrol (Knight)");
                    AIComponent.ChangeState(EnemyState.Patrol);
                }
            }
        }
        
        /// <summary>
        /// Get detection behavior - override in derived classes if needed
        /// </summary>
        protected virtual string GetDetectionBehavior()
        {
            return "chase"; // Default behavior
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);
            
            // Update AI state machine
            AIComponent?.Update(delta);
            
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
            // Check if current state allows taking damage or knockback (blocking prevents BOTH)
            if (AIComponent != null)
            {
                var currentState = AIComponent.GetState(AIComponent.CurrentState);
                if (currentState != null)
                {
                    // If damage is blocked, ALSO block knockback
                    if (!currentState.CanTakeDamage)
                    {
                        GD.Print($"[{Name}] 🛡️ BLOCKED! State ({AIComponent.CurrentState}) prevents damage!");
                        return; // Damage AND knockback completely blocked!
                    }
                    
                    // If knockback is blocked (but damage allowed), still skip knockback
                    if (!currentState.CanBeKnockedBack)
                    {
                        GD.Print($"[{Name}] 🛡️ Knockback blocked! State ({AIComponent.CurrentState}) is immune to knockback!");
                        HealthComponent?.TakeDamage(amount); // Still take damage though
                        return;
                    }
                }
            }
            
            GD.Print($"{Name} taking damage: {amount}");
            HealthComponent?.TakeDamage(amount);

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
            if (AIComponent == null)
            {
                GD.PrintErr("[Enemy] AIComponent is null, cannot apply knockback");
                return;
            }
            
            // Check if current state allows knockback
            var currentState = AIComponent.GetState(AIComponent.CurrentState);
            if (currentState != null && !currentState.CanBeKnockedBack)
            {
                GD.Print($"[{Name}] 🛡️ Current state ({AIComponent.CurrentState}) is immune to knockback!");
                return; // State prevents knockback (blocking, already knocked back, etc.)
            }
            
            // Get the KnockBackState instance from AIComponent's state dictionary
            var knockBackStateBase = AIComponent.GetState(EnemyState.Knockback);
            if (knockBackStateBase is KnockBackState knockBackState)
            {
                GD.Print($"[{Name}] 💥 Applying knockback!");
                
                // Apply the knockback velocity and duration to the KnockBackState
                knockBackState.ApplyKnockback(knockbackVelocity, knockbackDuration);

                // Change the enemy's state to KnockBack
                AIComponent.ChangeState(EnemyState.Knockback);
            }
            else
            {
                GD.PrintErr("[Enemy] KnockBackState not found or is not of the correct type in the state dictionary.");
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
            // Check if currently blocking - complete immunity!
            if (AIComponent != null && AIComponent.CurrentState == EnemyState.Block)
            {
                return 0f; // 🛡️ COMPLETE IMMUNITY while blocking!
            }

            return 1f; // Default: no resistance
        }
    }
}