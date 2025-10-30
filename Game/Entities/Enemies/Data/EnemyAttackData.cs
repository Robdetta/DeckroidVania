using Godot;
using System;

namespace DeckroidVania.Game.Entities.Enemies.Data
{
    [Serializable]
    public class EnemyAttackData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Damage { get; set; }
        public float Range { get; set; }
        public float Cooldown { get; set; }
        public string AnimationName { get; set; }
        public string ProjectileScene { get; set; }
        public int Weight { get; set; } = 100;
    }
}