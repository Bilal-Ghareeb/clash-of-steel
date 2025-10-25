using System;

public static class ShopEvents
{
    public static Action ScreenEnabled;
    public static Action LootBoxPurchased;
    public static Action <LootBoxData> LootBoxPurchaseIntiated;
    public static Action <string> DiamondPurchased;
    public static Action <LootBoxData> LootBoxDeatailsInspected;
    public static Action LootBoxClicked;
    public static Action LootBoxRewardClaimed;
}
