using UnityEngine;

public static class ActionResolver
{
    private const float AdvantageMultiplier = 1.15f;
    private const float DisadvantageMultiplier = 0.85f;


    public static int ResolveDamage(Combatant attacker, Combatant defender)
    {
        int effectiveAttackPoints = Mathf.Max(attacker.AttackPoints - defender.DefendPoints, 0);
        if (effectiveAttackPoints <= 0) return 0;

        float classMult = GetClassMultiplier(attacker.ClassType, defender.ClassType);

        float raw = attacker.BaseAttack * effectiveAttackPoints * classMult;
        return Mathf.Max(0, Mathf.RoundToInt(raw));
    }

    private static float GetClassMultiplier(string attackerClass, string defenderClass)
    {
        if (attackerClass == "Sword" && defenderClass == "Hammer") return AdvantageMultiplier;
        if (attackerClass == "Hammer" && defenderClass == "Shield") return AdvantageMultiplier;
        if (attackerClass == "Shield" && defenderClass == "Sword") return AdvantageMultiplier;

        if (defenderClass == "Sword" && attackerClass == "Hammer") return DisadvantageMultiplier;
        if (defenderClass == "Hammer" && attackerClass == "Shield") return DisadvantageMultiplier;
        if (defenderClass == "Shield" && attackerClass == "Sword") return DisadvantageMultiplier;

        return 1.0f;
    }
}
