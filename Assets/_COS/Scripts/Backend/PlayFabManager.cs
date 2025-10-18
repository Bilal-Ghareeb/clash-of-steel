using System;
using UnityEngine;

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
    }

    private void Start()
    {
        OnLoginAndDataReady += HandleLoginAndDataReady;
        AzureService.OnBattleStageRewardsClaimed += HandleBattleStageClaimed;
        AuthService.Login();
    }

    private void OnDisable()
    {
        OnLoginAndDataReady -= HandleLoginAndDataReady;
        AzureService.OnBattleStageRewardsClaimed -= HandleBattleStageClaimed;
    }

    public void RaiseLoginReady()
    {
        OnLoginAndDataReady?.Invoke();
    }

    private void HandleLoginAndDataReady()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }

    private void HandleBattleStageClaimed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
