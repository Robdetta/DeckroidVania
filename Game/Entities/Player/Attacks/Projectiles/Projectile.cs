using Godot;
using System;
using DeckroidVania2.Game.Player;

public partial class Projectile : Area3D
{
    [Export] public float Speed { get; set; }
    [Export] public float Lifetime { get; set; }

    private Vector3 _direction = Vector3.Forward;
    private float _timer = 0f;
    private int _damage = 0;
    public float KnockbackForce { get; set; }
    public float KnockbackDuration { get; set; }
    private Node _owner;

    public void Initialize(Vector3 direction, int damage, float speed = 15f, Color? color = null, float knockbackForce = 0, float knockbackDuration = 0, Node owner = null)
    {
        _direction = direction.Normalized();
        _damage = damage;
        Speed = speed;
        KnockbackForce = knockbackForce;
        KnockbackDuration = knockbackDuration;
        _owner = owner;
    }

    public override void _Process(double delta)
    {
        //GD.Print("Projectile position: " + GlobalTransform.origin);
        GlobalTranslate(_direction * Speed * (float)delta);
        _timer += (float)delta;
        if (_timer > Lifetime)
            QueueFree();
    }

    private Vector3 GetGlobalPosition(Node node)
    {
        if (node is Node3D node3D)
        {
            return node3D.GlobalPosition;
        }
        // Handle 2D nodes or other cases if needed
        GD.PushWarning($"Cannot get GlobalPosition from node type: {node.GetType()}");
        return Vector3.Zero; // Or some default value
    }
    private void OnBodyEntered(Node body)
    {
        GD.Print("Projectile hit: " + body.Name);
        if (body.HasMethod("TakeDamage"))
        {
            GD.Print($"Calling TakeDamage on {body.Name}");
            Vector3 ownerPosition = GetGlobalPosition(_owner);
            body.Call("TakeDamage", _damage, KnockbackForce, KnockbackDuration, ownerPosition);
        }
        QueueFree();
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
    }    

}
