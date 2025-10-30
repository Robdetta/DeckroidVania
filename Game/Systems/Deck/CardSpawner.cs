using Godot;
using System;

public partial class CardSpawner
{
	private DeckManager _deckManager;
	private HandManager _handManager;
	private PackedScene _cardScene;
	private Node _owner;

	public CardSpawner(DeckManager deckManager, HandManager handManager, PackedScene cardScene, Node owner)
	{
		_deckManager = deckManager;
		_handManager = handManager;
		_cardScene = cardScene;
		_owner = owner;
	}

	public void SpawnCard()
	{
		//GD.Print($"Active cards: {_handManager.CardCount}");
		if(_handManager.IsFull)
		{
			//GD.Print("CardSpawner: Max cards reached.");
			return;
		}

		var cardData = _deckManager.DrawCard();
		if (cardData == null)
		{
			GD.Print("CardSpawner: No cards left in the deck.");
			return;
		}

		var cardInstance = (Card)_cardScene.Instantiate();
		cardInstance.CallDeferred(
			"Initialize",
			cardData.CardName,
			cardData.CardType,
			cardData.ManaCost,
			cardData.ActivateEffect,
			cardData.SacrificeEffect
		);
		_handManager.AddCard(cardInstance);
	}
}
