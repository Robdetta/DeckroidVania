using DeckroidVania2.Game.Player.PlayerStates;
using DeckroidVania2.Game.Scripts.Inputs;
using DeckroidVania.Game.Combat.Hitbox;
using Godot;
using System;

namespace DeckroidVania2.Game.Player;

public partial class Player : CharacterBody3D
{
    public enum ActionState
    {
        None,
        Attacking,
        Casting,
        // Add more as needed
    }

    [Export]
    public PlayerAnimationTree playerAnimationTree;
    [Export]
    private MovementController _movementController;
    [Export]
    public float AttackLockoutDuration { get; set; } = 0.5f;
    private AttackManager _attackManager;
    private WeaponManager _weaponManager;
    private ActionState _currentActionState = ActionState.None;
    private float _actionTimer = 0f;
    private float _castingDuration = 0f; // NEW: Duration for the current cast
    private bool _allowMovementDuringCast = false; // NEW: Determines if player can move while casting
    public bool IsFacingRight() => _movementController._faceRight;

    public override void _Ready()
    {
        GD.Print("[Player] Player _Ready called."); // <-- ADD/VERIFY THIS
        _movementController.Initialize(this);
        _weaponManager = new WeaponManager(this);
        _weaponManager.EquipWeaponById(1);  //testing github syncing
        
        _attackManager = new AttackManager(this, _weaponManager, _weaponManager.GetCurrentWeapon().AttackIds[0]);

        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Attack += OnAttack;
            GD.Print("[Player] InputManager.Attack subscribed."); // <-- ADD/VERIFY THIS
        }
    }

    public override void _Process(double delta)
    {
        // Get the current movement state
        var currentState = _movementController.CurrentState;

        // --- Update AnimationTree parameters for Locomotion Transitions ---
        // These boolean values will be used in the AnimationTree's transition conditions (expressions).
        // Add parameter setters for other locomotion states (e.g., is_dead) here.

        // Handle action state timer (upper body animation control)
        // Handle action state timer (upper body animation control)
        if (_currentActionState != ActionState.None)
        {
            _actionTimer -= (float)delta;
            if (_actionTimer <= 0f)
            {
                EndActionState();
            }
            // NEW: If currently casting, determine movement lock
            if (_currentActionState == ActionState.Casting)
            {
                _movementController.IsMovementLocked = !_allowMovementDuringCast;
            }
            // Old logic: "return;" statement has been removed in previous refactor, 
            // ensuring locomotion updates even during actions if allowed.
        }

        // --- Always update locomotion animation blends ---
        // Pass absolute horizontal velocity for blending idle/run in GroundMovement blend space.
        playerAnimationTree.SetGroundBlend(Mathf.Abs(_movementController._velocity.X)); 
        // Pass raw Y velocity for blending jump up/fall down in Airborne blend space.
        playerAnimationTree.SetAirborneBlend(_movementController._velocity.Y);


        // --- Control Locomotion Animation States ---
        // These `ChangeLocomotionState` calls primarily manage which high-level state (GroundMovement, Airborne, Dash, etc.)
        // the locomotion state machine is in. Blending within these states is handled by the blend parameters.
        switch (currentState)
        {
            case PlayerState.Dashing:
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Dash)
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Dash);
                }
                break;

            case PlayerState.Jumping:
            case PlayerState.Falling: // Both map to Airborne locomotion animation state
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Airborne)
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Airborne);
                }
                break;
            case PlayerState.Normal:
                // Only travel to GroundMovement if not already in an idle/run state
                // This prevents re-traveling if the blend position is simply changing.
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Idle &&
                    playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Run)
                {
                     playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Idle); 
                }
                break;
            case PlayerState.Tumble:
                // If you have a specific Tumble animation state, use it here.
                // Otherwise, Airborne is a reasonable fallback.
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Airborne) 
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Airborne); 
                }
                break;
            case PlayerState.WallStick:
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.WallSlide)
                {              
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.WallSlide);
                }
                break;
            default:
                GD.PushWarning($"Unhandled PlayerState for Locomotion Animation: {currentState}");
                break;
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        _movementController.HandleMovement(delta);
    }

    public void StartAttack(float duration, float lockout = 0.5f)
    {
        GD.Print($"[Player] StartAttack called. Duration: {duration}, Lockout: {lockout}"); // <-- ADD/VERIFY THIS
        _currentActionState = ActionState.Attacking;
        _actionTimer = duration;

        var currentAttack = _attackManager.GetCurrentAttack();
        if (currentAttack != null)
        {
            _movementController.IsMovementLocked = !currentAttack.AllowMovement;
            GD.Print($"[Player] Attack '{currentAttack.Name}'. AllowMovement: {currentAttack.AllowMovement}. IsMovementLocked set to: {_movementController.IsMovementLocked}"); // <-- ADD/VERIFY THIS
        }
        else
        {
            _movementController.IsMovementLocked = true;
            GD.Print("[Player] Attack data not found, defaulting to IsMovementLocked = true."); // <-- ADD/VERIFY THIS
        }

        Velocity = Vector3.Zero;
        GetTree().CreateTimer(lockout).Timeout += OnAttackLockoutEnd;
    }

    public void StartCasting(float duration, bool allowMovement, string castingAnimationName = "Casting")
    {
        GD.Print($"[Player] StartCasting called. Duration: {duration}, AllowMovement: {allowMovement}, Animation: {castingAnimationName}");
        _currentActionState = ActionState.Casting;
        _castingDuration = duration;
        _actionTimer = duration; // Use action timer for casting duration
        _allowMovementDuringCast = allowMovement;

        // Set movement lock based on card property
        _movementController.IsMovementLocked = !_allowMovementDuringCast;

        // Play casting animation
        PlayerAnimationTree.ActionAnimationState actionAnimState;
        if (Enum.TryParse(castingAnimationName, out actionAnimState))
        {
            playerAnimationTree.ChangeActionState(actionAnimState);
            GD.Print($"[Player] StartCasting: Parsed animation '{castingAnimationName}' to {actionAnimState}. Calling ChangeActionState.");
        }
        else
        {
            GD.PushWarning($"[Player] StartCasting: Could not parse animation '{castingAnimationName}' to ActionAnimationState. Falling back to Casting animation.");
            playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.Casting); // Fallback
        }
        // No explicit timer for EndCasting here, _Process will call EndActionState when _actionTimer runs out
    }


    private void OnAttackLockoutEnd()
    {
        GD.Print("[Player] OnAttackLockoutEnd called."); // <-- ADD/VERIFY THIS
        if (_currentActionState == ActionState.Attacking)
        {
            _movementController.IsMovementLocked = false;
            GD.Print("[Player] OnAttackLockoutEnd: IsMovementLocked set to false."); // <-- ADD/VERIFY THIS
        }
    }

    private void EndActionState()
    {
        GD.Print("[Player] EndActionState called.");
        ActionState completedAction = _currentActionState; // Store current action before resetting
        _currentActionState = ActionState.None;
        _movementController.IsMovementLocked = false;
        playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.None); // Reset upper body animation to idle
        GD.Print("[Player] EndActionState: IsMovementLocked=false, ActionState=None, ChangeActionState(None) called.");

        // NEW: If a cast just finished, trigger the card's effect after casting
        if (completedAction == ActionState.Casting)
        {
            GD.Print("[Player] Casting finished. Triggering card effect.");
            // Here is where you'd trigger the actual card effect.
            // You'll need to pass the relevant card data to StartCasting and store it.
            // For now, this is a placeholder for where that logic will go.
        }
    }

  private void OnAttack()
    {
        GD.Print("[Player] OnAttack event triggered. Checking if allowed to attack."); // <-- ADD/VERIFY THIS
        var weapon = _weaponManager.GetCurrentWeapon();
        if (weapon == null || weapon.AttackIds.Length == 0)
        {
            GD.PrintErr("[Player] OnAttack: No weapon or attacks found. Returning."); // <-- ADD/VERIFY THIS
            return;
        }

        int attackId = weapon.AttackIds[0];
        GD.Print($"[Player] OnAttack: Attack ID: {attackId}. Current action state: {_currentActionState}."); // <-- ADD/VERIFY THIS

        if (_currentActionState == ActionState.None)
        {
            _attackManager.SetAttackById(attackId);
            var attack = _attackManager.GetCurrentAttack();
            if (attack == null)
            {
                GD.PrintErr("[Player] OnAttack: Attack data not found for ID " + attackId + ". Returning."); // <-- ADD/VERIFY THIS
                return;
            }

            GD.Print($"[Player] OnAttack: Starting attack: {attack.Name}, Animation: '{attack.Animation}'"); // <-- ADD/VERIFY THIS
            _attackManager.PerformAttack();
            StartAttack(attack.Duration, attack.Lockout);

            // Determine the correct animation based on player's locomotion state
            string animationToPlay = attack.Animation;
            if (_movementController.CurrentState == PlayerState.Jumping || _movementController.CurrentState == PlayerState.Falling)
            {
                // Only apply jumping attack animation override if it's a melee attack
                // Projectile attacks might have their own specific airborne animation handled by 'Projectile' ActionAnimationState
                if (string.IsNullOrEmpty(attack.ProjectileScene))
                {
                    animationToPlay = "JumpingAttack";
                    GD.Print("[Player] OnAttack: Overriding animation to 'JumpingAttack' due to airborne state.");
                }
            }

            PlayerAnimationTree.ActionAnimationState actionAnimState;
            if (Enum.TryParse(animationToPlay, out actionAnimState))
            {
                playerAnimationTree.ChangeActionState(actionAnimState);
                GD.Print($"[Player] OnAttack: Parsed animation '{animationToPlay}' to {actionAnimState}. Calling ChangeActionState.");
            }
            else
            {
                GD.PushWarning($"[Player] OnAttack: Could not parse animation '{animationToPlay}' to ActionAnimationState. Falling back to Attack animation.");
                playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.Attack);
            }
        }
        else
        {
            GD.Print($"[Player] OnAttack: Player is already in action state '{_currentActionState}'. Cannot start new attack."); // <-- ADD/VERIFY THIS
        }
    }

    public void SpawnAttackHitbox()
    {
        GD.Print("SpawnAttackHitbox called from animation"); // <-- ADD/VERIFY THIS
        _attackManager.ActivateHitbox();
    }


    public void SpawnAttackProjectile()
    {
        GD.Print("SpawnAttackProjectile called from animation event!");
        _attackManager.FireProjectile();
    }

    public void SpawnAttackHitbox(string configId)
    {
        GD.Print($"[Player] SpawnAttackHitbox called with configId: '{configId}'");
        
        var hitboxData = HitboxConfigLoader.LoadHitboxConfig(configId);
        
        var hitboxComponent = new HitboxComponent();
        AddChild(hitboxComponent);
        hitboxComponent.Initialize(hitboxData, "Enemy");
        
        GD.Print($"[Player] ✓ Spawned attack hitbox for '{configId}'");
    }

    private void ForceCancelAttack()
    {
        if (_currentActionState == ActionState.Attacking)
        {
            _attackManager.CancelAttack(0.08f);
            playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Idle); // Reset locomotion
            playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.None); // Reset action
            EndActionState();
        }
    }

    public bool CanMove()
    {
        return _currentActionState == ActionState.None;
    }

    public void EquipWeapon(int weaponId)
    {
        _weaponManager.EquipWeaponById(weaponId);

        var weapon = _weaponManager.GetCurrentWeapon();
        if (weapon != null && weapon.AttackIds.Length > 0)
        {
            _attackManager.SetAttackById(weapon.AttackIds[0]);
        }
    }

    public void ExecuteCardAttack(string attackName)
    {
        if (_attackManager != null)
        {
            GD.Print($"Player: Triggering card attack by name: {attackName}");
            _attackManager.SetAttackByName(attackName);
            var attack = _attackManager.GetCurrentAttack(); // Get attack data for duration/movement lock

            if (_currentActionState == ActionState.None && attack != null)
            {
                // *** NEW: Start casting instead of directly performing attack ***
                // For now, use attack.Duration and attack.AllowMovement as placeholders
                StartCasting(attack.Duration, attack.AllowMovement, "Casting"); // "Casting" is the default animation for now
                
                // If it's an instant melee attack that doesn't need casting,
                // you might still call _attackManager.PerformAttack() and spawn hitbox here.
                // For now, we assume cards initiate a cast.
            }
            else
            {
                GD.Print($"Player: Cannot start card attack, current action state: {_currentActionState}");
            }
        }
    }

    public void ExecuteCardAttack(int attackId)
    {
        if (_attackManager != null)
        {
            GD.Print($"Player: Triggering card attack by ID: {attackId}");
            _attackManager.SetAttackById(attackId);
            var attack = _attackManager.GetCurrentAttack(); // Get attack data for duration/movement lock

            if (_currentActionState == ActionState.None && attack != null)
            {
                // *** NEW: Start casting instead of directly performing attack ***
                StartCasting(attack.Duration, attack.AllowMovement, "Casting"); // "Casting" is the default animation for now

                // If it's an instant melee attack that doesn't need casting,
                // you might still call _attackManager.PerformAttack() and spawn hitbox here.
                // For now, we assume cards initiate a cast.
            }
            else
            {
                GD.Print($"Player: Cannot start card attack, current action state: {_currentActionState}");
            }
        }
    }
}