namespace DeckroidVania.Game.Entities.Components;

public interface IEnemyData
{
    string Id { get; }
    string Name { get; }
    string Type { get; }
    int MaxHealth { get; }
    int HealthRegen { get; }
    // Add other common properties here (speed, attack range, etc.)
}