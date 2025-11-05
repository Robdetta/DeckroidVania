using Godot;
using System;
using System.Collections.Generic;

namespace DeckroidVania.Game.Combat.Hitbox
{
    public static class HitboxConfigLoader
    {
        private static Dictionary<string, HitboxData> _cachedConfigs = new();

        public static HitboxData LoadHitboxConfig(string hitboxId)
        {
            if (_cachedConfigs.TryGetValue(hitboxId, out var cached))
                return cached;

            var filePath = "res://Game/Entities/Enemies/Data/HitboxConfigs.json";
            var fileContent = FileAccess.GetFileAsString(filePath);

            if (string.IsNullOrEmpty(fileContent))
            {
                GD.PushWarning($"[HitboxConfigLoader] Failed to load: {filePath}");
                return new HitboxData();
            }

            var json = new Json();
            var error = json.Parse(fileContent);

            if (error != Error.Ok)
            {
                GD.PushWarning($"[HitboxConfigLoader] JSON parse error: {error}");
                return new HitboxData();
            }

            var data = json.Data.AsGodotDictionary();
            if (!data.ContainsKey("hitboxes"))
            {
                GD.PushWarning("[HitboxConfigLoader] No 'hitboxes' key in JSON");
                return new HitboxData();
            }

            var hitboxes = data["hitboxes"].AsGodotDictionary();
            if (!hitboxes.ContainsKey(hitboxId))
            {
                GD.PushWarning($"[HitboxConfigLoader] Hitbox '{hitboxId}' not found");
                return new HitboxData();
            }

            var hitboxData = hitboxes[hitboxId].AsGodotDictionary();
            var sizeData = hitboxData["size"].AsGodotDictionary();
            var offsetData = hitboxData["offset"].AsGodotDictionary();

            var config = new HitboxData
            {
                Size = new Vector3(
                    (float)sizeData["x"].AsDouble(),
                    (float)sizeData["y"].AsDouble(),
                    (float)sizeData["z"].AsDouble()
                ),
                Offset = new Vector3(
                    (float)offsetData["x"].AsDouble(),
                    (float)offsetData["y"].AsDouble(),
                    (float)offsetData["z"].AsDouble()
                ),
                Lifetime = (float)hitboxData["lifetime"].AsDouble(),
                Damage = (int)hitboxData["damage"].AsInt64()
            };

            _cachedConfigs[hitboxId] = config;
            GD.Print($"[HitboxConfigLoader] ✓ Loaded config '{hitboxId}': Size={config.Size}, Damage={config.Damage}");
            return config;
        }
    }
}
