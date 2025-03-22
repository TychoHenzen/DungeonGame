using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonGame;

/// <summary>
/// Inventory state
/// </summary>
public class InventoryState : GameState, ITextureUser
{
    private Rectangle[] _itemSlots;
    private Rectangle[] _equipmentSlots;
    private Rectangle _dungeonButton;
    private Rectangle _equipButton;  // New equip button
    private Texture2D _texture;
    private MouseState _previousMouseState;  // Track previous mouse state
        
    public InventoryState(DungeonGame.SignatureGame game) : base(game) { }
        
    public override void LoadContent()
    {
        // Get screen dimensions for responsive layout
        int screenWidth = Game.GraphicsDevice.Viewport.Width;
        int screenHeight = Game.GraphicsDevice.Viewport.Height;
        
        // Create item slot rectangles - moved to left side with smaller width
        _itemSlots = new Rectangle[20];
        for (int i = 0; i < 20; i++)
        {
            int x = 50 + (i % 4) * 180;
            int y = 100 + (i / 4) * 120;
            _itemSlots[i] = new Rectangle(x, y, 160, 100);
        }
            
        // Create equipment slot rectangles - moved further right to avoid overlap
        string[] slots = {"weapon", "shield", "helmet", "armor", "amulet", "ring", "boots"};
        _equipmentSlots = new Rectangle[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            int column = i % 2;
            int row = i / 2;
            _equipmentSlots[i] = new Rectangle(screenWidth - 450 + column * 220, 100 + row * 120, 160, 100);
        }
            
        // Create buttons at bottom of screen
        _dungeonButton = new Rectangle(screenWidth - 250, screenHeight - 120, 200, 50);
        _equipButton = new Rectangle(screenWidth - 470, screenHeight - 120, 200, 50);
        
        _previousMouseState = Mouse.GetState();
    }
        
    public override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        bool isNewClick = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
            
        // Check for mouse clicks
        if (isNewClick)
        {
            // Check item slots for selection
            for (int i = 0; i < _itemSlots.Length; i++)
            {
                if (_itemSlots[i].Contains(mouseState.Position))
                {
                    var inventory = Game.GetInventory();
                    if (i < inventory.Items.Count)
                    {
                        // Select item for dungeon or equip
                        Game.SetSelectedDungeonItem(inventory.Items[i]);
                    }
                    break;
                }
            }
                
            // Check equip button - new functionality
            if (_equipButton.Contains(mouseState.Position) && Game.GetSelectedDungeonItem() != null)
            {
                // Equip the selected item
                Game.GetPlayer().EquipItem(Game.GetSelectedDungeonItem());
            }
                
            // Check dungeon button
            if (_dungeonButton.Contains(mouseState.Position) && Game.GetSelectedDungeonItem() != null)
            {
                Game.StartDungeon(Game.GetSelectedDungeonItem());
            }
            
            // Check equipment slots for unequipping (optional click on equipped item to unequip)
            for (int i = 0; i < _equipmentSlots.Length; i++)
            {
                if (_equipmentSlots[i].Contains(mouseState.Position))
                {
                    string[] slots = {"weapon", "shield", "helmet", "armor", "amulet", "ring", "boots"};
                    Game.GetPlayer().UnequipItem(slots[i]);
                    break;
                }
            }
        }
        
