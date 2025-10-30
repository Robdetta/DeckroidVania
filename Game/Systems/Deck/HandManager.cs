using Godot;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

public partial class HandManager
{
    private Node2D _slotArea;
    private List<Card> _activeCards;

    public HandManager(Node2D slotArea)
    {
        _slotArea = slotArea;
        _activeCards = new List<Card>();
    }

    public Node2D GetSlotArea() => _slotArea;
    public List<Card> GetActiveCards() => _activeCards;

    // Add a card to the hand
    public void AddCard(Card card)
    {
        _activeCards.Add(card);
        _slotArea.AddChild(card);
        UpdateLayout();
    }

    public void UpdateLayout()
    {
        for (int i = 0; i < _activeCards.Count; i++)
        {
            var card = _activeCards[i];
            var slot = _slotArea.GetNode<Marker2D>($"Slot{i + 1}");

            card.Position = slot.Position;

            if (i == 0)
            {
                card.Scale = new Vector2(1.5f, 1.5f);
                card.Modulate = new Color(1f, 1f, 1f, 1f); // Highlight the first card
            }
            else
            {
                card.Scale = new Vector2(1.0f, 1.0f);
                card.Modulate = new Color(1f, 1f, 1f, 0.5f); // Dim other cards
            }
        }
    }
    // Update slot placeholders (e.g., visibility for empty slots)
    public void UpdateSlotPlaceholders()
    {
        for (int i = 0; i < 5; i++)
        {
            var slot = _slotArea.GetNode<Marker2D>($"Slot{i + 1}");
            if (i < _activeCards.Count)
            {
                slot.Modulate = new Color(1f, 1f, 1f, 1f); // Fully visible
            }
            else
            {
                slot.Modulate = new Color(1f, 1f, 1f, 0.3f); // Dim for empty
            }
        }
    }

    public void RemoveCard(Card card)
    {
        if (card == null) return;
        if (_activeCards.Contains(card))
        {
            _activeCards.Remove(card);
            card.QueueFree(); // Remove the card node from the scene
            UpdateLayout();   // Rearrange the hand visually
        }
    }

    public Card GetHighlightedCard()
    {
        if (_activeCards.Count == 0)
            return null;
        return _activeCards[0];
    }

    public int GetCardCount()
    {
        return _activeCards.Count;
    }


    // Check if the hand is full
    public bool IsFull => _activeCards.Count >= 5;
    // Return the current cards in hand
    public List<Card> Cards => _activeCards;
    // Get the current count of cards in hand
    public int CardCount => _activeCards.Count;

}