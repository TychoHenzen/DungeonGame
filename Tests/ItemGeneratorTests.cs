using System;
using NUnit.Framework;
using DungeonGame;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class ItemGeneratorTests
    {
        [Test]
        public void GenerateRandomItem_ShouldCreateValidItem()
        {
            // Arrange - nothing needed
            
            // Act
            var item = ItemGenerator.GenerateRandomItem();
            
            // Assert
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Name, Is.Not.Null.Or.Empty);
            Assert.That(item.Type, Is.Not.Null.Or.Empty);
            Assert.That(item.Slot, Is.Not.Null.Or.Empty);
            Assert.That(item.Power, Is.GreaterThan(0));
            Assert.That(item.Signature, Is.Not.Null);
            Assert.That(item.Signature.Length, Is.EqualTo(Signature.Dimensions));
        }
        
        [Test]
        public void GenerateItemWithSignature_ShouldCreateItemWithSimilarSignature()
        {
            // Arrange
            float[] baseSignature = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
            float variance = 0.3f;
            
            // Act
            var item = ItemGenerator.GenerateItemWithSignature(baseSignature, variance);
            
            // Assert
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Signature, Is.Not.Null);
            
            // Check if signatures are similar within variance
            var baseSig = new Signature(baseSignature);
            var itemSig = new Signature(item.Signature);
            float distance = baseSig.CalculateDistanceFrom(itemSig);
            
            Assert.That(distance, Is.LessThan(variance * Signature.Dimensions));
        }
        
        [Test]
        public void GenerateSimilarSignature_ShouldReturnArrayOfCorrectLength()
        {
            // Arrange
            float[] baseSignature = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
            
            // Act
            float[] result = ItemGenerator.GenerateSimilarSignature(baseSignature, 0.2f);
            
            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.EqualTo(baseSignature.Length));
        }
    }
}
