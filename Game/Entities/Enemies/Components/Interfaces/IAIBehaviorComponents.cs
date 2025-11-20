using Godot;
using System.Collections.Generic;
using DeckroidVania.Game.Entities.Enemies.Base;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{
    public interface IAIBehaviorComponent
    {
        void Initialize(Enemy enemy, List<string> attackPattern, Dictionary<string, dynamic> behaviorConfig);
        string DecideAttack(float distanceToPlayer, int playerHealth, int enemyHealth);
        void Reset();
        (float attackFreq, float blockFreq) GetPatternFrequencies();
    }
}