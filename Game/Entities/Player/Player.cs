using DeckroidVania2.Game.Player.PlayerStates;
using DeckroidVania2.Game.Scripts.Inputs;
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
    private PlayerAnimationTree playerAnimationTree;
    [Export]
    private MovementController _movementController;
    [Export]
    public float AttackLockoutDuration { get; set; } = 0.5f;
    private AttackManager _attackManager;
    private WeaponManager _weaponManager;
    private ActionState _currentActionState = ActionState.None;
    private float _actionTimer = 0f;
    private PlayerState _stateBeforeLock;
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
        //animationTree.Set("parameters/StateMachine/GroundMovement/blend_position", Mathf.Abs(Velocity.X));


        // Get the current movement state
        var currentState = _movementController.CurrentState;

        if (_currentActionState == ActionState.Attacking)
        {
            switch (currentState)
            {
                case PlayerState.Dashing:
                case PlayerState.Jumping:
                case PlayerState.Falling:
                case PlayerState.Tumble:
                case PlayerState.Normal:
                    if (Mathf.Abs(Velocity.X) > 0.01f || Mathf.Abs(Velocity.Z) > 0.01f)
                        ForceCancelAttack();
                    break;
            }
        }

        // Handle action state first
        if (_currentActionState != ActionState.None)
        {
            _actionTimer -= (float)delta;
            if (_actionTimer <= 0f)
            {
                EndActionState();
            }
            // Play attack/cast animation, block movement animation changes
            return;
        }

        playerAnimationTree.SetGroundBlend(Mathf.Abs(Velocity.X));
        playerAnimationTree.SetAirborneBlend(Velocity.Y);


        switch (currentState)
        {
            case PlayerState.Dashing:
                if (playerAnimationTree.CurrentState != PlayerAnimationTree.AnimationState.Dash)
                {
                    playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Dash);
                }
                break;

            case PlayerState.Jumping:
                if (playerAnimationTree.CurrentState != PlayerAnimationTree.AnimationState.Airborne)
                {
                    playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Airborne);
                }
                break;
            case PlayerState.Normal:
                if (playerAnimationTree.CurrentState != PlayerAnimationTree.AnimationState.Normal)
                {
                    playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Normal);
                }
                break;
            case PlayerState.Falling:
                if (playerAnimationTree.CurrentState != PlayerAnimationTree.AnimationState.Airborne)
                {
                    playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Airborne);
                }
                break;
            case PlayerState.Tumble:
                if (playerAnimationTree.CurrentState != PlayerAnimationTree.AnimationState.Airborne)
                {
                    //TODO: Add animation state for 'Tumble' or some form of substitution
                    //playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Tumble);
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
        _stateBeforeLock = _movementController.CurrentState;
        _currentActionState = ActionState.Attacking;
        _actionTimer = duration;
        _movementController.ChangeState(PlayerState.Locked);

        Velocity = Vector3.Zero;
        GetTree().CreateTimer(lockout).Timeout += OnAttackLockoutEnd;

        //playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Attack);
        // Optionally: disable movement input here
    }

    private void OnAttackLockoutEnd()
    {
        // After lockout, allow canceling by movement/jump/dash
        if (_currentActionState == ActionState.Attacking)
        {
            // Optionally: transition to Attacking state or back to previous state
            if (_stateBeforeLock == PlayerState.Falling || _stateBeforeLock == PlayerState.Jumping || _stateBeforeLock == PlayerState.Falling)
                _movementController.ChangeState(PlayerState.Falling);
            else
                _movementController.ChangeState(PlayerState.Normal);
        }
    }

    private void EndActionState()
    {
        _currentActionState = ActionState.None;
        // Optionally: re-enable movement input here
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

            playerAnimationTree.ChangeState((PlayerAnimationTree.AnimationState)Enum.Parse(
                typeof(PlayerAnimationTree.AnimationState), attack.Animation));
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

    private void ForceCancelAttack()
    {
        if (_currentActionState == ActionState.Attacking)
        {
            _attackManager.CancelAttack(0.08f); // e.g., 0.08 seconds linger
            playerAnimationTree.ChangeState(PlayerAnimationTree.AnimationState.Normal); // or Idle
            EndActionState();
        }
    }

    public bool CanMove()
    {
        // Only allow movement if not attacking or casting (or add other states as needed)
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
}
