using System;

namespace DungeonGame;

/// <summary>
/// Represents a magical signature with standardized dimensions
/// </summary>
public class Signature
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
    public static Signature CreateRandom(Random random = null)
    {
        random ??= new Random();
        var values = new float[Dimensions];
        
        for (int i = 0; i < Dimensions; i++)
        {
            values[i] = (float)random.NextDouble();
        }
        
        return new Signature(values);
    }

    /// <summary>
    /// Creates a signature similar to the base signature with some variance
    /// </summary>
    public static Signature CreateSimilar(Signature baseSignature, float variance, Random random = null)
    {
        random ??= new Random();
        var values = new float[Dimensions];
        
        for (int i = 0; i < Dimensions; i++)
        {
            // Add random variance within the specified range
            float delta = ((float)random.NextDouble() * 2 - 1) * variance;
            values[i] = Math.Clamp(baseSignature[i] + delta, 0f, 1f);
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
            if (index < 0 || index >= Dimensions)
            {
                throw new IndexOutOfRangeException("Signature dimension index out of range");
            }
            return _values[index];
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

    /// <summary>
    /// Gets a description of this signature
    /// </summary>
    public string GetDescription()
    {
        var description = "";
        for (int i = 0; i < Dimensions; i++)
        {
            string dimensionDesc = _values[i] < 0.5f 
                ? SignatureDimensions.LowDescriptors[i] 
                : SignatureDimensions.HighDescriptors[i];
            
            if (description.Length > 0)
                description += ", ";
                
            description += dimensionDesc;
        }
        return description;
    }
}
