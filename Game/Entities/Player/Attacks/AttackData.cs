using Godot;

public class AttackData
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Damage { get; set; }
    public float Range { get; set; }
    public string Animation { get; set; }
    public float Duration { get; set; }
    public float Lockout { get; set; } 
    public string Hitbox { get; set; }
    // Projectile-specific fields (optional)
    public float ProjectileSpeed { get; set; }          // For projectile attacks
    public float ProjectileLifetime { get; set; }        // How long the projectile exists
    public string ProjectileColor { get; set; }          // Color of the projectile
    public string ProjectileScene { get; set; } // Path/UID to projectile scene
    public float[] HitboxSize { get; set; }      // Use float[] instead of Vector3
    public float[] HitboxOffset { get; set; }    // Use float[] instead of Vector3

    public float KnockbackForce { get; set; } 
    public float KnockbackDuration { get; set; }

    // Helper properties to get Vector3
    public Vector3 HitboxSizeVec => HitboxSize != null && HitboxSize.Length == 3
        ? new Vector3(HitboxSize[0], HitboxSize[1], HitboxSize[2])
        : Vector3.One;

    public Vector3 HitboxOffsetVec => HitboxOffset != null && HitboxOffset.Length == 3
        ? new Vector3(HitboxOffset[0], HitboxOffset[1], HitboxOffset[2])
        : Vector3.Zero;

}