using System;
using DungeonGame.Code.Helpers;

namespace DungeonGame.Code.Entities;

/// <summary>
/// Represents a magical signature with standardized dimensions
/// </summary>
public sealed class Signature
{
    private readonly float[] _values;

    /// <summary>
    /// Gets the number of dimensions in the signature
    /// </summary>
    public static int Dimensions => SignatureDimensions.Names.Length;

    /// <summary>
    /// Creates a new signature with the specified values
    /// </summary>
    /// <param name="values">The signature values (must match the number of dimensions)</param>
    /// <exception cref="ArgumentException">Thrown when values length doesn't match dimensions</exception>
    public Signature(float[] values)
    {
        if (values.Length != Dimensions)
        {
            throw new ArgumentException($"Signature must have exactly {Dimensions} dimensions");
        }
        
        _values = new float[Dimensions];
        Array.Copy(values, _values, Dimensions);
    }

    /// <summary>
    /// Creates a random signature
    /// </summary>
    public static Signature CreateRandom()
    {
        var values = new float[Dimensions];
        
        for (var i = 0; i < Dimensions; i++)
        {
            values[i] = (float)(Random.Shared.NextDouble() * 2 - 1);
        }
        
        return new Signature(values);
    }

    /// <summary>
    /// Creates a signature similar to the base signature with some variance
    /// </summary>
    public static Signature CreateSimilar(Signature baseSignature, float variance)
    {
        var values = new float[Dimensions];
        
        for (var i = 0; i < Dimensions; i++)
        {
            // Add random variance within the specified range
            var delta = ((float)Random.Shared.NextDouble() * 2 - 1) * variance;
            values[i] = Math.Clamp(baseSignature[i] + delta, -1f, 1f);
        }
        
        return new Signature(values);
    }

    /// <summary>
    /// Gets the value at the specified dimension
    /// </summary>
    public float this[int index]
    {
        get
        {
            if (index >= 0 && index < Dimensions)
            {
                return _values[index];
            }

            throw new IndexOutOfRangeException("Signature dimension index out of range");
        }
    }

    /// <summary>
    /// Gets the raw values array (copy)
    /// </summary>
    public float[] GetValues()
    {
        var copy = new float[Dimensions];
        Array.Copy(_values, copy, Dimensions);
        return copy;
    }

    /// <summary>
    /// Calculates similarity between this signature and another (1 = identical, 0 = completely different)
    /// </summary>
    public float CalculateSimilarityWith(Signature other)
    {
        return SignatureHelper.CalculateSimilarity(_values, other._values);
    }

    /// <summary>
    /// Calculates Euclidean distance between this signature and another
    /// </summary>
    public float CalculateDistanceFrom(Signature other)
    {
        return SignatureHelper.CalculateDistance(_values, other._values);
    }
}
