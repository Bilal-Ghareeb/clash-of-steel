using System;
using System.Collections.Generic;
using UnityEngine;

public class WeaponsHUDController : MonoBehaviour
{
    private WeaponsHUDView m_View;
    private BattleManager m_Battle;

    private readonly Dictionary<Combatant, WeaponItemComponent> m_PlayerCombatantsUI = new();
    private readonly Dictionary<Combatant, WeaponItemComponent> m_EnemyCombatantsUI = new();

    public event Action<Combatant> OnRequestSwitch;

    private bool m_IsPlayerAllocationPhase = false;

    public void Setup(BattleManager battle , WeaponsHUDView view)
    {
        m_View = view;
        m_Battle = battle;
    }

    private void OnEnable()
    {
        m_Battle.OnBattleStarted += HandleBattleStarted;
        m_Battle.OnCombatantDamaged += HandleCombatantDamaged;
        m_Battle.OnClassComparison += HandleClassComparison;
        m_Battle.OnWeaponSwitched += HandleWeaponSwitched;

        m_Battle.OnEnemyTurnStarted += HandleResetAllAttackColors;
        m_Battle.OnPlayerTurnStarted += HandleResetAllAttackColors;

        m_Battle.OnPlayerAllocationPhaseStarted += HandlePlayerAllocationPhaseStarted;
        m_Battle.OnAllocationsRevealed += HandleAllocationsRevealed;
        m_Battle.OnTurnChanged += HandleTurnChanged;
    }

    private void OnDisable()
    {
        if (m_Battle != null)
        {
            m_Battle.OnBattleStarted -= HandleBattleStarted;
            m_Battle.OnCombatantDamaged -= HandleCombatantDamaged;
            m_Battle.OnClassComparison -= HandleClassComparison;
            m_Battle.OnWeaponSwitched -= HandleWeaponSwitched;

            m_Battle.OnEnemyTurnStarted -= HandleResetAllAttackColors;
            m_Battle.OnPlayerTurnStarted -= HandleResetAllAttackColors;

            m_Battle.OnPlayerAllocationPhaseStarted -= HandlePlayerAllocationPhaseStarted;
            m_Battle.OnAllocationsRevealed -= HandleAllocationsRevealed;
            m_Battle.OnTurnChanged -= HandleTurnChanged;
        }
    }

    private void HandleBattleStarted()
    {
        CreateWeaponsCardsHUD(m_Battle.PlayerTeam, true);
        CreateWeaponsCardsHUD(m_Battle.EnemyTeam, false);
    }

    private void CreateWeaponsCardsHUD(List<Combatant> combatants, bool isPlayer)
    {
        var combatantsDictionary = isPlayer ? m_PlayerCombatantsUI : m_EnemyCombatantsUI;
        foreach (var kvp in combatantsDictionary)
        {
            m_View.RemoveWeaponItemClickCallback(kvp.Value);
        }
        combatantsDictionary.Clear();

        m_View.ClearContainer(isPlayer);

        if (combatants == null || combatants.Count == 0) return;

        for (int i = 0; i < combatants.Count; i++)
        {
            var combatant = combatants[i];
            if (combatant?.InstanceData == null) continue;

            var weaponItem = m_View.CreateWeaponItem(combatant, WeaponItemComponentDisplayContext.Battle);
            if (weaponItem == null) continue;

            bool isFirst = i == 0;
            float scale = isFirst ? 1f : 0.7f;
            m_View.AddWeaponItemToContainer(weaponItem, isPlayer, scale, 0);

            if (!isFirst && isPlayer)
            {
                int indexCopy = i;
                Action onClick = () => HandleWeaponItemClicked(combatant, indexCopy);
                m_View.SetWeaponItemClickCallback(weaponItem, onClick, true);
            }

            combatantsDictionary[combatant] = weaponItem;
        }
    }

    private void HandleWeaponSwitched(Combatant newActive, Combatant oldActive, bool deductCost, bool isPlayer)
    {
        var team = isPlayer ? m_Battle.PlayerTeam : m_Battle.EnemyTeam;
        int newIndex = team.IndexOf(newActive);
        if (newIndex >= 0)
        {
            UpdateWeaponsOrderAfterSwitch(isPlayer, newIndex);
        }
    }

