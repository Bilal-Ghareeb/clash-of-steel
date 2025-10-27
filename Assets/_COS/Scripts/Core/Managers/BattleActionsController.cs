using System.Threading.Tasks;
using UnityEngine;

public class BattleActionsController : MonoBehaviour
{
    private BattleActionsView m_View;
    private BattleManager m_Battle;

    private int m_AttackPoints;
    private int m_DefendPoints;
    private int m_ReservePoints;
    private int m_SwitchPoint;

    [Header("Sounds")]
    [SerializeField] private SoundData m_buttonClickSound;
    [SerializeField] private SoundData m_tickSound;
    [SerializeField] private SoundData m_fightTickSound;
    [SerializeField] private SoundData m_turnChangeAlertSound;
    [SerializeField] private SoundData m_revealPointsSound;

    public void Setup(BattleManager battle , BattleActionsView view)
    {
        m_Battle = battle;
        m_View = view;
    }

    private void OnEnable()
    {
        m_Battle.OnBattleCountdownStarted += HandleBattleCountdownStarted;
        m_Battle.OnPlayerAllocationPhaseStarted += HandlePlayerAllocationPhaseStarted;
        m_Battle.OnEnemyFinishedAllocating += HandleEnemyFinishedAllocating;
        m_Battle.OnEnemyTurnStarted += HandleEnemyTurnStarted;
        m_Battle.OnAllocationsRevealed += HandleAllocationsRevealed;
        m_Battle.OnPlayerTurnStarted += HandlePlayerTurnStarted;
        m_Battle.OnWeaponSwitched += HandleWeaponSwitched;
        m_Battle.OnPlayerWeaponEntranceCompleted += HandlePlayerWeaponEntranceCompleted;
        m_Battle.OnBattleEnded += HandleBattleEnded;
        BattleActionsEvents.SpendPointRequested += HandleSpendPointRequested;
    }

    private void OnDisable()
    {
        if (m_Battle != null)
        {
            m_Battle.OnBattleCountdownStarted -= HandleBattleCountdownStarted;
            m_Battle.OnPlayerAllocationPhaseStarted -= HandlePlayerAllocationPhaseStarted;
            m_Battle.OnEnemyFinishedAllocating -= HandleEnemyFinishedAllocating;
            m_Battle.OnEnemyTurnStarted -= HandleEnemyTurnStarted;
            m_Battle.OnAllocationsRevealed -= HandleAllocationsRevealed;
            m_Battle.OnPlayerTurnStarted -= HandlePlayerTurnStarted;
            m_Battle.OnWeaponSwitched -= HandleWeaponSwitched;
            m_Battle.OnPlayerWeaponEntranceCompleted -= HandlePlayerWeaponEntranceCompleted;
            m_Battle.OnBattleEnded -= HandleBattleEnded;
            BattleActionsEvents.SpendPointRequested -= HandleSpendPointRequested;
            m_View.OnCountdownNumberChanged -= HandleCountdownNumberChanged;
        }
    }

    private void HandleBattleCountdownStarted()
    {
        m_View.OnCountdownNumberChanged += HandleCountdownNumberChanged;
        m_View.StartCountdown();
    }

    private void HandlePlayerAllocationPhaseStarted(int availablePoints)
    {
        ResetLocalPoints();
        m_View.ShowActionButtonsForPlayer();
        UpdatePointsUI();
    }

    private void HandleEnemyFinishedAllocating()
    {
        m_View.HideEnemyPointsHolder();
    }

    private void HandleEnemyTurnStarted()
    {
        AudioManager.Instance.PlaySFX(m_turnChangeAlertSound);
        UpdateEnemyPoints();
        m_View.ShowEnemyTurn();
    }

    private void HandleAllocationsRevealed((int attack, int defend) playerPublic, (int attack, int defend) enemyPublic)
    {
        m_View.ShowAllocations(playerPublic, enemyPublic);
        AudioManager.Instance.PlaySFX(m_revealPointsSound);
    }

    private void HandlePlayerTurnStarted()
    {
        AudioManager.Instance.PlaySFX(m_turnChangeAlertSound);
        m_View.ShowPlayerTurn();
    }

    private void HandleCountdownNumberChanged(string number)
    {
        switch (number)
        {
            case "FIGHT!":
                AudioManager.Instance.PlaySFX(m_fightTickSound);
                break;
            default:
                AudioManager.Instance.PlaySFX(m_tickSound);
                break;
        }
    }

    private void HandleWeaponSwitched(Combatant incomingCombatant, Combatant outgoingCombatant, bool deductCost, bool isPlayer)
    {
        if (isPlayer)
        {
            m_View.SetPlayerButtonsEnabled(false);
            if (deductCost)
                m_SwitchPoint += 1;
            UpdatePointsUI();

            int remaining = m_Battle.GetCurrentPlayerAvailablePoints() - m_SwitchPoint;
            if (remaining <= 0)
            {
                var actor = m_Battle.GetActivePlayerCombatant();
                FinalizeAfterDelay(actor, 600);
                return;
            }
        }
        else
        {
            Debug.Log("Enemy Switched UI Points Should Update");
        }
    }

    private void HandlePlayerWeaponEntranceCompleted()
    {
        m_View.SetPlayerButtonsEnabled(true);
    }

    private void HandleBattleEnded()
    {
        m_View.HideAllUI();
    }

    private async void HandleSpendPointRequested(string actionName)
    {
        AudioManager.Instance.PlaySFX(m_buttonClickSound);
        var actor = m_Battle?.GetActivePlayerCombatant();
        if (actor == null) return;

        int totalAvailable = m_Battle.GetCurrentPlayerAvailablePoints();
        int spent = m_AttackPoints + m_DefendPoints + m_ReservePoints + m_SwitchPoint;
        if (spent >= totalAvailable) return;

        switch (actionName)
        {
            case "Attack":
                m_AttackPoints++;
                break;
            case "Defend":
                m_DefendPoints++;
                break;
            case "Reserve":
                m_ReservePoints++;
                break;
        }

        UpdatePointsUI();
        spent = m_AttackPoints + m_DefendPoints + m_ReservePoints + m_SwitchPoint;

        if (spent >= totalAvailable)
        {
            m_View.SetPlayerButtonsEnabled(false);
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
        m_View.HideAllUI();
    }

    private async void FinalizeAfterDelay(Combatant actor, int ms)
    {
        await Task.Delay(ms);
        FinalizeAllocations(actor);
    }

    private void UpdatePointsUI()
    {
        var actor = m_Battle?.GetActivePlayerCombatant();
        if (actor == null || m_Battle == null) return;

        int totalAvailable = m_Battle.GetCurrentPlayerAvailablePoints();
        int basePts = m_Battle.GetPlayerCurrentBasePoints();
        int spent = m_AttackPoints + m_DefendPoints + m_ReservePoints + m_SwitchPoint;
        int remaining = Mathf.Max(0, totalAvailable - spent);

        m_View.UpdatePointsUI(remaining, basePts, m_AttackPoints, m_DefendPoints, m_ReservePoints, totalAvailable > basePts);
    }

    private void UpdateEnemyPoints()
    {
        if (m_Battle == null) return;

        int totalAvailable = m_Battle.GetCurrentEnemyAvailablePoints();
        int basePts = m_Battle.GetEnemyCurrentBasePoints();
        m_View.UpdateEnemyPointsUI(totalAvailable, basePts, totalAvailable > basePts);
    }

    private void ResetLocalPoints()
    {
        m_AttackPoints = 0;
        m_DefendPoints = 0;
        m_ReservePoints = 0;
        m_SwitchPoint = 0;
    }
}