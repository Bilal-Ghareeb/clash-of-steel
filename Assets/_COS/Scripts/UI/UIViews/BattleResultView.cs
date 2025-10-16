using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class BattleResultView : UIView
{
    private BattleManager m_battle;

    private Label m_resultScreenTitle;
    private VisualElement m_battleResultPanel;
    private VisualElement m_rewardsContainer;
    private Button m_actionButton;

    private VisualTreeAsset m_rewardItemAsset;

    private bool m_playerWon;

    public BattleResultView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        m_rewardItemAsset = Resources.Load<VisualTreeAsset>("RewardItem");
    }

    public void InitializeManagers(BattleManager manager)
    {
        m_battle = manager;
        m_battle.OnBattleEnded += Show;
    }

    public override void Dispose()
    {
        base.Dispose();
        m_battle.OnBattleEnded -= Show;
        UnregisterButtonCallbacks();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_battleResultPanel = m_TopElement.Q<VisualElement>("BattleResultView");
        m_rewardsContainer = m_TopElement.Q<VisualElement>("Rewards-Container");
        m_actionButton = m_TopElement.Q<Button>("action-btn");
        m_resultScreenTitle = m_TopElement.Q<Label>("result-title");
    }

    protected override void RegisterButtonCallbacks()
    {
        UnregisterButtonCallbacks();

        if (m_playerWon)
            m_actionButton.clicked += OnActionButtonClickedWin;
        else
            m_actionButton.clicked += OnActionButtonClickedLose;
    }

    protected void UnregisterButtonCallbacks()
    {
        m_actionButton.clicked -= OnActionButtonClickedWin;
        m_actionButton.clicked -= OnActionButtonClickedLose;
    }

    public override void Show()
    {
        base.Show();

        m_battleResultPanel.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_battleResultPanel.experimental.animation.Scale(1f, 200);

        SetupScreenBasedOnBattleResult();
        RegisterButtonCallbacks();
    }

    private void SetupScreenBasedOnBattleResult()
    {
        m_playerWon = m_battle.PlayerWon;

        m_resultScreenTitle.text = m_playerWon ? "STAGE CLEARED!" : "STAGE LOST!";
        m_rewardsContainer.style.display = m_playerWon ? DisplayStyle.Flex : DisplayStyle.None;

        m_actionButton.text = m_playerWon ? "CLAIM & LEAVE" : "LEAVE";

        m_actionButton.RemoveFromClassList("lose-style");
        m_actionButton.RemoveFromClassList("win-style");
        m_actionButton.AddToClassList(m_playerWon ? "win-style" : "lose-style");

        if (m_playerWon)
            PopulateRewards();
    }

    private void PopulateRewards()
    {
        m_rewardsContainer.Clear();

        var currentStage = BattleStageManager.Instance?.CurrentStage;
        if (currentStage == null)
        {
            Debug.LogWarning("BattleResultView: No current stage found for rewards.");
            return;
        }

        StageRewardData rewards = currentStage.rewards;
        if (rewards == null)
        {
            Debug.LogWarning("BattleResultView: No rewards data found for current stage.");
            return;
        }

        if (rewards.GD > 0)
        {
            TemplateContainer itemTemplate = m_rewardItemAsset.Instantiate();
            var rewardItem = new RewardItemComponent();
            rewardItem.SetVisualElements(itemTemplate);
            rewardItem.SetRewardData(rewards.GD);
            m_rewardsContainer.Add(itemTemplate);
        }
    }

    private async void OnActionButtonClickedWin()
    {
        var stage = BattleStageManager.Instance.CurrentStage;
        if (stage == null)
        {
            Debug.LogError("No current stage found!");
            return;
        }

        int stageId = stage.id;
        int gold = stage.rewards?.GD ?? stage.rewards?.GD ?? 0;

        await PlayFabManager.Instance.GrantStageRewardsAsync(stageId, gold);
    }


    private void OnActionButtonClickedLose()
    {
        SceneManager.LoadScene("GameScene");
    }

    public override void Hide()
    {
        base.Hide();
    }
}
