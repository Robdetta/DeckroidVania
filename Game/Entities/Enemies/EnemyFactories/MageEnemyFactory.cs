using Godot;
using DeckroidVania.Game.Entities.Enemies.Types;
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

        GD.Print($"MageEnemyFactory: Creating MageEnemy at {markerTransform.Origin}");
        
        mageEnemy.GlobalTransform = markerTransform;
        mageEnemy.RotationDegrees = new Vector3(0, 90,0);
        mageEnemy.ProcessMode = ProcessModeEnum.Pausable;
        
        // Verify initialization
       //GD.Print($"MageEnemyFactory: Created MageEnemy with health: {mageEnemy.GetStartingHealth()}");

        return mageEnemy;
    }
}