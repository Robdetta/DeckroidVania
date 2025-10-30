using Godot;
using System.Collections.Generic;
using System.Text.Json;

public static class DeckBuilder
{
    public static DeckData CreateDefaultDeck(List<CardData> allCards, int copiesPerCard = 5)
    {
        var deck = new DeckData { Name = "Tutorial Deck" };
        foreach (var card in allCards)
        {
            for (int i = 0; i < copiesPerCard; i++)
                deck.CardIds.Add(card.Id);
        }
        return deck;
    }

    public static void SaveDeck(DeckData deck, string fileName)
    {
        var json = JsonSerializer.Serialize(deck, new JsonSerializerOptions { WriteIndented = true });
        var path = $"user://{fileName}";
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        file.StoreString(json);
    }

    public static DeckData LoadDeck(string fileName)
    {
        var path = $"user://{fileName}";
        if (!FileAccess.FileExists(path))
            return null;
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        return JsonSerializer.Deserialize<DeckData>(json);
    }
}