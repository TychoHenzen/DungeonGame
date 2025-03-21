using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonGame;

/// <summary>
/// Base class for game states
/// </summary>
public abstract class GameState
{
    protected DungeonGame.SignatureGame Game;
        
    public GameState(DungeonGame.SignatureGame game)
    {
        Game = game;
    }
        
    public virtual void LoadContent() { }
    public virtual void Update(GameTime gameTime) { }
    public virtual void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont) { }
}