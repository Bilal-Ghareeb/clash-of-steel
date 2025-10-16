using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using CatalogItem = PlayFab.EconomyModels.CatalogItem;

public class PlayFabManager : MonoBehaviour
{
    private static PlayFabManager m_instance;
    public static PlayFabManager Instance => m_instance;

    private const string StarterWeaponId = "sword_common_01";
    private const string ContentTypeWeapon = "Weapon";

    private PlayFab.EconomyModels.EntityKey m_entity;
    private List<CatalogItem> m_weaponCatalog = new List<CatalogItem>();

    private List<WeaponInstance> m_playerWeapons = new List<WeaponInstance>();
    public IReadOnlyList<WeaponInstance> PlayerWeapons => m_playerWeapons;

    private Dictionary<string, WeaponProgressionData> m_progressionFormulas
    = new Dictionary<string, WeaponProgressionData>();

    public IReadOnlyDictionary<string, WeaponProgressionData> ProgressionFormulas => m_progressionFormulas;

    private Dictionary<string, CatalogItem> m_currencyCatalog = new Dictionary<string, CatalogItem>();

    private Dictionary<string, int> m_playerCurrencies = new();
    public Dictionary<string, int> PlayerCurrencies => m_playerCurrencies;

    public event Action<Dictionary<string, int>> OnCurrenciesUpdated;

    private List<StageData> m_battleStages = new();
    public IReadOnlyList<StageData> BattleStages => m_battleStages;

    private int m_currentStageId = 1;
    public int CurrentStageId => m_currentStageId;

    public event Action OnLoginAndDataReady;
    public event Action OnBattleStageRewardsClaimed;

    private void Awake()
    {
        if (m_instance != null && m_instance != this) { Destroy(gameObject); }
        else { m_instance = this; DontDestroyOnLoad(gameObject); }
    }

