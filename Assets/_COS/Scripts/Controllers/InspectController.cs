using UnityEngine;

public class InspectController : MonoBehaviour
{
    [SerializeField] private WeaponInspectPresenter m_inspectedWeaponModelPresenter;

    [SerializeField] private SoundData m_levelUpSFX;
    [SerializeField] private StatueEyeGlow m_statueEyeGlow;


    private InspectView m_view;
    private WeaponInstanceBase m_currentWeapon;

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
        m_inspectedWeaponModelPresenter.ShowWeapon(m_currentWeapon);
        m_view.RefreshWeaponStats(m_currentWeapon);
    }

    private async void HandleWeaponLevelUp()
    {
        if (m_currentWeapon is not WeaponInstance playerWeapon)
            return;

        AudioManager.Instance.PlaySFX(m_levelUpSFX);
        m_statueEyeGlow.TriggerGlow();

        var progression = PlayFabManager.Instance.EconomyService.ProgressionFormulas[playerWeapon.CatalogData.progressionId];
        int cost = WeaponProgressionCalculator.GetCostForLevelUp(playerWeapon.InstanceData.level, progression);
        string currencyId = progression.currencyId;

        if (!PlayFabManager.Instance.EconomyService.PlayerCurrencies.TryGetValue(currencyId, out int playerCurrency) || playerCurrency < cost)
        {
            m_view.SetLevelUpInteractable(false);
            return;
        }

        if (playerWeapon.InstanceData.level >= progression.maxLevel)
        {
            m_view.SetLevelUpInteractable(false);
            m_view.RefreshWeaponStats(m_currentWeapon);
            return;
        }

        PlayFabManager.Instance.EconomyService.PlayerCurrencies[currencyId] = playerCurrency - cost;
        PlayFabManager.Instance.EconomyService.NotifyCurrenciesUpdated();

        playerWeapon.InstanceData.level++;

        m_view.PlayLevelUpAnimation();
        m_view.RefreshWeaponStats(m_currentWeapon);

        try
        {
            await PlayFabManager.Instance.AzureService.LevelWeaponAsync(playerWeapon.Item.Id, currencyId, cost);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Level-up failed: {ex.Message}");
            playerWeapon.InstanceData.level--;
            PlayFabManager.Instance.EconomyService.PlayerCurrencies[currencyId] = playerCurrency;
            PlayFabManager.Instance.EconomyService.NotifyCurrenciesUpdated();
            m_view.RefreshWeaponStats(m_currentWeapon);
        }
    }

    private void HandleBackButtonClicked()
    {
        m_inspectedWeaponModelPresenter.ClearCurrent();
    }
}
