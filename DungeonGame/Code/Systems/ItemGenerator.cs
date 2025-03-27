#region

using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Helpers;
using DungeonGame.Const;

#endregion

namespace DungeonGame.Code.Systems;

/// <summary>
///     Item generator
/// </summary>
public static class ItemGenerator
{
    public static Item GenerateRandomItem()
    {
        // Get random item type
        var itemTypes = ItemTypes.Types.Values.ToList();
        var itemType = itemTypes[Random.Shared.Next(itemTypes.Count)];

        // Generate signature
        var signature = Signature.CreateRandom();

        // Generate name with adjectives
        var name = GenerateItemName(itemType.Name, signature);

        // Calculate power based on signature intensity
        var signatureIntensity = signature.GetValues().Sum(Math.Abs) / Signature.Dimensions;
        var power = (int)(itemType.BasePower * (1 + signatureIntensity));

        return new Item
        {
            Name = name,
            Type = itemType.Name,
            Slot = itemType.Slot,
            Power = power,
            Signature = signature
        };
    }

    public static Item GenerateItemWithSignature(Signature baseSignature)
    {
        return GenerateItemWithSignature(baseSignature, Constants.Game.DefaultSignatureVariance);
    }

    public static Item GenerateItemWithSignature(Signature baseSignature, float variance)
    {
        // Get random item type
        var itemTypes = ItemTypes.Types.Values.ToList();
        var itemType = itemTypes[Random.Shared.Next(itemTypes.Count)];

        // Generate similar signature
        var signature = Signature.CreateSimilar(baseSignature, variance);

        // Generate name with adjectives
        var name = GenerateItemName(itemType.Name, signature);

        // Calculate power based on signature intensity
        var signatureIntensity = signature.GetValues().Sum(Math.Abs) / Signature.Dimensions;
        var power = (int)(itemType.BasePower * (1 + signatureIntensity));

        return new Item
        {
            Name = name,
            Type = itemType.Name,
            Slot = itemType.Slot,
            Power = power,
            Signature = signature
        };
    }

    private static string GenerateItemName(string itemType, Signature signature)
    {
        // Generate adjectives based on signature
        var adjectives = new List<string>();

        for (var i = 0; i < Signature.Dimensions; i++)
        {
            if (signature[i] > Constants.Game.SignatureHighThreshold)
            {
                adjectives.Add(SignatureDimensions.HighDescriptors[i]);
            }
            else if (signature[i] < Constants.Game.SignatureLowThreshold)
            {
                adjectives.Add(SignatureDimensions.LowDescriptors[i]);
            }
        }

        // Take 1-2 random adjectives for the name
        if (adjectives.Count != 0)
        {
            return itemType;
        }

        var adjectiveCount = Math.Min(adjectives.Count, Random.Shared.Next(1, 3));
        var selectedAdjectives = adjectives.OrderBy(x => Random.Shared.Next()).Take(adjectiveCount).ToList();
        return string.Join(" ", selectedAdjectives) + " " + itemType;
    }
}