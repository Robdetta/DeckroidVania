public class WeaponData
{
    // Unique identifier for the weapon
    public int Id { get; set; } 
    // Display name of the weapon
    public string Name { get; set; }
    // List of attack IDs this weapon can perform (references AttackData)
    public int[] AttackIds { get; set; }
    // Multiplies base attack damage
    public float DamageMultiplier { get; set; } 
    // Weapon's effective range
    public float Range { get; set; } 
    // Path or UID to the hitbox scene (invisible area for collision/effects)
    public string HitboxScene { get; set; } 
    // Node path to attach the weapon to (e.g., player's hand)
    public string AttachmentNode { get; set; }
    // Path to the weapon's 3D model or sprite
    public string Scene { get; set; }
    public string ProjectileSpawnMarker { get; set; } // Path to the spawn marker for projectiles   
}