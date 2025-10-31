using Godot;
using System;

namespace DeckroidVania.Game.Entities.Enemies.Components.Interfaces
{

    /// Interface for vision/detection component
    /// Responsibilities: Detect targets, manage vision area
    public interface IVisionComponent
    {

        /// Fired when a valid target enters vision range

        event Action<Node3D> OnTargetDetected;
        

        /// Fired when a valid target leaves vision range
        event Action<Node3D> OnTargetLost;
        

        /// Setup the vision area detection
        /// <param name="visionArea">The Area3D node to monitor</param>
        /// <param name="targetGroup">Which group to detect (e.g., "Player")</param>
        void Initialize(Area3D visionArea, string targetGroup = "Player");
        

        /// Check if a target is currently in vision

        bool HasTargetInVision();
    }
}
