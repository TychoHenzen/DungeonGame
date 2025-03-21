using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace DungeonGame;

/// <summary>
/// Dungeon state
/// </summary>
public class DungeonState : GameState
{
    private Rectangle _statusPanel;
    private Rectangle _mapPanel;
    private Rectangle _combatLogPanel;
        
    public DungeonState(DungeonGame.SignatureGame game) : base(game) { }
        
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
        spriteBatch.Draw(null, _statusPanel, Color.DarkSlateGray * 0.5f);
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
        spriteBatch.Draw(null, _mapPanel, Color.Black * 0.5f);
            
        if (Game.IsRunningDungeon())
        {
            // Draw progress bar
            float progress = Game.GetRunTimer() / (dungeon.Length * 60);
            Rectangle progressBar = new Rectangle(_mapPanel.X, _mapPanel.Y + _mapPanel.Height + 20, 
                (int)(_mapPanel.Width * progress), 20);
                
            spriteBatch.Draw(null, progressBar, Color.Green);
                
            // Draw time remaining
            float timeRemaining = (dungeon.Length * 60) - Game.GetRunTimer();
            spriteBatch.DrawString(smallFont, $"Time: {(int)(timeRemaining / 60)}:{(int)(timeRemaining % 60):D2}", 
                new Vector2(_mapPanel.X + 10, _mapPanel.Y + _mapPanel.Height + 50), Color.White);
                
            // Draw simplified exploration map
            DrawSimplifiedMap(spriteBatch, dungeon, progress, smallFont);
        }
        else if (Game.GetDungeonResult() != null)
        {
            // Draw dungeon result
            var result = Game.GetDungeonResult();
                
            string resultText = result.Success ? "SUCCESS!" : "FAILED";
            Color resultColor = result.Success ? Color.Green : Color.Red;
                
            spriteBatch.DrawString(defaultFont, resultText, 
                new Vector2(_mapPanel.X + _mapPanel.Width / 2 - 60, _mapPanel.Y + 30), resultColor);
                
            // Draw result stats
            spriteBatch.DrawString(smallFont, $"Enemies Defeated: {result.Stats.EnemiesDefeated}/{dungeon.Enemies.Count}", 
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 100), Color.White);
                
            spriteBatch.DrawString(smallFont, $"Damage Dealt: {result.Stats.TotalDamageDealt}", 
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 130), Color.White);
                
            spriteBatch.DrawString(smallFont, $"Damage Taken: {result.Stats.TotalDamageTaken}", 
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 160), Color.White);
                
            spriteBatch.DrawString(smallFont, $"Remaining Health: {result.PlayerStats.RemainingHealth}/{result.PlayerStats.MaxHealth}", 
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + 190), Color.White);
                
            // Draw loot
            if (result.Success && result.Loot.Count > 0)
            {
                spriteBatch.DrawString(smallFont, "Loot:", 
                    new Vector2(_mapPanel.X + 20, _mapPanel.Y + 230), Color.Gold);
                    
                for (int i = 0; i < result.Loot.Count; i++)
                {
                    spriteBatch.DrawString(smallFont, result.Loot[i].Name, 
                        new Vector2(_mapPanel.X + 40, _mapPanel.Y + 260 + i * 30), Color.White);
                }
            }
                
            // Draw return instruction
            spriteBatch.DrawString(smallFont, "Press I to return to inventory", 
                new Vector2(_mapPanel.X + 20, _mapPanel.Y + _mapPanel.Height - 30), Color.White);
        }
            
        // Draw combat log panel
        spriteBatch.Draw(null, _combatLogPanel, Color.DarkBlue * 0.5f);
        spriteBatch.DrawString(smallFont, "Combat Log", new Vector2(_combatLogPanel.X + 10, _combatLogPanel.Y + 10), Color.White);
            
        // Draw combat log if available
        if (Game.GetDungeonResult() != null)
        {
            var log = Game.GetDungeonResult().CombatLog;
                
            for (int i = 0; i < Math.Min(log.Count, 30); i++)
            {
                Color textColor = Color.White;
                    
                // Color coding
                if (log[i].Contains("DUNGEON CLEARED")) textColor = Color.Green;
                else if (log[i].Contains("PLAYER DEFEATED")) textColor = Color.Red;
                else if (log[i].Contains("was defeated")) textColor = Color.LightBlue;
                else if (log[i].Contains("Barely survived")) textColor = Color.Orange;
                    
                spriteBatch.DrawString(smallFont, log[i], 
                    new Vector2(_combatLogPanel.X + 10, _combatLogPanel.Y + 40 + i * 18), textColor, 0, Vector2.Zero, 0.8f, SpriteEffects.None, 0);
            }
        }
    }
        
    private void DrawSimplifiedMap(SpriteBatch spriteBatch, Dungeon dungeon, float progress, SpriteFont smallFont)
    {
        // Create a simplified map visualization
        int roomsCount = dungeon.Tiles.Count;
        int roomsToShow = (int)Math.Ceiling(roomsCount * progress);
            
        int tileSize = 80;
        int startX = _mapPanel.X + 50;
        int startY = _mapPanel.Y + 50;
            
        // Draw explored rooms
        for (int i = 0; i < roomsToShow; i++)
        {
            var tile = dungeon.Tiles[i];
                
            // Calculate position - simple path
            int x = startX + i * (tileSize + 20);
            int y = startY + (i % 2) * 40; // Zigzag path
                
            if (x > _mapPanel.Right - tileSize)
            {
                // Move to next row
                x = startX + (i % 3) * (tileSize + 20);
                y = startY + 150 + (i / 3) * 130;
            }
                
            // Draw tile
            Rectangle tileRect = new Rectangle(x, y, tileSize, tileSize);
            Color tileColor = GetTileColor(tile.Type);
                
            spriteBatch.Draw(null, tileRect, tileColor);
                
            // Draw tile name
            spriteBatch.DrawString(smallFont, tile.Type, 
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
        
    private Color GetTileColor(string tileType)
    {
        return tileType switch
        {
            "Water" => Color.LightBlue,
            "Lava" => Color.OrangeRed,
            "Ice" => Color.AliceBlue,
            "Grass" => Color.LightGreen,
            "Sand" => Color.Khaki,
            "Crystal" => Color.Purple * 0.7f,
            "Wood" => Color.SaddleBrown,
            _ => Color.LightGray // Stone or default
        };
    }
}
