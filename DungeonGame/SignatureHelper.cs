using System;

namespace DungeonGame;

/// <summary>
/// Helper class for signature-related operations
/// </summary>
public static class SignatureHelper
{
    /// <summary>
    /// Calculates Euclidean distance between two signatures
    /// </summary>
    public static float CalculateDistance(float[] sig1, float[] sig2)
    {
        float sumSquaredDiffs = 0;
        int length = Math.Min(sig1.Length, sig2.Length);

        for (int i = 0; i < length; i++)
        {
            sumSquaredDiffs += (sig1[i] - sig2[i]) * (sig1[i] - sig2[i]);
        }

        return (float)Math.Sqrt(sumSquaredDiffs);
    }

    /// <summary>
    /// Calculates similarity between two signatures (1 = identical, 0 = completely different)
    /// </summary>
    public static float CalculateSimilarity(float[] sig1, float[] sig2)
    {
        float maxDistance = (float)Math.Sqrt(sig1.Length); // Maximum possible distance in n-dimensional space with values 0-1
        return 1 - CalculateDistance(sig1, sig2) / maxDistance;
    }
}
