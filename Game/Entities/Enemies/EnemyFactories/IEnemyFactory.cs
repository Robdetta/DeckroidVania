using Godot;

public interface IEnemyFactory
{
    /// <summary>
    /// Creates an enemy of the factory's type.
    /// </summary>
    /// <param name="markerTransform">The transform to position the enemy at.</param>
    /// <returns>The newly created enemy node.</returns>
    Node3D CreateEnemy(Transform3D markerTransform);
}