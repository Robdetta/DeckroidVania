using Godot;
using System.Collections.Generic;
using System.Text.Json;

public static class DeckBuilder
{
     
    /// Creates a default deck based on loaded JSON cards.
    
    public static DeckData CreateDefaultDeck(List<CardData> allCards, int copiesPerCard = 10)
    {
        var deck = new DeckData { Name = "Tutorial Deck" };
        foreach (var card in allCards)
        {
            for (int i = 0; i < copiesPerCard; i++)
            {
                deck.CardIds.Add(card.Id);
            }
        }
        
        GD.Print($"DeckBuilder: Created a fresh temporary deck containing {deck.CardIds.Count} cards ({copiesPerCard} copies per card).");
        return deck;
    }

     
    /// Saves the current deck data directly into Godot's local persistent user folder.
    
    public static void SaveDeck(DeckData deck, string fileName)
    {
        var json = JsonSerializer.Serialize(deck, new JsonSerializerOptions { WriteIndented = true });
        var path = $"user://{fileName}";
        
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(json);
            GD.Print($"DeckBuilder: Successfully saved deck containing {deck.CardIds.Count} cards to: {path}");
        }
        else
        {
            GD.PrintErr($"DeckBuilder: Failed to write to save file path: {path}");
        }
    }

     
    /// Loads the saved deck from the persistent user folder. 
    /// If forceReload is true, it ignores the saved file and recreates a fresh default deck.
    
    public static DeckData LoadDeck(string fileName, bool forceReload = false)
    {
        var path = $"user://{fileName}";
        
        if (forceReload)
        {
            GD.Print("DeckBuilder: ForceReload is active. Deleting saved user file to regenerate deck...");
            if (FileAccess.FileExists(path))
            {
                DirAccess.RemoveAbsolute(path);
            }
            return null; // Forces CardManager to fall back and generate a fresh default deck
        }

        if (!FileAccess.FileExists(path))
        {
            GD.Print($"DeckBuilder: No save file found at {path}. A fresh default deck will be created.");
            return null;
        }
            
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        var json = file.GetAsText();
        
        var loadedDeck = JsonSerializer.Deserialize<DeckData>(json);
        GD.Print($"DeckBuilder: Loaded saved deck from disk containing {loadedDeck?.CardIds.Count ?? 0} cards.");
        return loadedDeck;
    }
}