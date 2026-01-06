using Godot;

public partial class _Sandbox : BaseLevel
{
    protected override void ConfigureEnemySpawns()
    {
        GD.Print("Sandbox: Configuring enemy spawns");

        // Different enemy configuration for sandbox
        _enemySpawns.Add(new EnemySpawnInfo(EnemyTypesEnum.MAGE, "Marker1"));
        _enemySpawns.Add(new EnemySpawnInfo(EnemyTypesEnum.MAGE, "Marker2"));
        _enemySpawns.Add(new EnemySpawnInfo(EnemyTypesEnum.KNIGHT, "Marker3"));


        // Add more spawns as needed

        GD.Print($"Sandbox: Configured {_enemySpawns.Count} enemy spawns");
    }
}