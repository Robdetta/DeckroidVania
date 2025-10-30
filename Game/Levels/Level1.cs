using Godot;

public partial class Level1 : BaseLevel
{
    protected override void ConfigureEnemySpawns()
    {
        GD.Print("Level1: Configuring enemy spawns");
        
        // Define which enemies spawn at which markers in this level
        _enemySpawns.Add(new EnemySpawnInfo("mage", "Marker1"));
        // Add more spawns as needed
        _enemySpawns.Add(new EnemySpawnInfo("knight", "Marker2"));
        
        GD.Print($"Level1: Configured {_enemySpawns.Count} enemy spawns");
    }
}