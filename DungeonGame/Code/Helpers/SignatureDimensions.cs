namespace DungeonGame.Code.Helpers;

/// <summary>
///     Dimensions of the magic signature
/// </summary>
public static class SignatureDimensions
{
    public static readonly string[] Names =
    {
        "Temperature",
        "Hardness",
        "Wetness",
        "Luminosity",
        "Weight",
        "Conductivity",
        "Volatility",
        "Resonance"
    };

    public static readonly string[] LowDescriptors =
    {
        "Cold",
        "Soft",
        "Dry",
        "Dark",
        "Light",
        "Insulating",
        "Stable",
        "Dissonant"
    };

    public static readonly string[] HighDescriptors =
    {
        "Hot",
        "Hard",
        "Wet",
        "Bright",
        "Heavy",
        "Conductive",
        "Volatile",
        "Resonant"
    };
}