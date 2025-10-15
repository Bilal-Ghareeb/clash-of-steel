using UnityEngine;
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
    }

    private void OnActionButtonClickedWin()
    {
        Debug.Log("Player Won — claim rewards and leave");
    }

    private void OnActionButtonClickedLose()
    {
        Debug.Log("Player Lost — leave without rewards");
    }

    public override void Hide()
    {
        base.Hide();
        UnregisterButtonCallbacks();
    }
}
