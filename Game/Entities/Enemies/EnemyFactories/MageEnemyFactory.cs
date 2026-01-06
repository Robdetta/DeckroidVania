using DeckroidVania.Game.Entities.Enemies.Types;
using Godot;
using static Godot.Node;

public partial class MageEnemyFactory : IEnemyFactory
{
    public Node3D CreateEnemy(Transform3D markerTransform)
    {
        var scene = ResourceLoader.Load<PackedScene>("uid://cd1sqekbl0tw1");

        // Cast to MageEnemy after instantiation
        var mageEnemy = scene.Instantiate() as MageEnemy;

        if (mageEnemy == null)
        {
            GD.PrintErr("MageEnemyFactory: Failed to instantiate MageEnemy - check if scene root has MageEnemy script attached");
            return null;
        }

        AttachHealthbar(mageEnemy);

        GD.Print($"MageEnemyFactory: Creating MageEnemy at {markerTransform.Origin}");

        mageEnemy.GlobalTransform = markerTransform;
        mageEnemy.RotationDegrees = new Vector3(0, 90, 0);
        mageEnemy.ProcessMode = ProcessModeEnum.Pausable;

        // Verify initialization
        //GD.Print($"MageEnemyFactory: Created MageEnemy with health: {mageEnemy.GetStartingHealth()}");

        return mageEnemy;
    }

    private void AttachHealthbar(MageEnemy mage)
    {
        var barScene = ResourceLoader.Load<PackedScene>("uid://c2r4i18vexhku"); // Game/Entities/Enemies/Components/ComponentScenes/WorldHealthbar.tscn
        var healthbar = (WorldUIHealthbar)barScene.Instantiate();
        mage.AddChild(healthbar);

        // set bar above head
        var collisionShape = mage.GetNode<CollisionShape3D>("CollisionShape3D")?.Shape as BoxShape3D;
        if (collisionShape == null)
            GD.PrintErr("Failed to get shape from knight");

        healthbar.GlobalPosition = new Vector3(0, collisionShape.Size.Y, 0);
    }
}