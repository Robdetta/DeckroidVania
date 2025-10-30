using Godot;

public partial class KnightAnimationTree : EnemyAnimationTree
{
    public enum KnightAnimationState
    {
        // Base states (inherited)
        Idle = 0,
        Walking = 1,
        Running = 2,
        Knockback = 3,
        Falling = 4,
        Dead = 5,
                
        // Knight-specific states
        Shield = 6,
        Attack = 7
    }

    public override void ChangeState(EnemyAnimationState newState)
    {
        // Handle knight-specific states
        if (newState == (EnemyAnimationState)KnightAnimationState.Shield)
        {
            _currentState = newState;
            _playback.Travel("Shield");
            GD.Print("[KnightAnimationTree] Changed to Shield");
            return;
        }

        if (newState == (EnemyAnimationState)KnightAnimationState.Attack)
        {
            _currentState = newState;
            _playback.Travel("Attack");
            GD.Print("[KnightAnimationTree] Changed to Attack");
            return;
        }

        // Fallback to base class for common states
        base.ChangeState(newState);
    }
}