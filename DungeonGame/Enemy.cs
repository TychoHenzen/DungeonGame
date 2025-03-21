namespace DungeonGame;

/// <summary>
/// Enemy class
/// </summary>
public class Enemy
{
    public string Name { get; set; }
    public string Type { get; set; }
    public float Health { get; set; }
    public float Damage { get; set; }
    public float[] Signature { get; set; }
}