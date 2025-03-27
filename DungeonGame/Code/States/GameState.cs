#region

using DungeonGame.Code.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DungeonGame.Code.States;

/// <summary>
///     Base class for game states
/// </summary>
public abstract class GameState(SignatureGame game)
{
    protected readonly SignatureGame Game = game;

    public virtual void LoadContent()
    {
    }

    public virtual void Update(GameTime gameTime)
    {
    }

    public virtual void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
    }
}