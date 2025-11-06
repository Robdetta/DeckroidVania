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

            GD.Print($"\n[HitboxComponent] ═══ HITBOX INITIALIZATION ═══");
            GD.Print($"[HitboxComponent] Parent: {GetParent()?.Name} at {GetParent<Node3D>()?.GlobalPosition}");
            GD.Print($"[HitboxComponent] Offset: {data.Offset} | Size: {data.Size} | Damage: {data.Damage}");

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

            // Set position offset - parent node handles rotation/direction automatically
            // Since we're now a child of Visual/Knight node, its rotation transforms our local offset correctly
            // No need to manually flip Z-axis - the parent's 180° rotation does it for us!
            Position = data.Offset;
            GD.Print($"[HitboxComponent] ▶ HitboxComponent.Position set to: {Position} (parent handles direction)");
            
            // Debug: Print final global position after being added to scene
            CallDeferred(nameof(PrintGlobalPosition));

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

            GD.Print($"[HitboxComponent] 💥 HIT! Target: {body.Name} (Group: {_targetGroup})");

            // Apply damage based on target group (similar to DamageZone pattern)
            if (body.IsInGroup("Player"))
            {
                // Player uses HealthSystem singleton
                if (HealthSystem.Instance != null)
                {
                    HealthSystem.Instance.TakeDamage(_data.Damage);
                    GD.Print($"[HitboxComponent] ✓ Dealt {_data.Damage} damage to Player via HealthSystem");
                }
                else
                {
                    GD.PrintErr("[HitboxComponent] ✗ HealthSystem.Instance is null!");
                }
            }
            else if (body.IsInGroup("Enemy"))
            {
                // Enemy has TakeDamage method with knockback
                if (body.HasMethod("TakeDamage"))
                {
                    Vector3 attackerPos = GetParent<Node3D>()?.GlobalPosition ?? GlobalPosition;
                    body.Call("TakeDamage", _data.Damage, 50f, 0.3f, attackerPos);
                    GD.Print($"[HitboxComponent] ✓ Dealt {_data.Damage} damage to Enemy with knockback");
                }
                else
                {
                    GD.PrintErr($"[HitboxComponent] ✗ Enemy {body.Name} has no TakeDamage method!");
                }
            }

            // Destroy hitbox after hit (can make this configurable later)
            GD.Print("[HitboxComponent] 🗑️ Destroying hitbox after hit");
            QueueFree();
        }

        private void PrintGlobalPosition()
        {
            GD.Print($"[HitboxComponent] ✓ Active at GlobalPosition: {GlobalPosition}\n");
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