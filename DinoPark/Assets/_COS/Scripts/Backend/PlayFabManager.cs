using PlayFab;
using UnityEngine;
using PlayFab.ClientModels;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    private static PlayFabManager m_instance;
    public static PlayFabManager Instance { get { return m_instance; } }

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            m_instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoginWithCustomID()
    {
        var request = new LoginWithAndroidDeviceIDRequest
        {
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successfully logged in: " + result.PlayFabId);

        PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
        {
            PlayFabId = result.PlayFabId
        },
        profileResult =>
        {
            if (profileResult.PlayerProfile != null &&
                !string.IsNullOrEmpty(profileResult.PlayerProfile.DisplayName))
            {
                Debug.Log("Existing DisplayName: " + profileResult.PlayerProfile.DisplayName);
                SceneManager.LoadScene(1);
            }
            else
            {
                var nameGenerator = gameObject.AddComponent<UniqueDisplayNameGenerator>();
                nameGenerator.GenerateAndSetDisplayName(
                    uniqueName =>
                    {
                        Debug.Log("New DisplayName assigned: " + uniqueName);
                        SceneManager.LoadScene(1);
                    },
                    error =>
                    {
                        Debug.LogError("Could not set unique name: " + error);
                        SceneManager.LoadScene(1);
                    });
            }
        },
        error =>
        {
            Debug.LogError("Failed to get player profile: " + error.GenerateErrorReport());
        });
    }


    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }
}