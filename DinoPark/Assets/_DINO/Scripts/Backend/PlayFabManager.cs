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
        Debug.Log("Successfully logged in with Custom ID: " + result.PlayFabId);

        var updateNameRequest = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = "Temp"
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(updateNameRequest, OnDisplayNameUpdateSuccess, OnDisplayNameUpdateFailure);

        SceneManager.LoadScene(1);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("Login failed: " + error.GenerateErrorReport());
    }

    private void OnDisplayNameUpdateSuccess(UpdateUserTitleDisplayNameResult result)
    {
        Debug.Log("Successfully updated display name to: " + result.DisplayName);
    }

    private void OnDisplayNameUpdateFailure(PlayFabError error)
    {
        Debug.LogError("Failed to update display name: " + error.GenerateErrorReport());
    }
}