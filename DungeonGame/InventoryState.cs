using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonGame;

/// <summary>
/// Inventory state with drag-and-drop functionality
/// </summary>
public class InventoryState : GameState, ITextureUser
{
    private Rectangle[] _itemSlots;
    private Rectangle[] _equipmentSlots;
    private Rectangle[] _dungeonSlots;  // Multiple dungeon slots
    private Rectangle _dungeonButton;
    private Texture2D _texture;
    private MouseState _previousMouseState;
    
    // Drag and drop state
    private Item _draggedItem;
    private Vector2 _dragPosition;
    private bool _isDragging;
    private int _dragSourceIndex = -1;
    private string _dragSourceType = string.Empty;  // "inventory", "equipment", or "dungeon"
    
    // Number of unlocked dungeon slots
    private int _unlockedDungeonSlots = 1;  // Start with 1 unlocked slot
    
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
        
        // Create dungeon slots (up to 3 that can be unlocked)
        _dungeonSlots = new Rectangle[3];
        for (int i = 0; i < 3; i++)
        {
            _dungeonSlots[i] = new Rectangle(screenWidth - 250, screenHeight - 400 + i * 120, 200, 100);
        }
        
        // Create buttons at bottom of screen
        _dungeonButton = new Rectangle(screenWidth - 250, screenHeight - 120, 200, 50);
        
