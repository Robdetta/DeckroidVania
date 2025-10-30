using Godot;
using Godot.Collections;
using System.Linq;
using DeckroidVania2.Game.Scripts.Inputs;

namespace Game.Scripts;

public partial class GameController : Node
{
    [Export] public Node3D World3D { get; private set; }
    [Export] public World2d World2D { get; private set; }
    [Export, ExportGroup("Levels")] public Array<PackedScene> Levels { get; private set; }
    [Export] public UIManager UIManager { get; private set; }

    private LevelManager _levelManager;
    private PackedScene _curLevel;

    public override void _Ready()
    {
        Global.GameController = this;
        ProcessMode = ProcessModeEnum.Always;

        // Show start menu first
        UIManager.ShowStartMenu();
    }

    private void InitializeWorld()
    {
        _levelManager = new LevelManager();

        if (Levels.Any())
        {
            _curLevel = Levels.First();
            
            // Load level first
            if (_levelManager.LoadLevel(_curLevel, World2D)) // Change World3D to World2D
            {
                // Wait a frame to ensure level is loaded and markers are registered
                // var timer = new Timer();
                // AddChild(timer);
                // timer.WaitTime = 0.1f; // Small delay to ensure everything is ready
                // timer.OneShot = true;
                // timer.Timeout += () =>
                {
                    // Spawn player and enemy after level is loaded
                    World2D.SpawnPlayer(_levelManager.PlayerSpawn);
                    //World2D.SpawnEnemyAtMarker("mage", "Marker1");
                    
                    // Spawn all enemies configured for this level
                    var enemySpawns = _levelManager.GetEnemySpawns();
                    foreach (var spawn in enemySpawns)
                    {
                        World2D.SpawnEnemyAtMarker(spawn.EnemyType, spawn.MarkerName);
                    }

                    // Show gameplay overlays
                    UIManager.ShowGameplayHUD();
                    UIManager.ShowCardHand();
                    
                    //timer.QueueFree();
                };
                //timer.Start();
            }
            else
            {
                GD.PrintErr("Failed to load level.");
            }
        }
        else
        {
            GD.PrintErr("No levels available to load.");
        }
    }

    private void LoadNextLevel()
    {
        if (_levelManager != null)
        {
            int index = Levels.IndexOf(_curLevel) + 1;
            index = index == Levels.Count ? 0 : index;

            // Clear all enemies from previous level
            World2D.EnemySpawner.ClearAllEnemies();

            // Unload the current level first
            _levelManager.UnloadCurrentLevel();

            bool success = _levelManager.LoadLevel(Levels[index], World2D);

            if (success)
            {
                _curLevel = Levels[index];
                
                // Respawn player at new level's spawn point
                World2D.RespawnPlayer(_levelManager.PlayerSpawn.GlobalTransform);

                // Spawn enemies for new level
                var enemySpawns = _levelManager.GetEnemySpawns();
                foreach (var spawn in enemySpawns)
                {
                    World2D.SpawnEnemyAtMarker(spawn.EnemyType, spawn.MarkerName);
                }
                
                GD.Print($"Successfully loaded level: {Levels[index].ResourcePath}");
            }
            else
            {
                GD.PrintErr($"Failed to load level at index {index}");
            }
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed(ControlsSchema.UI_PAUSE))
        {
            if (GetTree().Paused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        GetTree().Paused = true;
        CardManager.Instance.PauseSpawning();
        UIManager.ShowPauseMenu();
    }

    public void ResumeGame()
    {
        GetTree().Paused = false;
        CardManager.Instance.ResumeSpawning();
        UIManager.HideMenu();
    }

    public void StartGame()
    {
        UIManager.HideMenu(); // Hide start menu
        InitializeWorld();
    }

    // public void ShowDeckbuilder()
    // {
    //     UIManager.ShowDeckBuilder();
    // }

    // public void ShowSettings()
    // {
    //     UIManager.ShowSettingsMenu();
    // }

    // public void ShowCredits()
    // {
    //     UIManager.ShowCreditsMenu();
    // }

    public void ReturnToMainMenu()
    {
        // Hide all gameplay overlays
        UIManager.HideGameplayHUD();
        UIManager.HideCardHand();
        // Show start menu
        UIManager.ShowStartMenu();
    }
}