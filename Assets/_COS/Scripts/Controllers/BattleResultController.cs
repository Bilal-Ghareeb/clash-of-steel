using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultController : MonoBehaviour
{
    [SerializeField] private MainBattleSceneUIManager uiManager;
    [SerializeField] private BattleManager battle;

    private BattleResultView resultView;

    private void Awake()
    {
        if (battle == null)
            battle = FindAnyObjectByType<BattleManager>();

        if (uiManager == null)
            uiManager = FindAnyObjectByType<MainBattleSceneUIManager>();
    }

    private void OnEnable()
    {
        battle.OnBattleEnded += OnBattleEnded;
    }

    private void OnDisable()
    {
        battle.OnBattleEnded -= OnBattleEnded;
        if (resultView != null)
            resultView.OnActionButtonClicked -= OnActionButtonClicked;
    }

    private void OnBattleEnded()
    {
        AudioManager.Instance.StopAllAmbience();

        resultView = uiManager.GetResultView();
        resultView.Setup(battle.PlayerWon, GetStageRewards());
        resultView.Show();
        resultView.PlayShowAnimation();

        resultView.OnActionButtonClicked += OnActionButtonClicked;
    }

    private StageRewardData GetStageRewards()
    {
        return PlayFabManager.Instance.PlayerService?.CurrentStage?.rewards;
    }

    private async void OnActionButtonClicked()
    {
        if (battle.PlayerWon)
        {
            var stage = PlayFabManager.Instance.PlayerService?.CurrentStage;
            if (stage == null)
            {
                Debug.LogError("No current stage found!");
                return;
            }

            int stageId = stage.id;
            int gold = stage.rewards?.GD ?? 0;
            await PlayFabManager.Instance.AzureService.GrantStageRewardsAsync(stageId, gold);
        }

        SceneManager.LoadScene("GameScene");
    }
}
