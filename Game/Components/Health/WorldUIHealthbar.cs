using DeckroidVania.Game.Entities.Enemies.Components;
using Godot;

public partial class WorldUIHealthbar : Sprite3D
{
    private HealthComponent _healthComponent;
    private ShaderMaterial _barMaterial;

    public void Initialize(HealthComponent healthComponent)
    {
        _healthComponent = healthComponent;
        _barMaterial = GetNode<MeshInstance3D>("MeshInstance3D").GetSurfaceOverrideMaterial(0) as ShaderMaterial;
        UpdateHealthBar(0);
        _healthComponent.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(int _)
    {
        float ratio = (float)_healthComponent.CurrentHealth / _healthComponent.MaxHealth;
        _barMaterial.SetShaderParameter("health_ratio", ratio);
    }
}
