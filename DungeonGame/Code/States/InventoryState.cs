#region

using System;
using DungeonGame.Code.Core;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Enums;
using DungeonGame.Code.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace DungeonGame.Code.States;

/// <summary>
///     Inventory state with drag-and-drop functionality
/// </summary>
public class InventoryState : GameState, ITextureUser
{
    // Drag and drop state
    private Item _draggedItem;
    private Vector2 _dragPosition;
    private int _dragSourceIndex = -1;
    private string _dragSourceType = string.Empty;
    private Rectangle _dungeonButton;
    private Rectangle[] _dungeonSlots;
    private Rectangle[] _equipmentSlots;
    private bool _isDragging;
    private Rectangle[] _itemSlots;
    private MouseState _previousMouseState;
    private Texture2D _texture;

    // Number of unlocked dungeon slots
    private int _unlockedDungeonSlots = 1;

    public InventoryState(SignatureGame game) : base(game)
    {
    }

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }

    public override void LoadContent()
    {
        // Get screen dimensions
        var screenWidth = Game.GraphicsDevice.Viewport.Width;
        var screenHeight = Game.GraphicsDevice.Viewport.Height;

        // Layout constants
        const int inventoryX = 50;
        const int inventoryY = 100;
        const int inventorySlotWidth = 160;
        const int inventorySlotHeight = 100;
        const int inventorySpacingX = 20;
        const int inventorySpacingY = 20;

        // Create inventory slots (4x5 grid on left side)
        _itemSlots = new Rectangle[20];
        for (var i = 0; i < 20; i++)
        {
            var row = i / 4;
            var col = i % 4;
            var x = inventoryX + col * (inventorySlotWidth + inventorySpacingX);
            var y = inventoryY + row * (inventorySlotHeight + inventorySpacingY);
            _itemSlots[i] = new Rectangle(x, y, inventorySlotWidth, inventorySlotHeight);
        }

        // Calculate last inventory slot position to ensure equipment doesn't overlap
        var lastInventorySlotRight = inventoryX + 3 * (inventorySlotWidth + inventorySpacingX) + inventorySlotWidth;

        // Equipment section (right side of screen)
        var equipmentX = Math.Max(lastInventorySlotRight + 100, screenWidth - 400);
        const int equipmentY = 100;
        const int equipmentSlotWidth = 160;
        const int equipmentSlotHeight = 100;
        const int equipmentSpacingY = 20;

        // Create equipment slots (vertical list on right)
        string[] slots = { "weapon", "shield", "helmet", "armor", "amulet", "ring", "boots" };
        _equipmentSlots = new Rectangle[slots.Length];

        for (var i = 0; i < slots.Length; i++)
        {
            _equipmentSlots[i] = new Rectangle(
                equipmentX,
                equipmentY + i * (equipmentSlotHeight + equipmentSpacingY),
                equipmentSlotWidth,
                equipmentSlotHeight);
        }

        // Dungeon slots section (bottom right)
        const int dungeonSlotWidth = 200;
        const int dungeonSlotHeight = 100;
        const int dungeonSlotSpacingY = 20;
        var dungeonSlotsX = Math.Max(equipmentX + inventorySlotWidth + 100, screenWidth - 400);


        // Create dungeon slots
        _dungeonSlots = new Rectangle[3];
        for (var i = 0; i < 3; i++)
        {
            _dungeonSlots[i] = new Rectangle(
                dungeonSlotsX,
                equipmentY + i * (dungeonSlotHeight + dungeonSlotSpacingY),
                dungeonSlotWidth,
                dungeonSlotHeight);
        }

        // Calculate last dungeon slot position
        var lastDungeonSlotBottom = equipmentY + 2 * (dungeonSlotHeight + dungeonSlotSpacingY) + dungeonSlotHeight;

        // Create dungeon button below dungeon slots
        _dungeonButton = new Rectangle(
            dungeonSlotsX,
            lastDungeonSlotBottom + 30,
            dungeonSlotWidth,
            50);

        _previousMouseState = Mouse.GetState();
    }

    public override void Update(GameTime gameTime)
    {
        var mouseState = Mouse.GetState();
        var isNewClick = mouseState.LeftButton == ButtonState.Pressed &&
                         _previousMouseState.LeftButton == ButtonState.Released;
        var isReleased = mouseState.LeftButton == ButtonState.Released &&
                         _previousMouseState.LeftButton == ButtonState.Pressed;

        // Handle starting a drag operation
        if (isNewClick && !_isDragging)
        {
            // Check inventory slots
            for (var i = 0; i < _itemSlots.Length; i++)
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
                var slots = Enum.GetValues<SlotType>();
                for (var i = 0; i < _equipmentSlots.Length; i++)
                {
                    if (!_equipmentSlots[i].Contains(mouseState.Position))
                    {
                        continue;
                    }

                    var equippedItem = Game.GetPlayer().GetEquippedItem(slots[i]);
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

            // Check dungeon slots
            if (!_isDragging)
            {
                for (var i = 0; i < _unlockedDungeonSlots; i++)
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
            var slots = Enum.GetValues<SlotType>();
            var itemDropped = false;

            for (var i = 0; i < _equipmentSlots.Length; i++)
            {
                if (!_equipmentSlots[i].Contains(mouseState.Position))
                {
                    continue;
                }

                // Only allow if item slot matches
                if (_draggedItem.Slot == slots[i])
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

            // Check if dropped on inventory slot
            if (!itemDropped)
            {
                for (var i = 0; i < _itemSlots.Length; i++)
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
                for (var i = 0; i < _unlockedDungeonSlots; i++)
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
            for (var i = 0; i < _unlockedDungeonSlots; i++)
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
        spriteBatch.DrawString(defaultFont, "Equipment", new Vector2(_equipmentSlots[0].X, 50), Color.White);

        // Draw dungeon slots title
        spriteBatch.DrawString(defaultFont, "Dungeon Slots", new Vector2(_dungeonSlots[0].X, _dungeonSlots[0].Y - 30),
            Color.White);

        // Draw inventory slots
        var inventory = Game.GetInventory();
        for (var i = 0; i < _itemSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(_texture, _itemSlots[i], Color.Gray * 0.5f);

            // Draw item if exists and not being dragged
            if (i < inventory.Items.Count)
            {
                var item = inventory.Items[i];
                var isBeingDragged = _isDragging && _dragSourceType == "inventory" && _dragSourceIndex == i;

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
        var slots = Enum.GetValues<SlotType>();
        for (var i = 0; i < _equipmentSlots.Length; i++)
        {
            // Draw slot background
            spriteBatch.Draw(_texture, _equipmentSlots[i], Color.DarkGray * 0.5f);

            // Draw slot name
            spriteBatch.DrawString(smallFont, slots[i].ToString(),
                new Vector2(_equipmentSlots[i].X + 10, _equipmentSlots[i].Y + 10), Color.White);

            // Draw equipped item if exists and not being dragged
            var player = Game.GetPlayer();
            var equippedItem = player.GetEquippedItem(slots[i]);
            var isBeingDragged = _isDragging && _dragSourceType == "equipment" && _dragSourceIndex == i;

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
        for (var i = 0; i < _dungeonSlots.Length; i++)
        {
            // Draw slot with different color based on locked/unlocked
            var slotColor = i < _unlockedDungeonSlots ? Color.RoyalBlue * 0.5f : Color.DarkGray * 0.3f;
            spriteBatch.Draw(_texture, _dungeonSlots[i], slotColor);

            // Draw slot label
            var slotLabel = i < _unlockedDungeonSlots ? $"Dungeon {i + 1}" : "Locked";
            spriteBatch.DrawString(smallFont, slotLabel,
                new Vector2(_dungeonSlots[i].X + 10, _dungeonSlots[i].Y + 10), Color.White);

            // Draw dungeon item if exists and slot is unlocked
            if (i < _unlockedDungeonSlots)
            {
                var dungeonItem = Game.GetDungeonSlotItem(i);
                var isBeingDragged = _isDragging && _dragSourceType == "dungeon" && _dragSourceIndex == i;

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
        var anyDungeonSlotFilled = false;
        for (var i = 0; i < _unlockedDungeonSlots; i++)
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
            var dragRect = new Rectangle((int)_dragPosition.X - 80, (int)_dragPosition.Y - 50, 160, 100);
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

    private void DrawSignatureIndicator(SpriteBatch spriteBatch, Signature signature, Vector2 position, float width,
        float height)
    {
        // Draw a bar for each dimension
        var barWidth = width / Signature.Dimensions;
        for (var i = 0; i < Signature.Dimensions; i++)
        {
            // Map -1 to 1 range to 0 to 1 for display
            var normalizedValue = (signature[i] + 1) / 2;

            // Calculate bar height
            var barHeight = normalizedValue * height;

            // Draw bar
            var barRect = new Rectangle(
                (int)(position.X + i * barWidth),
                (int)(position.Y + height - barHeight),
                (int)barWidth - 2,
                (int)barHeight
            );

            // Color based on dimension
            var barColor = GetDimensionColor(i);

            spriteBatch.Draw(_texture, barRect, barColor);
        }
    }

    private Color GetDimensionColor(int dimension)
    {
        return dimension switch
        {
            0 => Color.Red, // Temperature
            1 => Color.Brown, // Hardness
            2 => Color.Blue, // Wetness
            3 => Color.Yellow, // Luminosity
            4 => Color.Gray, // Weight
            5 => Color.Cyan, // Conductivity
            6 => Color.Orange, // Volatility
            7 => Color.Purple, // Resonance
            _ => Color.White
        };
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