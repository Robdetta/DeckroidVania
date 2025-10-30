using Godot;
using System.Collections.Generic;
using System.Text.Json;

public class WeaponManager
{
    private Node _owner;
    private List<WeaponData> _weapons = new();
    private WeaponData _currentWeapon;
    private Node _currentWeaponInstance;
    private Node _handNode;
    private string _currentHitboxScene;

    public WeaponManager(Node owner)
    {
        _owner = owner;
        LoadWeapons();
        GD.Print($"_owner type: {_owner.GetType().Name}");
    }

    private void LoadWeapons()
    {
        var path = "res://Data/weapons.json";
        if (FileAccess.FileExists(path))
        {
            var json = FileAccess.GetFileAsString(path);
            _weapons = JsonSerializer.Deserialize<List<WeaponData>>(json);
        }
    }

    public void EquipWeaponById(int id)
    {
        var weapon = _weapons.Find(w => w.Id == id);
        if (weapon == null) return;

        _currentWeaponInstance?.QueueFree();

        // Store hitbox scene path for later use
        _currentHitboxScene = weapon.HitboxScene;

        // Find and store hand node reference
        // Remove "Player/" if _owner is already the Player node
        var handPath = weapon.AttachmentNode.StartsWith("Player/") 
            ? weapon.AttachmentNode.Substring(7) 
            : weapon.AttachmentNode;
        _handNode = _owner.GetNodeOrNull<Node>(handPath);

        // Load and instance weapon scene
        var weaponScene = GD.Load<PackedScene>(weapon.Scene);
        _currentWeaponInstance = weaponScene.Instantiate();

        _handNode?.AddChild(_currentWeaponInstance);

        _currentWeapon = weapon;
    }

    public Node3D GetCurrentWeaponInstance()
    {
        return _currentWeaponInstance as Node3D;
    }

    public WeaponData GetCurrentWeapon() => _currentWeapon;
    public string GetCurrentHitboxScene() => _currentHitboxScene;
    public Node GetHandNode() => _handNode;
}