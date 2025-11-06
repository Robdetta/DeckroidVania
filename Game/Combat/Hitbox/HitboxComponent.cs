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
            _hitboxArea.Position = Vector3.Zero;
            AddChild(_hitboxArea);

            // Create CollisionShape3D
            _collisionShape = new CollisionShape3D();
            _collisionShape.Position = Vector3.Zero;
            _hitboxArea.AddChild(_collisionShape);

            // Create and configure box shape
            _boxShape = new BoxShape3D();
            _boxShape.Size = data.Size;
            _collisionShape.Shape = _boxShape;

            // Set position offset - parent node handles rotation/direction automatically
            Position = data.Offset;
            
            // Connect signals
            _hitboxArea.BodyEntered += OnBodyEntered;

            // Enable immediately
            Enable();
        }

        public override void _Process(double delta)
        {
            if (!_enabled) return;

            _lifetimeRemaining -= (float)delta;

            if (_lifetimeRemaining <= 0)
            {
                QueueFree();
            }
        }

        public void Enable()
        {
            if (_hitboxArea == null) return;
            _enabled = true;
            _hitboxArea.Monitoring = true;
            _hitboxArea.Visible = true;
        }

        public void Disable()
        {
            if (_hitboxArea == null) return;
            _enabled = false;
            _hitboxArea.Monitoring = false;
            _hitboxArea.Visible = false;
        }

        private void OnBodyEntered(Node3D body)
        {
            if (!_enabled) return;
            if (!body.IsInGroup(_targetGroup)) return;

            // Apply damage based on target group
            if (body.IsInGroup("Player"))
            {
                // Player uses HealthSystem singleton
                if (HealthSystem.Instance != null)
                {
                    HealthSystem.Instance.TakeDamage(_data.Damage);
                }
            }
            else if (body.IsInGroup("Enemy"))
            {
                // Enemy has TakeDamage method with knockback
                if (body.HasMethod("TakeDamage"))
                {
                    Vector3 attackerPos = GetParent<Node3D>()?.GlobalPosition ?? GlobalPosition;
                    body.Call("TakeDamage", _data.Damage, 50f, 0.3f, attackerPos);
                }
            }

            // Destroy hitbox after hit
            QueueFree();
        }

        private void PrintGlobalPosition()
        {
            // Removed debug output
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