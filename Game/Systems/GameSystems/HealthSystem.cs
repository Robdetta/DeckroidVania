using Godot;
using System;

public partial class HealthSystem : Node
{	
	// 1) Static Instance property
    public static HealthSystem Instance { get; private set; }

	[Signal] public delegate void HealthChangedEventHandler(int current, int max);
    [Signal] public delegate void DiedEventHandler();

	[Export] public int MaxHealth { get; set; } = 100;
    private int _current;
    public int Current {
        get => _current;
        set {
            _current = Mathf.Clamp(value, 0, MaxHealth);
            EmitSignal(nameof(HealthChanged), _current, MaxHealth);
            if (_current == 0) EmitSignal(nameof(Died));
        }
    }

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{	
		Instance = this;
		Current = MaxHealth;
	}

	public void TakeDamage(int amount) => Current -= amount;
    public void Heal(int amount)       => Current += amount;

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
