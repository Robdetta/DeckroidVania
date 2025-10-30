using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class MarkerManager : Node
{
    private List<Marker3D> _spawnMarkers = new();

    public override void _Ready()
    {
        GD.Print($"MarkerManager: Initialized at path {GetPath()}");
    }

    public void ClearMarkers()
    {
        _spawnMarkers.Clear();
        GD.Print("MarkerManager: Cleared all markers");
    }

    public void RegisterMarker(Marker3D marker)
    {
        if (marker != null && !_spawnMarkers.Contains(marker))
        {
            _spawnMarkers.Add(marker);
            GD.Print($"MarkerManager: Registered marker: {marker.Name}"); // Add this line
        }
    }

    public void RegisterMarkers(IEnumerable<Marker3D> markers)
    {
        foreach (var marker in markers)
        {
            RegisterMarker(marker);
        }
    }

    public bool ActivateMarker(Marker3D marker)
    {
        if (_spawnMarkers.Contains(marker))
        {
            marker.Visible = true;
            return true;
        }
        return false;
    }

    public void DeactivateMarker(Marker3D marker)
    {
        if (_spawnMarkers.Contains(marker))
        {
            marker.Visible = false;
        }
    }

    public Marker3D GetAvailableMarker()
    {
        return _spawnMarkers.FirstOrDefault(m => m.Visible);
    }

    public Marker3D GetMarkerByName(string markerName)
    {
        var marker = _spawnMarkers.FirstOrDefault(m => m.Name == markerName);
        if (marker == null)
        {
            GD.PrintErr($"MarkerManager: Marker '{markerName}' not found. Available markers: {string.Join(", ", _spawnMarkers.Select(m => m.Name))}");
        }
        return marker;
    }
}