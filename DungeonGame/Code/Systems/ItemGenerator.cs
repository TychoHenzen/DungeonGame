using System;
using System.Collections.Generic;
using System.Linq;
using DungeonGame.Code.Entities;
using DungeonGame.Code.Helpers;

namespace DungeonGame.Code.Systems;

/// <summary>
/// Item generator
/// </summary>
public static class ItemGenerator
{
    private static readonly Random _random = new();

    public static Item GenerateRandomItem()
    {
        // Get random item type
        var itemTypes = ItemTypes.Types.Values.ToList();
        var itemType = itemTypes[_random.Next(itemTypes.Count)];

        // Generate signature
        var signature = GenerateRandomSignature();

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

    public static Item GenerateItemWithSignature(Signature baseSignature, float variance = 0.3f)
    {
        // Get random item type
        var itemTypes = ItemTypes.Types.Values.ToList();
        var itemType = itemTypes[_random.Next(itemTypes.Count)];

        // Generate similar signature
        Signature signature = GenerateSimilarSignature(baseSignature, variance);

        // Generate name with adjectives
        string name = GenerateItemName(itemType.Name, signature);

        // Calculate power based on signature intensity
        float signatureIntensity = signature.GetValues().Sum(Math.Abs) / Signature.Dimensions;
        int power = (int)(itemType.BasePower * (1 + signatureIntensity));

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
            if (signature[i] > 0.5f)
            {
                adjectives.Add(SignatureDimensions.HighDescriptors[i]);
            }
            else if (signature[i] < -0.5f)
            {
                adjectives.Add(SignatureDimensions.LowDescriptors[i]);
            }
        }

        // Take 1-2 random adjectives for the name
        if (adjectives.Count != 0)
        {
            return itemType;
        }

        var adjectiveCount = Math.Min(adjectives.Count, _random.Next(1, 3));
        var selectedAdjectives = adjectives.OrderBy(x => _random.Next()).Take(adjectiveCount).ToList();
        return string.Join(" ", selectedAdjectives) + " " + itemType;
    }

    private static Signature GenerateRandomSignature()
    {
        return Signature.CreateRandom(_random);
    }

    public static Signature GenerateSimilarSignature(Signature baseSignature, float variance)
    {
        try
        {
            return Signature.CreateSimilar(baseSignature, variance, _random);
        }
        catch (ArgumentException)
        {
            // Fallback for invalid signatures
            return GenerateRandomSignature();
        }
    }
}