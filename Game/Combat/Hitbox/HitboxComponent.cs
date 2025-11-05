using Godot;
using System;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;

namespace DeckroidVania.Game.Combat.Hitbox
{
    public partial class HitboxComponent : Node3D
    {
        private Area3D _hitboxArea;
        private CollisionShape3D _collisionShape;
        private BoxShape3D _boxShape;
        private MeshInstance3D _debugMesh; // Visual debug mesh
        private HitboxData _data;
        private float _lifetimeRemaining;
        private bool _enabled = false;
        private string _targetGroup = "Player";

        public void Initialize(HitboxData data, string targetGroup = "Player")
        {
            _data = data;
            _targetGroup = targetGroup;
            _lifetimeRemaining = data.Lifetime;

            // Create Area3D dynamically
            _hitboxArea = new Area3D();
            _hitboxArea.Name = "HitboxArea";
            _hitboxArea.Position = Vector3.Zero; // Keep at root position
            AddChild(_hitboxArea);

            // Create CollisionShape3D
            _collisionShape = new CollisionShape3D();
            _collisionShape.Position = Vector3.Zero; // Center collision at Area3D origin
            _hitboxArea.AddChild(_collisionShape);

            // Create and configure box shape
            _boxShape = new BoxShape3D();
            _boxShape.Size = data.Size;
            _collisionShape.Shape = _boxShape;

            // Create debug visual mesh
            _debugMesh = new MeshInstance3D();
            _debugMesh.Name = "HitboxDebugMesh";
            _debugMesh.Position = Vector3.Zero; // Center mesh at root
            AddChild(_debugMesh);
            
            var boxMesh = new BoxMesh();
            boxMesh.Size = data.Size;
            _debugMesh.Mesh = boxMesh;
            
            // Create semi-transparent material for debug visualization
            var material = new StandardMaterial3D();
            material.AlbedoColor = new Color(1, 0, 0, 0.3f); // Red with transparency
            material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
            _debugMesh.SetSurfaceOverrideMaterial(0, material);

            // Set position offset with direction correction
            Vector3 adjustedOffset = data.Offset;
            
            if (GetParent() is Enemy enemy)
            {
                // Try to get facing direction from Enemy's MovementComponent
                IMovementComponent movementComp = enemy.MovementComponent;
                if (movementComp != null)
                {
                    bool isFacingRight = movementComp.FaceRight;
                    
                    if (!isFacingRight)
                    {
                        adjustedOffset.Z *= -1; // Flip Z offset for left-facing
                    }
                    GD.Print($"[HitboxComponent] Enemy FaceRight: {isFacingRight}, Final Offset: {adjustedOffset}");
                }
                else
                {
                    // Fallback: check Visual node rotation
                    Node3D visualNode = enemy.GetNodeOrNull<Node3D>("Visual");
                    if (visualNode != null)
                    {
                        float rotationY = visualNode.Rotation.Y;
                        bool isFacingLeft = Mathf.Abs(rotationY) > Mathf.Pi * 0.5f;
                        
                        if (isFacingLeft)
                        {
                            adjustedOffset.Z *= -1;
                        }
                        GD.Print($"[HitboxComponent] Fallback - Rotation Y: {rotationY:F4}, Facing Left: {isFacingLeft}, Final Offset: {adjustedOffset}");
                    }
                }
            }
            
            Position = adjustedOffset;

            // Connect signals
            _hitboxArea.BodyEntered += OnBodyEntered;

            // Enable immediately
            Enable();

            GD.Print($"[HitboxComponent] ✓ Spawned hitbox - Size: {data.Size}, Position: {Position}, Damage: {data.Damage}, Lifetime: {data.Lifetime}s");
        }

        public override void _Process(double delta)
        {
            if (!_enabled) return;

            _lifetimeRemaining -= (float)delta;

            if (_lifetimeRemaining <= 0)
            {
                GD.Print("[HitboxComponent] ⏱️ Lifetime expired, destroying hitbox");
                QueueFree();
            }
        }

        public void Enable()
        {
            if (_hitboxArea == null) return;
            _enabled = true;
            _hitboxArea.Monitoring = true;
            _hitboxArea.Visible = true;
            GD.Print($"[HitboxComponent] ✓ Enabled");
        }

        public void Disable()
        {
            if (_hitboxArea == null) return;
            _enabled = false;
            _hitboxArea.Monitoring = false;
            _hitboxArea.Visible = false;
            GD.Print($"[HitboxComponent] ✗ Disabled");
        }

        private void OnBodyEntered(Node3D body)
        {
            if (!_enabled) return;
            if (!body.IsInGroup(_targetGroup)) return;

            GD.Print($"[HitboxComponent] 💥 HIT! Dealing {_data.Damage} damage to {body.Name}");

            // Get the attacker position (parent position)
            Vector3 attackerPos = GetParent<Node3D>()?.GlobalPosition ?? GlobalPosition;

            // Deal damage based on target type
            if (body.HasMethod("TakeDamage"))
            {
                // Check if it's an enemy (has TakeDamage with knockback parameters)
                if (body.IsInGroup("Enemy"))
                {
                    // Enemy TakeDamage: (int amount, float knockbackForce, float knockbackDuration, Vector3 attackerPosition)
                    body.Call("TakeDamage", _data.Damage, 50f, 0.3f, attackerPos);
                    GD.Print($"[HitboxComponent] ✓ Applied enemy damage with knockback");
                }
                else if (body.IsInGroup("Player"))
                {
                    // Player uses HealthSystem - just apply raw damage
                    body.Call("TakeDamage", _data.Damage);
                    GD.Print($"[HitboxComponent] ✓ Applied player damage");
                }
            }
            else
            {
                GD.PrintErr($"[HitboxComponent] ✗ {body.Name} has no TakeDamage method!");
            }

            // Destroy hitbox after hit (can make this configurable later)
            QueueFree();
        }

        public override void _ExitTree()
        {
            if (_hitboxArea != null)
            {
                _hitboxArea.BodyEntered -= OnBodyEntered;
            }
        }
    }
}