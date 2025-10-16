using Newtonsoft.Json;
using PlayFab.EconomyModels;
using System.Linq;
using System.Threading.Tasks;

public class WeaponInstance : WeaponInstanceBase
{
    public InventoryItem Item { get; }
    public CatalogItem CatalogBaseItem { get; }
    public WeaponInstanceData InstanceData { get; }

    public override string IconUrl => CatalogBaseItem?.Images?.FirstOrDefault()?.Url;

    public WeaponInstance(InventoryItem item, CatalogItem catalogItemRef)
    {
        Item = item;
        CatalogBaseItem = catalogItemRef;

        string catalogJson = JsonConvert.SerializeObject(catalogItemRef.DisplayProperties);
        CatalogData = JsonConvert.DeserializeObject<WeaponData>(catalogJson);

        string instanceJson = JsonConvert.SerializeObject(item.DisplayProperties);
        InstanceData = JsonConvert.DeserializeObject<WeaponInstanceData>(instanceJson);

        Level = InstanceData.level;
        Asset = WeaponAssetProvider.Database.GetAssetFor(CatalogData.name);
    }

    public bool IsOnCooldown => InstanceData?.IsOnCooldown ?? false;
    public float RemainingCooldownSeconds => InstanceData?.RemainingCooldownSeconds ?? 0f;

    public async Task DownloadIconAsync()
    {
        await EnsureIconLoadedAsync();
    }
}
