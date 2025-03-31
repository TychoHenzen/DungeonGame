namespace DungeonGame.Const;

public class CombatConstants
{
    // Combat constants
    public float PlayerRecoveryPercent { get; init; } = 0.15f;
    public float AffinityAttackBonus { get; init; } = 0.5f;
    public float AffinityDefenseBonus { get; init; } = 0.3f;
    public float AffinitySpeedBonus { get; init; } = 0.2f;
    public float AffinityScaleFactor { get; init; } = 2.5f;
    public float PlayerSpeedAdvantageFactor { get; init; } = 0.5f;
    public float EnemyDefenseFactor { get; init; } = 0.2f;
    public float PlayerDefenseFactor { get; init; } = 0.5f;
    public float LowHealthThreshold { get; init; } = 0.3f;
    public int MaxCombatRounds { get; init; } = 20;

    // Item generation constants
    public int MinLootCount { get; init; } = 1;
    public int MaxLootCount { get; init; } = 4;
}
