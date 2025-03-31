#region

using DungeonGame.Code.Entities;

#endregion

namespace Tests;

[TestFixture]
public class SignatureTests
{
    [Test]
    public void CreateRandom_ShouldCreateValidSignature()
    {
        // Arrange
        // Act
        var signature = Signature.CreateRandom();

        // Assert
        Assert.That(signature, Is.Not.Null);
        Assert.That(signature.GetValues().Length, Is.EqualTo(Signature.Dimensions));
    }

    [Test]
    public void CreateSimilar_ShouldCreateSignatureWithinVariance()
    {
        // Arrange
        var baseSignature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f]);
        var variance = 0.2f;

        // Act
        var similar = Signature.CreateSimilar(baseSignature, variance);

        // Assert
        Assert.That(similar, Is.Not.Null);
        Assert.That(baseSignature.CalculateDistanceFrom(similar), Is.LessThan(variance * Signature.Dimensions));
    }

    [Test]
    public void CalculateSimilarityWith_IdenticalSignatures_ShouldReturnOne()
    {
        // Arrange
        var signature1 = new Signature([0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f]);
        var signature2 = new Signature([0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f]);

        // Act
        var similarity = signature1.CalculateSimilarityWith(signature2);

        // Assert
        Assert.That(similarity, Is.EqualTo(1.0f).Within(0.001f));
    }

    [Test]
    public void CalculateDistanceFrom_OppositeSignatures_ShouldReturnMaxDistance()
    {
        // Arrange
        var signature1 = new Signature([1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f]);
        var signature2 = new Signature([-1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f]);

        // Act
        var distance = signature1.CalculateDistanceFrom(signature2);

        // Assert
        var expectedDistance = (float)Math.Sqrt(4 * Signature.Dimensions); // 2^2 * dimensions
        Assert.That(distance, Is.EqualTo(expectedDistance).Within(0.001f));
    }

    [Test]
    public void Constructor_InvalidDimensions_ShouldThrowArgumentException()
    {
        // Arrange
        float[] invalidValues = [0.1f, 0.2f, 0.3f]; // Too few dimensions

        // Act & Assert
        Assert.That(() => new Signature(invalidValues), Throws.ArgumentException);
    }
}
