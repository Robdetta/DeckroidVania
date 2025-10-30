using Godot;

public partial class _Sandbox : BaseLevel
{
    protected override void ConfigureEnemySpawns()
    {
        GD.Print("Sandbox: Configuring enemy spawns");
        
        // Different enemy configuration for sandbox
        _enemySpawns.Add(new EnemySpawnInfo("mage", "Marker1"));
        _enemySpawns.Add(new EnemySpawnInfo("mage", "Marker2"));   
        _enemySpawns.Add(new EnemySpawnInfo("knight", "Marker3"));


        // Add more spawns as needed

        GD.Print($"Sandbox: Configured {_enemySpawns.Count} enemy spawns");
    }
}