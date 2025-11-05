using Godot;

namespace DeckroidVania.Game.Combat.Hitbox
{
    public class HitboxData
    {
        public Vector3 Size { get; set; } = new Vector3(1, 1, 1);
        public Vector3 Offset { get; set; } = Vector3.Zero;
        public float Lifetime { get; set; } = 0.2f;
        public int Damage { get; set; } = 10;
    }
}
