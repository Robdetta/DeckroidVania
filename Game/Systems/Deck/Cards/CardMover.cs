using Godot;
using System.Collections.Generic;

public partial class CardMover
{
	public static void ShuffleLeft(List<Card> cards, Node2D slotArea)
	{
		if(cards.Count > 1)
		{

		    GD.Print("Shuffling Left...");
			//Rotate the first card to the end
			var firstCard = cards[0];
			cards.RemoveAt(0);
			cards.Add(firstCard);

			// Update card positions
			UpdateCardPositions(cards, slotArea);
		}
	}

	public static void ShuffleRight(List<Card> cards, Node2D slotArea)
	{
		if(cards.Count > 1)
		{
			GD.Print("Shuffling Right...");
			//Rotate the last card to the start
			var lastCard = cards[cards.Count - 1];
			cards.RemoveAt(cards.Count - 1);
			cards.Insert(0, lastCard);

			// Update card positions
			UpdateCardPositions(cards, slotArea);
		}
	}

	public static void UpdateCardPositions(List<Card> cards, Node2D slotArea)
	{
		for(int i = 0; i < cards.Count; i++)
		{
			var card = cards[i];
			var slot = slotArea.GetNode<Marker2D>($"Slot{i + 1}");

			//GD.Print($"Assigning Card {i} to Slot {slot.Name}: Position {slot.Position}");

			card.Position = slot.Position;

			if(i == 0)
			{
				card.Scale = new Vector2(1.5f, 1.5f);
				card.Modulate = new Color(1f, 1f, 1f, 1f);
			}
			else
			{
				card.Scale = new Vector2(1.0f, 1.0f);
				card.Modulate = new Color(1f, 1f, 1f, 0.5f);
			}
			//GD.Print($"Card {i}: Position {card.Position} -> Slot Position {slot.Position}");
		}
	}

}
