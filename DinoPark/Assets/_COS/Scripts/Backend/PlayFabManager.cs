using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.CloudScriptModels;
using PlayFab.EconomyModels;
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

    private List<InventoryItem> m_playerWeapons = new List<InventoryItem>();
    public IReadOnlyList<InventoryItem> PlayerWeapons => m_playerWeapons;

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
                FetchAndCachePlayerWeapons();
            },
            error =>
            {
                Debug.LogError("SearchItems failed: " + error.GenerateErrorReport());
                LoadNextScene();
            });
    }

    private void FetchAndCachePlayerWeapons()
    {
        var request = new GetInventoryItemsRequest
        {
            Entity = m_entity,
        };

        PlayFabEconomyAPI.GetInventoryItems(request,
            result =>
            {
                m_playerWeapons = result.Items
                    ?.Where(i => m_weaponCatalog.Any(c => c.Id == i.Id))
                    .ToList()
                    ?? new List<InventoryItem>();

                Debug.Log($"Cached {m_playerWeapons.Count} player weapon(s).");

                var ownedFriendlyIds = new HashSet<string>();
                foreach (var inv in m_playerWeapons)
                {
                    var catalogItem = m_weaponCatalog.FirstOrDefault(c => c.Id == inv.Id);
                    if (catalogItem != null && catalogItem.AlternateIds != null)
                    {
                        foreach (var alt in catalogItem.AlternateIds)
                        {
                            if (alt.Type == "FriendlyId" && !string.IsNullOrEmpty(alt.Value))
                            {
                                ownedFriendlyIds.Add(alt.Value);
                            }
                        }
                    }
                }

                if (!ownedFriendlyIds.Contains(StarterWeaponId))
                {
                    GrantWeapon(StarterWeaponId);
                }
                else
                {
                    DebugPlayerWeapons();
                    LoadNextScene();
                }
            },
            error =>
            {
                Debug.LogError("GetInventoryItems failed: " + error.GenerateErrorReport());
                LoadNextScene();
            });
    }

    public void GrantWeapon(string weaponFriendlyId)
    {
        var request = new ExecuteFunctionRequest
        {
            FunctionName = "GrantWeaponToPlayer",
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

                LoadNextScene();
            },
            error =>
            {
                Debug.LogError("Failed to call Azure Function: " + error.GenerateErrorReport());
                LoadNextScene();
            });
    }
    #endregion

    private void DebugPlayerWeapons()
    {
        foreach(var weapon in PlayerWeapons)
        {
            Debug.Log(weapon.DisplayProperties.ToString());
        }
    }

    #region Scene Flow
    private void LoadNextScene()
    {
        SceneManager.LoadScene(1);
    }
    #endregion
}
