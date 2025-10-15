using UnityEngine.UIElements;

public class RewardItemComponent
{
    private VisualElement m_rewardImage;
    private Label m_rewardAmount;

    public void SetVisualElements(TemplateContainer rewardItemUXMLTemplate)
    {
        if (rewardItemUXMLTemplate == null) return;

        m_rewardImage = rewardItemUXMLTemplate.Q<VisualElement>("reward-icon");
        m_rewardAmount = rewardItemUXMLTemplate.Q<Label>("reward-amount");
    }

    public void SetRewardData(int amount)
    {
        m_rewardAmount.text = amount.ToString();
    }

}
