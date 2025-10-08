using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

public class InspectView : UIView
{
    private WeaponInstanceBase m_currentWeapon;
    private VisualElement m_backButton;
    private VisualElement m_weaponLevelUpButton;
    private VisualElement m_currencyIcon;

    private Label m_weaponName;
    private Label m_weaponDescription;
    private Label m_currentHealth;
    private Label m_currentDamage;
    private Label m_nextLvlCost;
    private Label m_maxLevelReached;
    private Label m_weaponLvl;

    private WeaponInspectPresenter m_inspectedWeaponModelPresenter;

    private readonly Queue<Func<Task>> m_inspectedWeaponLevelUpRequests = new();
    private readonly SemaphoreSlim m_semaphore = new(1, 1);
    private bool m_isProcessingQueue = false;

    public InspectView(VisualElement topElement, WeaponInspectPresenter presenter) : base(topElement)
    {
        m_inspectedWeaponModelPresenter = presenter;
        InspectWeaponEvents.WeaponSelectedForInspect += OnWeaponSelectedForInspect;
    }

    public override void Show()
    {
        base.Show();
        InspectWeaponEvents.InspectWeaponViewShown?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        InspectWeaponEvents.WeaponSelectedForInspect -= OnWeaponSelectedForInspect;
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_backButton = m_TopElement.Q<VisualElement>("Back-btn");
        m_weaponLevelUpButton = m_TopElement.Q<VisualElement>("Lvlup-btn");
        m_currencyIcon = m_TopElement.Q<VisualElement>("Currency-icon");
        m_weaponName = m_TopElement.Q<Label>("Weapon-name");
        m_weaponDescription = m_TopElement.Q<Label>("Weapon-description");
        m_currentHealth = m_TopElement.Q<Label>("Health-number");
        m_currentDamage = m_TopElement.Q<Label>("Damage-number");
        m_weaponLvl = m_TopElement.Q<Label>("Lvl-text");
        m_nextLvlCost = m_TopElement.Q<Label>("Currency-text");
        m_maxLevelReached = m_TopElement.Q<Label>("Max-text");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_backButton.RegisterCallback<ClickEvent>(ReturnToArsenal);
        m_weaponLevelUpButton.RegisterCallback<ClickEvent>(LevelUpWeapon);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_backButton.UnregisterCallback<ClickEvent>(ReturnToArsenal);
        m_weaponLevelUpButton.UnregisterCallback<ClickEvent>(LevelUpWeapon);
    }

    private void OnWeaponSelectedForInspect(WeaponInstanceBase weapon)
    {
        m_currentWeapon = weapon;
        m_weaponName.text = weapon.CatalogData.name;
        m_weaponDescription.text = weapon.CatalogData.description;
        UpdateWeaponDataUI();
        m_inspectedWeaponModelPresenter.ShowWeapon(weapon);
    }

    private void ReturnToArsenal(ClickEvent evt)
    {
        m_inspectedWeaponModelPresenter.ClearCurrent();
        InspectWeaponEvents.BackToArsenalButtonPressed?.Invoke();
    }

    private async void LevelUpWeapon(ClickEvent evt)
    {
        if (m_currentWeapon is not WeaponInstance playerWeapon)
            return;

        WeaponProgressionData progression = PlayFabManager.Instance.ProgressionFormulas[playerWeapon.CatalogData.progressionId];
        int cost = WeaponProgressionCalculator.GetCostForLevelUp(playerWeapon.InstanceData.level, progression);
        string currencyFriendlyId = progression.currencyId;

        if (!PlayFabManager.Instance.PlayerCurrencies.TryGetValue(currencyFriendlyId, out int playerCurrency) || playerCurrency < cost)
        {
            m_weaponLevelUpButton.SetEnabled(false);
            m_weaponLevelUpButton.style.opacity = 0.6f;
            return;
        }

        if (playerWeapon.InstanceData.level == progression.maxLevel)
        {
            m_weaponLevelUpButton.SetEnabled(false);
            m_weaponLevelUpButton.style.opacity = 0.6f;
            UpdateWeaponDataUI();
            return;
        }

        // Deduct locally
        PlayFabManager.Instance.PlayerCurrencies[currencyFriendlyId] = playerCurrency - cost;
        OnCurrenciesUpdated();
        playerWeapon.InstanceData.level++;
        UpdateWeaponDataUI();

        // Queue async PlayFab request
        m_inspectedWeaponLevelUpRequests.Enqueue(async () =>
        {
            try
            {
                await PlayFabManager.Instance.LevelWeaponAsync(
                    playerWeapon.Item.Id,
                    currencyFriendlyId,
                    cost
                );
            }
            catch (Exception ex)
            {
                // Roll back
                PlayFabManager.Instance.PlayerCurrencies[currencyFriendlyId] = playerCurrency;
                OnCurrenciesUpdated();
                playerWeapon.InstanceData.level--;
                UpdateWeaponDataUI();
                Debug.LogError("Level-up request failed: " + ex.Message);
            }
        });

        if (!m_isProcessingQueue)
            await ProcessQueue();
    }

    private async Task ProcessQueue()
    {
        m_isProcessingQueue = true;
        while (m_inspectedWeaponLevelUpRequests.Count > 0)
        {
            await m_semaphore.WaitAsync();
            try
            {
                var task = m_inspectedWeaponLevelUpRequests.Dequeue();
                await task();
            }
            finally
            {
                m_semaphore.Release();
            }
        }
        m_isProcessingQueue = false;
    }

    private void UpdateWeaponDataUI()
    {
        if (m_currentWeapon == null)
            return;

        var progression = PlayFabManager.Instance.ProgressionFormulas[m_currentWeapon.CatalogData.progressionId];

        int level = 1;
        if (m_currentWeapon is WeaponInstance playerWeapon)
            level = playerWeapon.InstanceData.level;

        m_weaponLvl.text = level.ToString();
        m_currentHealth.text = WeaponProgressionCalculator.GetDamage(m_currentWeapon.CatalogData.baseHealth, level, progression).ToString();
        m_currentDamage.text = WeaponProgressionCalculator.GetDamage(m_currentWeapon.CatalogData.baseDamage, level, progression).ToString();


        int cost = WeaponProgressionCalculator.GetCostForLevelUp(level, progression);
        m_nextLvlCost.text = cost.ToString();

        string currencyFriendlyId = progression.currencyId;
        bool hasEnoughCurrency = PlayFabManager.Instance.PlayerCurrencies.TryGetValue(currencyFriendlyId, out int playerCurrency) && playerCurrency >= cost;
        bool isMaxLevel = level >= progression.maxLevel;

        m_maxLevelReached.style.display = isMaxLevel ? DisplayStyle.Flex : DisplayStyle.None;
        m_currencyIcon.style.display = isMaxLevel ? DisplayStyle.None : DisplayStyle.Flex;
        m_nextLvlCost.style.display = isMaxLevel ? DisplayStyle.None : DisplayStyle.Flex;

        m_weaponLevelUpButton.SetEnabled(!isMaxLevel && hasEnoughCurrency);
        m_weaponLevelUpButton.style.opacity = (!isMaxLevel && hasEnoughCurrency) ? 1.0f : 0.6f;
    }

    private void OnCurrenciesUpdated()
    {
        PlayFabManager.Instance.NotifyCurrenciesUpdated();
    }
}