    #region Login
    public void LoginWithCustomID()
    {
#if UNITY_ANDROID
        var request = new LoginWithAndroidDeviceIDRequest
        {
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);
#else
        var request = new LoginWithCustomIDRequest {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
#endif
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"Logged in: {result.PlayFabId}");

        PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest(),
            resp => {
                m_entity = new PlayFab.EconomyModels.EntityKey { Id = resp.Entity.Id, Type = resp.Entity.Type };
                CheckOrAssignDisplayName(result.PlayFabId);
            },
            err => {
                Debug.LogError("GetEntityToken failed: " + err.GenerateErrorReport());
                LoadNextScene();
            });
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }
    #endregion

    #region Display Name
    private void CheckOrAssignDisplayName(string playFabId)
    {
        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest { PlayFabId = playFabId },
            profileResult =>
            {
                var hasName = profileResult?.PlayerProfile != null &&
                              !string.IsNullOrEmpty(profileResult.PlayerProfile.DisplayName);

                if (hasName)
                {
                    Debug.Log($"Existing DisplayName: {profileResult.PlayerProfile.DisplayName}");
                    ContinueAfterLogin();
                }
                else
                {
                    AssignNewDisplayName();
                }
            },
            error => {
                Debug.LogError("GetPlayerProfile failed: " + error.GenerateErrorReport());
                ContinueAfterLogin();
            });
    }

    private void AssignNewDisplayName()
    {
        var nameGenerator = gameObject.AddComponent<UniqueDisplayNameGenerator>();
        nameGenerator.GenerateAndSetDisplayName(
            onSuccess: uniqueName => {
                Debug.Log($"New DisplayName assigned: {uniqueName}");
                ContinueAfterLogin();
            },
            onFailure: err => {
                Debug.LogError("Could not set unique name: " + err);
                ContinueAfterLogin();
            });
    }
    #endregion

    #region Economy v2 (Catalog + Inventory)
    private async void ContinueAfterLogin()
    {
        await Task.WhenAll(
            FetchCatalogWeaponsAsync(),
            FetchCatalogFormulasAsync(),
            FetchCatalogCurrenciesAsync(),
            FetchCatalogStagesAsync(),
            FetchAndCachePlayerInventoryAsync()
        );

        OnLoginAndDataReady?.Invoke();
    }

    private async Task FetchCatalogWeaponsAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new SearchItemsRequest
        {
            Filter = $"contentType eq '{ContentTypeWeapon}'",
        };

        PlayFabEconomyAPI.SearchItems(request,
            result =>
            {
                m_weaponCatalog = result.Items?.ToList() ?? new List<CatalogItem>();
                Debug.Log($"Found {m_weaponCatalog.Count} weapon(s) in catalog.");
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError("SearchItems (Weapons) failed: " + error.GenerateErrorReport());
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
                    Debug.Log($"Loaded {m_battleStages.Count} stages from catalog.");
                }
                else
                {
                    Debug.LogWarning("No StageData item found in catalog.");
                }

                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError("SearchItems (StageData) failed: " + error.GenerateErrorReport());
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

                        Debug.Log($"Loaded {m_progressionFormulas.Count} progression formulas.");
                    }
                    else
                    {
                        Debug.LogWarning("No ProgressionFormulas item found in Formula catalog.");
                    }
                }
                else
                {
                    Debug.LogWarning("No Formula items found in catalog.");
                }

                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError("SearchItems (Formulas) failed: " + error.GenerateErrorReport());
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

                Debug.Log($"Found {m_currencyCatalog.Count} currency/currencies in catalog.");
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError("SearchItems (Currencies) failed: " + error.GenerateErrorReport());
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }


    private void RefreshCurrencies()
    {
        var request = new GetInventoryItemsRequest { Entity = m_entity };
        PlayFabEconomyAPI.GetInventoryItems(request,
            result =>
            {
                m_playerCurrencies.Clear();
                foreach (var item in result.Items)
                {
                    if (item.Type == "currency" && item.Id != null)
                    {
                        string friendlyId = GetCurrencyFriendlyId(item.Id);
                        m_playerCurrencies[friendlyId] = item.Amount ?? 0;
                    }
                }
                NotifyCurrenciesUpdated();
            },
            error => Debug.LogError("Failed to refresh currencies: " + error.GenerateErrorReport()));
    }

    public void NotifyCurrenciesUpdated()
    {
        OnCurrenciesUpdated?.Invoke(m_playerCurrencies);
    }

    private async Task FetchAndCachePlayerInventoryAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new GetInventoryItemsRequest { Entity = m_entity };

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
                            else
                            {
                                Debug.LogWarning($"Inventory item {item.Id} not found in catalog!");
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

                    if (!ownedFriendlyIds.Contains(StarterWeaponId))
                    {
                        await GrantStarterBundleAsync(StarterWeaponId);
                    }
                    else
                    {
                        LoadNextScene();
                    }

                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing inventory: {ex}");
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                Debug.LogError("GetInventoryItems failed: " + error.GenerateErrorReport());
                LoadNextScene();
                tcs.TrySetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }


    public async Task GrantStarterBundleAsync(string weaponFriendlyId)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantStarterBundle",
            FunctionParameter = new { weaponId = weaponFriendlyId },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                try
                {
                    Debug.Log("Azure Function executed successfully.");
                    if (result.FunctionResult != null)
                    {
                        Debug.Log("Function result: " + result.FunctionResult.ToString());
                    }

                    await FetchAndCachePlayerInventoryAsync();

                    tcs.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error after executing GrantStarterBundle: {ex}");
                    tcs.TrySetException(ex);
                }
            },
            error =>
            {
                Debug.LogError("Failed to call Azure Function: " + error.GenerateErrorReport());
                LoadNextScene();
                tcs.TrySetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }


    public async Task LevelWeaponAsync(string weaponInstanceId,string currencyFriendlyId,int cost)
    {
        var taskCompletionSource = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "LevelWeapon",
            FunctionParameter = new
            {
                instanceId = weaponInstanceId,
                currencyFriendlyId,
                cost
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            result =>
            {
                taskCompletionSource.SetResult(true);
            },
            error =>
            {
                taskCompletionSource.SetException(new Exception(error.GenerateErrorReport()));
            }
        );

        await taskCompletionSource.Task;
    }

    public async Task GrantStageRewardsAsync(int stageId, int gold)
    {
        var tcs = new TaskCompletionSource<bool>();

        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantStageRewards",
            FunctionParameter = new { stageId, gold },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            async result =>
            {
                Debug.Log($"GrantStageRewards executed: {result.FunctionResult}");
                await FetchAndCachePlayerInventoryAsync();
                await FetchPlayerStageProgressAsync();
                OnBattleStageRewardsClaimed?.Invoke();
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError($"GrantStageRewards failed: {error.GenerateErrorReport()}");
                tcs.SetException(new Exception(error.ErrorMessage));
            });

        await tcs.Task;
    }


    #endregion

    public async Task FetchPlayerStageProgressAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        PlayFabClientAPI.GetUserData(new GetUserDataRequest(),
            result =>
            {
                if (result.Data != null && result.Data.TryGetValue("CurrentStageId", out var stageData))
                {
                    if (int.TryParse(stageData.Value, out int stageId))
                        m_currentStageId = stageId;
                    else
                        m_currentStageId = 1;
                }
                else
                {
                    m_currentStageId = 1;
                }

                Debug.Log($"Fetched player current stage: {m_currentStageId}");
                tcs.SetResult(true);
            },
            error =>
            {
                Debug.LogError("Failed to fetch player stage progress: " + error.GenerateErrorReport());
                m_currentStageId = 1;
                tcs.SetResult(false);
            });

        await tcs.Task;
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


    public CatalogItem GetCatalogItemByFriendlyId(string friendlyId)
    {
        return m_weaponCatalog?.FirstOrDefault(item =>
           item.AlternateIds != null &&
           item.AlternateIds.Any(a => a.Type == "FriendlyId" && a.Value == friendlyId));
    }


    #region Scene Flow
    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }
    #endregion
}
