using GooglePlayGames;
using GooglePlayGames.BasicApi;
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using System;
using UnityEngine;


public class AuthService
{
    #region Events
    public static event Action<string> OnAuthProgress;
    #endregion

    #region Properites
    public bool IsGoogleLinked { get; private set; }
    #endregion


    public AuthService()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public void Login()
    {
#if UNITY_ANDROID
        ReportProgress("Attempting Google sign-in...");
        TryGoogleLogin();
#else
        ReportProgress("Logging in with Custom ID...");
        LoginWithCustomID();
#endif
    }

    public void LinkGooglePlayAccount(Action<bool> onLinked = null)
    {
        PlayGamesPlatform.Instance.RequestServerSideAccess(false, serverAuthCode =>
        {
            if (string.IsNullOrEmpty(serverAuthCode))
            {
                onLinked?.Invoke(false);
                return;
            }

            var linkRequest = new LinkGooglePlayGamesServicesAccountRequest
            {
                ServerAuthCode = serverAuthCode,
                ForceLink = true
            };

            PlayFabClientAPI.LinkGooglePlayGamesServicesAccount(linkRequest,
                result =>
                {
                    IsGoogleLinked = true;
                    onLinked?.Invoke(true);
                },
                error =>
                {
                    onLinked?.Invoke(false);
                });
        });
    }

    public void CheckIfGoogleIsLinked(Action<bool> onComplete = null)
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(),
            result =>
            {
                IsGoogleLinked = result.AccountInfo?.GooglePlayGamesInfo != null;
                onComplete?.Invoke(IsGoogleLinked);
            },
            error =>
            {
                IsGoogleLinked = false;
                onComplete?.Invoke(false);
            });
    }


    private void TryGoogleLogin()
    {
        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            if (success == SignInStatus.Success)
            {
                ReportProgress("Google sign-in succeeded — requesting auth code...");
                PlayGamesPlatform.Instance.RequestServerSideAccess(false, serverAuthCode =>
                {
                    if (!string.IsNullOrEmpty(serverAuthCode))
                    {
                        ReportProgress("Logging in to PlayFab with Google...");
                        TryPlayFabLoginWithGoogle(serverAuthCode);
                    }
                    else
                    {
                        ReportProgress("No auth code found — using device ID login...");
                        LoginWithCustomID();
                    }
                });
            }
            else
            {
                ReportProgress("Google sign-in failed — using device ID login...");
                LoginWithCustomID();
            }
        });
    }

    private void TryPlayFabLoginWithGoogle(string serverAuthCode)
    {
        var request = new LoginWithGooglePlayGamesServicesRequest
        {
            ServerAuthCode = serverAuthCode,
            CreateAccount = false
        };

        PlayFabClientAPI.LoginWithGooglePlayGamesServices(request,
            OnAnyLoginSuccess,
            error =>
            {
                ReportProgress("Google account not linked — logging in with Custom ID...");
                LoginWithCustomID();
            });
    }


    public void LoginWithCustomID()
    {
#if UNITY_ANDROID
        var request = new LoginWithAndroidDeviceIDRequest
        {
            AndroidDeviceId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithAndroidDeviceID(request, OnAnyLoginSuccess, OnAnyLoginFailure);
#else
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnAnyLoginSuccess, OnAnyLoginFailure);
#endif
    }


    private void OnAnyLoginSuccess(LoginResult result)
    {
        ReportProgress("Login successful — getting entity token...");

        PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest(),
            async resp =>
            {
                var entityKey = new PlayFab.EconomyModels.EntityKey
                {
                    Id = resp.Entity.Id,
                    Type = resp.Entity.Type
                };

                ReportProgress("Setting up PlayFab context...");
                PlayFabManager.Instance.PlayFabContext.SetEntityData(result.PlayFabId, entityKey);

                ReportProgress("Syncing server time...");
                await PlayFabManager.Instance.TimeService.SyncServerTimeAsync();

                ReportProgress("Checking display name...");
                await PlayFabManager.Instance.PlayerService.CheckOrAssignDisplayNameAsync(result.PlayFabId);

                ReportProgress("Fetching catalog and inventory...");
                await PlayFabManager.Instance.EconomyService.FetchAllCatalogsAndInventoryAsync();

                ReportProgress("Setting current stage...");
                await PlayFabManager.Instance.PlayerService.SetCurrentStage();

                ReportProgress("WELCOME!!");
                PlayFabManager.Instance.RaiseLoginReady();
            },
            err =>
            {
                Debug.LogError("GetEntityToken failed: " + err.GenerateErrorReport());
            });
    }

    private void OnAnyLoginFailure(PlayFabError error)
    {
        Debug.LogError($"? Login failed: {error.GenerateErrorReport()}");
    }

    private void ReportProgress(string message)
    {
        OnAuthProgress?.Invoke(message);
    }
}
