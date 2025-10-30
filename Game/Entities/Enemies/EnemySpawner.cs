using Godot;
using System.Collections.Generic;

public partial class EnemySpawner : Node
{
    private MarkerManager _markerManager;
    private Dictionary<string, IEnemyFactory> _enemyFactories = new();

    public override void _Ready()
    {
        GD.Print("EnemySpawner: Initializing...");
        
        // Get MarkerManager (should be a sibling node)
        _markerManager = GetParent().GetNode<MarkerManager>("MarkerManager");
        if (_markerManager == null)
        {
            GD.PrintErr("EnemySpawner: Failed to find MarkerManager!");
            return;
        }
        
        // Register factories
        RegisterFactories();
        
        GD.Print("EnemySpawner: Initialization complete");
    }

    private void RegisterFactories()
    {
        GD.Print("EnemySpawner: Registering factories...");
        _enemyFactories["mage"] = new MageEnemyFactory();
        _enemyFactories["knight"] = new KnightEnemyFactory();
        GD.Print($"EnemySpawner: Registered factories: {string.Join(", ", _enemyFactories.Keys)}");
    }

    public void SpawnEnemyAtMarker(string enemyType, string markerName)
    {
        GD.Print($"EnemySpawner: Attempting to spawn {enemyType} at {markerName}");

        if (_markerManager == null)
        {
            GD.PrintErr("EnemySpawner: MarkerManager is null!");
            return;
        }

        var marker = _markerManager.GetMarkerByName(markerName);
        if (marker == null)
        {
            //GD.PrintErr($"EnemySpawner: Marker '{markerName}' not found!");
            return;
        }

        if (!_enemyFactories.TryGetValue(enemyType, out var factory))
        {
            GD.PrintErr($"EnemySpawner: No factory found for enemy type '{enemyType}'!");
            return;
        }

        var enemy = factory.CreateEnemy(marker.GlobalTransform);
        AddChild(enemy);
        GD.Print($"EnemySpawner: Successfully spawned {enemyType} at {markerName}");
    }
    
    public void ClearAllEnemies()
    {
        GD.Print("EnemySpawner: Clearing all enemies...");
        var children = GetChildren();
        foreach (var child in children)
        {
            child.QueueFree();
        }
        GD.Print($"EnemySpawner: Cleared {children.Count} enemies");
    }
}