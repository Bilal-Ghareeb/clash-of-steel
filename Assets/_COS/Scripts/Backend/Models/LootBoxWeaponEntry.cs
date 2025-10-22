using PlayFab.EconomyModels;

public struct LootBoxWeaponEntry
{
    public CatalogItem WeaponCatalogItem;
    public float Weight;

    public LootBoxWeaponEntry(CatalogItem item, float weight)
    {
        WeaponCatalogItem = item;
        Weight = weight;
    }
}
