using Godot;
using System;

public partial class PlayerAnimationTree : AnimationTree
{
	// Called when the node enters the scene tree for the first time.

	public enum AnimationState
	{
		Normal = 0,
		Airborne = 1,
		Dead = 2,
		Dash = 3,
		Run = 4,
		Attack = 5,
		Projectile = 6
	}

	private AnimationState currentState = AnimationState.Normal;
	private AnimationNodeStateMachinePlayback playback;

	public override void _Ready()
	{
		GD.Print("PlayerAnimationTree ready");
		playback = (AnimationNodeStateMachinePlayback)Get("parameters/StateMachine/playback");
		Active = true;
	}

	public AnimationState CurrentState
	{
		get { return currentState; }
	}

	public void ChangeState(AnimationState newState)
	{	
		if (currentState == newState) return;

		currentState = newState;

		switch (currentState)
		{
			case AnimationState.Normal:
				playback.Travel("GroundMovement");
				break;
			case AnimationState.Airborne:
				playback.Travel("Airborne");
				break;
			case AnimationState.Dead:
				playback.Travel("Dead");
				break;
			case AnimationState.Dash:
				playback.Travel("Dash");
				break;
			case AnimationState.Run:
				playback.Travel("Run");
				break;
			case AnimationState.Attack:
				playback.Travel("Attack");
				break;
			case AnimationState.Projectile:
				playback.Travel("Projectile");
				break;
		}
	}

	public void SetGroundBlend(float blendValue){
		Set("parameters/StateMachine/GroundMovement/blend_position", blendValue);
	}

	public void SetAirborneBlend(float blendValue){
		Set("parameters/StateMachine/AirMovement/blend_position", blendValue);
	}

	public void SetDashBlend(float blendValue){
		Set("parameters/StateMachine/Dash/blend_position", blendValue);
	}

	public void SetAttackBlend(float blendValue){
		Set("parameters/StateMachine/Attack/blend_position", blendValue);
	}

}
