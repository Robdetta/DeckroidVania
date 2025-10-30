using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DeckroidVania.Game.Entities.Enemies.Data
{
    public class AttackRange
    {
        [JsonPropertyName("minDistance")]
        public float MinDistance { get; set; }

        [JsonPropertyName("maxDistance")]
        public float MaxDistance { get; set; }

        [JsonPropertyName("attacks")]
        public List<string> Attacks { get; set; } = new List<string>();
    }

    public class AttackRanges
    {
        [JsonPropertyName("melee")]
        public AttackRange Melee { get; set; }

        [JsonPropertyName("ranged")]
        public AttackRange Ranged { get; set; }

        // Future: Could add "aoe", "defensive", "support" ranges
    }
}
