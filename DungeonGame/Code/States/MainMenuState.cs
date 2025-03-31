#region

using DungeonGame.Code.Core;
using DungeonGame.Code.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

#endregion

namespace DungeonGame.Code.States;

/// <summary>
///     Main menu state
/// </summary>
public class MainMenuState(SignatureGame game) : GameState(game)
{
    public override void Update(GameTime gameTime)
    {
        // Check for input to transition to inventory
        if (Keyboard.GetState().IsKeyDown(Keys.Enter))
        {
            Game.ChangeState(GameStateType.Inventory);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
        // Draw title and instructions
        spriteBatch.DrawString(defaultFont, "Magic Signature Game", new Vector2(100, 100), Color.White);
        spriteBatch.DrawString(smallFont, "Press Enter to Start", new Vector2(100, 150), Color.White);
    }
}
