using Godot;
using System.Collections.Generic;
using System.Text.Json;

namespace DeckroidVania.Game.Entities.Enemies.Data
{
    public class AttackDefinitionsFile
    {
        public List<EnemyAttackData> Attacks { get; set; }  // Changed to EnemyAttackData
    }

    public static class AttackDatabase
    {
        private static Dictionary<string, EnemyAttackData> _attacks = new Dictionary<string, EnemyAttackData>();  // Changed to EnemyAttackData
        private static bool _isInitialized = false;

        private static readonly string ATTACKS_FILE_PATH = "res://Game/Entities/Enemies/Data/EnemyAttacks.json";

        public static void Initialize()
        {
            if (_isInitialized)
                return;

            LoadAllAttacks();

            _isInitialized = true;
            GD.Print($"[AttackDatabase] Loaded {_attacks.Count} attack definitions");
        }

        private static void LoadAllAttacks()
        {
            try
            {
                string jsonText = FileAccess.GetFileAsString(ATTACKS_FILE_PATH);

                if (string.IsNullOrEmpty(jsonText))
                {
                    GD.PushError($"[AttackDatabase] Attack definitions file not found: {ATTACKS_FILE_PATH}");
                    return;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var attackFile = JsonSerializer.Deserialize<AttackDefinitionsFile>(jsonText, options);

                if (attackFile?.Attacks == null)
                {
                    GD.PushError("[AttackDatabase] Failed to deserialize attack definitions");
                    return;
                }

                foreach (var attack in attackFile.Attacks)
                {
                    if (attack != null && !string.IsNullOrEmpty(attack.Id))
                    {
                        _attacks[attack.Id] = attack;
                        GD.Print($"[AttackDatabase] Loaded: {attack.Id} - {attack.Name}");
                    }
                }
            }
            catch (System.Exception e)
            {
                GD.PushError($"[AttackDatabase] Error loading: {e.Message}");
            }
        }

        public static EnemyAttackData GetAttack(string attackId)  // Changed return type to EnemyAttackData
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            if (_attacks.TryGetValue(attackId, out var attack))
            {
                return attack;
            }

            GD.PushError($"[AttackDatabase] Attack not found: {attackId}");
            return null;
        }

        public static bool HasAttack(string attackId)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            return _attacks.ContainsKey(attackId);
        }

        public static List<EnemyAttackData> GetAllAttacks()  // Changed return type to EnemyAttackData
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            return new List<EnemyAttackData>(_attacks.Values);
        }

        public static void Reload()
        {
            _attacks.Clear();
            _isInitialized = false;
            Initialize();
        }
    }
}