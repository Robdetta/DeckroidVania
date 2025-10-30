using Godot;
using System.Collections.Generic;

public partial class Hitbox : Area3D
{
    private int _damage;
    private Node _attacker;
    private HashSet<Node> _alreadyHit = new();
    public float KnockbackForce { get; set; }
    public float KnockbackDuration { get; set; }


    public void Configure(int damage, Node attacker, Vector3 size, Vector3 localOffset,float knockbackForce, float knockbackDuration)
    {
        GD.Print($"Configuring hitbox - Damage: {damage}, Size: {size}, Offset: {localOffset}");
        _damage = damage;
        _attacker = attacker;
        _alreadyHit.Clear();
        KnockbackForce = knockbackForce;
        KnockbackDuration = knockbackDuration;

        var shape = GetNode<CollisionShape3D>("CollisionShape3D");
        if (shape != null && shape.Shape is BoxShape3D box)
        {
            box.Size = size;
            GD.Print("Hitbox shape configured successfully");
        }
        else
        {
            GD.Print("Failed to configure hitbox shape!");
        }
        Position = localOffset;
    }

    public void Enable()
    {
        GD.Print($"Hitbox enabled - Monitoring: {Monitoring}, Position: {GlobalPosition}");
        _alreadyHit.Clear();
        Monitoring = true;
        Monitorable = true;
    }

    public void Disable()
    {
        GD.Print("Hitbox disabled");
        Monitoring = false;
    }

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        Monitoring = false;
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
        GD.Print($"Hitbox collided with: {body.Name}");
        if (body == _attacker) return;
        if (_alreadyHit.Contains(body)) return;
        _alreadyHit.Add(body);

        if (body.HasMethod("TakeDamage"))
        {
            GD.Print($"Calling TakeDamage on {body.Name}");
            Vector3 attackerPosition = GetGlobalPosition(_attacker);
            body.Call("TakeDamage", _damage, KnockbackForce, KnockbackDuration, attackerPosition);
        }
    }
}
