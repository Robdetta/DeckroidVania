using Godot;
using System;

namespace DeckroidVania.Game.Entities.Enemies.Data
{
    [Serializable]
    public class HitboxInfo
    {
        public float[] Size { get; set; } = { 1, 1, 1 };  // x, y, z
        public float[] Offset { get; set; } = { 0, 0, 0 }; // x, y, z
        public float Lifetime { get; set; } = 0.2f;

        public Vector3 SizeVec => new Vector3(Size[0], Size[1], Size[2]);
        public Vector3 OffsetVec => new Vector3(Offset[0], Offset[1], Offset[2]);
    }

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
        public HitboxInfo Hitbox { get; set; }
        
        /// <summary>
        /// Optional: If set, this "attack" triggers a state transition instead of dealing damage
        /// Examples: "Block", "Dodge", "Teleport", "Enrage"
        /// </summary>
        public string StateTransition { get; set; }
    }
}