using Godot;
using System;
using System.Threading.Tasks;

public partial class UIManager : Node
{
	[Export] public PackedScene StartMenuScene;
	[Export] public PackedScene PauseMenuScene;
	[Export] public PackedScene DeckBuilderScene;
	[Export] public PackedScene CardHandScene { get; set; }
	private Node _cardHandInstance;

	[Export] public PackedScene GameplayHUDScene;
	private CanvasLayer _hudInstance;

	public void ShowStartMenu()      => ShowUI(StartMenuScene);
	public void ShowPauseMenu()      => ShowUI(PauseMenuScene);
	public void HideMenu()           => HideUI();

	private Node currentUI;

	public void ShowUI(PackedScene uiScene)
	{	
		if(currentUI != null)
		{
			currentUI.QueueFree();
			currentUI = null;
		}

		if(uiScene != null)
		{
			currentUI = uiScene.Instantiate();
			AddChild(currentUI);
			GD.Print($"Showing UI: {uiScene.ResourcePath}");
		}
	}

	public void HideUI()
	{
		if(currentUI != null)
		{
			currentUI.QueueFree();
			currentUI = null;
			GD.Print("Hiding UI");
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	
    private void OnHealthChanged(int cur, int max)
    {
        var hb = _hudInstance.GetNode<ProgressBar>("PlayerHealthBar");
        hb.MaxValue = max;
        hb.Value    = cur;
    }

	private void OnManaChanged(int cur, int max)
	{
		var mb = _hudInstance.GetNode<TextureProgressBar>("PlayerManaBar");
		mb.MaxValue = max;
		mb.Value = cur;
	}

    private void OnDeckCountChanged(int current, int total)
    {
        GD.Print($"OnDeckCountChanged called: {current}/{total}"); // Debug
        var deckLabel = _hudInstance?.GetNode<Label>("DeckCountLabel");
        if (deckLabel != null)
        {
            deckLabel.Text = $"Deck: {current}/{total}";
            GD.Print($"Label text set to: {deckLabel.Text}"); // Debug
        }
        else
        {
            GD.PrintErr("DeckCountLabel not found in GameplayHUD!");
        }
    }


	private void OnPlayerDied()
	{
		GD.Print("Player died!");
	}


	public void InitializeCardHand()
    {
        if (CardHandScene != null)
        {
            _cardHandInstance = CardHandScene.Instantiate();
            AddChild(_cardHandInstance);
        }
        else
        {
            GD.PrintErr("CardHandScene is null in World2D");
        }
    }

	private async void TryConnectDeckCount()
    {
        if (CardManager.Instance != null)
        {
            CardManager.Instance.Connect(
                "DeckCountChanged",  // Signal name from CardManager
                new Callable(this, nameof(OnDeckCountChanged))  // Call this method when signal fires
            );
        }
        else
        {
            await ToSignal(GetTree().CreateTimer(0.1), "timeout");
            TryConnectDeckCount();
        }
    }

	private async Task WaitForCardManager()
    {
        while (CardManager.Instance == null)
        {
            await Task.Delay(100); // Wait 100ms
        }
    }

	public async void ShowGameplayHUD()
	{
		if (GameplayHUDScene != null && _hudInstance == null)
		{
			_hudInstance = (CanvasLayer)GameplayHUDScene.Instantiate();
			AddChild(_hudInstance);

			// Grab the ProgressBar
			var healthBar = _hudInstance.GetNode<ProgressBar>("PlayerHealthBar");
			healthBar.MaxValue = HealthSystem.Instance.MaxHealth;
			healthBar.Value = HealthSystem.Instance.Current;

			var manaBar = _hudInstance.GetNode<TextureProgressBar>("PlayerManaBar");
			manaBar.MaxValue = ManaSystem.Instance.MaxMana;
			manaBar.Value = ManaSystem.Instance.Current;


			// Wait for CardManager to be ready
            await WaitForCardManager();

			// Initialize deck count display
			if (CardManager.Instance != null)
			{
				var deckLabel = _hudInstance.GetNode<Label>("DeckCountLabel");
				if (deckLabel != null)
				{
					int deckCount = CardManager.Instance.GetDeckCount();
					int totalDeckSize = CardManager.Instance.GetTotalDeckSize();
					deckLabel.Text = $"Deck: {deckCount}/{totalDeckSize}";
				}
				else
				{
					GD.PrintErr("DeckCountLabel not found in GameplayHUD!");
				}
			}

			// Subscribe to live updates
			if (HealthSystem.Instance != null)
			{
				HealthSystem.Instance.Connect(
					"HealthChanged",
					new Callable(this, nameof(OnHealthChanged))
				);
				HealthSystem.Instance.Connect(
					"Died",
					new Callable(this, nameof(OnPlayerDied))
				);
			}
			else
			{
				GD.PrintErr("HealthSystem.Instance is null!");
			}

			if (ManaSystem.Instance != null)
			{
				ManaSystem.Instance.Connect(
					"ManaChanged",
					new Callable(this, nameof(OnManaChanged))
				);
			}
			else
			{
				GD.PrintErr("ManaSystem.Instance is null!");
			}

			TryConnectDeckCount();
		}
		else
		{
			GD.PrintErr("GameplayHUDScene is null or _hudInstance already exists!");
		}
	}

	public void HideGameplayHUD()
	{
		if (_hudInstance != null)
		{
			_hudInstance.QueueFree();
			_hudInstance = null;
		}
	}


	public void ShowCardHand()
	{
		if (CardHandScene != null && _cardHandInstance == null)
		{
			_cardHandInstance = CardHandScene.Instantiate();
			AddChild(_cardHandInstance);
		}
	}

	public void HideCardHand()
	{
		if (_cardHandInstance != null)
		{
			_cardHandInstance.QueueFree();
			_cardHandInstance = null;
		}
	}

}
