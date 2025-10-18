using PlayFab;
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;


public class EconomyService
{
    #region Fields
    private List<CatalogItem> m_weaponCatalog = new();
    private List<WeaponInstance> m_playerWeapons = new();
    private Dictionary<string, WeaponProgressionData> m_progressionFormulas = new();
    private Dictionary<string, CatalogItem> m_currencyCatalog = new();
    private Dictionary<string, int> m_playerCurrencies = new();
    private List<StageData> m_battleStages = new();
    #endregion

    #region Properties
    public IReadOnlyList<WeaponInstance> PlayerWeapons => m_playerWeapons;
    public IReadOnlyDictionary<string, WeaponProgressionData> ProgressionFormulas => m_progressionFormulas;
    public Dictionary<string, int> PlayerCurrencies => m_playerCurrencies;
    public IReadOnlyList<StageData> BattleStages => m_battleStages;
    #endregion

    #region Events
    public event Action<Dictionary<string, int>> OnCurrenciesUpdated;
    #endregion

    public async Task FetchAllCatalogsAndInventoryAsync()
    {
        await Task.WhenAll(
            FetchCatalogWeaponsAsync(),
            FetchCatalogFormulasAsync(),
            FetchCatalogCurrenciesAsync(),
            FetchCatalogStagesAsync(),
            FetchAndCachePlayerInventoryAsync()
        );
    }

    private async Task FetchCatalogWeaponsAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new SearchItemsRequest
        {
            Filter = $"contentType eq 'Weapon'",
        };

        PlayFabEconomyAPI.SearchItems(request,
            result =>
            {
                m_weaponCatalog = result.Items?.ToList() ?? new List<CatalogItem>();
                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }

    private async Task FetchCatalogFormulasAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new SearchItemsRequest
        {
            Filter = $"contentType eq 'Formula'"
        };

        PlayFabEconomyAPI.SearchItems(request,
            result =>
            {
                if (result.Items != null && result.Items.Count > 0)
                {
                    var formulasItem = result.Items.FirstOrDefault(i =>
                        i.AlternateIds?.Any(a => a.Type == "FriendlyId" && a.Value == "ProgressionFormulas") == true
                        || i.Id == "ProgressionFormulas");

                    if (formulasItem != null && formulasItem.DisplayProperties != null)
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(formulasItem.DisplayProperties);
                        m_progressionFormulas = Newtonsoft.Json.JsonConvert
                            .DeserializeObject<Dictionary<string, WeaponProgressionData>>(json);
                    }
                }

                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }

    private async Task FetchCatalogStagesAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new SearchItemsRequest
        {
            Filter = $"contentType eq 'Stage'"
        };

        PlayFabEconomyAPI.SearchItems(request,
            result =>
            {
                var stageItem = result.Items?.FirstOrDefault(
                    i => i.AlternateIds?.Any(a => a.Type == "FriendlyId" && a.Value == "StageData") == true
                      || i.Id == "StageData");

                if (stageItem != null && stageItem.DisplayProperties != null)
                {
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(stageItem.DisplayProperties);
                    var stageList = Newtonsoft.Json.JsonConvert.DeserializeObject<StageList>(json);
                    m_battleStages = stageList.stages;
                }

                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }

    private async Task FetchCatalogCurrenciesAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new SearchItemsRequest
        {
            Filter = $"type eq 'currency'",
        };

        PlayFabEconomyAPI.SearchItems(request,
            result =>
            {
                m_currencyCatalog = result.Items?
                    .Where(item => item != null)
                    .ToDictionary(item => item.Id, item => item)
                    ?? new Dictionary<string, CatalogItem>();

                tcs.SetResult(true);
            },
            error =>
            {
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }

    public void NotifyCurrenciesUpdated()
    {
        OnCurrenciesUpdated?.Invoke(m_playerCurrencies);
    }

    public async Task FetchAndCachePlayerInventoryAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new GetInventoryItemsRequest { Entity = PlayFabManager.Instance.PlayFabContext.EntityKey };

        PlayFabEconomyAPI.GetInventoryItems(request,
            async result =>
            {
                try
                {
                    m_playerWeapons.Clear();
                    m_playerCurrencies.Clear();

                    foreach (var item in result.Items)
                    {
                        if (item.Type == "currency" && item.Id != null)
                        {
                            string friendlyId = GetCurrencyFriendlyId(item.Id);
                            m_playerCurrencies[friendlyId] = item.Amount ?? 0;
                        }
                        else
                        {
                            var catalogItem = m_weaponCatalog.FirstOrDefault(c => c.Id == item.Id);
                            if (catalogItem != null)
                            {
                                var weaponInstance = new WeaponInstance(item, catalogItem);
                                m_playerWeapons.Add(weaponInstance);
                            }
                        }
                    }

                    var downloadTasks = m_playerWeapons
                        .Select(w => w.DownloadIconAsync())
                        .ToArray();

                    await Task.WhenAll(downloadTasks);

                    var ownedFriendlyIds = new HashSet<string>();
                    foreach (var weapon in m_playerWeapons)
                    {
                        var catalogItem = m_weaponCatalog.FirstOrDefault(c => c.Id == weapon.Item.Id);
                        if (catalogItem?.AlternateIds != null)
                        {
                            foreach (var alt in catalogItem.AlternateIds)
                            {
                                if (alt.Type == "FriendlyId" && !string.IsNullOrEmpty(alt.Value))
                                    ownedFriendlyIds.Add(alt.Value);
                            }
                        }
                    }

                    if (!ownedFriendlyIds.Contains("sword_common_01"))
                    {
                        await PlayFabManager.Instance.AzureService.GrantStarterBundleAsync("sword_common_01");
                    }

                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }

    public CatalogItem GetCatalogItemByFriendlyId(string friendlyId)
    {
        return m_weaponCatalog?.FirstOrDefault(item =>
           item.AlternateIds != null &&
           item.AlternateIds.Any(a => a.Type == "FriendlyId" && a.Value == friendlyId));
    }

    public WeaponData GetWeaponDataByFriendlyId(string friendlyId)
    {
        var catalogItem = m_weaponCatalog?.FirstOrDefault(item =>
            item.AlternateIds != null &&
            item.AlternateIds.Any(a => a.Type == "FriendlyId" && a.Value == friendlyId));

        if (catalogItem == null || catalogItem.DisplayProperties == null)
            return null;

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(catalogItem.DisplayProperties);
        return Newtonsoft.Json.JsonConvert.DeserializeObject<WeaponData>(json);
    }

    public string GetCurrencyFriendlyId(string currencyId)
    {
        if (m_currencyCatalog.TryGetValue(currencyId, out var catalogItem))
        {
            if (catalogItem?.AlternateIds != null)
            {
                foreach (var alt in catalogItem.AlternateIds)
                {
                    if (alt.Type == "FriendlyId" && !string.IsNullOrEmpty(alt.Value))
                        return alt.Value;
                }
            }

            return currencyId;
        }

        return currencyId;
    }
}
