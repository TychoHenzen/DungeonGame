using System;
using NUnit.Framework;
using DungeonGame;

namespace DungeonGame.Tests
{
    [TestFixture]
    public class SignatureHelperTests
    {
        [Test]
        public void CalculateDistance_IdenticalSignatures_ShouldReturnZero()
        {
            // Arrange
            float[] sig1 = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
            float[] sig2 = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
            
            // Act
            float distance = SignatureHelper.CalculateDistance(sig1, sig2);
            
            // Assert
            Assert.That(distance, Is.EqualTo(0).Within(0.001f));
        }
        
        [Test]
        public void CalculateDistance_OppositeSignatures_ShouldReturnMaxDistance()
        {
            // Arrange
            float[] sig1 = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
            float[] sig2 = new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };
            
            // Act
            float distance = SignatureHelper.CalculateDistance(sig1, sig2);
            
            // Assert
            float expectedDistance = (float)Math.Sqrt(4 * sig1.Length); // 2^2 * dimensions
            Assert.That(distance, Is.EqualTo(expectedDistance).Within(0.001f));
        }
        
        [Test]
        public void CalculateSimilarity_IdenticalSignatures_ShouldReturnOne()
        {
            // Arrange
            float[] sig1 = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
            float[] sig2 = new float[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
            
            // Act
            float similarity = SignatureHelper.CalculateSimilarity(sig1, sig2);
            
            // Assert
            Assert.That(similarity, Is.EqualTo(1.0f).Within(0.001f));
        }
        
        [Test]
        public void CalculateSimilarity_OppositeSignatures_ShouldReturnZero()
        {
            // Arrange
            float[] sig1 = new float[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
            float[] sig2 = new float[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };
            
            // Act
            float similarity = SignatureHelper.CalculateSimilarity(sig1, sig2);
            
            // Assert
            Assert.That(similarity, Is.EqualTo(0.0f).Within(0.001f));
        }
        
        [Test]
        public void CalculateSimilarity_DifferentLengthSignatures_ShouldHandleGracefully()
        {
            // Arrange
            float[] sig1 = new float[] { 0.5f, 0.5f, 0.5f, 0.5f };
            float[] sig2 = new float[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };
            
            // Act
            float similarity = SignatureHelper.CalculateSimilarity(sig1, sig2);
            
            // Assert
            Assert.That(similarity, Is.GreaterThan(0.0f));
            Assert.That(similarity, Is.LessThanOrEqualTo(1.0f));
        }
    }
}
