using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Interfaces;
using DungeonGame.Code.Models;
using DungeonGame.Code.States;
using DungeonGame.Code.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonGame.Code.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Main game class
    /// </summary>
    public class SignatureGame : Game
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Texture2D _uiTexture;

        // Game state
        private GameState _currentState;
        private Dictionary<GameStateType, GameState> _gameStates;

        // Game resources
        private SpriteFont _defaultFont;
        private SpriteFont _smallFont;

        // Game data
        private Player _player;
        private Inventory _inventory;
        private Dungeon _currentDungeon;
        private Item[] _dungeonSlotItems;
        private Item _selectedDungeonItem;
        private DungeonResult _dungeonResult;
        private bool _runningDungeon;
        private float _runTimer;

        public SignatureGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = 1280;
            _graphics.PreferredBackBufferHeight = 720;
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
            _dungeonSlotItems = new Item[3]; // 3 slots that can be unlocked over time

            // Initialize player and inventory
            _player = new Player();
            _inventory = new Inventory(20); // 20 slots

            // Generate starter items
            for (int i = 0; i < 3; i++)
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
                Exit();

            // Update current state
            _currentState.Update(gameTime);

            // Update dungeon run if active
            if (_runningDungeon && _currentDungeon != null)
            {
                // Increment timer to visualize exploration progress
                _runTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Simulate dungeon after a short time to allow for visualization
                if (_runTimer >= 3.0f) // Wait 3 seconds before simulating
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
            if (selectedItem == null) return;

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

            if (_dungeonResult.Success && _dungeonSlotItems.Count(item => item != null) == 0)
            {
                // Unlock the next slot after successful run
                UnlockNextDungeonSlot();
            }
        }

        public Item GetDungeonSlotItem(int slotIndex)
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
        public Player GetPlayer() => _player;
        public Inventory GetInventory() => _inventory;
        public Dungeon GetCurrentDungeon() => _currentDungeon;
        public DungeonResult GetDungeonResult() => _dungeonResult;
        public bool IsRunningDungeon() => _runningDungeon;
        public float GetRunTimer() => _runTimer;
        public Item GetSelectedDungeonItem() => _selectedDungeonItem;

        // Setters
        public void SetSelectedDungeonItem(Item item) => _selectedDungeonItem = item;
    }
}