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
        SpellAttack = 7,
        MeleeAttack = 8
    }

    public override void ChangeState(EnemyAnimationState newState)
    {
        // Handle mage-specific states
        if (newState == (EnemyAnimationState)MageAnimationState.MeleeAttack)
        {
            _currentState = newState;
            _playback.Travel("Melee");
            GD.Print("[MageAnimationTree] Changed to MeleeAttack");
            return;
        }

        if (newState == (EnemyAnimationState)MageAnimationState.SpellAttack)
        {
            _currentState = newState;
            _playback.Travel("Casting");
            GD.Print("[MageAnimationTree] Changed to Casting");
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

        GD.Print($"[MageAnimationTree] Playing attack animation: {animationName}");
        
        // Map animation name to AnimationTree node name based on attack type
        string nodeToTravel = DetermineAnimationNode(animationName);
        
        GD.Print($"[MageAnimationTree]   Animation: {animationName} → Node: {nodeToTravel}");
        
        // Travel to the appropriate AnimationTree node
        _playback.Travel(nodeToTravel);
        _currentState = (EnemyAnimationState)MageAnimationState.SpellAttack;
    }

    /// <summary>
    /// Maps attack animation names to AnimationTree node names
    /// Since the JSON already uses node names (Casting, Melee), just pass through
    /// </summary>
    private string DetermineAnimationNode(string animationName)
    {
        // The animationName from JSON is already the node name (Casting or Melee)
        // Just validate it's one we expect
        if (animationName == "Melee" || animationName == "Casting")
        {
            return animationName;
        }
        
        // Fallback for old animation names (if any slip through)
        if (animationName == "staff_strike")
        {
            return "Melee";
        }
        
        if (animationName == "fireball")
        {
            return "Casting";
        }
        
        // Default to Casting for any unknown spell-like attacks
        GD.PushWarning($"[MageAnimationTree] Unknown animation '{animationName}', defaulting to Casting");
        return "Casting";
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