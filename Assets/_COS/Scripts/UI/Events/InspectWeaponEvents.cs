using System;

public static class InspectWeaponEvents
{
    public static Action<WeaponInstanceBase> WeaponSelectedForInspect;
    public static Action InspectWeaponViewShown;
    public static Action BackToArsenalButtonPressed;
}
