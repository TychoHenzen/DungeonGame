#region

using System;
using System.Linq;
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
///     Dungeon state
/// </summary>
public class DungeonState(SignatureGame game) : GameState(game), ITextureUser
{
    private Rectangle _combatLogPanel;
    private Rectangle _mapPanel;
    private Rectangle _statusPanel;
    private Texture2D _texture;

    public void SetTexture(Texture2D texture)
    {
        _texture = texture;
    }

    public override void LoadContent()
    {
        // Define UI panels
        _statusPanel = new Rectangle(50, 50, 300, 200);
        _mapPanel = new Rectangle(400, 50, 500, 500);
        _combatLogPanel = new Rectangle(950, 50, 280, 600);
    }

    public override void Update(GameTime gameTime)
    {
        // Check for input to transition back to inventory
        if (!Game.IsRunningDungeon() && Keyboard.GetState().IsKeyDown(Keys.I))
        {
            Game.ChangeState(GameStateType.Inventory);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
        var dungeon = Game.GetCurrentDungeon();

        if (dungeon == null) return;

        // Draw dungeon title
        spriteBatch.DrawString(defaultFont, "Dungeon Exploration", new Vector2(50, 20), Color.White);

        // Draw status panel
        spriteBatch.Draw(_texture, _statusPanel, Color.DarkSlateGray * 0.5f);
        spriteBatch.DrawString(smallFont, "Status", new Vector2(_statusPanel.X + 10, _statusPanel.Y + 10), Color.White);

        // Draw player status
        var player = Game.GetPlayer();
        var playerStats = player.CalculateStats();

        spriteBatch.DrawString(smallFont, $"HP: {playerStats.MaxHealth}",
            new Vector2(_statusPanel.X + 20, _statusPanel.Y + 40), Color.White);

        spriteBatch.DrawString(smallFont, $"Attack: {playerStats.Attack}",
            new Vector2(_statusPanel.X + 20, _statusPanel.Y + 70), Color.White);

        spriteBatch.DrawString(smallFont, $"Defense: {playerStats.Defense}",
            new Vector2(_statusPanel.X + 20, _statusPanel.Y + 100), Color.White);

        spriteBatch.DrawString(smallFont, $"Speed: {playerStats.Speed}",
            new Vector2(_statusPanel.X + 20, _statusPanel.Y + 130), Color.White);

        // Draw dungeon info
        spriteBatch.DrawString(smallFont, $"Difficulty: {dungeon.Difficulty}/3",
            new Vector2(_statusPanel.X + 20, _statusPanel.Y + 160), Color.White);

        // Draw map panel
        spriteBatch.Draw(_texture, _mapPanel, Color.Black * 0.5f);

        if (Game.IsRunningDungeon())
        {
            // Draw progress bar
            var progress = Game.GetRunTimer() / 3.0f; // 3 seconds before simulation completes
            progress = Math.Min(progress, 1.0f);

            var progressBar = new Rectangle(_mapPanel.X, _mapPanel.Y + _mapPanel.Height + 20,
                (int)(_mapPanel.Width * progress), 20);

            spriteBatch.Draw(_texture, progressBar, Color.Green);

            // Draw time remaining
            var timeRemaining = 3.0f - Game.GetRunTimer();
            if (timeRemaining > 0)
            {
                spriteBatch.DrawString(smallFont, $"Exploring: {timeRemaining:F1}s",
                    new Vector2(_mapPanel.X + 10, _mapPanel.Y + _mapPanel.Height + 50), Color.White);
            }
            else
            {
                spriteBatch.DrawString(smallFont, "Calculating results...",
                    new Vector2(_mapPanel.X + 10, _mapPanel.Y + _mapPanel.Height + 50), Color.White);
            }

            // Draw the tile map visualization
            DrawSimplifiedMap(spriteBatch, dungeon, progress, smallFont);
        }
        else if (Game.GetDungeonResult() != null)
        {
            // Draw dungeon result
            var result = Game.GetDungeonResult();

            var resultText = result.Success ? "SUCCESS!" : "FAILED";
            var resultColor = result.Success ? Color.Green : Color.Red;

            spriteBatch.DrawString(defaultFont, resultText,
                new Vector2(_mapPanel.X + _mapPanel.Width / 2 - 60, _mapPanel.Y + 30), resultColor);

            // Draw result stats
            spriteBatch.DrawString(smallFont,
                $"Enemies Defeated: {result.Stats.EnemiesDefeated}/{dungeon.Enemies.Count}",
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 100), Color.White);

            spriteBatch.DrawString(smallFont, $"Damage Dealt: {result.Stats.TotalDamageDealt}",
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 130), Color.White);

            spriteBatch.DrawString(smallFont, $"Damage Taken: {result.Stats.TotalDamageTaken}",
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 160), Color.White);

