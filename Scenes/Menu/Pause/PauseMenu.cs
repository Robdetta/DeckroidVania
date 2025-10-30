using Game.Scripts;
using Godot;
using System;


public partial class PauseMenu : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		var resumeButton = GetNode<Button>("Panel/VBoxContainer/Resume");
		resumeButton.GrabFocus();
        resumeButton.Pressed += OnResumePressed;
	}

	private void OnResumePressed()
    {
        // Call GameController to resume the game
        var gameController = GetTree().Root.GetNode<GameController>("GameController");
        gameController.ResumeGame();
    }

	// // Called every frame. 'delta' is the elapsed time since the previous frame.
	// public override void _Process(double delta)
	// {
	// }
}
