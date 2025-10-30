using Godot;
using System;

public partial class Card : Control
{
    [Export] public string CardNameText;
    [Export] public string CardTypeText;
    [Export] public int ManaCostValue;
    [Export] public string ActivateEffectText;
    [Export] public string SacrificeEffectText;

    private Label _cardNameLabel;
    private Label _cardTypeLabel;
    private Label _manaCostLabel;
    private Label _activateEffectLabel;
    private Label _sacrificeEffectLabel;
    private Sprite2D _cardArtSprite;
    private CardData _pendingCardData;

    public override void _Ready()
    {
        GD.Print("Card _Ready Called");

        _cardNameLabel = GetNode<Label>("CardVisuals/Name/CardName");
        _cardTypeLabel = GetNode<Label>("CardVisuals/Type/CardType");
        _manaCostLabel = GetNode<Label>("CardVisuals/Mana/ManaCost");
        _activateEffectLabel = GetNode<Label>("CardVisuals/Activate/ActivateEffect");
        _sacrificeEffectLabel = GetNode<Label>("CardVisuals/Sacrifice/SacrificeEffect");
        _cardArtSprite = GetNode<Sprite2D>("CardVisuals/CardArt");


        if (_cardNameLabel == null) GD.PrintErr("CardNameLabel node not found in _Ready");
        if (_manaCostLabel == null) GD.PrintErr("ManaCostLabel node not found in _Ready");

        UpdateCard();
    }

    private void UpdateCard()
    {
        //GD.Print("Updating card visuals...");

        // if(_cardNameLabel != null)
        //    _cardNameLabel.Text = CardNameText;
        if (_cardNameLabel != null)
        {
            //GD.Print("CardNameLabel is valid");
            _cardNameLabel.Text = CardNameText;
        }
        else
        {
            GD.PrintErr("CardNameLabel is null in UpdateCard");
        }

        if(_cardTypeLabel != null)
            _cardTypeLabel.Text = CardTypeText;
            
        if (_manaCostLabel != null)
        {
            //GD.Print("ManaCostLabel is valid");
            _manaCostLabel.Text = ManaCostValue.ToString();
        }
        else
        {
            GD.PrintErr("ManaCostLabel is null in UpdateCard");
        }
        // if(_manaCostLabel != null)
        //     _manaCostLabel.Text = ManaCostValue.ToString();
        
        if(_activateEffectLabel != null)
            _activateEffectLabel.Text = ActivateEffectText;
        
        if(_sacrificeEffectLabel != null)
            _sacrificeEffectLabel.Text = SacrificeEffectText;
    }

    public void Initialize(string cardName, string cardType, int manaCost, string activateEffect, string sacrificeEffect)    
    {
        // Store incoming values into the exported fields so other code can read them
        CardNameText = cardName;
        CardTypeText = cardType;
        ManaCostValue = manaCost;
        ActivateEffectText = activateEffect;
        SacrificeEffectText = sacrificeEffect;

        // Set the node name to match the card title
        this.Name = CardNameText;

        // Update the visual labels
        UpdateCard();
    }

}
