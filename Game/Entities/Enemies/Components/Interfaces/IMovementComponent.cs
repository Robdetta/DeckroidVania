using Godot;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface IMovementComponent
    {
        Vector3 Velocity { get; set; }
        bool FaceRight { get; set; }
        Vector3 StartPosition { get; }
        
        void Initialize(CharacterBody3D owner, float speed, float patrolRange);
        void SetHorizontalVelocity(float speed);
        void ApplyMovement(double delta);
        bool IsWallAhead();
        bool IsEdgeAhead();
    }
}