            spriteBatch.DrawString(smallFont,
                $"Remaining Health: {result.PlayerStats.RemainingHealth}/{result.PlayerStats.MaxHealth}",
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 190), Color.White);

            // Draw loot
            if (result.Success && result.Loot.Count > 0)
            {
                spriteBatch.DrawString(smallFont, "Loot:",
                    new Vector2(_mapPanel.X + 20, _mapPanel.Y + 230), Color.Gold);

                for (var i = 0; i < result.Loot.Count; i++)
                {
                    spriteBatch.DrawString(smallFont, result.Loot[i].Name,
                        new Vector2(_mapPanel.X + 40, _mapPanel.Y + 260 + i * 30), Color.White);
                }
            }

            // Draw return instruction
            spriteBatch.DrawString(smallFont, "Press I to return to inventory",
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + _mapPanel.Height - 30), Color.White);

            // Draw final map state
            DrawSimplifiedMap(spriteBatch, dungeon, 1.0f, smallFont);
        }

        // Draw combat log panel
        spriteBatch.Draw(_texture, _combatLogPanel, Color.DarkBlue * 0.5f);
        spriteBatch.DrawString(smallFont, "Combat Log", new Vector2(_combatLogPanel.X + 10, _combatLogPanel.Y + 10),
            Color.White);

        // Draw combat log if available
        if (Game.GetDungeonResult() != null)
        {
            var log = Game.GetDungeonResult().CombatLog;

            for (var i = 0; i < Math.Min(log.Count, 30); i++)
            {
                var textColor = Color.White;

                // Color coding
                if (log[i].Contains("DUNGEON CLEARED")) textColor = Color.Green;
                else if (log[i].Contains("PLAYER DEFEATED")) textColor = Color.Red;
                else if (log[i].Contains("was defeated")) textColor = Color.LightBlue;
                else if (log[i].Contains("Barely survived")) textColor = Color.Orange;
                else if (log[i].Contains("Found")) textColor = Color.Gold;

                spriteBatch.DrawString(smallFont, log[i],
                    new Vector2(_combatLogPanel.X + 10, _combatLogPanel.Y + 40 + i * 18), textColor, 0, Vector2.Zero,
                    0.8f, SpriteEffects.None, 0);
            }
        }
    }

    private void DrawSimplifiedMap(SpriteBatch spriteBatch, Dungeon dungeon, float progress, SpriteFont smallFont)
    {
        // Handle case where TileMap isn't initialized yet (compatibility with old code)
        if (dungeon.TileMap == null)
        {
            // Fall back to old visualization
            DrawLegacyMap(spriteBatch, dungeon, progress, smallFont);
            return;
        }

        // Draw a representation of the tile map
        var tileSize = Math.Min(
            _mapPanel.Width / dungeon.Width,
            _mapPanel.Height / dungeon.Height
        );

        var startX = _mapPanel.X + (_mapPanel.Width - tileSize * dungeon.Width) / 2;
        var startY = _mapPanel.Y + (_mapPanel.Height - tileSize * dungeon.Height) / 2;

        // Calculate how much of the map to reveal based on progress
        var revealedTiles = (int)(dungeon.Width * dungeon.Height * progress);

        // Draw the tile map
        for (var x = 0; x < dungeon.Width; x++)
        {
            for (var y = 0; y < dungeon.Height; y++)
            {
                var tileIndex = y * dungeon.Width + x;

                // Only draw tiles that are revealed based on progress
                if (tileIndex < revealedTiles)
                {
                    var tile = dungeon.TileMap[x, y];
                    var tileRect = new Rectangle(
                        startX + x * tileSize,
                        startY + y * tileSize,
                        tileSize,
                        tileSize
                    );

                    // Draw tile with appropriate color
                    var tileColor = GetTileColor(tile.Type);
                    spriteBatch.Draw(_texture, tileRect, tileColor);

                    // Draw player position
                    if (x == dungeon.PlayerX && y == dungeon.PlayerY)
                    {
                        var playerRect = new Rectangle(
                            tileRect.X + tileRect.Width / 4,
                            tileRect.Y + tileRect.Height / 4,
                            tileRect.Width / 2,
                            tileRect.Height / 2
                        );

                        spriteBatch.Draw(_texture, playerRect, Color.White);
                    }

                    // Draw enemies that haven't been defeated yet
                    var enemy = dungeon.GetEnemyAt(x, y);
                    if (enemy != null)
                    {
                        var enemyRect = new Rectangle(
                            tileRect.X + tileRect.Width / 3,
                            tileRect.Y + tileRect.Height / 3,
                            tileRect.Width / 3,
                            tileRect.Height / 3
                        );

                        spriteBatch.Draw(_texture, enemyRect, Color.Red);
                    }

                    // Draw defeated enemies (optional)
                    var isDefeated = dungeon.DefeatedEnemies?.Any(e => e.X == x && e.Y == y) == true;
                    if (isDefeated)
                    {
                        var defeatRect = new Rectangle(
                            tileRect.X + tileRect.Width / 3,
                            tileRect.Y + tileRect.Height / 3,
                            tileRect.Width / 3,
                            tileRect.Height / 3
                        );

                        spriteBatch.Draw(_texture, defeatRect, Color.Gray);
                    }
                }
                else
                {
                    // Draw fog of war for unrevealed tiles
                    var tileRect = new Rectangle(
                        startX + x * tileSize,
                        startY + y * tileSize,
                        tileSize,
                        tileSize
                    );

                    spriteBatch.Draw(_texture, tileRect, Color.Black * 0.7f);
                }
            }
        }

        // Draw legend
        var legendX = startX + dungeon.Width * tileSize + 20;
        var legendY = startY;

        // Draw title for the legend
        spriteBatch.DrawString(smallFont, "Legend:", new Vector2(legendX, legendY), Color.White);
        legendY += 25;

        // Draw player legend
        var playerLegend = new Rectangle(legendX, legendY, tileSize / 2, tileSize / 2);
        spriteBatch.Draw(_texture, playerLegend, Color.White);
        spriteBatch.DrawString(smallFont, "Player", new Vector2(legendX + tileSize, legendY), Color.White);
        legendY += tileSize;

        // Draw enemy legend
        var enemyLegend = new Rectangle(legendX, legendY, tileSize / 2, tileSize / 2);
        spriteBatch.Draw(_texture, enemyLegend, Color.Red);
        spriteBatch.DrawString(smallFont, "Enemy", new Vector2(legendX + tileSize, legendY), Color.White);
        legendY += tileSize;

        // Draw defeated enemy legend
        var defeatedLegend = new Rectangle(legendX, legendY, tileSize / 2, tileSize / 2);
        spriteBatch.Draw(_texture, defeatedLegend, Color.Gray);
        spriteBatch.DrawString(smallFont, "Defeated Enemy", new Vector2(legendX + tileSize, legendY), Color.White);
        legendY += tileSize;

        // Draw tile legends for common types
        TileType[] commonTiles = { TileType.Stone, TileType.Water, TileType.Lava, TileType.Grass, TileType.Sand };
        foreach (var tileType in commonTiles)
        {
            var tileLegend = new Rectangle(legendX, legendY, tileSize / 2, tileSize / 2);
            spriteBatch.Draw(_texture, tileLegend, GetTileColor(tileType));
            spriteBatch.DrawString(smallFont, tileType.ToString(), new Vector2(legendX + tileSize, legendY),
                Color.White);

            legendY += tileSize;
        }

        // Draw enemies defeated count
        var defeatedCount = dungeon.DefeatedEnemies?.Count ?? 0;
        spriteBatch.DrawString(smallFont, $"Enemies: {defeatedCount}/{dungeon.Enemies.Count}",
            new Vector2(startX, startY + dungeon.Height * tileSize + 20), Color.White);
    }

    // Legacy map drawing method for compatibility
    private void DrawLegacyMap(SpriteBatch spriteBatch, Dungeon dungeon, float progress, SpriteFont smallFont)
    {
        // Create a simplified map visualization
        var roomsCount = dungeon.Tiles.Count;
        var roomsToShow = (int)Math.Ceiling(roomsCount * progress);

        var tileSize = 80;
        var startX = _mapPanel.X + 50;
        var startY = _mapPanel.Y + 50;

        // Draw explored rooms
        for (var i = 0; i < roomsToShow; i++)
        {
            var tile = dungeon.Tiles[i];

            // Calculate position - simple path
            var x = startX + i * (tileSize + 20);
            var y = startY + i % 2 * 40; // Zigzag path

            if (x > _mapPanel.Right - tileSize)
            {
                // Move to next row
                x = startX + i % 3 * (tileSize + 20);
                y = startY + 150 + i / 3 * 130;
            }

            // Draw tile
            var tileRect = new Rectangle(x, y, tileSize, tileSize);
            var tileColor = GetTileColor(tile.Type);

            spriteBatch.Draw(_texture, tileRect, tileColor);

            // Draw tile name
            spriteBatch.DrawString(smallFont, tile.Type.ToString(),
                new Vector2(x + 10, y + 10), Color.Black);

            // Draw enemy if there is one
            if (i < dungeon.Enemies.Count)
            {
                var enemy = dungeon.Enemies[i];
                spriteBatch.DrawString(smallFont, enemy.Name,
                    new Vector2(x + 10, y + tileSize - 30), Color.Red);
            }
        }
    }

    private Color GetTileColor(TileType tileType)
    {
        return tileType switch
        {
            TileType.Water => Color.LightBlue,
            TileType.Lava => Color.OrangeRed,
            TileType.Ice => Color.AliceBlue,
            TileType.Grass => Color.LightGreen,
            TileType.Sand => Color.Khaki,
            TileType.Crystal => Color.Purple * 0.7f,
            TileType.Wood => Color.SaddleBrown,
            _ => Color.LightGray // Stone or default
        };
    }
}
