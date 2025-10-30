using Godot;

public partial class MageAnimationTree : EnemyAnimationTree
{
    public enum MageAnimationState
    {
        // Base states (inherited)
        Idle = 0,
        Walking = 1,
        Knockback = 2,
        Falling = 3,
        Dead = 4,
        Attack = 5,
        // Mage-specific states
        Casting = 6,
        SpellAttack = 7
    }

    public override void ChangeState(EnemyAnimationState newState)
    {
        // Handle mage-specific states
        if (newState == (EnemyAnimationState)MageAnimationState.Casting)
        {
            _currentState = newState;
            _playback.Travel("Casting");
            GD.Print("[MageAnimationTree] Changed to Casting");
            return;
        }

        if (newState == (EnemyAnimationState)MageAnimationState.SpellAttack)
        {
            _currentState = newState;
            _playback.Travel("SpellAttack");
            GD.Print("[MageAnimationTree] Changed to SpellAttack");
            return;
        }

        // Fallback to base class for common states
        base.ChangeState(newState);
    }

    public override void PlayAttackAnimation(string animationName)
    {
        if (string.IsNullOrEmpty(animationName))
        {
            GD.PushWarning("[MageAnimationTree] Animation name is empty!");
            return;
        }

        GD.Print($"[MageAnimationTree] Playing spell attack: {animationName}");
        
        // Travel to the specific spell animation
        _playback.Travel(animationName);
        _currentState = (EnemyAnimationState)MageAnimationState.SpellAttack;
    }

    public bool IsCastingOrAttacking()
    {
        return _currentState == (EnemyAnimationState)MageAnimationState.Casting || 
               _currentState == (EnemyAnimationState)MageAnimationState.SpellAttack;
    }


    //WIP
    //  public void PlayCastingAnimation()
    // {
    //     GD.Print("[MageAnimationTree] Playing casting animation");
    //     _playback.Travel("Casting");
    //     _currentState = (EnemyAnimationState)MageAnimationState.Casting;
    // }

    // /// <summary>
    // /// Returns true if the mage is currently casting or attacking
    // /// </summary>
    // public bool IsCastingOrAttacking()
    // {
    //     return _currentState == (EnemyAnimationState)MageAnimationState.Casting || 
    //            _currentState == (EnemyAnimationState)MageAnimationState.SpellAttack;
    // }

    // /// <summary>
    // /// Gets the current animation play position (0.0 to 1.0)
    // /// Useful for timing projectile spawns during animation
    // /// </summary>
    // public float GetAnimationProgress()
    // {
    //     if (_playback != null)
    //     {
    //         var currentAnimationLength = Get("anim_player").Call("get_current_animation_length");
    //         var currentAnimationPosition = Get("anim_player").Call("get_current_animation_position");

    //         if ((float)currentAnimationLength > 0)
    //         {
    //             return (float)currentAnimationPosition / (float)currentAnimationLength;
    //         }
    //     }
    //     return 0f;
    // }


}