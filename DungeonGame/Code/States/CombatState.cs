using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DungeonGame;

/// <summary>
/// Combat state - for future detailed combat visualization
/// </summary>
public class CombatState : GameState
{
    public CombatState(DungeonGame.SignatureGame game) : base(game) { }
        
    public override void Draw(SpriteBatch spriteBatch, SpriteFont defaultFont, SpriteFont smallFont)
    {
        // This would be implemented when we add detailed combat visuals
        spriteBatch.DrawString(defaultFont, "Combat Visualization (Future Feature)", new Vector2(100, 100), Color.White);
    }
}