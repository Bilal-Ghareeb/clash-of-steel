using System;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleResultView : UIView
{
    public event Action OnActionButtonClicked;

    private Label m_resultScreenTitle;
    private VisualElement m_battleResultPanel;
    private VisualElement m_rewardsContainer;
    private Button m_actionButton;

    private VisualTreeAsset m_rewardItemAsset;

    public BattleResultView(VisualElement topElement, bool hideOnAwake = true)
        : base(topElement, hideOnAwake)
    {
        m_rewardItemAsset = Resources.Load<VisualTreeAsset>("RewardItem");
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
        m_actionButton.clicked += () => OnActionButtonClicked?.Invoke();
    }

    protected void UnregisterButtonCallbacks()
    {
        m_actionButton.clicked -= () => OnActionButtonClicked?.Invoke();
    }

    public void Setup(bool playerWon, StageRewardData rewards)
    {
        m_resultScreenTitle.text = playerWon ? "STAGE CLEARED!" : "STAGE LOST!";
        m_rewardsContainer.style.display = playerWon ? DisplayStyle.Flex : DisplayStyle.None;
        m_actionButton.text = playerWon ? "CLAIM & LEAVE" : "LEAVE";

        m_actionButton.RemoveFromClassList("lose-style");
        m_actionButton.RemoveFromClassList("win-style");
        m_actionButton.AddToClassList(playerWon ? "win-style" : "lose-style");

        if (playerWon)
            PopulateRewards(rewards);
    }

    private void PopulateRewards(StageRewardData rewards)
    {
        m_rewardsContainer.Clear();
        if (rewards == null) return;

        if (rewards.GD > 0)
        {
            TemplateContainer itemTemplate = m_rewardItemAsset.Instantiate();
            var rewardItem = new RewardItemComponent();
            rewardItem.SetVisualElements(itemTemplate);
            rewardItem.SetRewardData(rewards.GD);
            m_rewardsContainer.Add(itemTemplate);
        }
    }

    public void PlayShowAnimation()
    {
        m_battleResultPanel.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_battleResultPanel.experimental.animation.Scale(1f, 200);
    }
}
