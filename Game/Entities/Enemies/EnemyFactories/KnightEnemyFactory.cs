using Godot;
using DeckroidVania.Game.Entities.Enemies.Types;
using static Godot.Node;

public partial class KnightEnemyFactory : IEnemyFactory
{
    public Node3D CreateEnemy(Transform3D markerTransform)
    {
        var scene = ResourceLoader.Load<PackedScene>("uid://c2l8ymvdetauj");

        // Cast to KnightEnemy after instantiation
        var knightEnemy  = scene.Instantiate() as KnightEnemy;     
           
        if (knightEnemy == null)
        {
            GD.PrintErr("KnightEnemyFactory: Failed to instantiate KnightEnemy - check if scene root has KnightEnemy script attached");
            return null;
        }

        GD.Print($"KnightEnemyFactory: Creating KnightEnemy at {markerTransform.Origin}");
        
        knightEnemy.GlobalTransform = markerTransform;
        knightEnemy.RotationDegrees = new Vector3(0, 90,0);
        knightEnemy.ProcessMode = ProcessModeEnum.Pausable;
        
        // Verify initialization
       //.Print($"KnightEnemyFactory: Created KnightEnemy with health: {knightEnemy.HealthComponent.MaxHealth}");

        return knightEnemy;
    }
}