using Godot;

namespace DeckroidVania2.Game.Systems.Deck.Effects
{
    public class EffectContext
    {
        // By changing this to Node3D, any Player, Enemy, or 3D object can be the Source.
        // This resolves the "Entity namespace not found" error!
        public Node3D Source { get; set; }

        public Vector3 TargetPosition { get; set; }

        public SceneTree Tree { get; set; }

        public EffectContext(Node3D source, Vector3 targetPosition, SceneTree tree)
        {
            Source = source;
            TargetPosition = targetPosition;
            Tree = tree;
        }
    }
}