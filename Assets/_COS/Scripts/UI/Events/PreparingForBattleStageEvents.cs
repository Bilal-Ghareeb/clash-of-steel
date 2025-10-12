using System;
using System.Collections.Generic;

public static class PreparingForBattleStageEvents
{
    public static Action PreparingForBattleStageShown;
    public static Action LeavePreparingForBattle;
    public static Action<List<WeaponInstance> , List<StageEnemyData>> RequestBeginBattle;
}
