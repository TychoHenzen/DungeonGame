using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonGame;

/// <summary>
/// Inventory state
/// </summary>
public class InventoryState : GameState
{
    private Rectangle[] _itemSlots;
    private Rectangle[] _equipmentSlots;
    private Rectangle _dungeonButton;
        
    public InventoryState(DungeonGame.SignatureGame game) : base(game) { }
        
    public override void LoadContent()
    {
        // Create item slot rectangles
        _itemSlots = new Rectangle[20];
        for (int i = 0; i < 20; i++)
        {
            int x = 100 + (i % 5) * 220;
            int y = 300 + (i / 5) * 120;
            _itemSlots[i] = new Rectangle(x, y, 200, 100);
        }
            
        // Create equipment slot rectangles
        string[] slots = {"weapon", "shield", "helmet", "armor", "amulet", "ring", "boots"};
        _equipmentSlots = new Rectangle[slots.Length];
        for (int i = 0; i < slots.Length; i++)
        {
            _equipmentSlots[i] = new Rectangle(800 + (i % 2) * 220, 100 + (i / 2) * 120, 200, 100);
        }
            
        // Create dungeon button
        _dungeonButton = new Rectangle(900, 600, 200, 50);
    }
        
    public override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
            
        // Check for mouse clicks
        if (mouseState.LeftButton == ButtonState.Pressed)
        {
            // Check item slots
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
                
            // Check dungeon button
            if (_dungeonButton.Contains(mouseState.Position) && Game.GetSelectedDungeonItem() != null)
            {
                Game.StartDungeon(Game.GetSelectedDungeonItem());
            }
        }
    }
        
    public override void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
        // Draw inventory title
        spriteBatch.DrawString(defaultFont, "Inventory", new Vector2(100, 50), Color.White);
            
        // Draw equipment title
        spriteBatch.DrawString(defaultFont, "Equipment", new Vector2(800, 50), Color.White);
            
        // Draw inventory slots
        var inventory = Game.GetInventory();
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(null, _itemSlots[i], Color.Gray * 0.5f);
                
            // Draw item if exists
            if (i < inventory.Items.Count)
            {
                var item = inventory.Items[i];
                    
                // Highlight if selected
                if (item == Game.GetSelectedDungeonItem())
                {
                    spriteBatch.Draw(null, _itemSlots[i], Color.Purple * 0.5f);
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
                    new Vector2(_itemSlots[i].X + 10, _itemSlots[i].Y + 70), 180, 20);
            }
        }
            
        // Draw equipment slots
        string[] slots = {"Weapon", "Shield", "Helmet", "Armor", "Amulet", "Ring", "Boots"};
        for (int i = 0; i < _equipmentSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(null, _equipmentSlots[i], Color.DarkGray * 0.5f);
                
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
                    new Vector2(_equipmentSlots[i].X + 10, _equipmentSlots[i].Y + 70), 180, 20);
            }
        }
            
        // Draw dungeon button
        spriteBatch.Draw(null, _dungeonButton, Game.GetSelectedDungeonItem() != null ? Color.Green : Color.Gray);
        spriteBatch.DrawString(smallFont, "Start Dungeon", 
            new Vector2(_dungeonButton.X + 40, _dungeonButton.Y + 15), Color.White);
            
        // Draw selected item info if exists
        if (Game.GetSelectedDungeonItem() != null)
        {
            var item = Game.GetSelectedDungeonItem();
            spriteBatch.DrawString(defaultFont, "Selected Item:", new Vector2(100, 200), Color.White);
            spriteBatch.DrawString(smallFont, item.Name, new Vector2(250, 200), Color.White);
            spriteBatch.DrawString(smallFont, $"Type: {item.Type} | Power: {item.Power}", new Vector2(250, 230), Color.White);
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
                
            spriteBatch.Draw(null, barRect, barColor);
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
}