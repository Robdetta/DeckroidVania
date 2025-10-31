using Godot;
using System;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;

namespace DeckroidVania.Game.Entities.Enemies.Components
{
    /// <summary>
    /// Handles vision and target detection for enemies
    /// Responsibilities: Monitor vision area, detect targets, fire events when targets enter/exit
    /// </summary>
    public partial class VisionComponent : Node, IVisionComponent
    {
        // Events that other components can subscribe to
        // Think of events as "announcements" - "Hey everyone, I found a target!"
        public event Action<Node3D> OnTargetDetected;
        public event Action<Node3D> OnTargetLost;
        
        private Area3D _visionArea;           // The detection bubble
        private string _targetGroup = "Player"; // What we're looking for
        private Node3D _currentTarget;         // Who we're currently tracking
        
        /// <summary>
        /// Setup vision detection
        /// </summary>
        /// <param name="visionArea">The Area3D node (the detection sphere)</param>
        /// <param name="targetGroup">Which group to look for (default: "Player")</param>
        public void Initialize(Area3D visionArea, string targetGroup = "Player")
        {
            _visionArea = visionArea;
            _targetGroup = targetGroup;
            
            if (_visionArea == null)
            {
                GD.PushWarning("[VisionComponent] No VisionArea provided!");
                return;
            }
            
            // Connect to Godot signals - when bodies enter/exit the Area3D
            // "+=" means "subscribe to this event"
            _visionArea.BodyEntered += OnBodyEnteredVisionArea;
            _visionArea.BodyExited += OnBodyExitedVisionArea;
            
            GD.Print($"[VisionComponent] Initialized - Looking for '{_targetGroup}' group");
        }
        
        /// <summary>
        /// Called automatically by Godot when ANY body enters the VisionArea
        /// </summary>
        /// <param name="body">The Node3D that entered (could be player, enemy, wall, etc.)</param>
        private void OnBodyEnteredVisionArea(Node3D body)
        {
            // Filter: Only care about bodies in our target group (e.g., "Player")
            if (!body.IsInGroup(_targetGroup))
            {
                return; // Not the target we're looking for, ignore it
            }
            
            GD.Print($"[VisionComponent] Target detected: {body.Name}");
            
            _currentTarget = body;
            
            // Fire the event - tell everyone subscribed that we found a target
            // "?." means "only invoke if someone is listening"
            OnTargetDetected?.Invoke(body);
        }
        
        /// <summary>
        /// Called automatically by Godot when ANY body exits the VisionArea
        /// </summary>
        /// <param name="body">The Node3D that left</param>
        private void OnBodyExitedVisionArea(Node3D body)
        {
            // Only care if it's the target we were tracking
            if (!body.IsInGroup(_targetGroup))
            {
                return;
            }
            
            GD.Print($"[VisionComponent] Target lost: {body.Name}");
            
            _currentTarget = null;
            
            // Fire the event - tell everyone we lost the target
            OnTargetLost?.Invoke(body);
        }
        
        /// <summary>
        /// Check if we currently have a target in vision
        /// </summary>
        public bool HasTargetInVision()
        {
            // Make sure the target still exists and wasn't deleted
            return _currentTarget != null && GodotObject.IsInstanceValid(_currentTarget);
        }
        
        /// <summary>
        /// Cleanup when this component is removed from the scene tree
        /// Good practice: Always disconnect signals to prevent memory leaks
        /// </summary>
        public override void _ExitTree()
        {
            if (_visionArea != null)
            {
                // Unsubscribe from events
                _visionArea.BodyEntered -= OnBodyEnteredVisionArea;
                _visionArea.BodyExited -= OnBodyExitedVisionArea;
            }
        }
    }
}
