using Godot;
using System;

public partial class PlayerAnimationTree : AnimationTree
{
    public enum LocomotionAnimationState
    {
        Idle, Run, Airborne, Dash, WallSlide, Dead
    }

    public enum ActionAnimationState
    {
        None, Attack, Projectile, JumpingAttack
    }

    private AnimationNodeStateMachinePlayback _locomotionPlayback;
    private AnimationNodeStateMachinePlayback _actionPlayback;

    private LocomotionAnimationState _currentLocomotionState = LocomotionAnimationState.Idle;
    private ActionAnimationState _currentActionState = ActionAnimationState.None;


    public override void _Ready()
    {
        GD.Print("[PlayerAnimationTree] Ready called."); // <-- ADD/VERIFY THIS
        _locomotionPlayback = (AnimationNodeStateMachinePlayback)Get("parameters/Locomotion/playback");
        _actionPlayback = (AnimationNodeStateMachinePlayback)Get("parameters/Actions/playback");
        Active = true;
        GD.Print($"[PlayerAnimationTree] Locomotion Playback valid: {_locomotionPlayback != null}"); // <-- ADD/VERIFY THIS
        GD.Print($"[PlayerAnimationTree] Action Playback valid: {_actionPlayback != null}");     // <-- ADD/VERIFY THIS
    }

    public LocomotionAnimationState CurrentLocomotionState => _currentLocomotionState;
    public ActionAnimationState CurrentActionState => _currentActionState;

    public void ChangeLocomotionState(LocomotionAnimationState newState)
    {
        if (_currentLocomotionState == newState) return;
        _currentLocomotionState = newState;

        switch (newState)
        {
            case LocomotionAnimationState.Idle:
            case LocomotionAnimationState.Run:
                _locomotionPlayback.Travel("GroundMovement");
                break;
            case LocomotionAnimationState.Airborne:
                _locomotionPlayback.Travel("Airborne");
                break;
            case LocomotionAnimationState.Dash:
                _locomotionPlayback.Travel("Dash");
                break;
            case LocomotionAnimationState.WallSlide:
                _locomotionPlayback.Travel("WallSlide");
                break;
            case LocomotionAnimationState.Dead:
                _locomotionPlayback.Travel("Dead");
                break;
        }
    }

    public void ChangeActionState(ActionAnimationState newState)
    {
        if (_currentActionState == newState) return;
        _currentActionState = newState;

        GD.Print($"[PlayerAnimationTree] Changing Action State to: {newState}"); // <-- ADD/VERIFY THIS

        switch (newState)
        {
            case ActionAnimationState.None:
                _actionPlayback.Travel("Idle");
                GD.Print($"[PlayerAnimationTree] Actions: Travel to 'Idle'."); // <-- ADD/VERIFY THIS
                break;
            case ActionAnimationState.Attack:
                _actionPlayback.Travel("Attack");
                GD.Print($"[PlayerAnimationTree] Actions: Travel to 'Attack'."); // <-- ADD/VERIFY THIS
                break;
            case ActionAnimationState.Projectile:
                _actionPlayback.Travel("Projectile");
                GD.Print($"[PlayerAnimationTree] Actions: Travel to 'Projectile'."); // <-- ADD/VERIFY THIS
                break;            
            case ActionAnimationState.JumpingAttack: // NEW: Handle Jumping Attack
                _actionPlayback.Travel("JumpingAttack");
                GD.Print($"[PlayerAnimationTree] Actions: Travel to 'JumpingAttack'.");
                break;        
        }
    }

    public void SetGroundBlend(float blendValue) {
        Set("parameters/Locomotion/GroundMovement/blend_position", blendValue);
    }

    public void SetAirborneBlend(float blendValue) {
        Set("parameters/Locomotion/Airborne/blend_position", blendValue);
    }

    public void SetAttackBlend(float blendValue) {
        Set("parameters/Actions/Attack/blend_position", blendValue);
    }
}
