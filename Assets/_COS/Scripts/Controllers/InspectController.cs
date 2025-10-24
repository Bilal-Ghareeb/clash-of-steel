using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class InspectController : MonoBehaviour
{
    [SerializeField] private WeaponInspectPresenter m_inspectedWeaponModelPresenter;
    [SerializeField] private SoundData m_levelUpSFX;
    [SerializeField] private StatueEyeGlow m_statueEyeGlow;

    private InspectView m_view;
    private WeaponInstanceBase m_currentWeapon;

    private int m_displayedLevel;
    private Queue<int> m_levelUpQueue = new();
    private bool m_isSyncing = false;

    public void Setup(InspectView view)
    {
        m_view = view;
    }

    private void OnEnable()
    {
        InspectWeaponEvents.WeaponSelectedForInspect += HandleWeaponSelectedForInspect;
        InspectWeaponEvents.LevelUpWeaponClicked += HandleWeaponLevelUp;
        InspectWeaponEvents.BackButtonClicked += HandleBackButtonClicked;
    }

    private void OnDisable()
    {
        InspectWeaponEvents.WeaponSelectedForInspect -= HandleWeaponSelectedForInspect;
        InspectWeaponEvents.LevelUpWeaponClicked -= HandleWeaponLevelUp;
        InspectWeaponEvents.BackButtonClicked -= HandleBackButtonClicked;
    }

    private void HandleWeaponSelectedForInspect(WeaponInstanceBase weapon)
    {
        m_currentWeapon = weapon;
        int localLevel = 0;

        if (weapon is WeaponInstance w)
        {
            if (LocalWeaponProgressionCache.TryGetLocalLevel(w.Item.Id, out localLevel))
                m_displayedLevel = localLevel;
            else
                m_displayedLevel = w.InstanceData.level;
        }

        m_inspectedWeaponModelPresenter.ShowWeapon(m_currentWeapon);
        m_view.RefreshWeaponStats(weapon, m_displayedLevel);
        UpdateLevelUpButtonState();
    }

    private void HandleWeaponLevelUp()
    {
        if (m_currentWeapon is not WeaponInstance weapon) return;

        var progression = PlayFabManager.Instance.EconomyService.ProgressionFormulas[weapon.CatalogData.progressionId];
        if (m_displayedLevel >= progression.maxLevel) return;

        int cost = WeaponProgressionCalculator.GetCostForLevelUp(m_displayedLevel, progression);
        if (!PlayFabManager.Instance.EconomyService.PlayerCurrencies.TryGetValue(progression.currencyId, out int playerCurrency) || playerCurrency < cost)
            return;

        PlayFabManager.Instance.EconomyService.PlayerCurrencies[progression.currencyId] -= cost;
        PlayFabManager.Instance.EconomyService.NotifyCurrenciesUpdated();

        m_displayedLevel++;
        m_levelUpQueue.Enqueue(cost);
        LocalWeaponProgressionCache.SetLocalLevel(weapon.Item.Id, m_displayedLevel);

        m_view.PlayLevelUpAnimation();
        AudioManager.Instance.PlaySFX(m_levelUpSFX);
        m_statueEyeGlow.TriggerGlow();

        m_view.RefreshWeaponStats(weapon, m_displayedLevel);

        UpdateLevelUpButtonState();

        if (!m_isSyncing)
            _ = SyncLevelUpsAsync(weapon, progression);
    }

    private async Task SyncLevelUpsAsync(WeaponInstance weapon, WeaponProgressionData progression)
    {
        m_isSyncing = true;

        while (m_levelUpQueue.Count > 0)
        {
            int cost = m_levelUpQueue.Dequeue();

            try
            {
                await PlayFabManager.Instance.AzureService.LevelWeaponAsync(
                    weapon.Item.Id,
                    weapon.CatalogData.progressionId,
                    1
                );
                weapon.InstanceData.level++;
                LocalWeaponProgressionCache.SetLocalLevel(weapon.Item.Id, weapon.InstanceData.level);
            }
            catch (Exception)
            {
                PlayFabManager.Instance.EconomyService.PlayerCurrencies[progression.currencyId] += cost;
                m_displayedLevel--;
                LocalWeaponProgressionCache.SetLocalLevel(weapon.Item.Id, m_displayedLevel);
                m_view.RefreshWeaponStats(weapon, m_displayedLevel);
            }

            PlayFabManager.Instance.EconomyService.NotifyCurrenciesUpdated();
            UpdateLevelUpButtonState();
        }

        m_isSyncing = false;
    }

    private void UpdateLevelUpButtonState()
    {
        if (m_currentWeapon is not WeaponInstance weapon) return;

        var progression = PlayFabManager.Instance.EconomyService.ProgressionFormulas[weapon.CatalogData.progressionId];
        bool canAfford = PlayFabManager.Instance.EconomyService.PlayerCurrencies[progression.currencyId] >= WeaponProgressionCalculator.GetCostForLevelUp(m_displayedLevel, progression);
        bool notMaxed = m_displayedLevel < progression.maxLevel;

        m_view.SetLevelUpInteractable(canAfford && notMaxed);
    }

    private void HandleBackButtonClicked()
    {
        m_inspectedWeaponModelPresenter.ClearCurrent();
    }
}
