using System;

public static class PreparingForBattleStageEvents
{
    public static Action ScreenEnabled;
    public static Action LeavePreparingForBattle;
    public static Action RequestBeginBattle;
    public static Action<int> TeamSlotClicked;
    public static Action<WeaponInstance> ArsenalWeaponClicked; 
    public static Action RequestFetchPlayerArsenal;
    public static Action RequestStageInfo;
    public static Action ClearContainers;
}
