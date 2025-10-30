using Godot;
using System;
using System.Collections.Generic;

public partial class DeckManager
{
	private List<CardData> _deck;

	public DeckManager(List<CardData> initialDeck)
	{
		_deck = new List<CardData>(initialDeck);
		Shuffle();
	}

	public void Shuffle()
	{
		var rng = new Random();
		for(int i = _deck.Count - 1; i > 0; i--)
		{
			int swapIndex = rng.Next(i + 1);
			(_deck[i], _deck[swapIndex]) = (_deck[swapIndex], _deck[i]);
		}
		GD.Print("DeckManager: Deck shuffled.");
	}

	public CardData DrawCard()
	{
		if(_deck.Count == 0)
		{
			GD.Print("DeckManager: Deck is empty. Cannot draw card.");
			return null;
		}
		var card = _deck[0];
		_deck.RemoveAt(0);
		GD.Print($"DeckManager: Drew card. Remaining cards: {_deck.Count}");
		return card;
	}


	public void AddtoDeck(CardData card)
	{
		_deck.Add(card);
		GD.Print("DeckManager: Card added to deck.");
	}

	public int GetDeckCount()
    {
        return _deck.Count;
    }
	public int RemainingCards => _deck.Count;
}
