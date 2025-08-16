using GooglePlayGames.BasicApi;
using GooglePlayGames;
using PlayFab.ClientModels;
using PlayFab;
using UnityEngine;

public class PlayFabManager : MonoBehaviour
{
    private static PlayFabManager m_instance;
    public static PlayFabManager Instance {  get { return m_instance; } }

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            m_instance = this;
        }
    }

    public void LoginWithGPGS()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    internal void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            PlayGamesPlatform.Instance.RequestServerSideAccess(false, ProcessServerAuthCode);
        }
    }

    private void ProcessServerAuthCode(string serverAuthCode)
    {
        Debug.Log("Server Auth Code: " + serverAuthCode);

        var request = new LoginWithGooglePlayGamesServicesRequest
        {
            ServerAuthCode = serverAuthCode,
            CreateAccount = true,
            TitleId = PlayFabSettings.TitleId
        };

        PlayFabClientAPI.LoginWithGooglePlayGamesServices(request, OnLoginWithGooglePlayGamesServicesSuccess, OnLoginWithGooglePlayGamesServicesFailure);
    }

    private void OnLoginWithGooglePlayGamesServicesSuccess(LoginResult result)
    {
        Debug.Log("PF Login Success LoginWithGooglePlayGamesServices");
    }

    private void OnLoginWithGooglePlayGamesServicesFailure(PlayFabError error)
    {
        Debug.Log("PF Login Failure LoginWithGooglePlayGamesServices: " + error.GenerateErrorReport());
    }

}
