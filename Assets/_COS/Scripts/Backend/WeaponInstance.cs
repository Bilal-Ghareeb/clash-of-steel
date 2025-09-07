using Newtonsoft.Json;
using PlayFab.EconomyModels;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class WeaponInstance
{
    public InventoryItem Item { get; }
    public CatalogItem CatalogBaseItem { get; }
    public WeaponData Data { get; }
    public WeaponAsset Asset { get; }

    public string IconUrl => CatalogBaseItem?.Images?.FirstOrDefault()?.Url;
    public Texture2D IconTexture { get; private set; }

    public WeaponInstance(InventoryItem item, CatalogItem catalogItemRef)
    {
        Item = item;
        CatalogBaseItem = catalogItemRef;

        string json = JsonConvert.SerializeObject(item.DisplayProperties);
        Data = JsonConvert.DeserializeObject<WeaponData>(json);

        Asset = WeaponAssetProvider.Database.GetAssetFor(Data.name);
    }

    public async Task DownloadIconAsync()
    {
        if (string.IsNullOrEmpty(IconUrl)) return;
        if (IconTexture != null) return;

        using (var req = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(IconUrl))
        {
            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                IconTexture = UnityEngine.Networking.DownloadHandlerTexture.GetContent(req);
            }
            else
            {
                Debug.LogError($"Failed to load icon: {req.error}");
            }
        }
    }
}
