using UnityEngine;
using UnityEngine.SceneManagement;

public class BattleResultController : MonoBehaviour
{
    [SerializeField] private MainBattleSceneUIManager uiManager;
    [SerializeField] private BattleManager battle;

    private BattleResultView m_resultView;

    private void OnEnable()
    {
        battle.OnBattleEnded += OnBattleEnded;
    }

    private void OnDisable()
    {
        battle.OnBattleEnded -= OnBattleEnded;
        if (m_resultView != null)
            m_resultView.OnActionButtonClicked -= OnActionButtonClicked;
    }

    private void OnBattleEnded()
    {
        AudioManager.Instance.StopAllAmbience();

        m_resultView = uiManager.GetResultView();
        m_resultView.Setup(battle.PlayerWon, GetStageRewards());
        m_resultView.Show();
        m_resultView.PlayShowAnimation();

        m_resultView.OnActionButtonClicked += OnActionButtonClicked;
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
