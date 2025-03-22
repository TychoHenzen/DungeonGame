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

        for (int i = 0; i < sig1.Length; i++)
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
        return 1 - CalculateDistance(sig1, sig2) / 4f;
    }
}