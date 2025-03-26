using DungeonGame.Code.Entities;
using DungeonGame.Code.Systems;

namespace Tests
{
    [TestFixture]
    public class DungeonGeneratorTests
    {
        [Test]
        public void GenerateDungeon_ShouldCreateValidDungeon()
        {
            // Arrange
            Signature itemSignature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f]);

            // Act
            var dungeon = DungeonGenerator.GenerateDungeon(itemSignature);

            // Assert
            Assert.That(dungeon, Is.Not.Null);
            Assert.That(dungeon.Signature, Is.Not.Null);
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
            Signature itemSignature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f]);

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
            var signature1 = new Signature([0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f, 0.9f]);
            var signature2 = new Signature([-0.9f, -0.9f, -0.9f, -0.9f, -0.9f, -0.9f, -0.9f, -0.9f]);

            // Act
            var dungeon1 = DungeonGenerator.GenerateDungeon(signature1);
            var dungeon2 = DungeonGenerator.GenerateDungeon(signature2);

            // Assert
            Assert.That(dungeon1.Signature, Is.Not.EqualTo(dungeon2.Signature));

            // Count different tile types to verify dungeons are different
            int differentTileCount = 0;

            // Compare the same size area of both dungeons
            int minWidth = Math.Min(dungeon1.Width, dungeon2.Width);
            int minHeight = Math.Min(dungeon1.Height, dungeon2.Height);

            for (int x = 0; x < minWidth; x++)
            {
                for (int y = 0; y < minHeight; y++)
                {
                    if (dungeon1.TileMap[x, y].Type != dungeon2.TileMap[x, y].Type)
                    {
                        differentTileCount++;
                    }
                }
            }

            // Dungeons should have at least some different tiles
            Assert.That(differentTileCount, Is.GreaterThan(0),
                "Dungeons with different signatures should have different tile compositions");
        }
    }
}