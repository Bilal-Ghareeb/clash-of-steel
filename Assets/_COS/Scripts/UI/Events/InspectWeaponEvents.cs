using System;

public static class InspectWeaponEvents
{
    public static Action<WeaponInstanceBase> WeaponSelectedForInspect;
    public static Action ScreenEnabled;
    public static Action BackButtonClicked;
    public static Action LevelUpWeaponClicked;
}
