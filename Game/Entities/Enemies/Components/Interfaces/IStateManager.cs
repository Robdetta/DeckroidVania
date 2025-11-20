using Godot;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Interfaces;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface IStateManager
    {
        EnemyState CurrentState { get; }
        Node3D CurrentTarget { get; }
        
        void Initialize(EnemyState initialState, string detectionBehavior);
        void CreateStates(float speed, float patrolRange, float chaseSpeed);
        void ChangeState(EnemyState newState);
        void SetTarget(Node3D target);
        void ClearTarget();
        void Update(double delta);
        bool HasTarget();
        float GetDistanceToTarget();
        IEnemyState GetState(EnemyState state);
    }
}
