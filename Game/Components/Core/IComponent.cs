using Godot;
using System;

namespace DeckroidVania.Game.Entities.Components;

public interface IComponent
{
    void Initialize(Node owner);
    void Update(double delta);
    void Cleanup();
}
