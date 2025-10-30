using Godot;
using System;

namespace DeckroidVania.Game.Entities.Enemies.Base;


public interface IEnemy
{
    void TakeDamage(int amount, float knockbackForce, float knockbackDuration, Vector3 attackerPosition);

    bool IsDead { get; }
}