using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleActionsView : UIView
{
    private VisualElement m_playerButtonsContainer;
    private VisualElement m_enemyRevealContainer;
    private VisualElement m_playerPointsHolder;
    private VisualElement m_enemyPointsHolder;

    private Button m_playerAttackButton;
    private Button m_playerDefendButton;
    private Button m_playerReserveButton;

    private Button m_enemyAttackButton;
    private Button m_enemyDefendButton;

    private Label m_playerCurrentPoints;
    private Label m_playerMaxPoints;
    private Label m_enemyCurrentPoints;
    private Label m_enemyMaxPoints;

    private Label m_playerPointsSpentOnAttack;
    private Label m_playerPointsSpentOnDefend;
    private Label m_playerPointsSpentOnReserve;

    private Label m_enemyPointsSpentOnAttack;
    private Label m_enemyPointsSpentOnDefend;

    private Label m_CountdownLabel;


    public BattleActionsView(VisualElement topElement, bool hideOnAwake = true)
        : base(topElement, hideOnAwake)
    {
    }

    public override void Show()
    {
        base.Show();
        SetVisualElements();
        RegisterButtonCallbacks();
    }

    public override void Dispose()
    {
        base.Dispose();
        UnregisterButtonCallbacks();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_playerButtonsContainer = m_TopElement.Q<VisualElement>("PlayerActionButtonsContainer");
        m_enemyRevealContainer = m_TopElement.Q<VisualElement>("EnemyActionButtonsContainer");

        m_playerPointsHolder = m_TopElement.Q<VisualElement>("PlayerPointsHolder");
        m_enemyPointsHolder = m_TopElement.Q<VisualElement>("EnemyPointsHolder");

        m_playerAttackButton = m_TopElement.Q<Button>("AttackButton");
        m_playerDefendButton = m_TopElement.Q<Button>("DefendButton");
        m_playerReserveButton = m_TopElement.Q<Button>("ReserveButton");

        m_enemyAttackButton = m_TopElement.Q<Button>("EnemyAttackButton");
        m_enemyDefendButton = m_TopElement.Q<Button>("EnemyDefendButton");

        m_CountdownLabel = m_TopElement.Q<Label>("CountdownLabel");

        m_playerCurrentPoints = m_TopElement.Q<Label>("PlayerCurrentPoint");
        m_playerMaxPoints = m_TopElement.Q<Label>("PlayerMaxPoints");
        m_enemyCurrentPoints = m_TopElement.Q<Label>("EnemyCurrentPoint");
        m_enemyMaxPoints = m_TopElement.Q<Label>("EnemyMaxPoints");

        m_playerPointsSpentOnAttack = m_TopElement.Q<Label>("PointsOnAttack");
        m_playerPointsSpentOnDefend = m_TopElement.Q<Label>("PointsOnDefend");
        m_playerPointsSpentOnReserve = m_TopElement.Q<Label>("PointsOnReserve");

        m_enemyPointsSpentOnAttack = m_TopElement.Q<Label>("EnemyPointsOnAttack");
        m_enemyPointsSpentOnDefend = m_TopElement.Q<Label>("EnemyPointsOnDefend");

        HideAllUI();
    }

    protected override void RegisterButtonCallbacks()
    {
        m_playerAttackButton?.RegisterCallback<ClickEvent>(evt => BattleActionsEvents.SpendPointRequested?.Invoke("Attack"));
        m_playerDefendButton?.RegisterCallback<ClickEvent>(evt => BattleActionsEvents.SpendPointRequested?.Invoke("Defend"));
        m_playerReserveButton?.RegisterCallback<ClickEvent>(evt => BattleActionsEvents.SpendPointRequested?.Invoke("Reserve"));
    }

    private void UnregisterButtonCallbacks()
    {
        m_playerAttackButton?.UnregisterCallback<ClickEvent>(evt => BattleActionsEvents.SpendPointRequested?.Invoke("Attack"));
        m_playerDefendButton?.UnregisterCallback<ClickEvent>(evt => BattleActionsEvents.SpendPointRequested?.Invoke("Defend"));
        m_playerReserveButton?.UnregisterCallback<ClickEvent>(evt => BattleActionsEvents.SpendPointRequested?.Invoke("Reserve"));
    }

    public async void StartCountdown()
    {
        HideAllUI();
        string[] seq = { "3", "2", "1", "FIGHT!" };
        m_CountdownLabel.style.display = DisplayStyle.Flex;

        foreach (string s in seq)
        {
            m_CountdownLabel.text = s;
            await Task.Delay(800);
        }
        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    public async void ShowPlayerTurn()
    {
        HideAllUI();
        m_CountdownLabel.style.display = DisplayStyle.Flex;
        m_CountdownLabel.text = "YOUR TURN";
        await Task.Delay(1200);
        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    public async void ShowEnemyTurn()
    {
        HideAllUI();
        m_CountdownLabel.style.display = DisplayStyle.Flex;
        m_CountdownLabel.text = "ENEMY TURN";
        await Task.Delay(1200);
        ShowEnemyPointsHolder();
        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    public void ShowActionButtonsForPlayer()
    {
        m_playerButtonsContainer.style.display = DisplayStyle.Flex;
        m_playerPointsHolder.style.display = DisplayStyle.Flex;

        m_playerAttackButton.style.display = DisplayStyle.Flex;
        m_playerDefendButton.style.display = DisplayStyle.Flex;
        m_playerReserveButton.style.display = DisplayStyle.Flex;

        SetPlayerButtonsEnabled(true);
    }

    public void UpdatePointsUI(int currentPoints, int maxPoints, int attackPoints, int defendPoints, int reservePoints, bool highlightCurrent)
    {
        m_playerCurrentPoints.text = currentPoints.ToString();
        m_playerMaxPoints.text = maxPoints.ToString();
        m_playerPointsSpentOnAttack.text = attackPoints.ToString();
        m_playerPointsSpentOnDefend.text = defendPoints.ToString();
        m_playerPointsSpentOnReserve.text = reservePoints.ToString();
        m_playerCurrentPoints.style.color = highlightCurrent ? Color.yellow : Color.white;
    }

    public void UpdateEnemyPointsUI(int currentPoints, int maxPoints, bool highlightCurrent)
    {
        m_enemyCurrentPoints.text = currentPoints.ToString();
        m_enemyMaxPoints.text = maxPoints.ToString();
        m_enemyPointsSpentOnAttack.text = string.Empty;
        m_enemyPointsSpentOnDefend.text = string.Empty;
        m_enemyCurrentPoints.style.color = highlightCurrent ? Color.yellow : Color.white;
    }

    public async void ShowAllocations((int attack, int defend) playerPublic, (int attack, int defend) enemyPublic)
    {
        await Task.Delay(500);

        if (playerPublic.attack > 0 || enemyPublic.defend > 0)
            m_playerButtonsContainer.style.display = DisplayStyle.Flex;

        if (playerPublic.defend > 0 || enemyPublic.attack > 0)
            m_enemyRevealContainer.style.display = DisplayStyle.Flex;

        m_playerReserveButton.style.display = DisplayStyle.None;

        m_playerAttackButton.style.display = playerPublic.attack > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        m_playerPointsSpentOnAttack.text = playerPublic.attack > 0 ? playerPublic.attack.ToString() : string.Empty;

        m_enemyDefendButton.style.display = playerPublic.defend > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        m_enemyPointsSpentOnDefend.text = playerPublic.defend > 0 ? playerPublic.defend.ToString() : string.Empty;

        m_enemyAttackButton.style.display = enemyPublic.attack > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        m_enemyPointsSpentOnAttack.text = enemyPublic.attack > 0 ? enemyPublic.attack.ToString() : string.Empty;

        m_playerDefendButton.style.display = enemyPublic.defend > 0 ? DisplayStyle.Flex : DisplayStyle.None;
        m_playerPointsSpentOnDefend.text = enemyPublic.defend > 0 ? enemyPublic.defend.ToString() : string.Empty;

        await Task.Delay(2500);
    }

    public void SetPlayerButtonsEnabled(bool enabled)
    {
        m_playerAttackButton?.SetEnabled(enabled);
        m_playerDefendButton?.SetEnabled(enabled);
        m_playerReserveButton?.SetEnabled(enabled);
    }

    public void ShowEnemyPointsHolder()
    {
        m_enemyPointsHolder.style.display = DisplayStyle.Flex;
    }

    public void HideEnemyPointsHolder()
    {
        m_enemyPointsHolder.style.display = DisplayStyle.None;
    }

    public void HideAllUI()
    {
        m_playerButtonsContainer.style.display = DisplayStyle.None;
        m_enemyRevealContainer.style.display = DisplayStyle.None;
        m_playerPointsHolder.style.display = DisplayStyle.None;
        m_enemyPointsHolder.style.display = DisplayStyle.None;
        m_CountdownLabel.style.display = DisplayStyle.None;
    }
}