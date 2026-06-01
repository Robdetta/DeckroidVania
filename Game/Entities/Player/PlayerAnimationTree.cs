using Godot;
using System;

public partial class PlayerAnimationTree : AnimationTree
{
	// Called when the node enters the scene tree for the first time.

	// public enum AnimationState
	// {
	// 	Normal = 0,
	// 	Airborne = 1,
	// 	Dead = 2,
	// 	Dash = 3,
	// 	Run = 4,
	// 	Attack = 5,
	// 	Projectile = 6,
	// 	WallSlide = 7
	// }

	// New enums to clearly separate locomotion and action animation states
    public enum LocomotionAnimationState
    {
        Idle,      // Corresponds to "GroundMovement" or a general idle
        Run,       // Corresponds to "Run"
        Airborne,  // Corresponds to "Airborne" (Jump/Fall)
        Dash,      // Corresponds to "Dash"
        WallSlide, // Corresponds to "WallSlide"
        Dead       // If death is a locomotion override
    }

    public enum ActionAnimationState
    {
        None,      // A "neutral" upper body state when no action is happening
        Attack,    // Corresponds to "Attack"
        Projectile // Corresponds to "Projectile"
    }

    private AnimationNodeStateMachinePlayback _locomotionPlayback;
    private AnimationNodeStateMachinePlayback _actionPlayback;

	// Keep track of current states for comparison
    private LocomotionAnimationState _currentLocomotionState = LocomotionAnimationState.Idle;
    private ActionAnimationState _currentActionState = ActionAnimationState.None;

    public override void _Ready()
    {
        GD.Print("PlayerAnimationTree ready");
        // Get references to the playback objects for both state machines
        // The paths correspond to the names of the AnimationNodeStateMachine nodes in your BlendTree
        _locomotionPlayback = (AnimationNodeStateMachinePlayback)Get("parameters/Locomotion/playback");
        _actionPlayback = (AnimationNodeStateMachinePlayback)Get("parameters/Actions/playback");
        Active = true;
    }

    public LocomotionAnimationState CurrentLocomotionState => _currentLocomotionState;
    public ActionAnimationState CurrentActionState => _currentActionState;

    // Method to change locomotion animations
    public void ChangeLocomotionState(LocomotionAnimationState newState)
    {
        if (_currentLocomotionState == newState) return;

        _currentLocomotionState = newState;
        GD.Print($"Changing Locomotion State to: {newState}");

        switch (newState)
        {
            case LocomotionAnimationState.Idle:
                _locomotionPlayback.Travel("GroundMovement"); // Assuming "GroundMovement" handles idle/walk
                break;
            case LocomotionAnimationState.Run:
                _locomotionPlayback.Travel("Run");
                break;
            case LocomotionAnimationState.Airborne:
                _locomotionPlayback.Travel("Airborne"); // Covers jumping and falling
                break;
            case LocomotionAnimationState.Dash:
                _locomotionPlayback.Travel("Dash");
                break;
            case LocomotionAnimationState.WallSlide:
                _locomotionPlayback.Travel("WallSlide");
                break;
            case LocomotionAnimationState.Dead:
                _locomotionPlayback.Travel("Dead"); // Death might be a global override
                break;
            default:
                GD.PushWarning($"Unhandled LocomotionAnimationState: {newState}");
                break;
        }
    }

    // Method to change action animations (upper body)
    public void ChangeActionState(ActionAnimationState newState)
    {
        if (_currentActionState == newState) return;

        _currentActionState = newState;
        // GD.Print($"Changing Action State to: {newState}"); // Keep for debugging if needed

        switch (newState)
        {
            case ActionAnimationState.None:
                // When action ends, ensure flags are reset for transitions
                Set("parameters/Actions/action_is_attacking", false);
                Set("parameters/Actions/action_is_casting_projectile", false);
                _actionPlayback.Travel("Idle"); // Back to upper body idle
                break;
            case ActionAnimationState.Attack:
                // Set flag to true to trigger attack animation transition
                Set("parameters/Actions/action_is_attacking", true);
                Set("parameters/Actions/action_is_casting_projectile", false); // Ensure other actions are off
                _actionPlayback.Travel("Attack");
                break;
            case ActionAnimationState.Projectile:
                // Set flag to true to trigger projectile animation transition
                Set("parameters/Actions/action_is_attacking", false); // Ensure other actions are off
                Set("parameters/Actions/action_is_casting_projectile", true);
                _actionPlayback.Travel("Projectile");
                break;
            default:
                GD.PushWarning($"Unhandled ActionAnimationState: {newState}");
                break;
        }
    }

	public void SetGroundBlend(float blendValue){
		Set("parameters/Locomotion/GroundMovement/blend_position", blendValue);
	}

	public void SetAirborneBlend(float blendValue){
		Set("parameters/Airborne/AirMovement/blend_position", blendValue);
	}

	public void SetDashBlend(float blendValue){
		Set("parameters/StateMachine/Dash/blend_position", blendValue);
	}

	public void SetAttackBlend(float blendValue){
		Set("parameters/Actions/Attack/blend_position", blendValue);
	}

}