        _previousMouseState = Mouse.GetState();
    }
    
    public override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        bool isNewClick = mouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released;
        bool isReleased = mouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed;
        
        // Handle starting a drag operation
        if (isNewClick && !_isDragging)
        {
            // Check inventory slots
            for (int i = 0; i < _itemSlots.Length; i++)
            {
                if (_itemSlots[i].Contains(mouseState.Position))
                {
                    var inventory = Game.GetInventory();
                    if (i < inventory.Items.Count)
                    {
                        // Start dragging the item
                        _draggedItem = inventory.Items[i];
                        _isDragging = true;
                        _dragPosition = new Vector2(mouseState.X, mouseState.Y);
                        _dragSourceIndex = i;
                        _dragSourceType = "inventory";
                    }
                    break;
                }
            }
            
            // Check equipment slots
            if (!_isDragging)
            {
                string[] slots = {"weapon", "shield", "helmet", "armor", "amulet", "ring", "boots"};
                for (int i = 0; i < _equipmentSlots.Length; i++)
                {
                    if (_equipmentSlots[i].Contains(mouseState.Position))
                    {
                        var equippedItem = Game.GetPlayer().GetEquippedItem(slots[i].ToLower());
                        if (equippedItem != null)
                        {
                            // Start dragging the equipped item
                            _draggedItem = equippedItem;
                            _isDragging = true;
                            _dragPosition = new Vector2(mouseState.X, mouseState.Y);
                            _dragSourceIndex = i;
                            _dragSourceType = "equipment";
                        }
                        break;
                    }
                }
            }
            
            // Check dungeon slots
            if (!_isDragging)
            {
                for (int i = 0; i < _unlockedDungeonSlots; i++)
                {
                    if (_dungeonSlots[i].Contains(mouseState.Position))
                    {
                        var dungeonItem = Game.GetDungeonSlotItem(i);
                        if (dungeonItem != null)
                        {
                            // Start dragging the dungeon item
                            _draggedItem = dungeonItem;
                            _isDragging = true;
                            _dragPosition = new Vector2(mouseState.X, mouseState.Y);
                            _dragSourceIndex = i;
                            _dragSourceType = "dungeon";
                        }
                        break;
                    }
                }
            }
        }
        
        // Update drag position if dragging
        if (_isDragging)
        {
            _dragPosition = new Vector2(mouseState.X, mouseState.Y);
        }
        
        // Handle releasing the drag
        if (isReleased && _isDragging)
        {
            // Check if dropped on equipment slot
            string[] slots = {"weapon", "shield", "helmet", "armor", "amulet", "ring", "boots"};
            bool itemDropped = false;
            
            for (int i = 0; i < _equipmentSlots.Length; i++)
            {
                if (_equipmentSlots[i].Contains(mouseState.Position))
                {
                    // Only allow if item slot matches
                    if (_draggedItem.Slot.ToLower() == slots[i].ToLower())
                    {
                        // Handle different source types
                        if (_dragSourceType == "inventory")
                        {
                            // Remove from inventory
                            Game.GetInventory().RemoveItem(_draggedItem);
                        }
                        else if (_dragSourceType == "equipment")
                        {
                            // Unequip from previous slot
                            Game.GetPlayer().UnequipItem(slots[_dragSourceIndex]);
                        }
                        else if (_dragSourceType == "dungeon")
                        {
                            // Remove from dungeon slot
                            Game.ClearDungeonSlot(_dragSourceIndex);
                        }
                        
                        // Equip the item
                        Game.GetPlayer().EquipItem(_draggedItem);
                        itemDropped = true;
                    }
                    break;
                }
            }
            
            // Check if dropped on inventory slot
            if (!itemDropped)
            {
                for (int i = 0; i < _itemSlots.Length; i++)
                {
                    if (_itemSlots[i].Contains(mouseState.Position))
                    {
                        // Add to inventory
                        if (_dragSourceType == "equipment")
                        {
                            // Unequip from equipment slot
                            Game.GetPlayer().UnequipItem(slots[_dragSourceIndex]);
                        }
                        else if (_dragSourceType == "dungeon")
                        {
                            // Remove from dungeon slot
                            Game.ClearDungeonSlot(_dragSourceIndex);
                        }
                        
                        // Add to inventory if not already there
                        if (_dragSourceType != "inventory")
                        {
                            Game.GetInventory().AddItem(_draggedItem);
                        }
                        
                        itemDropped = true;
                        break;
                    }
                }
            }
            
            // Check if dropped on dungeon slot
            if (!itemDropped)
            {
                for (int i = 0; i < _unlockedDungeonSlots; i++)
                {
                    if (_dungeonSlots[i].Contains(mouseState.Position))
                    {
                        // Move to dungeon slot
                        if (_dragSourceType == "inventory")
                        {
                            // Remove from inventory
                            Game.GetInventory().RemoveItem(_draggedItem);
                        }
                        else if (_dragSourceType == "equipment")
                        {
                            // Unequip from equipment slot
                            Game.GetPlayer().UnequipItem(slots[_dragSourceIndex]);
                        }
                        else if (_dragSourceType == "dungeon" && _dragSourceIndex != i)
                        {
                            // Clear the source dungeon slot
                            Game.ClearDungeonSlot(_dragSourceIndex);
                        }
                        
                        // Set the dungeon slot
                        if (_dragSourceType != "dungeon" || _dragSourceIndex != i)
                        {
                            Game.SetDungeonSlotItem(_draggedItem, i);
                        }
                        
                        itemDropped = true;
                        break;
                    }
                }
            }
            
            // End dragging
            _isDragging = false;
            _draggedItem = null;
            _dragSourceIndex = -1;
            _dragSourceType = string.Empty;
        }
        
        // Check dungeon button
        if (isNewClick && _dungeonButton.Contains(mouseState.Position))
        {
            // Check if any dungeon slot has an item
            for (int i = 0; i < _unlockedDungeonSlots; i++)
            {
                var dungeonItem = Game.GetDungeonSlotItem(i);
                if (dungeonItem != null)
                {
                    Game.StartDungeon(dungeonItem, i);
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
        
        // Draw dungeon slots title
        spriteBatch.DrawString(defaultFont, "Dungeon Slots", new Vector2(Game.GraphicsDevice.Viewport.Width - 250, Game.GraphicsDevice.Viewport.Height - 450), Color.White);
        
        // Draw inventory slots
        var inventory = Game.GetInventory();
        for (int i = 0; i < _itemSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(_texture, _itemSlots[i], Color.Gray * 0.5f);
            
            // Draw item if exists and not being dragged
            if (i < inventory.Items.Count)
            {
                var item = inventory.Items[i];
                bool isBeingDragged = _isDragging && _dragSourceType == "inventory" && _dragSourceIndex == i;
                
                if (!isBeingDragged)
                {
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
            
            // Draw equipped item if exists and not being dragged
            var player = Game.GetPlayer();
            var equippedItem = player.GetEquippedItem(slots[i].ToLower());
            bool isBeingDragged = _isDragging && _dragSourceType == "equipment" && _dragSourceIndex == i;
            
            if (equippedItem != null && !isBeingDragged)
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
        
        // Draw dungeon slots
        for (int i = 0; i < _dungeonSlots.Length; i++)
        {
            // Draw slot with different color based on locked/unlocked
            Color slotColor = i < _unlockedDungeonSlots ? Color.RoyalBlue * 0.5f : Color.DarkGray * 0.3f;
            spriteBatch.Draw(_texture, _dungeonSlots[i], slotColor);
            
            // Draw slot label
            string slotLabel = i < _unlockedDungeonSlots ? $"Dungeon {i + 1}" : "Locked";
            spriteBatch.DrawString(smallFont, slotLabel, 
                new Vector2(_dungeonSlots[i].X + 10, _dungeonSlots[i].Y + 10), Color.White);
            
            // Draw dungeon item if exists and slot is unlocked
            if (i < _unlockedDungeonSlots)
            {
                var dungeonItem = Game.GetDungeonSlotItem(i);
                bool isBeingDragged = _isDragging && _dragSourceType == "dungeon" && _dragSourceIndex == i;
                
                if (dungeonItem != null && !isBeingDragged)
                {
                    // Draw item name
                    spriteBatch.DrawString(smallFont, dungeonItem.Name, 
                        new Vector2(_dungeonSlots[i].X + 10, _dungeonSlots[i].Y + 30), Color.White);
                    
                    // Draw item power
                    spriteBatch.DrawString(smallFont, $"Power: {dungeonItem.Power}", 
                        new Vector2(_dungeonSlots[i].X + 10, _dungeonSlots[i].Y + 50), Color.White);
                    
                    // Draw signature indicator
                    DrawSignatureIndicator(spriteBatch, dungeonItem.Signature, 
                        new Vector2(_dungeonSlots[i].X + 10, _dungeonSlots[i].Y + 70), 180, 20);
                }
            }
        }
        
        // Draw dungeon button
        bool anyDungeonSlotFilled = false;
        for (int i = 0; i < _unlockedDungeonSlots; i++)
        {
            if (Game.GetDungeonSlotItem(i) != null)
            {
                anyDungeonSlotFilled = true;
                break;
            }
        }
        
        spriteBatch.Draw(_texture, _dungeonButton, anyDungeonSlotFilled ? Color.Green : Color.Gray);
        spriteBatch.DrawString(smallFont, "Start Dungeon", 
            new Vector2(_dungeonButton.X + 40, _dungeonButton.Y + 15), Color.White);
        
        // Draw dragged item if dragging
        if (_isDragging && _draggedItem != null)
        {
            // Draw semi-transparent background
            Rectangle dragRect = new Rectangle((int)_dragPosition.X - 80, (int)_dragPosition.Y - 50, 160, 100);
            spriteBatch.Draw(_texture, dragRect, Color.White * 0.7f);
            
            // Draw item name
            spriteBatch.DrawString(smallFont, _draggedItem.Name, 
                new Vector2(dragRect.X + 10, dragRect.Y + 10), Color.Black);
            
            // Draw item power
            spriteBatch.DrawString(smallFont, $"Power: {_draggedItem.Power}", 
                new Vector2(dragRect.X + 10, dragRect.Y + 30), Color.Black);
            
            // Draw item type
            spriteBatch.DrawString(smallFont, $"Type: {_draggedItem.Type}", 
                new Vector2(dragRect.X + 10, dragRect.Y + 50), Color.Black);
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
    
    // Helper method to unlock additional dungeon slots
    public void UnlockDungeonSlot()
    {
        if (_unlockedDungeonSlots < _dungeonSlots.Length)
        {
            _unlockedDungeonSlots++;
        }
    }
}