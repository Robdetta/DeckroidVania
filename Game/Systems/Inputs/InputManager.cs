using DeckroidVania2.Game.Scripts.Inputs;
using Godot;

public partial class InputManager : Node
{   
    public static InputManager Instance { get; private set; }

    [Signal] public delegate void ShuffleLeftEventHandler();
    [Signal] public delegate void ShuffleRightEventHandler();
    [Signal] public delegate void AttackEventHandler();
    //Unsure if ProjectileAttackEvent is needed
    [Signal] public delegate void ProjectileAttackEventHandler();

    [Signal] public delegate void ActivateCardEventHandler();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // Check for input and emit relevant signals
        if (Input.IsActionJustPressed(ControlsSchema.SHIFT_LEFT))
        {
            GD.Print("InputManager: Emitting ShuffleLeftEventHandler");
            EmitSignal(SignalName.ShuffleRight);
        }
        else if (Input.IsActionJustPressed(ControlsSchema.SHIFT_RIGHT))
        {
            GD.Print("InputManager: Emitting ShuffleRightEventHandler");
            EmitSignal(SignalName.ShuffleLeft);
        }

        if (Input.IsActionJustPressed(ControlsSchema.UI_ATTACK))
            EmitSignal(SignalName.Attack);

        if (Input.IsActionJustPressed(ControlsSchema.UI_PROJECTILE_ATTACK))
            EmitSignal(SignalName.ProjectileAttack);
        
        if (Input.IsActionJustPressed(ControlsSchema.UI_CARD))
            EmitSignal(SignalName.ActivateCard);
        
    }
}
