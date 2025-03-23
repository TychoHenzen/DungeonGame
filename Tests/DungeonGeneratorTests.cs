using System;
using NUnit.Framework;
using DungeonGame;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class DungeonGeneratorTests
    {
        [Test]
        public void GenerateDungeon_ShouldCreateValidDungeon()
        {
            // Arrange
            float[] itemSignature = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
            
            // Act
            var dungeon = DungeonGenerator.GenerateDungeon(itemSignature);
            
            // Assert
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.Signature, Is.Not.Null);
            Assert.That(dungeon.Signature.Length, Is.EqualTo(Signature.Dimensions));
            Assert.That(dungeon.Width, Is.GreaterThan(0));
            Assert.That(dungeon.Height, Is.GreaterThan(0));
            Assert.That(dungeon.TileMap, Is.Not.Null);
            Assert.That(dungeon.Enemies, Is.Not.Null);
            Assert.That(dungeon.Difficulty, Is.InRange(1, 3));
        }
        
        [Test]
        public void GenerateDungeon_ShouldCreateTraversableDungeon()
        {
            // Arrange
            float[] itemSignature = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
            
            // Act
            var dungeon = DungeonGenerator.GenerateDungeon(itemSignature);
            
            // Assert
            // Check that there are passable tiles
            bool hasPassableTiles = false;
            for (int x = 0; x < dungeon.Width; x++)
            {
                for (int y = 0; y < dungeon.Height; y++)
                {
                    if (dungeon.TileMap[x, y].IsPassable)
                    {
                        hasPassableTiles = true;
                        break;
                    }
                }
                if (hasPassableTiles) break;
            }
            
            Assert.That(hasPassableTiles, Is.True, "Dungeon should have passable tiles");
            Assert.That(dungeon.Enemies.Count, Is.GreaterThan(0), "Dungeon should have enemies");
        }
        
        [Test]
        public void GenerateDungeon_DifferentSignatures_ShouldCreateDifferentDungeons()
        {
            // Arrange
            float[] signature1 = new float[] { 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f };
            float[] signature2 = new float[] { -0.9f, -0.9f, -0.9f, -0.9f, -0.9f, -0.9f, -0.9f, -0.9f };
            
            // Act
            var dungeon1 = DungeonGenerator.GenerateDungeon(signature1);
            var dungeon2 = DungeonGenerator.GenerateDungeon(signature2);
            
            // Assert
            Assert.That(dungeon1.Signature, Is.Not.EqualTo(dungeon2.Signature));
            
            // Count different tile types to verify dungeons are different
            int hotDungeonHotTiles = 0;
            int coldDungeonHotTiles = 0;
            
            for (int x = 0; x < dungeon1.Width; x++)
            {
                for (int y = 0; y < dungeon1.Height; y++)
                {
                    if (dungeon1.TileMap[x, y].Type == "Lava")
                        hotDungeonHotTiles++;
                }
            }
            
            for (int x = 0; x < dungeon2.Width; x++)
            {
                for (int y = 0; y < dungeon2.Height; y++)
                {
                    if (dungeon2.TileMap[x, y].Type == "Lava")
                        coldDungeonHotTiles++;
                }
            }
            
            // Hot signature should have more hot tiles
            Assert.That(hotDungeonHotTiles, Is.GreaterThan(coldDungeonHotTiles));
        }
    }
}
