using Godot;
using System;
using DeckroidVania.Game.Entities.Enemies.Base;
using DeckroidVania.Game.Entities.Enemies.Components.Interfaces;
using DeckroidVania2.Game.Systems.GameSystems; // For HealthSystem

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
        private string _targetGroup = "Player"; // Default, but overridden in Initialize

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

            // --- DYNAMICALLY SET COLLISION LAYERS/MASKS BASED ON TARGET GROUP ---
            if (_targetGroup == "Enemy") // This Hitbox is for a Player's attack, targets Enemies
            {
                // Player's "Hit box" is Layer 3, masks Enemy (Layer 2)
                _hitboxArea.CollisionLayer = (uint)Math.Pow(2, 2); // Layer 3 (value 4)
                _hitboxArea.CollisionMask = (uint)Math.Pow(2, 1);  // Mask Layer 2 (value 2)
                GD.PrintErr($"HitboxComponent DEBUG: Player Attack Hitbox - Layer: {_hitboxArea.CollisionLayer} (Layer 3), Mask: {_hitboxArea.CollisionMask} (Mask Layer 2)");
            }
            else if (_targetGroup == "Player") // This Hitbox is for an Enemy's attack, targets Player
            {
                // Enemy attack should be on Layer 2, masks Player (Layer 1)
                _hitboxArea.CollisionLayer = (uint)Math.Pow(2, 1); // Layer 2 (value 2)
                _hitboxArea.CollisionMask = (uint)Math.Pow(2, 0);  // Mask Layer 1 (value 1)
                GD.PrintErr($"HitboxComponent DEBUG: Enemy Attack Hitbox - Layer: {_hitboxArea.CollisionLayer} (Layer 2), Mask: {_hitboxArea.CollisionMask} (Mask Layer 1)");
            }
            else
            {
                GD.PrintErr($"HitboxComponent DEBUG: Unknown targetGroup '{_targetGroup}'. Defaulting to Player Attack layers/masks.");
                // Default to player attack behavior if targetGroup is unexpected
                _hitboxArea.CollisionLayer = (uint)Math.Pow(2, 2); // Layer 3 (value 4)
                _hitboxArea.CollisionMask = (uint)Math.Pow(2, 1);  // Mask Layer 2 (value 2)
            }
            // --- END DYNAMICALLY SET COLLISION LAYERS/MASKS ---

            // Enable immediately
            Enable();

            GD.Print($"HitboxComponent Initialized: TargetGroup='{_targetGroup}', Damage={_data.Damage}, Lifetime={_data.Lifetime}, Size={_data.Size}, Offset={_data.Offset}, KnockbackForce={_data.KnockbackForce}, KnockbackDuration={_data.KnockbackDuration}");
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
            GD.Print($"HitboxComponent OnBodyEntered: Hit {body.Name}, Group Check for '{_targetGroup}'");

            if (!_enabled) return;
            if (!body.IsInGroup(_targetGroup))
            {
                GD.Print($"HitboxComponent: Body {body.Name} is not in target group '{_targetGroup}'.");
                return;
            }

            // Apply damage based on target group
            if (body.IsInGroup("Player"))
            {
                // Player uses HealthSystem singleton
                if (HealthSystem.Instance != null)
                {
                    HealthSystem.Instance.TakeDamage(_data.Damage);
                    GD.Print($"HitboxComponent: Player hit for {_data.Damage} damage.");
                }
            }
            else if (body.IsInGroup("Enemy"))
            {
                // Enemy has TakeDamage method with knockback
                if (body.HasMethod("TakeDamage"))
                {
                    Vector3 attackerPos = GetParent<Node3D>()?.GlobalPosition ?? GlobalPosition;
                    // Ensure KnockbackForce and KnockbackDuration are passed
                    body.Call("TakeDamage", _data.Damage, _data.KnockbackForce, _data.KnockbackDuration, attackerPos);
                    GD.Print($"HitboxComponent: Enemy {body.Name} hit for {_data.Damage} damage, KnockbackForce={_data.KnockbackForce}, KnockbackDuration={_data.KnockbackDuration}.");
                }
                else
                {
                    GD.PrintErr($"HitboxComponent: Enemy {body.Name} does not have a 'TakeDamage' method.");
                }
            }

            // Destroy hitbox after hit
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