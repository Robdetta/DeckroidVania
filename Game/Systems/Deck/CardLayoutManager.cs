using Godot;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;

public partial class CardLayoutManager
{
    public static void UpdateLayout(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i]; // Highlight the first card
            
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
        }
    }

}