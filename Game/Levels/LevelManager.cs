using Godot;
using System.Collections.Generic;

public class LevelManager
{
    //private Node currentLevel;
    private BaseLevel _currentLevel;
    //public Marker3D PlayerSpawn { get; private set; }
    public Marker3D PlayerSpawn => _currentLevel?.PlayerSpawn;

    public bool LoadLevel(PackedScene levelScene, Node3D world)
    {
        GD.Print("LevelManager: Starting level load...");

        if (levelScene == null)
        {
            GD.PrintErr("LevelManager: Level scene is null");
            return false;
        }

        // Unload current level if exists
        if (_currentLevel != null)
        {
            _currentLevel.QueueFree();
            _currentLevel = null;
        }

        // Instance the level
        var levelNode = levelScene.Instantiate();
        if (levelNode is not BaseLevel level)
        {
            GD.PrintErr("LevelManager: Level scene does not have a BaseLevel script attached!");
            levelNode.QueueFree();
            return false;
        }

        _currentLevel = level;
        world.AddChild(_currentLevel);
        GD.Print($"LevelManager: Loaded level: {levelScene.ResourcePath}");

        // Register all markers from this level
        var markerManager = world.GetNodeOrNull<MarkerManager>("MarkerManager");
        if (markerManager != null)
        {
            // Clear previous markers
            markerManager.ClearMarkers();

            // Register markers from new level
            var markers = _currentLevel.GetAllMarkers();
            markerManager.RegisterMarkers(markers);
            GD.Print($"LevelManager: Registered {markers.Count} markers");
        }
        else
        {
            GD.PrintErr("LevelManager: MarkerManager not found!");
            return false;
        }

        return true;
    }

    public List<EnemySpawnInfo> GetEnemySpawns()
    {
        return _currentLevel?.GetEnemySpawns() ?? new List<EnemySpawnInfo>();
    }

    public void UnloadCurrentLevel()
    {
        if (_currentLevel != null)
        {
            _currentLevel.QueueFree();
            _currentLevel = null;
            GD.Print("Unloaded current level.");
        }
    }
}
