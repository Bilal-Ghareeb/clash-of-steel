using PlayFab.EconomyModels;
using System.Linq;

public class CatalogWeaponInstance : WeaponInstanceBase
{
    public CatalogItem CatalogItem { get; }

    public override string IconUrl => CatalogItem?.Images?.FirstOrDefault()?.Url;

    public CatalogWeaponInstance(CatalogItem catalogItem)
    {
        CatalogItem = catalogItem;

        if (catalogItem?.DisplayProperties != null)
        {
            string catalogJson = Newtonsoft.Json.JsonConvert.SerializeObject(catalogItem.DisplayProperties);
            CatalogData = Newtonsoft.Json.JsonConvert.DeserializeObject<WeaponData>(catalogJson);
        }

        Level = 1;

        Asset = WeaponAssetProvider.Database.GetAssetFor(CatalogData.GetLocalizedName());
    }
}
