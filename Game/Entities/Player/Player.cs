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
    // REMOVED: private PlayerState _stateBeforeLock; // No longer needed
    public bool IsFacingRight() => _movementController._faceRight;

    public override void _Ready()
    {
        _movementController.Initialize(this);
        _weaponManager = new WeaponManager(this);
        _weaponManager.EquipWeaponById(1);  //testing github syncing
        
        _attackManager = new AttackManager(this, _weaponManager, _weaponManager.GetCurrentWeapon().AttackIds[0]);

        
        if (InputManager.Instance != null)
        {
            InputManager.Instance.Attack += OnAttack;
            //InputManager.Instance.ProjectileAttack += OnProjectileAttack;
        }
    }

    public override void _Process(double delta)
    {
        // Called every frame. Delta is time since the last frame.
        // Update game logic here.

        // Get the current movement state
        var currentState = _movementController.CurrentState;

        // REMOVED: Old attack-canceling logic based on movement state.
        // This is now handled by MovementController.IsMovementLocked and AttackData.AllowMovement.

        // Handle action state first (upper body animation control)
        if (_currentActionState != ActionState.None)
        {
            _actionTimer -= (float)delta;
            if (_actionTimer <= 0f)
            {
                EndActionState();
            }
            // REMOVED: 'return;' statement. Locomotion should still update if movement is allowed.
        }

        // Always update locomotion animation blends, even if an action is happening
        playerAnimationTree.SetGroundBlend(Mathf.Abs(Velocity.X));
        playerAnimationTree.SetAirborneBlend(Velocity.Y);


        // Control locomotion animations based on movement controller state
        switch (currentState)
        {
            case PlayerState.Dashing:
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Dash)
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Dash);
                }
                break;

            case PlayerState.Jumping:
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Airborne)
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Airborne);
                }
                break;
            case PlayerState.Normal:
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Idle) 
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Idle); 
                }
                break;
            case PlayerState.Falling:
                if (playerAnimationTree.CurrentLocomotionState != PlayerAnimationTree.LocomotionAnimationState.Airborne)
                {
                    playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Airborne);
                }
                break;
            case PlayerState.Tumble:
                // Consider adding a specific LocomotionAnimationState.Tumble if you have one
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
                // Handle other movement states
                break;
        }

    }

    public override void _PhysicsProcess(double delta)
    {
        _movementController.HandleMovement(delta);
    }

    public void StartAttack(float duration, float lockout = 0.5f)
    {
        _currentActionState = ActionState.Attacking;
        _actionTimer = duration;

        // Set IsMovementLocked based on the attack data (already correct from previous step)
        var currentAttack = _attackManager.GetCurrentAttack();
        if (currentAttack != null)
        {
            _movementController.IsMovementLocked = !currentAttack.AllowMovement;
        }
        else
        {
            _movementController.IsMovementLocked = true;
        }

        Velocity = Vector3.Zero;
        GetTree().CreateTimer(lockout).Timeout += OnAttackLockoutEnd;

        // Animation change is handled in OnAttack()
    }

    private void OnAttackLockoutEnd()
    {
        if (_currentActionState == ActionState.Attacking)
        {
            _movementController.IsMovementLocked = false;
            // REMOVED: Old state transition logic. Locomotion state machine handles transitions naturally.
        }
    }


    private void EndActionState()
    {
        _currentActionState = ActionState.None;
        _movementController.IsMovementLocked = false;
        playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.None); // Reset upper body animation to idle
    }

    private void OnAttack()
    {
        GD.Print("OnAttack called");
        var weapon = _weaponManager.GetCurrentWeapon();
        if (weapon == null || weapon.AttackIds.Length == 0)
            return;

        int attackId = weapon.AttackIds[0];

        GD.Print($"OnAttack called. Current attack ID: {attackId}");
        if (_currentActionState == ActionState.None)
        {
            _attackManager.SetAttackById(attackId);
            var attack = _attackManager.GetCurrentAttack();
            if (attack == null)
                return;

            _attackManager.PerformAttack();
            StartAttack(attack.Duration, attack.Lockout);

            // Use ChangeActionState for attack animations
            PlayerAnimationTree.ActionAnimationState actionAnimState;
            if (Enum.TryParse(attack.Animation, out actionAnimState))
            {
                playerAnimationTree.ChangeActionState(actionAnimState);
            }
            else
            {
                GD.PushWarning($"Could not find ActionAnimationState for: {attack.Animation}. Falling back to Attack animation.");
                playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.Attack); // Fallback
            }
        }
    }

    public void SpawnAttackHitbox()
    {
        GD.Print("SpawnAttackHitbox called from animation");
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
            _attackManager.CancelAttack(0.08f); // e.g., 0.08 seconds linger
            playerAnimationTree.ChangeLocomotionState(PlayerAnimationTree.LocomotionAnimationState.Idle); // Reset locomotion
            playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.None); // Reset action
            EndActionState();
        }
    }

    public bool CanMove()
    {
        // Only allow movement if not attacking or casting (or add other states as needed)
        // Note: The IsMovementLocked flag in MovementController is the primary control for movement input.
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
            _attackManager.PerformAttack();
            
            // If it's a projectile attack, your AttackManager will know what to do!
            // If it's a melee sweep, you can also trigger the animation/hitbox here:
            // _attackManager.ActivateHitbox(); // ActivateHitbox is typically called by an animation event
            
            // Trigger action animation
            var attack = _attackManager.GetCurrentAttack();
            if (attack != null)
            {
                PlayerAnimationTree.ActionAnimationState actionAnimState;
                if (Enum.TryParse(attack.Animation, out actionAnimState))
                {
                    playerAnimationTree.ChangeActionState(actionAnimState);
                }
                else
                {
                    GD.PushWarning($"Could not find ActionAnimationState for: {attack.Animation}. Falling back to Attack animation.");
                    playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.Attack); // Fallback
                }
            }
        }
    }

    public void ExecuteCardAttack(int attackId)
    {
        if (_attackManager != null)
        {
            GD.Print($"Player: Triggering card attack by ID: {attackId}");
            _attackManager.SetAttackById(attackId);
            _attackManager.PerformAttack();
            // _attackManager.ActivateHitbox(); // ActivateHitbox is typically called by an animation event

            // Trigger action animation
            var attack = _attackManager.GetCurrentAttack();
            if (attack != null)
            {
                PlayerAnimationTree.ActionAnimationState actionAnimState;
                if (Enum.TryParse(attack.Animation, out actionAnimState))
                {
                    playerAnimationTree.ChangeActionState(actionAnimState);
                }
                else
                {
                    GD.PushWarning($"Could not find ActionAnimationState for: {attack.Animation}. Falling back to Attack animation.");
                    playerAnimationTree.ChangeActionState(PlayerAnimationTree.ActionAnimationState.Attack); // Fallback
                }
            }
        }
    }
}