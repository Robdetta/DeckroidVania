using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface IAIComponent
    {
        EnemyState CurrentState { get; }
        Node3D CurrentTarget { get; }
        
        void Initialize(EnemyState initialState, string detectionBehavior);
        void ChangeState(EnemyState newState);
        void SetTarget(Node3D target);
        void ClearTarget();
        void Update(double delta);
        bool HasTarget();
        float GetDistanceToTarget();
    }
}
