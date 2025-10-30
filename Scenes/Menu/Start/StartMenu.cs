using Game.Scripts;
using Godot;
using System;

public partial class StartMenu : CanvasLayer
{
    public override void _Ready()
    {
        var startButton = GetNode<Button>("Panel/VBoxContainer/StartGame");
        startButton.GrabFocus();
        startButton.Pressed += OnStartGamePressed;

        // Optional: Deckbuilder button
        // var deckButton = GetNodeOrNull<Button>("Panel/VBoxContainer/Deckbuilder");
        // if (deckButton != null)
        //     deckButton.Pressed += OnDeckbuilderPressed;
    }

    private void OnStartGamePressed()
    {
        var gameController = GetTree().Root.GetNode<GameController>("GameController");
        gameController.StartGame();
    }

    // private void OnDeckbuilderPressed()
    // {
    //     var gameController = GetTree().Root.GetNode<GameController>("GameController");
    //     gameController.ShowDeckbuilder();
    // }
}
