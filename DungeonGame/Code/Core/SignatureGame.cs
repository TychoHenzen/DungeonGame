#region

using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Enums;
using DungeonGame.Code.Interfaces;
using DungeonGame.Code.Models;
using DungeonGame.Code.States;
using DungeonGame.Code.Systems;
using DungeonGame.Const;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace DungeonGame.Code.Core;

/// <inheritdoc />
/// <summary>
///     Main game class
/// </summary>
public class SignatureGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    protected Dungeon _currentDungeon;

    // Game state
    private GameState _currentState;

    // Game resources
    private SpriteFont _defaultFont;
    protected DungeonResult? _dungeonResult;
    protected Item?[] _dungeonSlotItems;
    private Dictionary<GameStateType, GameState> _gameStates;
    protected Inventory _inventory;

    // Game data
    protected Player _player;
    protected bool _runningDungeon;
    protected float _runTimer;
    private Item _selectedDungeonItem;
    private SpriteFont _smallFont;
    private SpriteBatch _spriteBatch;
    private Texture2D _uiTexture;

    public SignatureGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics.PreferredBackBufferWidth = Constants.UI.DefaultScreenWidth;
        _graphics.PreferredBackBufferHeight = Constants.UI.DefaultScreenHeight;
        Window.AllowUserResizing = true;
    }

    protected override void Initialize()
    {
        // Initialize game states
        _gameStates = new Dictionary<GameStateType, GameState>
        {
            { GameStateType.MainMenu, new MainMenuState(this) },
            { GameStateType.Inventory, new InventoryState(this) },
            { GameStateType.Dungeon, new DungeonState(this) },
            { GameStateType.Combat, new CombatState(this) }
        };

        // Set initial state
        _currentState = _gameStates[GameStateType.MainMenu];
        _dungeonSlotItems = new Item[Constants.Dungeon.DefaultDungeonSlots];

        // Initialize player and inventory
        _player = new Player();
        _inventory = new Inventory(Constants.Game.DefaultInventoryCapacity);

        // Generate starter items
        for (var i = 0; i < 3; i++)
        {
            _inventory.AddItem(ItemGenerator.GenerateRandomItem());
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load fonts
        _defaultFont = Content.Load<SpriteFont>("Fonts/DefaultFont");
        _smallFont = Content.Load<SpriteFont>("Fonts/SmallFont");

        try
        {
            // Load UI texture
            _uiTexture = Content.Load<Texture2D>("UIBox");
        }
        catch (ContentLoadException)
        {
            // Create a 1x1 white texture as fallback
            _uiTexture = new Texture2D(GraphicsDevice, 1, 1);
            _uiTexture.SetData(new[] { Color.White });
        }

        // Load state content
        foreach (var state in _gameStates.Values)
        {
            state.LoadContent();
            if (state is ITextureUser textureUser)
            {
                textureUser.SetTexture(_uiTexture);
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        // Exit on Escape key
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed ||
            Keyboard.GetState().IsKeyDown(Keys.Escape))
        {
            Exit();
        }

        // Update current state
        _currentState.Update(gameTime);

        // Update dungeon run if active
        if (_runningDungeon && _currentDungeon != null)
        {
            // Increment timer to visualize exploration progress
            _runTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Simulate dungeon after a short time to allow for visualization
            if (_runTimer >= Constants.Dungeon.DungeonSimulationDelay)
            {
                CompleteDungeon();
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw current state
        _currentState.Draw(_spriteBatch, _defaultFont, _smallFont);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public virtual void ChangeState(GameStateType newState)
    {
        _currentState = _gameStates[newState];
    }

    public void StartDungeon(Item selectedItem, int slotIndex = 0)
    {
        if (selectedItem == null)
        {
            return;
        }

        _selectedDungeonItem = selectedItem;
        _currentDungeon = DungeonGenerator.GenerateDungeon(selectedItem.Signature);
        _runningDungeon = true;
        _runTimer = 0;
        _dungeonResult = null;

        // Consume the item - it's used to create the dungeon
        _dungeonSlotItems[slotIndex] = null;

        ChangeState(GameStateType.Dungeon);
    }

    private void CompleteDungeon()
    {
        _runningDungeon = false;

        // Run the dungeon exploration simulation
        var dungeonExplorer = new DungeonExplorer(_currentDungeon, _player);
        _dungeonResult = dungeonExplorer.ExploreAutomatically();

        // Add loot to inventory
        if (_dungeonResult.Success && _dungeonResult.Loot.Count > 0)
        {
            foreach (var item in _dungeonResult.Loot)
            {
                _inventory.AddItem(item);
            }
        }

        if (_dungeonResult.Success && _dungeonSlotItems.Any(item => item != null))
        {
            // Unlock the next slot after successful run
            UnlockNextDungeonSlot();
        }
    }

    public Item? GetDungeonSlotItem(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _dungeonSlotItems.Length)
        {
            return _dungeonSlotItems[slotIndex];
        }

        return null;
    }

    public void SetDungeonSlotItem(Item item, int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _dungeonSlotItems.Length)
        {
            _dungeonSlotItems[slotIndex] = item;
        }
    }

    public void ClearDungeonSlot(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < _dungeonSlotItems.Length)
        {
            _dungeonSlotItems[slotIndex] = null;
        }
    }

    public void UnlockNextDungeonSlot()
    {
        var inventoryState = _gameStates[GameStateType.Inventory] as InventoryState;
        inventoryState?.UnlockDungeonSlot();
    }

    // Getters for game objects
    public Player GetPlayer()
    {
        return _player;
    }

    public Inventory GetInventory()
    {
        return _inventory;
    }

    public Dungeon GetCurrentDungeon()
    {
        return _currentDungeon;
    }

    public DungeonResult GetDungeonResult()
    {
        return _dungeonResult;
    }

    public bool IsRunningDungeon()
    {
        return _runningDungeon;
    }

    public float GetRunTimer()
    {
        return _runTimer;
    }

    public Item GetSelectedDungeonItem()
    {
        return _selectedDungeonItem;
    }

    // Setters
    public void SetSelectedDungeonItem(Item item)
    {
        _selectedDungeonItem = item;
    }
}