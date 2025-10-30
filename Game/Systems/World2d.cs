using Godot;
using System.Collections.Generic;

namespace Game.Scripts;

public partial class World2d : Node3D
{
    [Export] public PackedScene PlayerScene { get; set; }
    private CharacterBody3D _player;
    [Export] public MarkerManager MarkerManager { get; set; } // Assign in the editor
    [Export] public EnemySpawner EnemySpawner { get; set; }

    private readonly Dictionary<string, IEnemyFactory> _enemyFactories = new();


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GD.Print("World2D initialized");
        // Register enemy factories
        _enemyFactories["mage"] = new MageEnemyFactory(); // Add more factories as needed

        if (MarkerManager == null)
        {
            GD.PrintErr("Marker Manager not assigned in World2d!");
        }

        if (EnemySpawner == null)
        {
            GD.PrintErr("EnemySpawner not assigned in World2d!");
        }

    }

    public void SpawnPlayer(Marker3D playerSpawn)
    {
        if (PlayerScene != null)
        {
            // Instance the player
            var playerNode = (Node3D)PlayerScene.Instantiate();
            AddChild(playerNode);

            // Position the player at the origin or a desired starting point
            playerNode.GlobalTransform = playerSpawn.GlobalTransform;

            // Set player and its children to pausable
            playerNode.ProcessMode = ProcessModeEnum.Pausable;

            _player = GetNode<CharacterBody3D>("Player/Player");

            GD.Print("Player spawned in World2D");
        }
        else
        {
            GD.PrintErr("PlayerScene is null in World2D");
        }
    }

    public void RespawnPlayer(Transform3D playerSpawn)
    {
        if (_player != null)
        {
            _player.GlobalTransform = playerSpawn;
        }
    }

    public Node3D SpawnEnemy(string enemyType, Transform3D markerTransform)
    {
        if (_enemyFactories.TryGetValue(enemyType, out var factory))
        {
            return factory.CreateEnemy(markerTransform);
        }
        else
        {
            GD.PrintErr($"No enemy factory registered for type: {enemyType}");
            return null;
        }
    }


    public void SpawnEnemyAtMarker(string enemyType, string markerName)
    {
        GD.Print($"World2D: Requesting spawn of {enemyType} at {markerName}");
        EnemySpawner?.SpawnEnemyAtMarker(enemyType, markerName);
    }

}

