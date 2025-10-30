using Godot;
using System;

public partial class ManaSystem : Node
{
    public static ManaSystem Instance { get; private set; }

    [Signal] public delegate void ManaChangedEventHandler(int current, int max);

    [Export] public int MaxMana { get; set; } = 100;
    private int _current;
    public int Current
    {
        get => _current;
        set
        {
            _current = Mathf.Clamp(value, 0, MaxMana);
            EmitSignal(nameof(ManaChanged), _current, MaxMana);
        }
    }

    public override void _Ready()
    {
        Instance = this;
        Current = MaxMana;
    }

    public void SpendMana(int amount) => Current -= amount;
    public void RestoreMana(int amount) => Current += amount;
}