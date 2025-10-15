using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class BattleActionsView : UIView
{
    private BattleManager m_Battle;
    private WeaponsHUDView m_WeaponsHUD;

    private VisualElement m_playerButtonsContainer;
    private VisualElement m_enemyRevealContainer;
    private VisualElement m_playerPointsHolder;

    private Button m_playerAttackButton;
    private Button m_playerDefendButton;
    private Button m_playerReserveButton;

    private Button m_enemyAttackButton;
    private Button m_enemyDefendButton;

    private Label m_playerCurrentPoints;
    private Label m_playerMaxPoints;

    private Label m_playerPointsSpentOnAttack;
    private Label m_playerPointsSpentOnDefend;
    private Label m_playerPointsSpentOnReserve;

    private Label m_enemyPointsSpentOnAttack;
    private Label m_enemyPointsSpentOnDefend;

    private Label m_CountdownLabel;

    private int m_AttackPoints;
    private int m_DefendPoints;
    private int m_ReservePoints;
    private int m_SwitchPoint;

    public BattleActionsView(VisualElement topElement, bool hideOnAwake = true)
        : base(topElement, hideOnAwake) { }

    public void InitializeBattleManager(BattleManager battle)
    {
        m_Battle = battle;

        m_Battle.OnBattleCountdownStarted += OnCountdownStarted;
        m_Battle.OnPlayerAllocationPhaseStarted += OnPlayerAllocationPhaseStarted;
        m_Battle.OnEnemyTurnStarted += OnEnemyTurnStarted;
        m_Battle.OnAllocationsRevealed += OnAllocationsRevealed;
        m_Battle.OnPlayerTurnStarted += OnPlayerTurnStarted;
        m_Battle.OnPlayerWeaponSwitched += HandleSwitchRequest;
        m_Battle.OnPlayerWeaponEntranceCompleted += () => SetPlayerButtonsEnabled(true);

        HideAllUI();
    }

    public override void Dispose()
    {
        base.Dispose();
        if (m_Battle == null) return;
        m_Battle.OnBattleCountdownStarted -= OnCountdownStarted;
        m_Battle.OnPlayerAllocationPhaseStarted -= OnPlayerAllocationPhaseStarted;
        m_Battle.OnEnemyTurnStarted -= OnEnemyTurnStarted;
        m_Battle.OnAllocationsRevealed -= OnAllocationsRevealed;
        m_Battle.OnPlayerTurnStarted -= OnPlayerTurnStarted;
        m_Battle.OnPlayerWeaponSwitched -= HandleSwitchRequest;
        m_Battle.OnPlayerWeaponEntranceCompleted += () => SetPlayerButtonsEnabled(true);
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_playerButtonsContainer = m_TopElement.Q<VisualElement>("PlayerActionButtonsContainer");
        m_enemyRevealContainer = m_TopElement.Q<VisualElement>("EnemyActionButtonsContainer");
        m_playerPointsHolder = m_TopElement.Q<VisualElement>("PlayerPointsHolder");

        m_playerAttackButton = m_TopElement.Q<Button>("AttackButton");
        m_playerDefendButton = m_TopElement.Q<Button>("DefendButton");
        m_playerReserveButton = m_TopElement.Q<Button>("ReserveButton");

        m_enemyAttackButton = m_TopElement.Q<Button>("EnemyAttackButton");
        m_enemyDefendButton = m_TopElement.Q<Button>("EnemyDefendButton");

        m_CountdownLabel = m_TopElement.Q<Label>("CountdownLabel");
        m_playerCurrentPoints = m_TopElement.Q<Label>("PlayerCurrentPoint");
        m_playerMaxPoints = m_TopElement.Q<Label>("PlayerMaxPoints");

        m_playerPointsSpentOnAttack = m_TopElement.Q<Label>("PointsOnAttack");
        m_playerPointsSpentOnDefend = m_TopElement.Q<Label>("PointsOnDefend");
        m_playerPointsSpentOnReserve = m_TopElement.Q<Label>("PointsOnReserve");

        m_enemyPointsSpentOnAttack = m_TopElement.Q<Label>("EnemyPointsOnAttack");
        m_enemyPointsSpentOnDefend = m_TopElement.Q<Label>("EnemyPointsOnDefend");
    }

    protected override void RegisterButtonCallbacks()
    {
        if (m_playerAttackButton != null)
            m_playerAttackButton.clicked += () => SpendPoint("Attack");

        if (m_playerDefendButton != null)
            m_playerDefendButton.clicked += () => SpendPoint("Defend");

        if (m_playerReserveButton != null)
            m_playerReserveButton.clicked += () => SpendPoint("Reserve");
    }

    private async void OnCountdownStarted()
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

    private void OnPlayerAllocationPhaseStarted(int availablePoints)
    {
        HideAllUI();
        ShowActionButtonsForPlayer();
        ResetLocalPoints();
        UpdatePointsUI();

        if (m_WeaponsHUD != null)
        {
            var actor = m_Battle.GetActivePlayerCombatant();
        }
    }


    private async void OnPlayerTurnStarted()
    {
        HideAllUI();
        m_CountdownLabel.style.display = DisplayStyle.Flex;
        m_CountdownLabel.text = "YOUR TURN";

        await Task.Delay(1200);
        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    private async void OnEnemyTurnStarted()
    {
        HideAllUI();
        m_CountdownLabel.style.display = DisplayStyle.Flex;
        m_CountdownLabel.text = "ENEMY TURN";

        await Task.Delay(1200);
        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    private async void OnAllocationsRevealed((int attack, int defend) playerPublic, (int attack, int defend) enemyPublic)
    {
        await Task.Delay(500);

        if (playerPublic.attack > 0 || enemyPublic.defend > 0)
            m_playerButtonsContainer.style.display = DisplayStyle.Flex;

        if (playerPublic.defend > 0 || enemyPublic.attack > 0)
            m_enemyRevealContainer.style.display = DisplayStyle.Flex;

        m_playerReserveButton.style.display = DisplayStyle.None;

        int playerAttackPoints = playerPublic.attack;
        if (playerAttackPoints > 0)
        {
            m_playerAttackButton.style.display = DisplayStyle.Flex;
            m_playerPointsSpentOnAttack.text = playerAttackPoints.ToString();
        }
        else
        {
            m_playerAttackButton.style.display = DisplayStyle.None;
            m_playerPointsSpentOnAttack.text = string.Empty;
        }

        int enemyDefendPoints = playerPublic.defend;
        if (enemyDefendPoints > 0)
        {
            m_enemyDefendButton.style.display = DisplayStyle.Flex;
            m_enemyPointsSpentOnDefend.text = enemyDefendPoints.ToString();
        }
        else
        {
            m_enemyDefendButton.style.display = DisplayStyle.None;
            m_enemyPointsSpentOnDefend.text = string.Empty;
        }

        int enemyAttackPoints = enemyPublic.attack;
        if (enemyAttackPoints > 0)
        {
            m_enemyAttackButton.style.display = DisplayStyle.Flex;
            m_enemyPointsSpentOnAttack.text = enemyAttackPoints.ToString();
        }
        else
        {
            m_enemyAttackButton.style.display = DisplayStyle.None;
            m_enemyPointsSpentOnAttack.text = string.Empty;
        }

        int playerDefendPoints = enemyPublic.defend;
        if (playerDefendPoints > 0)
        {
            m_playerDefendButton.style.display = DisplayStyle.Flex;
            m_playerPointsSpentOnDefend.text = playerDefendPoints.ToString();
        }
        else
        {
            m_playerDefendButton.style.display = DisplayStyle.None;
            m_playerPointsSpentOnDefend.text = string.Empty;
        }

        await Task.Delay(2500);
    }

    private async void SpendPoint(string actionName)
    {
        var actor = GetCurrentPlayerCombatant();
        if (actor == null) return;

        int totalAvailable = m_Battle.GetCurrentPlayerAvailablePoints();
        int spent = m_AttackPoints + m_DefendPoints + m_ReservePoints;
        if (spent >= totalAvailable) return;

        switch (actionName)
        {
            case "Attack": m_AttackPoints++; break;
            case "Defend": m_DefendPoints++; break;
            case "Reserve": m_ReservePoints++; break;
        }

        UpdatePointsUI();
        spent = m_AttackPoints + m_DefendPoints + m_ReservePoints + m_SwitchPoint;

        if (spent >= totalAvailable)
        {
            m_playerAttackButton?.SetEnabled(false);
            m_playerDefendButton?.SetEnabled(false);
            m_playerReserveButton?.SetEnabled(false);

            await Task.Delay(600);
            FinalizeAllocations(actor);
        }
    }

    private void FinalizeAllocations(Combatant actor)
    {
        string error;
        bool ok = m_Battle.AllocateForCombatant(actor, m_AttackPoints, m_DefendPoints, m_ReservePoints, out error);
        if (!ok)
            Debug.LogError("Allocation failed: " + error);

        ResetLocalPoints();
        HideAllUI();
    }

    private async Task FinalizeAfterDelay(Combatant actor, int ms)
    {
        await Task.Delay(ms);
        FinalizeAllocations(actor);
    }

    private void UpdatePointsUI()
    {
        var actor = GetCurrentPlayerCombatant();
        if (actor == null || m_Battle == null) return;

        int totalAvailable = m_Battle.GetCurrentPlayerAvailablePoints();
        int basePts = m_Battle.GetCurrentPlayerBasePoints();
        int spent = m_AttackPoints + m_DefendPoints + m_ReservePoints + m_SwitchPoint;
        int remaining = Mathf.Max(0, totalAvailable - spent);

        m_playerCurrentPoints.text = remaining.ToString();
        m_playerMaxPoints.text = basePts.ToString();
        m_playerPointsSpentOnAttack.text = m_AttackPoints.ToString();
        m_playerPointsSpentOnDefend.text = m_DefendPoints.ToString();
        m_playerPointsSpentOnReserve.text = m_ReservePoints.ToString();
        m_playerCurrentPoints.style.color = totalAvailable > basePts ? Color.yellow : Color.white;
    }

    private void HandleSwitchRequest(Combatant incomingCombatant, Combatant outgoingCombatant , bool deductCost)
    {
        var actor = m_Battle.GetActivePlayerCombatant();
        if (actor == null)
            return;

        SetPlayerButtonsEnabled(false);

        if (deductCost)
            m_SwitchPoint += 1;

        UpdatePointsUI();

        int remaining = m_Battle.GetCurrentPlayerAvailablePoints() - m_SwitchPoint;
        if (remaining <= 0)
        {
            _ = FinalizeAfterDelay(actor, 600);
            return;
        }
    }


    private void ResetLocalPoints()
    {
        m_AttackPoints = 0;
        m_DefendPoints = 0;
        m_ReservePoints = 0;
        m_SwitchPoint = 0;
    }

    private Combatant GetCurrentPlayerCombatant()
    {
        return m_Battle?.GetActivePlayerCombatant();
    }

    private void HideAllUI()
    {
        m_playerButtonsContainer.style.display = DisplayStyle.None;
        m_enemyRevealContainer.style.display = DisplayStyle.None;
        m_playerPointsHolder.style.display = DisplayStyle.None;
        m_CountdownLabel.style.display = DisplayStyle.None;
    }

    private void ShowActionButtonsForPlayer()
    {
        m_playerButtonsContainer.style.display = DisplayStyle.Flex;
        m_playerPointsHolder.style.display = DisplayStyle.Flex;

        m_playerAttackButton.style.display = DisplayStyle.Flex;
        m_playerDefendButton.style.display = DisplayStyle.Flex;
        m_playerReserveButton.style.display = DisplayStyle.Flex;

        SetPlayerButtonsEnabled(true);
    }

    private void SetPlayerButtonsEnabled(bool enabled)
    {
        m_playerAttackButton?.SetEnabled(enabled);
        m_playerDefendButton?.SetEnabled(enabled);
        m_playerReserveButton?.SetEnabled(enabled);
    }

}
