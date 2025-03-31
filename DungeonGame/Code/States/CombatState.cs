#region

using DungeonGame.Code.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace DungeonGame.Code.States;

/// <summary>
///     Combat state - for future detailed combat visualization
/// </summary>
public class CombatState(SignatureGame game) : GameState(game)
{
    public override void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
        // This would be implemented when we add detailed combat visuals
        spriteBatch.DrawString(defaultFont, "Combat Visualization (Future Feature)", new Vector2(100, 100),
            Color.White);
    }
}
