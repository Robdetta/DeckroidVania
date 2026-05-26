using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using DeckroidVania.Game.Combat.Hitbox; // For AttackData

// This static class is responsible for loading all attack data from attacks.json
public static class AttackLoader
{
    private static List<AttackData> _allAttacks; // Cache the loaded data

    public static List<AttackData> LoadAttacksFromJson(string path)
    {
        // If data is already loaded, return the cached version (optimization)
        if (_allAttacks != null)
        {
            return _allAttacks;
        }

        if (FileAccess.FileExists(path))
        {
            var json = FileAccess.GetFileAsString(path);
            try
            {
                _allAttacks = JsonSerializer.Deserialize<List<AttackData>>(json);
                GD.Print($"AttackLoader: Successfully loaded {_allAttacks.Count} attacks from {path}");
                return _allAttacks;
            }
            catch (Exception e)
            {
                GD.PrintErr($"AttackLoader: Error deserializing attacks.json from {path}: {e.Message}");
                _allAttacks = new List<AttackData>(); // Return empty list on error
                return _allAttacks;
            }
        }
        GD.PrintErr($"AttackLoader: Failed to find attacks.json at {path}");
        _allAttacks = new List<AttackData>(); // Return empty list if file not found
        return _allAttacks;
    }
}