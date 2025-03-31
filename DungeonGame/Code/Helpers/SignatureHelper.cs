#region

using System;

#endregion

namespace DungeonGame.Code.Helpers;

/// <summary>
///     Helper class for signature-related operations
/// </summary>
public static class SignatureHelper
{
    /// <summary>
    ///     Calculates Euclidean distance between two signatures
    /// </summary>
    public static float CalculateDistance(float[] sig1, float[] sig2)
    {
        float sumSquaredDiffs = 0;
        var length = Math.Min(sig1.Length, sig2.Length);

        for (var i = 0; i < length; i++)
        {
            sumSquaredDiffs += (sig1[i] - sig2[i]) * (sig1[i] - sig2[i]);
        }

        return (float)Math.Sqrt(sumSquaredDiffs);
    }

    /// <summary>
    ///     Calculates similarity between two signatures (1 = identical, 0 = completely different)
    /// </summary>
    public static float CalculateSimilarity(float[] sig1, float[] sig2)
    {
        var maxDistance =
            (float)Math.Sqrt(Math.Max(sig1.Length, sig2.Length) *
                             4); // Maximum possible distance in n-dimensional space with values -1 to 1

        return 1 - Math.Min(CalculateDistance(sig1, sig2) / maxDistance, 1.0f);
    }
}
