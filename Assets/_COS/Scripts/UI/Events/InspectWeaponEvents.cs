using System;

public static class InspectWeaponEvents
{
    public static Action<WeaponInstance> WeaponSelectedForInspect;
    public static Action InspectWeaponViewShown;
    public static Action BackToArsenalButtonPressed;
}