        // Update previous mouse state
        _previousMouseState = mouseState;
    }
        
    public override void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
        // Draw inventory title
        spriteBatch.DrawString(defaultFont, "Inventory", new Vector2(50, 50), Color.White);
            
        // Draw equipment title
        spriteBatch.DrawString(defaultFont, "Equipment", new Vector2(Game.GraphicsDevice.Viewport.Width - 450, 50), Color.White);
            
        // Draw inventory slots
        var inventory = Game.GetInventory();
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(_texture, _itemSlots[i], Color.Gray * 0.5f);
                
            // Draw item if exists
            if (i < inventory.Items.Count)
            {
                var item = inventory.Items[i];
                    
                // Highlight if selected
                if (item == Game.GetSelectedDungeonItem())
                {
                    spriteBatch.Draw(_texture, _itemSlots[i], Color.Purple * 0.5f);
                }
                    
                // Draw item name
                spriteBatch.DrawString(smallFont, item.Name, 
                    new Vector2(_itemSlots[i].X + 10, _itemSlots[i].Y + 10), Color.White);
                    
                // Draw item power
                spriteBatch.DrawString(smallFont, $"Power: {item.Power}", 
                    new Vector2(_itemSlots[i].X + 10, _itemSlots[i].Y + 30), Color.White);
                    
                // Draw item type
                spriteBatch.DrawString(smallFont, $"Type: {item.Type}", 
                    new Vector2(_itemSlots[i].X + 10, _itemSlots[i].Y + 50), Color.White);
                    
                // Draw item signature indicator
                DrawSignatureIndicator(spriteBatch, item.Signature, 
                    new Vector2(_itemSlots[i].X + 10, _itemSlots[i].Y + 70), 140, 20);
            }
        }
            
        // Draw equipment slots
        string[] slots = {"Weapon", "Shield", "Helmet", "Armor", "Amulet", "Ring", "Boots"};
        for (int i = 0; i < _equipmentSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(_texture, _equipmentSlots[i], Color.DarkGray * 0.5f);
                
            // Draw slot name
            spriteBatch.DrawString(smallFont, slots[i], 
                new Vector2(_equipmentSlots[i].X + 10, _equipmentSlots[i].Y + 10), Color.White);
                
            // Draw equipped item if exists
            var player = Game.GetPlayer();
            var equippedItem = player.GetEquippedItem(slots[i].ToLower());
            if (equippedItem != null)
            {
                // Draw item name
                spriteBatch.DrawString(smallFont, equippedItem.Name, 
                    new Vector2(_equipmentSlots[i].X + 10, _equipmentSlots[i].Y + 30), Color.White);
                    
                // Draw item power
                spriteBatch.DrawString(smallFont, $"Power: {equippedItem.Power}", 
                    new Vector2(_equipmentSlots[i].X + 10, _equipmentSlots[i].Y + 50), Color.White);
                    
                // Draw signature indicator
                DrawSignatureIndicator(spriteBatch, equippedItem.Signature, 
                    new Vector2(_equipmentSlots[i].X + 10, _equipmentSlots[i].Y + 70), 140, 20);
            }
        }
            
        // Draw buttons
        spriteBatch.Draw(_texture, _dungeonButton, Game.GetSelectedDungeonItem() != null ? Color.Green : Color.Gray);
        spriteBatch.DrawString(smallFont, "Start Dungeon", 
            new Vector2(_dungeonButton.X + 40, _dungeonButton.Y + 15), Color.White);
            
        // Draw new equip button
        spriteBatch.Draw(_texture, _equipButton, Game.GetSelectedDungeonItem() != null ? Color.Blue : Color.Gray);
        spriteBatch.DrawString(smallFont, "Equip Item", 
            new Vector2(_equipButton.X + 60, _equipButton.Y + 15), Color.White);
            
        // Draw selected item info if exists
        if (Game.GetSelectedDungeonItem() != null)
        {
            var item = Game.GetSelectedDungeonItem();
            spriteBatch.DrawString(defaultFont, "Selected Item:", new Vector2(50, Game.GraphicsDevice.Viewport.Height - 180), Color.White);
            spriteBatch.DrawString(smallFont, item.Name, new Vector2(200, Game.GraphicsDevice.Viewport.Height - 180), Color.White);
            spriteBatch.DrawString(smallFont, $"Type: {item.Type} | Slot: {item.Slot} | Power: {item.Power}", 
                new Vector2(200, Game.GraphicsDevice.Viewport.Height - 150), Color.White);
        }
    }
        
    private void DrawSignatureIndicator(SpriteBatch spriteBatch, float[] signature, Vector2 position, float width, float height)
    {
        // Draw a bar for each dimension
        float barWidth = width / signature.Length;
        for (int i = 0; i < signature.Length; i++)
        {
            // Map -1 to 1 range to 0 to 1 for display
            float normalizedValue = (signature[i] + 1) / 2;
                
            // Calculate bar height
            float barHeight = normalizedValue * height;
                
            // Draw bar
            Rectangle barRect = new Rectangle(
                (int)(position.X + i * barWidth),
                (int)(position.Y + height - barHeight),
                (int)barWidth - 2,
                (int)barHeight
            );
                
            // Color based on dimension
            Color barColor = GetDimensionColor(i);
                
            spriteBatch.Draw(_texture, barRect, barColor);
        }
    }
        
    private Color GetDimensionColor(int dimension)
    {
        return dimension switch
        {
            0 => Color.Red,     // Temperature
            1 => Color.Brown,   // Hardness
            2 => Color.Blue,    // Wetness
            3 => Color.Yellow,  // Luminosity
            4 => Color.Gray,    // Weight
            5 => Color.Cyan,    // Conductivity
            6 => Color.Orange,  // Volatility
            7 => Color.Purple,  // Resonance
            _ => Color.White
        };
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }
}