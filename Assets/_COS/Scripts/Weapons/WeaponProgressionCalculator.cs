using UnityEngine;

public static class WeaponProgressionCalculator
{
    public static int GetDamage(int baseDamage, int level, WeaponProgressionData prog)
    {
        return baseDamage + (prog.damagePerLevel * (level - 1));
    }

    public static int GetHealth(int baseHealth, int level, WeaponProgressionData prog)
    {
        return baseHealth + (prog.healthPerLevel * (level - 1));
    }

    public static int GetCostForLevelUp(int level, WeaponProgressionData prog)
    {
        return Mathf.RoundToInt(prog.costBase * Mathf.Pow(prog.costMultiplier, level - 1));
    }
}