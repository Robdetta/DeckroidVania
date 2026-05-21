using System.Text.Json.Serialization;
using System.Collections.Generic;
//using Godot.Collections;

public class CardData
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("cardName")]
    public string CardName { get; set; }

    [JsonPropertyName("type")]
    public string CardType { get; set; }

    [JsonPropertyName("manaCost")]
    public int ManaCost { get; set; }

    [JsonPropertyName("activateEffectDesc")]
    public string ActivateEffectDesc { get; set; }

    // CRITICAL: Must be a List of EffectData, NOT a string!
    [JsonPropertyName("activateEffects")]
    public List<EffectData> ActivateEffects { get; set; } = new List<EffectData>();
}

public class EffectData
{
    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("params")]
    public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
}