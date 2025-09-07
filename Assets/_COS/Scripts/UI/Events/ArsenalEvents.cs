using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;

public static class ArsenalEvents 
{
    public static Action ScreenEnabled;

    public static Action ArsenalSetup;

    public static Action<WeaponItemComponent> WeaponItemClicked;

    public static Action<Rarity, WeaponType> GearFiltered;

    public static Action<IReadOnlyList<WeaponInstance>> ArsenalUpdated;

}
