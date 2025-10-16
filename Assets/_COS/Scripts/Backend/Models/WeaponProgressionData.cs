using System.Collections.Generic;

public class WeaponProgressionData
{
    public int damagePerLevel { get; set; }
    public int healthPerLevel { get; set; }
    public int costBase { get; set; }
    public float costMultiplier { get; set; }
    public int maxLevel { get; set; }
    public string currencyId { get; set; }
    public int cooldownBaseSeconds { get; set; }
    public double cooldownMultiplierPerLevel { get; set; }
    public Dictionary<string, double> cooldownMultiplierPerRarity { get; set; }
}