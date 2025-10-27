public static class AIController
{
    public static (int attack, int defend, int reserve) DecideAllocation(Combatant actor, Combatant[] opponents, int availablePoints)
    {
        int attack = availablePoints;
        int defend = 0;
        int reserve = 0;
        return (attack, defend, reserve);
    }
}
