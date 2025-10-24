using System;
using System.Collections.Generic;

public static class PreparingForBattleStageEvents
{
    public static Action ScreenEnabled;
    public static Action LeavePreparingForBattle;
    public static Action<List<WeaponInstance> , List<StageEnemyData>> RequestBeginBattle;
}
