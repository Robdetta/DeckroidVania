using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class CardManager : Node
{
    [Signal]
    public delegate void DeckCountChangedEventHandler(int current, int total);
    [Export] private PackedScene _cardScene = GD.Load<PackedScene>("uid://pi5b3ukve1re");
    [Export] private NodePath _inputManagerPath;
    public static CardManager Instance { get; private set; }


    private InputManager _inputManager;
    private DeckManager _deckManager;
    private HandManager _handManager;
    private CardSpawner _cardSpawner;
    private bool _isPaused = false;
    private Timer _spawnTimer;
    private int _totalDeckSize = 0; // Store initial deck size

    private float _spawnInterval = 3f;

    //private List<CardData> _cardsData = new List<CardData>();
    private List<Card> _activeCards = new List<Card>();
    private int _currentCardIndex = 0;
    private Node2D _slotArea;

    // Called when the node enters the scene tree for the first time.
    public async override void _Ready()
    {
        Instance = this;

        var slotArea = GetNode<Node2D>("../CardUI/CardDisplay/SlotArea");
        _handManager = new HandManager(slotArea);

        var allCards = await CardLoader.LoadCardsFromJson("Data/cards.json");
        var deckData = DeckBuilder.LoadDeck("deck1.json"); // currently 5x of each card, 25 total with 5 cards made
        if (deckData == null)
        {
            deckData = DeckBuilder.CreateDefaultDeck(allCards, 5);
            DeckBuilder.SaveDeck(deckData, "deck1.json");
        }
        var initialDeck = deckData.CardIds
            .Select(id => allCards.First(card => card.Id == id))
            .ToList();
        _totalDeckSize = initialDeck.Count;
        _deckManager = new DeckManager(initialDeck);


        _cardSpawner = new CardSpawner(_deckManager, _handManager, _cardScene, this);

        // Spawn the first card immediately
        _cardSpawner.SpawnCard();

        // Get the InputManager	
        _inputManager = GetNode<InputManager>(_inputManagerPath);
        _inputManager.Connect(nameof(InputManager.ShuffleLeft), new Callable(this, nameof(OnShuffleLeft)));
        _inputManager.Connect(nameof(InputManager.ShuffleRight), new Callable(this, nameof(OnShuffleRight)));
        _inputManager.Connect(nameof(InputManager.ActivateCard), new Callable(this, nameof(OnActivateCard)));
        // Shuffle the deck before adding cards

        // Create and configure the spawn timer
        _spawnTimer = new Timer();
        _spawnTimer.WaitTime = _spawnInterval;
        _spawnTimer.OneShot = false;
        _spawnTimer.Autostart = true;
        AddChild(_spawnTimer);
        _spawnTimer.Timeout += OnSpawnTimerTimeout;

        StartSpawning();
    }

    // Example: Adjust spawn interval in response to an event
    public void StartSpawning()
    {
        if (!_isPaused)
            _spawnTimer.Start();
    }
    public void StopSpawning()
    {
        _spawnTimer.Stop();
    }

    public void PauseSpawning()
    {
        _isPaused = true;
        _spawnTimer.Stop();
    }

    public void ResumeSpawning()
    {
        _isPaused = false;
        if (!_handManager.IsFull)
            _spawnTimer.Start();
    }

    public void SetSpawnInterval(float interval)
    {
        _spawnInterval = interval;
        _spawnTimer.WaitTime = interval;
    }

    private void OnActivateCard()
    {
        var selectedCard = _handManager.GetHighlightedCard();
        if (selectedCard == null)
            return;

        int manaCost = selectedCard.ManaCostValue;

        if (ManaSystem.Instance.Current >= manaCost)
        {
            ManaSystem.Instance.SpendMana(manaCost);
            GD.Print($"=== CARD PLAYED ===");
            GD.Print($"Played card: {selectedCard.CardNameText}, spent {manaCost} mana!");
            GD.Print($"Hand count before removal: {_handManager.GetCardCount()}");
            
            _handManager.RemoveCard(selectedCard);
            
            GD.Print($"Hand count after removal: {_handManager.GetCardCount()}");
            GD.Print($"Deck count: {_deckManager.GetDeckCount()}");
            GD.Print($"==================");
            
            StartSpawning();
        }
        else
        {
            GD.Print("Not enough mana to play this card!");
        }
    }

    private void OnSpawnTimerTimeout()
    {
        if (_isPaused || _handManager.IsFull)
        {
            _spawnTimer.Stop();
            return;
        }
        
        int deckCountBefore = _deckManager.GetDeckCount();
        int handCountBefore = _handManager.GetCardCount();
        
        GD.Print($"=== BEFORE SPAWN ===");
        GD.Print($"Deck Count: {deckCountBefore}");
        GD.Print($"Hand Count: {handCountBefore}");
        
        _cardSpawner.SpawnCard();
        
        int deckCountAfter = _deckManager.GetDeckCount();
        int handCountAfter = _handManager.GetCardCount();
        
        GD.Print($"=== AFTER SPAWN ===");
        GD.Print($"Deck Count: {deckCountAfter} (Difference: {deckCountBefore - deckCountAfter})");
        GD.Print($"Hand Count: {handCountAfter} (Difference: {handCountAfter - handCountBefore})");
        GD.Print($"==================");
        
        // Emit signal after spawning
        EmitSignal(SignalName.DeckCountChanged, deckCountAfter, _totalDeckSize);

        if (_handManager.IsFull)
            _spawnTimer.Stop();
    }

    public int GetDeckCount()
    {
        return _deckManager?.GetDeckCount() ?? 0;
    }

    public int GetTotalDeckSize()
    {
        return _totalDeckSize;
    }


    // Delegate the timer callback to CardSpawner
    private void OnShuffleLeft()
    {
        GD.Print("Shuffle Left");
        ShuffleLeft();
    }

    private void OnShuffleRight()
    {
        GD.Print("Shuffle Right");
        ShuffleRight();
    }

    public void ShuffleLeft()
    {
        GD.Print("CardManager: Calling CardMover.ShuffleLeft");
        CardMover.ShuffleLeft(_handManager.GetActiveCards(), _handManager.GetSlotArea());
        _handManager.UpdateLayout();

    }

    public void ShuffleRight()
    {
        GD.Print("CardManager: Calling CardMover.ShuffleRight");
        CardMover.ShuffleRight(_handManager.GetActiveCards(), _handManager.GetSlotArea());
        _handManager.UpdateLayout();
    }


    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}



