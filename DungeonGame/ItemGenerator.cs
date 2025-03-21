using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonGame;

/// <summary>
/// Item generator
/// </summary>
public static class ItemGenerator
{
    private static readonly Random _random = new Random();
        
    public static Item GenerateRandomItem()
    {
        // Get random item type
        var itemTypes = ItemTypes.Types.Values.ToList();
        var itemType = itemTypes[_random.Next(itemTypes.Count)];
            
        // Generate signature
        float[] signature = GenerateRandomSignature();
            
        // Generate name with adjectives
        string name = GenerateItemName(itemType.Name, signature);
            
        // Calculate power based on signature intensity
        float signatureIntensity = signature.Sum(v => Math.Abs(v)) / signature.Length;
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
        
    public static Item GenerateItemWithSignature(float[] baseSignature, float variance = 0.3f)
    {
        // Get random item type
        var itemTypes = ItemTypes.Types.Values.ToList();
        var itemType = itemTypes[_random.Next(itemTypes.Count)];
            
        // Generate similar signature
        float[] signature = GenerateSimilarSignature(baseSignature, variance);
            
        // Generate name with adjectives
        string name = GenerateItemName(itemType.Name, signature);
            
        // Calculate power based on signature intensity
        float signatureIntensity = signature.Sum(v => Math.Abs(v)) / signature.Length;
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
        
    private static string GenerateItemName(string itemType, float[] signature)
    {
        // Generate adjectives based on signature
        var adjectives = new List<string>();
            
        for (int i = 0; i < signature.Length; i++)
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
        if (adjectives.Count > 0)
        {
            int adjectiveCount = Math.Min(adjectives.Count, _random.Next(1, 3));
            var selectedAdjectives = adjectives.OrderBy(x => _random.Next()).Take(adjectiveCount).ToList();
            return string.Join(" ", selectedAdjectives) + " " + itemType;
        }
            
        return itemType;
    }
        
    private static float[] GenerateRandomSignature()
    {
        float[] signature = new float[SignatureDimensions.Names.Length];
            
        for (int i = 0; i < signature.Length; i++)
        {
            signature[i] = (float)(_random.NextDouble() * 2 - 1);
        }
            
        return signature;
    }

    public static float[] GenerateSimilarSignature(float[] baseSignature, float variance)
    {
        float[] signature = new float[baseSignature.Length];
            
        for (int i = 0; i < signature.Length; i++)
        {
            float newValue = baseSignature[i] + (float)(_random.NextDouble() * 2 - 1) * variance;
            signature[i] = Math.Max(-1, Math.Min(1, newValue)); // Clamp between -1 and 1
        }
            
        return signature;
    }
}