using Godot;
using System;

public partial class EnemyAnimationTree : AnimationTree
{
    public enum EnemyAnimationState
    {
        Idle = 0,
        Walking = 1,
        Running = 2,
        Knockback = 3,
        Falling = 4,
        Dead = 5,
        Attack = 6
    }

    protected EnemyAnimationState _currentState = EnemyAnimationState.Idle;
    protected AnimationNodeStateMachinePlayback _playback;

    public override void _Ready()
    {
        GD.Print("[EnemyAnimationTree] Ready");
        _playback = (AnimationNodeStateMachinePlayback)Get("parameters/StateMachine/playback");
        Active = true;
    }

    public EnemyAnimationState CurrentState => _currentState;

    public virtual void ChangeState(EnemyAnimationState newState)
    {
        if (_currentState == newState) return;

        _currentState = newState;

        switch (_currentState)
        {
            case EnemyAnimationState.Idle:
                _playback.Travel("Idle");
                GD.Print("[EnemyAnimationTree] Changed to Idle");
                break;
            case EnemyAnimationState.Walking:
                _playback.Travel("Walking");
                GD.Print("[EnemyAnimationTree] Changed to Walk");
                break;
            case EnemyAnimationState.Running:
                _playback.Travel("Running");
                GD.Print("[EnemyAnimationTree] Changed to Run");
                break;
            case EnemyAnimationState.Knockback:
                _playback.Travel("Knockback");
                GD.Print("[EnemyAnimationTree] Changed to Knockback");
                break;
            case EnemyAnimationState.Falling:
                _playback.Travel("Falling");
                GD.Print("[EnemyAnimationTree] Changed to Falling");
                break;
            case EnemyAnimationState.Dead:
                _playback.Travel("Dead");
                GD.Print("[EnemyAnimationTree] Changed to Dead");
                break;
            case EnemyAnimationState.Attack:
                _playback.Travel("Attack");
                GD.Print("[EnemyAnimationTree] Changed to Attack");
                break;
            
        }
    }

    public virtual void PlayAttackAnimation(string animationName)
    {
        if (string.IsNullOrEmpty(animationName))
        {
            GD.PushWarning("[EnemyAnimationTree] Animation name is empty!");
            return;
        }

        GD.Print($"[EnemyAnimationTree] Playing attack animation: {animationName}");
        
        // Travel to the specific animation state
        _playback.Travel(animationName);
        _currentState = EnemyAnimationState.Attack;
    }

    /// <summary>
    /// Set blend position for walk direction (left/right).
    /// blendValue: -1 = left, 1 = right
    /// </summary>
    public void SetWalkingBlend(float blendValue)
    {
        Set("parameters/Walking/blend_position", blendValue);
    }

    /// <summary>
    /// Set blend position for run direction (left/right).
    /// blendValue: -1 = left, 1 = right
    /// </summary>
    public void SetRunBlend(float blendValue)
    {
        Set("parameters/Runing/blend_position", blendValue);
    }
}   