#region

using DungeonGame.Code.Helpers;

#endregion

namespace Tests;

[TestFixture]
public class SignatureHelperTests
{
    [Test]
    public void CalculateDistance_IdenticalSignatures_ShouldReturnZero()
    {
        // Arrange
        var sig1 = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
        var sig2 = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };

        // Act
        var distance = SignatureHelper.CalculateDistance(sig1, sig2);

        // Assert
        Assert.That(distance, Is.EqualTo(0).Within(0.001f));
    }

    [Test]
    public void CalculateDistance_OppositeSignatures_ShouldReturnMaxDistance()
    {
        // Arrange
        var sig1 = new[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        var sig2 = new[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };

        // Act
        var distance = SignatureHelper.CalculateDistance(sig1, sig2);

        // Assert
        var expectedDistance = (float)Math.Sqrt(4 * sig1.Length); // 2^2 * dimensions
        Assert.That(distance, Is.EqualTo(expectedDistance).Within(0.001f));
    }

    [Test]
    public void CalculateSimilarity_IdenticalSignatures_ShouldReturnOne()
    {
        // Arrange
        var sig1 = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };
        var sig2 = new[] { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f };

        // Act
        var similarity = SignatureHelper.CalculateSimilarity(sig1, sig2);

        // Assert
        Assert.That(similarity, Is.EqualTo(1.0f).Within(0.001f));
    }

    [Test]
    public void CalculateSimilarity_OppositeSignatures_ShouldReturnZero()
    {
        // Arrange
        var sig1 = new[] { 1f, 1f, 1f, 1f, 1f, 1f, 1f, 1f };
        var sig2 = new[] { -1f, -1f, -1f, -1f, -1f, -1f, -1f, -1f };

        // Act
        var similarity = SignatureHelper.CalculateSimilarity(sig1, sig2);

        // Assert
        Assert.That(similarity, Is.EqualTo(0.0f).Within(0.001f));
    }

    [Test]
    public void CalculateSimilarity_DifferentLengthSignatures_ShouldHandleGracefully()
    {
        // Arrange
        var sig1 = new[] { 0.5f, 0.5f, 0.5f, 0.5f };
        var sig2 = new[] { 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f };

        // Act
        var similarity = SignatureHelper.CalculateSimilarity(sig1, sig2);

        // Assert
        Assert.That(similarity, Is.GreaterThan(0.0f));
        Assert.That(similarity, Is.LessThanOrEqualTo(1.0f));
    }
}
