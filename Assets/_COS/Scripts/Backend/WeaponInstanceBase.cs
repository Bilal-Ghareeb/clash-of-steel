using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public abstract class WeaponInstanceBase
{
    public WeaponData CatalogData { get; protected set; }
    public WeaponAsset Asset { get; protected set; }
    public Texture2D IconTexture { get; protected set; }
    public int Level { get; protected set; }

    public virtual string IconUrl { get; protected set; }

    private static readonly Dictionary<string, Texture2D> s_iconCache = new();

    public virtual void Initialize(WeaponData catalogData, int level)
    {
        CatalogData = catalogData;
        Level = level;
        Asset = WeaponAssetProvider.Database.GetAssetFor(catalogData.name);
    }

    public virtual int GetDamage()
    {
        var progression = PlayFabManager.Instance.EconomyService.ProgressionFormulas[CatalogData.progressionId];
        return WeaponProgressionCalculator.GetDamage(CatalogData.baseDamage, Level, progression);
    }

    public virtual int GetHealth()
    {
        var progression = PlayFabManager.Instance.EconomyService.ProgressionFormulas[CatalogData.progressionId];
        return WeaponProgressionCalculator.GetHealth(CatalogData.baseHealth, Level, progression);
    }

    public virtual async Task EnsureIconLoadedAsync()
    {
        if (IconTexture != null)
            return;

        if (string.IsNullOrEmpty(IconUrl))
            return;

        if (s_iconCache.TryGetValue(IconUrl, out var cachedTex))
        {
            IconTexture = cachedTex;
            return;
        }

        using (var req = UnityWebRequestTexture.GetTexture(IconUrl))
        {
            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result == UnityWebRequest.Result.Success)
            {
                IconTexture = DownloadHandlerTexture.GetContent(req);
                s_iconCache[IconUrl] = IconTexture;
            }
            else
            {
                Debug.LogWarning($"Failed to load icon for weapon '{CatalogData?.name}': {req.error}");
            }
        }
    }

    public virtual string GetRarityClass()
    {
        if (CatalogData == null)
            return WeaponItemComponentStyleClasses.GetUnknownCardStyle();

        return WeaponItemComponentStyleClasses.GetRarityClass(CatalogData.rarity);
    }

    public virtual string GetClassTypeClass()
    {
        if (CatalogData == null)
            return string.Empty;

        return WeaponItemComponentStyleClasses.GetClassTypeClass(CatalogData.@class);
    }
}
