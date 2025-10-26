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
        ReportProgress("ID_ATTEMPTGOOGLESIGNIN");
        TryGoogleLogin();
#else
        ReportProgress("ID_ATTEMPTWITHCUSTOMID");
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
                ReportProgress("ID_GOOGLESUCCESS");
                PlayGamesPlatform.Instance.RequestServerSideAccess(false, serverAuthCode =>
                {
                    if (!string.IsNullOrEmpty(serverAuthCode))
                    {
                        ReportProgress("ID_LOGINWITHGOOGLETOPLAYFAB");
                        TryPlayFabLoginWithGoogle(serverAuthCode);
                    }
                    else
                    {
                        ReportProgress("ID_NOAUTHCODE");
                        LoginWithCustomID();
                    }
                });
            }
            else
            {
                ReportProgress("ID_GOOGLESINGINFAILED");
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
                ReportProgress("ID_GOOGLNOTLINKED");
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
        ReportProgress("ID_LOGINSUCCESSFULL");

        PlayFabAuthenticationAPI.GetEntityToken(new GetEntityTokenRequest(),
            async resp =>
            {
                var entityKey = new PlayFab.EconomyModels.EntityKey
                {
                    Id = resp.Entity.Id,
                    Type = resp.Entity.Type
                };

                ReportProgress("ID_PFCONTEXT");
                PlayFabManager.Instance.PlayFabContext.SetEntityData(result.PlayFabId, entityKey);

                ReportProgress("ID_SYNTIME");
                await PlayFabManager.Instance.TimeService.SyncServerTimeAsync();

                ReportProgress("ID_DNAME");
                await PlayFabManager.Instance.PlayerService.CheckOrAssignDisplayNameAsync(result.PlayFabId);

                ReportProgress("ID_CATALOGINVENTORY");
                await PlayFabManager.Instance.EconomyService.FetchAllCatalogsAndInventoryAsync();

                ReportProgress("ID_CURRENTSTAGE");
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
