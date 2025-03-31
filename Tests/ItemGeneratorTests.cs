#region

using DungeonGame.Code.Entities;
using DungeonGame.Code.Systems;

#endregion

namespace Tests;

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
        Assert.That(item.Power, Is.GreaterThan(0));
        Assert.That(item.Signature, Is.Not.Null);
    }

    [Test]
    public void GenerateItemWithSignature_ShouldCreateItemWithSimilarSignature()
    {
        // Arrange
        var baseSignature = new Signature([0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f, 0.5f]);
        var variance = 0.3f;

        // Act
        var item = ItemGenerator.GenerateItemWithSignature(baseSignature, variance);

        // Assert
        Assert.That(item, Is.Not.Null);
        Assert.That(item.Signature, Is.Not.Null);

        // Check if signatures are similar within variance
        var baseSig = baseSignature;
        var itemSig = item.Signature;
        var distance = baseSig.CalculateDistanceFrom(itemSig);

        Assert.That(distance, Is.LessThan(variance * Signature.Dimensions));
    }
}
