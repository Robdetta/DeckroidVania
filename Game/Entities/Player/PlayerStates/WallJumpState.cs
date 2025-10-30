using Godot;
using DeckroidVania2.Game.Player.Interfaces;
using DeckroidVania2.Game.Scripts.Inputs;

namespace DeckroidVania2.Game.Player.PlayerStates
{
    public class WallJumpState : IPlayerState
    {
        private MovementController _controller;

        // private const float TEST_WALL_JUMP_HORIZONTAL_PUSH = 10f; // adjust as needed
        // private const float TEST_WALL_JUMP_VERTICAL_PUSH = 20f;   // adjust as needed

        public WallJumpState(MovementController controller)
        {
            this._controller = controller;
        }

        public void Enter()
        {
            // Add logic for entering the WallJumpState
            GD.Print("[WallJumpState] Entered WallJumpState");
            // Determine push direction from collision normals using a helper.
            float pushDirection = _controller.GetWallPushDirection();
            if (pushDirection == 0f)
            {
                // Fallback: use _faceRight flag.
                pushDirection = _controller._faceRight ? -1f : 1f;
                GD.Print("[WallJumpState] Fallback pushDirection from _faceRight: " + pushDirection);
            }
            else
            {
                // Normal horizontal air control resumes ONLY after lock expires
                _controller.ApplyHorizontalMovement(pushDirection);

                // Clamp horizontal speed after wall jump
                float maxWallJumpSpeed = 30f; // Adjust as needed
                _controller._velocity.X = Mathf.Clamp(_controller._velocity.X, -maxWallJumpSpeed, maxWallJumpSpeed);
                GD.Print("[WallJumpState] Detected pushDirection from collisions: " + pushDirection);
            }
            
            // Apply the wall jump impulse:
            float horizontalImpulse = pushDirection * _controller.WalljumpHorizontalPush;
            float verticalImpulse   = _controller.WalljumpVerticalPush;
            
            GD.Print("[WallJumpState] Applying horizontal impulse: " + horizontalImpulse +
                     ", vertical impulse: " + verticalImpulse);
            
            _controller._velocity.X = horizontalImpulse;
            _controller._velocity.Y = verticalImpulse;
            
            // For testing, print the resulting velocity.
            GD.Print("[WallJumpState] New velocity after wall jump impulse: " + _controller._velocity);

            // Set wall jump flag and lock
            _controller._isWallJump = true;
            _controller._wallJumpLockTime = _controller.WallJumpLockDuration;

            
            // Immediately transition into JumpingState (or a similar state that handles mid-air control).
            _controller.ChangeState(PlayerState.Jumping);
        }

        public void Exit()
        {

        }

        public void HandleInput(double delta)
        {

        }
        public void UpdateState(double delta)
        {
            
        }
    }
}