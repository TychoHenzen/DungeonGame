using Microsoft.Xna.Framework.Graphics;

namespace DungeonGame.Code.Interfaces;

/// <summary>
/// Interface for classes that need a texture
/// </summary>
public interface ITextureUser
{
    void SetTexture(Texture2D texture);
}