    private void UpdateWeaponsOrderAfterSwitch(bool isPlayer, int newActiveIndex)
    {
        var team = isPlayer ? m_Battle.PlayerTeam : m_Battle.EnemyTeam;
        var combatantsUI = isPlayer ? m_PlayerCombatantsUI : m_EnemyCombatantsUI;

        if (team == null || combatantsUI == null || newActiveIndex < 0 || newActiveIndex >= team.Count) return;

        var newActiveWeapon = team[newActiveIndex];
        var outgoingWeapon = team[0];

        if (combatantsUI.TryGetValue(newActiveWeapon, out var newActiveWeaponItem))
        {
            m_View.RemoveWeaponItemClickCallback(newActiveWeaponItem);
        }

        if (combatantsUI.TryGetValue(outgoingWeapon, out var oldActiveWeaponItem))
        {
            if (outgoingWeapon.IsAlive && isPlayer)
            {
                Action onClick = () => HandleWeaponItemClicked(outgoingWeapon, newActiveIndex);
                m_View.SetWeaponItemClickCallback(oldActiveWeaponItem, onClick, true);
            }
            else
            {
                m_View.RemoveWeaponItemClickCallback(oldActiveWeaponItem);
            }
        }

        m_View.ClearContainer(isPlayer);

        if (combatantsUI.TryGetValue(newActiveWeapon, out var activeWeaponItem))
        {
            m_View.AddWeaponItemToContainer(activeWeaponItem, isPlayer, 1f);
        }

        for (int i = 0; i < team.Count; i++)
        {
            if (i == newActiveIndex) continue;

            var combatant = team[i];
            if (!combatantsUI.TryGetValue(combatant, out var weaponItem)) continue;

            m_View.AddWeaponItemToContainer(weaponItem, isPlayer, 0.7f);
        }
    }

    private void HandleClassComparison(Combatant attacker, Combatant defender, float multiplier)
    {
        bool hasAdvantage = multiplier > 1.0f;
        bool hasDisadvantage = multiplier < 1.0f;
        float newAttackValue = attacker.BaseAttack * multiplier;

        if (m_PlayerCombatantsUI.TryGetValue(attacker, out var playerWeaponUI))
        {
            m_View.UpdateAttackPreview(playerWeaponUI, newAttackValue, hasAdvantage, hasDisadvantage);
        }

        if (m_EnemyCombatantsUI.TryGetValue(attacker, out var enemyWeaponUI))
        {
            m_View.UpdateAttackPreview(enemyWeaponUI, newAttackValue, hasAdvantage, hasDisadvantage);
        }
    }

    private void HandleResetAllAttackColors()
    {
        foreach (var item in m_PlayerCombatantsUI.Values)
        {
            m_View.ResetAttackHudData(item);
        }
        foreach (var item in m_EnemyCombatantsUI.Values)
        {
            m_View.ResetAttackHudData(item);
        }
    }

    private void HandleCombatantDamaged(Combatant attacker, Combatant defender, int defenderNewHealth, int attackerDamage)
    {
        if (defender == null) return;

        bool isPlayerAttacker = attacker.InstanceData is WeaponInstance;
        var attackerUI = isPlayerAttacker ? GetCombatantUI(attacker, true) : GetCombatantUI(attacker, false);
        var defenderUI = isPlayerAttacker ? GetCombatantUI(defender, false) : GetCombatantUI(defender, true);

        if (attackerUI == null || defenderUI == null)
        {
            Debug.LogWarning("Missing UI card for combatants in HandleCombatantDamaged");
            return;
        }

        if (defenderNewHealth <= 0)
        {
            m_View.ApplyDeadCardStyle(defenderUI);
        }

        m_View.UpdateHealth(defenderUI, defenderNewHealth);
    }

    private WeaponItemComponent GetCombatantUI(Combatant combatant, bool isPlayer)
    {
        var uiDict = isPlayer ? m_PlayerCombatantsUI : m_EnemyCombatantsUI;
        uiDict.TryGetValue(combatant, out var ui);
        return ui;
    }

    private void HandleWeaponItemClicked(Combatant combatant, int index)
    {
        if (!m_IsPlayerAllocationPhase) return;
        var active = m_Battle?.GetActivePlayerCombatant();
        if (active == null || combatant == active) return;
        OnRequestSwitch?.Invoke(combatant);
    }

    private void HandlePlayerAllocationPhaseStarted(int pts)
    {
        m_IsPlayerAllocationPhase = true;
    }

    private void HandleAllocationsRevealed((int attack, int defend) player , (int attack, int defend) enemy)
    {
        m_IsPlayerAllocationPhase = false;
    }

    private void HandleTurnChanged(int turnNumber , int points)
    {
        m_IsPlayerAllocationPhase = false;
    }
}