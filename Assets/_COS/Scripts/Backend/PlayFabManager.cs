using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using System.Linq;
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

    private Dictionary<string, CatalogItem> m_currencyCatalog = new Dictionary<string, CatalogItem>();
    public Dictionary<string, int> Currencies { get; private set; } = new();
    public event Action<Dictionary<string, int>> OnCurrenciesUpdated;


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
    private void ContinueAfterLogin()
    {
        FetchCatalogWeapons();
    }

    private void FetchCatalogWeapons()
    {
        var request = new SearchItemsRequest
        {
            Filter = $"contentType eq '{ContentTypeWeapon}'",
        };

        PlayFabEconomyAPI.SearchItems(request,
            result =>
            {
                m_weaponCatalog = result.Items?.ToList() ?? new List<CatalogItem>();
                Debug.Log($"Found {m_weaponCatalog.Count} weapon(s) in catalog.");
                FetchCatalogCurrencies();
            },
            error =>
            {
                Debug.LogError("SearchItems failed: " + error.GenerateErrorReport());
                LoadNextScene();
            });
    }

    private void FetchCatalogCurrencies()
    {
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
                FetchAndCachePlayerInventory();
            },
            error =>
            {
                Debug.LogError("SearchItems failed: " + error.GenerateErrorReport());
            });
    }

    private async void FetchAndCachePlayerInventory()
    {
        var request = new GetInventoryItemsRequest { Entity = m_entity };

        PlayFabEconomyAPI.GetInventoryItems(request,
            async result =>
            {
                m_playerWeapons.Clear();
                Currencies.Clear();

                foreach (var item in result.Items)
                {
                    if (item.Type == "currency" && item.Id != null)
                    {
                        string friendlyId = GetCurrencyFriendlyId(item.Id);
                        Currencies[friendlyId] = item.Amount ?? 0;
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

                await System.Threading.Tasks.Task.WhenAll(downloadTasks);

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
                    GrantStarterBundle(StarterWeaponId);
                }
                else
                {
                    DebugPlayerWeapons();
                    DebugCurrencies();
                    LoadNextScene();
                }
            },
            error =>
            {
                Debug.LogError("GetInventoryItems failed: " + error.GenerateErrorReport());
                LoadNextScene();
            });
    }

    public void GrantStarterBundle(string weaponFriendlyId)
    {
        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantStarterBundle",
            FunctionParameter = new { weaponId = weaponFriendlyId },
            GeneratePlayStreamEvent = true
        };

        PlayFabCloudScriptAPI.ExecuteFunction(request,
            result =>
            {
                Debug.Log("Azure Function executed successfully.");
                if (result.FunctionResult != null)
                {
                    Debug.Log("Function result: " + result.FunctionResult.ToString());
                }

                FetchAndCachePlayerInventory();
            },
            error =>
            {
                Debug.LogError("Failed to call Azure Function: " + error.GenerateErrorReport());
                LoadNextScene();
            });
    }
    #endregion

    private void NotifyCurrenciesChanged()
    {
        OnCurrenciesUpdated?.Invoke(Currencies);
    }

    private string GetCurrencyFriendlyId(string currencyId)
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

    private void DebugPlayerWeapons()
    {
        foreach (var weapon in PlayerWeapons)
        {
            Debug.Log("The Fetched Weapon is : " + weapon.Data.name + " With level = " + weapon.Data.level);
        }
    }

    private void DebugCurrencies()
    {
        foreach (var kvp in Currencies)
        {
            Debug.Log($"Currency: {kvp.Key} = {kvp.Value}");
        }
    }

    #region Scene Flow
    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }
    #endregion
}
