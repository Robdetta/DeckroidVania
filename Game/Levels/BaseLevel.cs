using Godot;
using System.Collections.Generic;

public abstract partial class BaseLevel : Node3D
{
    public Marker3D PlayerSpawn { get; protected set; }
    public Node EnemyMarkersNode { get; protected set; }

    protected List<EnemySpawnInfo> _enemySpawns = new();

    public override void _Ready()
    {
        GD.Print($"{Name}: Level ready");

        // Find PlayerSpawn and EnemyMarkersNode automatically
        PlayerSpawn = FindPlayerSpawn();
        EnemyMarkersNode = FindEnemyMarkersNode();

        if (PlayerSpawn == null)
        {
            GD.PrintErr($"{Name}: PlayerSpawn node not found!");
        }

        if (EnemyMarkersNode == null)
        {
            GD.PrintErr($"{Name}: EnemyMarkers node not found!");
        }

        ConfigureEnemySpawns();
    }

    // Helper methods to find the nodes
    private Marker3D FindPlayerSpawn()
    {
        return GetNode<Marker3D>("PlayerSpawn"); // Assumes "PlayerSpawn" is the node name
    }

    private Node FindEnemyMarkersNode()
    {
        return GetNode<Node>("EnemyMarkers"); // Assumes "EnemyMarkers" is the node name
    }

    // Override this in each level to define enemy spawns
    protected abstract void ConfigureEnemySpawns();

    public List<EnemySpawnInfo> GetEnemySpawns()
    {
        return _enemySpawns;
    }

    public List<Marker3D> GetAllMarkers()
    {
        var markers = new List<Marker3D>();
        if (EnemyMarkersNode != null)
        {
            GD.Print($"BaseLevel: Found EnemyMarkers node in {Name}"); // Add this line
            foreach (var child in EnemyMarkersNode.GetChildren())
            {
                if (child is Marker3D marker)
                {
                    GD.Print($"BaseLevel: Found marker {marker.Name} in {Name}"); // Add this line
                    markers.Add(marker);
                }
            }
        }
        return markers;
    }
}

// Data structure for enemy spawn information
public struct EnemySpawnInfo
{
    public string EnemyType { get; set; }
    public string MarkerName { get; set; }

    public EnemySpawnInfo(string enemyType, string markerName)
    {
        EnemyType = enemyType;
        MarkerName = markerName;
    }
}