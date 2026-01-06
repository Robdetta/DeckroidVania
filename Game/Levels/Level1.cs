using Godot;

public partial class Level1 : BaseLevel
{
    protected override void ConfigureEnemySpawns()
    {
        GD.Print("Level1: Configuring enemy spawns");

        // Define which enemies spawn at which markers in this level
        _enemySpawns.Add(new EnemySpawnInfo(EnemyTypesEnum.MAGE, "Marker1"));
        // Add more spawns as needed
        _enemySpawns.Add(new EnemySpawnInfo(EnemyTypesEnum.KNIGHT, "Marker2"));

        GD.Print($"Level1: Configured {_enemySpawns.Count} enemy spawns");
    }
}