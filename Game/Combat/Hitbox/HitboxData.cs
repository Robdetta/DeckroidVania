using Godot;

namespace DeckroidVania.Game.Combat.Hitbox
{
    public class HitboxData
    {
        public Vector3 Size { get; set; } = new Vector3(1, 1, 1);
        public Vector3 Offset { get; set; } = Vector3.Zero;
        public float Lifetime { get; set; } = 0.2f;
        public int Damage { get; set; } = 10;
        public float KnockbackForce { get; set; } = 0f; // ADDED THIS
        public float KnockbackDuration { get; set; } = 0f;
    }
}
