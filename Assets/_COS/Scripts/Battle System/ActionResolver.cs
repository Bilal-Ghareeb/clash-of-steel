using UnityEngine;

/// <summary>
/// Resolves the outcome of an attack from one combatant to another.
/// Supports multiple attack/defend points per turn and class advantages.
/// </summary>
public static class ActionResolver
{
    private const float AdvantageMultiplier = 1.15f;
    private const float DisadvantageMultiplier = 0.85f;

    /// <summary>
    /// Computes total damage dealt by attacker to defender this turn.
    /// </summary>
    /// <param name="attacker">Attacking combatant (must have AttackPoints set)</param>
    /// <param name="defender">Defending combatant (must have DefendPoints set)</param>
    /// <param name="hitsLanded">Optional: number of hits that actually connected</param>
    /// <returns>Total damage value to subtract from defender's HP</returns>
    public static int ResolveDamage(Combatant attacker, Combatant defender, out int hitsLanded)
    {
        // Each defend point cancels one attack point (like Jurassic World’s block system)
        hitsLanded = Mathf.Max(attacker.AttackPoints - defender.DefendPoints, 0);
        if (hitsLanded <= 0)
            return 0;

        // Base class advantage / disadvantage multiplier
        float classMult = GetClassMultiplier(attacker.ClassType, defender.ClassType);

        // Total raw damage = base attack * number of hits * multiplier
        float rawDamage = attacker.BaseAttack * hitsLanded * classMult;

        return Mathf.RoundToInt(rawDamage);
    }

    /// <summary>
    /// Overload that ignores hitsLanded output (for older calls)
    /// </summary>
    public static int ResolveDamage(Combatant attacker, Combatant defender)
    {
        return ResolveDamage(attacker, defender, out _);
    }

    private static float GetClassMultiplier(string attackerClass, string defenderClass)
    {
        // Advantage cycle: Sword > Hammer > Shield > Sword
        if (attackerClass == "Sword" && defenderClass == "Hammer") return AdvantageMultiplier;
        if (attackerClass == "Hammer" && defenderClass == "Shield") return AdvantageMultiplier;
        if (attackerClass == "Shield" && defenderClass == "Sword") return AdvantageMultiplier;

        if (defenderClass == "Sword" && attackerClass == "Hammer") return DisadvantageMultiplier;
        if (defenderClass == "Hammer" && attackerClass == "Shield") return DisadvantageMultiplier;
        if (defenderClass == "Shield" && attackerClass == "Sword") return DisadvantageMultiplier;

        // Neutral or same class
        return 1.0f;
    }
}
