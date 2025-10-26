using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayFabManager : MonoBehaviour
{
    public static PlayFabManager Instance { get; private set; }

    #region Services Properties
    public AuthService AuthService => ServiceLocator.Get<AuthService>();
    public EconomyService EconomyService => ServiceLocator.Get<EconomyService>();
    public PlayFabContext PlayFabContext => ServiceLocator.Get<PlayFabContext>();
    public TimeService TimeService => ServiceLocator.Get<TimeService>();
    public PlayerService PlayerService => ServiceLocator.Get<PlayerService>();
    public AzureService AzureService => ServiceLocator.Get<AzureService>();
    public NetworkService NetworkService => ServiceLocator.Get<NetworkService>();
    public IAPService IAPService => ServiceLocator.Get<IAPService>();
    public ADService ADService => ServiceLocator.Get<ADService>();
    #endregion

    #region Events
    public event Action OnLoginAndDataReady;
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ServiceLocator.Register(new AuthService());
        ServiceLocator.Register(new EconomyService());
        ServiceLocator.Register(new PlayFabContext());
        ServiceLocator.Register(new TimeService()); 
        ServiceLocator.Register(new PlayerService());
        ServiceLocator.Register(new AzureService());
        ServiceLocator.Register(new NetworkService());
        ServiceLocator.Register(new IAPService());
        ServiceLocator.Register(new ADService());
    }

    private async void Start()
    {
        await LocalizationManager.InitializeLocaleFromPrefsOrDefault();

        OnLoginAndDataReady += HandleLoginAndDataReady;
        AzureService.OnBattleStageRewardsClaimed += HandleBattleStageClaimed;
        NetworkService.OnDisconnected += HandleDisconnected;

        NetworkService.StartMonitoring(this, async isOnline =>
        {
            if (isOnline)
            {
                try
                {
                    await IAPService.InintIAP();
                    AuthService.Login();
                    ADService.Init();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize IAP or login: {ex.Message}");
                }
            }
        });
    }


    private void OnDisable()
    {
        OnLoginAndDataReady -= HandleLoginAndDataReady;
        AzureService.OnBattleStageRewardsClaimed -= HandleBattleStageClaimed;
        NetworkService.OnDisconnected -= HandleDisconnected;
    }

    private void HandleDisconnected()
    {
        NetworkService.StopMonitoring(this);

        if (SceneManager.GetActiveScene().name != "LoginScene")
        {
            SceneManager.LoadScene("LoginScene");
        }
    }

    public void RetryConnection()
    {
        NetworkService.StartMonitoring(this,async isOnline =>
        {
            if (isOnline)
            {
                await IAPService.InintIAP();
                AuthService.Login();
                AuthService.Login();
                ADService.Init();
            }
        });
    }

    public void RaiseLoginReady()
    {
        OnLoginAndDataReady?.Invoke();
    }

    private void HandleLoginAndDataReady()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void HandleBattleStageClaimed()
    {
        SceneManager.LoadScene("GameScene");
    }
}
