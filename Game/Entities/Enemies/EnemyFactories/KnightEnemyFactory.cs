using DeckroidVania.Game.Entities.Enemies.Types;
using Godot;
using static Godot.Node;

public partial class KnightEnemyFactory : IEnemyFactory
{
    public Node3D CreateEnemy(Transform3D markerTransform)
    {
        var scene = ResourceLoader.Load<PackedScene>("uid://c2l8ymvdetauj");

        // Cast to KnightEnemy after instantiation
        var knightEnemy = scene.Instantiate() as KnightEnemy;

        if (knightEnemy == null)
        {
            GD.PrintErr("KnightEnemyFactory: Failed to instantiate KnightEnemy - check if scene root has KnightEnemy script attached");
            return null;
        }

        AttachHealthbar(knightEnemy);

        GD.Print($"KnightEnemyFactory: Creating KnightEnemy at {markerTransform.Origin}");

        knightEnemy.GlobalTransform = markerTransform;
        knightEnemy.RotationDegrees = new Vector3(0, 90, 0);
        knightEnemy.ProcessMode = ProcessModeEnum.Pausable;

        // Verify initialization
        //.Print($"KnightEnemyFactory: Created KnightEnemy with health: {knightEnemy.HealthComponent.MaxHealth}");

        return knightEnemy;
    }

    private void AttachHealthbar(KnightEnemy knight)
    {
        var barScene = ResourceLoader.Load<PackedScene>("uid://c2r4i18vexhku"); // Game/Entities/Enemies/Components/ComponentScenes/WorldHealthbar.tscn
        var healthbar = (WorldUIHealthbar)barScene.Instantiate();
        knight.AddChild(healthbar);

        // set bar above head
        var collisionShape = knight.GetNode<CollisionShape3D>("CollisionShape3D")?.Shape as BoxShape3D;
        if (collisionShape == null)
            GD.PrintErr("Failed to get shape from knight");

        healthbar.GlobalPosition = new Vector3(0, collisionShape.Size.Y, 0);
    }
}