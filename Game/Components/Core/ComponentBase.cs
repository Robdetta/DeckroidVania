using Godot;
using System;

namespace DeckroidVania.Game.Entities.Components;

public abstract class ComponentBase : IComponent
{
    protected Node Owner { get; private set; }
    protected bool IsInitialized { get; private set; }

    public virtual void Initialize(Node owner)
    {
        Owner = owner;
        IsInitialized = true;
    }

    public virtual void Update(double delta)
    {
        if (!IsInitialized)
        {
            GD.PushError($"Component {GetType().Name} was not initialized before update!");
            return;
        }
        // Default update logic (if any) goes here
    }

    public virtual void Cleanup()
    {
        // Default cleanup logic (if any) goes here
        IsInitialized = false;
        Owner = null;
    }

}
