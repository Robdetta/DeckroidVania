using Godot;
using System.Collections.Generic;
using System.Text.Json;
using DeckroidVania2.Game.Player;


public class AttackManager
{
    private Node _owner;
    private List<AttackData> _attacks = new();
    private AttackData _currentAttack;
    private WeaponManager _weaponManager;
    private SceneTreeTimer _hitboxTimer;
    private Hitbox _hitbox;


    public AttackManager(Node owner, WeaponManager weaponManager, int defaultAttackId = 1)
    {
        _owner = owner;
        _weaponManager = weaponManager;
        LoadAttacks();
        _hitbox = _owner.GetNode<Hitbox>("Visual/RootNode/Hitbox");

        var defaultAttack = _attacks.Find(a => a.Id == defaultAttackId);
        if (defaultAttack != null)
            SetAttackByName(defaultAttack.Name);
        else if (_attacks.Count > 0)
            SetAttackByName(_attacks[0].Name); // fallback
    }

    private void LoadAttacks()
    {
        var path = "res://Data/attacks.json";
        if (FileAccess.FileExists(path))
        {
            var json = FileAccess.GetFileAsString(path);
            _attacks = JsonSerializer.Deserialize<List<AttackData>>(json);
        }
    }

    public void SetAttackByName(string name)
    {
        _currentAttack = _attacks.Find(a => a.Name == name);
        if (_currentAttack != null && !string.IsNullOrEmpty(_currentAttack.Hitbox))
            ConfigureHitbox();
    }

    public void SetAttackById(int id)
    {
        _currentAttack = _attacks.Find(a => a.Id == id);
        // Only configure hitbox for melee attacks
        if (_currentAttack != null && !string.IsNullOrEmpty(_currentAttack.Hitbox))
            ConfigureHitbox();
        // Optionally: disable melee hitbox if switching to projectile
        else if (_hitbox != null)
            _hitbox.Disable();
    }

    public void PerformAttack()
    {
        GD.Print("PerformAttack called");
        if (_currentAttack == null || _weaponManager == null) return;
        var weapon = _weaponManager.GetCurrentWeapon();
        var weaponInstance = _weaponManager.GetCurrentWeaponInstance(); // ✅ This is a Node3D
            Marker3D spawnMarker = null;
            if (weaponInstance != null && weaponInstance.HasNode("ProjectileSpawn"))
                spawnMarker = weaponInstance.GetNode<Marker3D>("ProjectileSpawn");
            else
                spawnMarker = _weaponManager.GetHandNode() as Marker3D; // fallback if needed
        if (weapon == null) return;

        // PROJECTILE ATTACK
        if (!string.IsNullOrEmpty(_currentAttack.ProjectileScene))
        {
            // Option 1: Call FireProjectile() immediately (not recommended for animation sync)
            // FireProjectile();
            //FireProjectile();
            // Option 2: Set a flag or prepare, and call FireProjectile() from animation event
            return;
        }
        // MELEE ATTACK (hitbox logic)
        if (!string.IsNullOrEmpty(_currentAttack.Hitbox))
        {
            GD.Print("Ready for melee attack, hitbox will be activated by animation event.");
            // Do NOT activate hitbox here; let animation event call 
            //ActivateHitbox();
        }

        GD.Print($"Performing attack: {_currentAttack.Name} for {_currentAttack.Damage} damage!");
    }

    public void FireProjectile()
    {
        GD.Print("FireProjectile called");
        var weaponInstance = _weaponManager.GetCurrentWeaponInstance();
        Node3D spawnNode = null;

        if (weaponInstance != null && weaponInstance.HasNode("ProjectileSpawn"))
            spawnNode = weaponInstance.GetNode<Node3D>("ProjectileSpawn");
        else
            spawnNode = _weaponManager.GetHandNode() as Node3D;

        if (spawnNode == null || string.IsNullOrEmpty(_currentAttack.ProjectileScene))
            return;

        var projectileScene = GD.Load<PackedScene>(_currentAttack.ProjectileScene);
        var projectile = projectileScene.Instantiate<Projectile>();
        projectile.GlobalTransform = spawnNode.GlobalTransform;

        // Use player's facing direction
        bool facingRight = (_owner as Player)?.IsFacingRight() ?? true;
        float facing = facingRight ? 1f : -1f;
        Vector3 direction = new Vector3(facing, 0, 0);

        float speed = _currentAttack.ProjectileSpeed > 0 ? _currentAttack.ProjectileSpeed : 15f;
        float lifetime = _currentAttack.ProjectileLifetime > 0 ? _currentAttack.ProjectileLifetime : 2f;
        Color color = Colors.White;
        if (!string.IsNullOrEmpty(_currentAttack.ProjectileColor))
            color = new Color(_currentAttack.ProjectileColor);

        projectile.Initialize(direction, _currentAttack.Damage, speed, color, _currentAttack.KnockbackForce, _currentAttack.KnockbackDuration, _owner); // Pass _owner
        projectile.Lifetime = lifetime;

        _owner.GetTree().CurrentScene.AddChild(projectile);
    }
    
    public void ActivateHitbox()
    {
        GD.Print("ActivateHitbox called");
        if (_hitbox == null || _weaponManager == null || _currentAttack == null)
        {
            GD.Print($"Failed to activate hitbox - Hitbox: {_hitbox != null}, WeaponManager: {_weaponManager != null}, CurrentAttack: {_currentAttack != null}");
            return;
        }
        _hitbox.Enable();
        _hitboxTimer = _owner.GetTree().CreateTimer(_currentAttack.Duration);
        _hitboxTimer.Timeout += () => _hitbox.Disable();
    }

   private void ConfigureHitbox()
    {
        if (_hitbox == null || _weaponManager == null || _currentAttack == null) return;
        // Only configure for melee attacks (not projectiles)
        if (string.IsNullOrEmpty(_currentAttack.Hitbox)) return;
        var weapon = _weaponManager.GetCurrentWeapon();
        if (weapon == null) return;
        int finalDamage = (int)(_currentAttack.Damage * weapon.DamageMultiplier);
        Vector3 size = _currentAttack.HitboxSizeVec;
        Vector3 offset = _currentAttack.HitboxOffsetVec;
        _hitbox.Configure(finalDamage, _owner, size, offset, _currentAttack.KnockbackForce, _currentAttack.KnockbackDuration);
    }

   public void CancelAttack(float lingerTime = 0f)
    {
        if (_hitbox != null)
        {
            if (lingerTime > 0f)
            {
                _owner.GetTree().CreateTimer(lingerTime).Timeout += () => _hitbox.Disable();
            }
            else
            {
                _hitbox.Disable();
            }
        }
        _hitboxTimer = null;
    }

    public AttackData GetCurrentAttack() => _currentAttack;
}