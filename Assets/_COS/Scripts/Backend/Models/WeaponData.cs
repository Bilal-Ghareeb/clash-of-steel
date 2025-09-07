[System.Serializable]
public class WeaponData
{
    public string @class;
    public string name;
    public string description;
    public int baseDamage;
    public int baseHealth;
    public int level;
    public string rarity;

    public ScalingData scaling;
}

[System.Serializable]
public struct ScalingData
{
    public int damagePerLevel;
    public int healthPerLevel;
}
