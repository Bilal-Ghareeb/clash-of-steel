using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponItemComponent
{
    public  VisualElement Root { get; private set; }
    private VisualElement m_weaponItemButton;
    private Button m_watchRewardedAdButton;
    private WeaponInstanceBase m_WeaponInstance;
    private VisualElement m_weaponImage;
    private Label m_Lvl;
    private Label m_healthNumber;
    private Label m_damageNumber;
    private Label m_chanceLabel;
    private VisualElement m_classIcon;
    private Texture2D m_unknownWeaponIcon;
    private bool m_isSelected = false;

    private VisualElement m_cooldownOverlay;
    private Label m_cooldownTimer;

    private Coroutine m_cooldownRoutine;

    private WeaponItemComponentDisplayContext m_displayContext; 

    public bool IsSelected
    {
        get => m_isSelected;
        set
        {
            m_isSelected = value;
            if (m_weaponItemButton == null) return;

            if (m_isSelected)
            {
                m_weaponItemButton.AddToClassList("weapon-scroll-item-selected");
                m_weaponItemButton.SetEnabled(false);
            }
            else
            {
                m_weaponItemButton.RemoveFromClassList("weapon-scroll-item-selected");
                m_weaponItemButton.SetEnabled(true);
            }
        }
    }

    public Action OnCustomClick { get; set; }
    public Action OnAdsButtonClick { get; set; }

    public void SetVisualElements(TemplateContainer weaponItemUXMLTemplate , WeaponItemComponentDisplayContext context)
    {
        if (weaponItemUXMLTemplate == null) return;

        m_displayContext = context;
        Root = weaponItemUXMLTemplate;
        m_weaponItemButton = weaponItemUXMLTemplate.Q<VisualElement>("Weapon-item-button");
        m_Lvl = weaponItemUXMLTemplate.Q<Label>("weapon-scroll-item-lvl");
        m_weaponImage = weaponItemUXMLTemplate.Q<VisualElement>("weapon-scroll-item-icon");
        m_unknownWeaponIcon = m_weaponImage?.resolvedStyle.backgroundImage.texture;
        m_classIcon = weaponItemUXMLTemplate.Q<VisualElement>("Class-icon");
        m_healthNumber = weaponItemUXMLTemplate.Q<Label>("health-number");
        m_damageNumber = weaponItemUXMLTemplate.Q<Label>("damage-number");
        m_chanceLabel = weaponItemUXMLTemplate.Q<Label>("weapon-scroll-item-chance");
        m_watchRewardedAdButton = weaponItemUXMLTemplate.Q<Button>("ad-btn");
        m_cooldownTimer = weaponItemUXMLTemplate.Q<Label>("countdown-counter");
        m_cooldownOverlay = weaponItemUXMLTemplate.Q<VisualElement>("countdown-timer-container");
    }

    public async void SetGameData(WeaponInstanceBase weaponInstance = null, int? overrideLevel = null)
    {
        if (weaponInstance == null)
        {
            ClearUnknownState();
            return;
        }

        if (m_cooldownRoutine != null)
        {
            CoroutineRunner.Stop(m_cooldownRoutine);
            m_cooldownRoutine = null;
        }

        m_WeaponInstance = weaponInstance;

        if (m_Lvl != null) m_Lvl.text = $"{overrideLevel ?? weaponInstance.Level}";
        if (m_healthNumber != null) m_healthNumber.text = weaponInstance.GetHealth().ToString();
        if (m_damageNumber != null) m_damageNumber.text = weaponInstance.GetDamage().ToString();

        if (m_weaponImage != null)
        {
            await weaponInstance.EnsureIconLoadedAsync();

            m_weaponImage.style.backgroundImage = new StyleBackground(weaponInstance.IconTexture);
        }

        if (weaponInstance is WeaponInstance playerweaponInstance && m_displayContext == WeaponItemComponentDisplayContext.PrepareForBattle)
        {
            if (playerweaponInstance.IsOnCooldown) 
            {
                m_watchRewardedAdButton.style.display = DisplayStyle.Flex;
                m_watchRewardedAdButton.RegisterCallback<ClickEvent>(OnAdsButtonClikced);
                m_cooldownOverlay.style.display = DisplayStyle.Flex;
                StartCooldownTimer(playerweaponInstance);
            }
            else
            {
                m_watchRewardedAdButton.style.display = DisplayStyle.None;
                m_watchRewardedAdButton.UnregisterCallback<ClickEvent>(OnAdsButtonClikced);
                m_cooldownOverlay.style.display = DisplayStyle.None;
                m_cooldownTimer.text = "";
            }
        }

        if (m_weaponItemButton != null)
        {
            m_weaponItemButton.ClearClassList();
            m_weaponItemButton.AddToClassList(weaponInstance.GetRarityClass());
        }

        if (m_classIcon != null)
        {
            m_classIcon.ClearClassList();
            m_classIcon.AddToClassList(weaponInstance.GetClassTypeClass());
        }

        IsSelected = false;
    }

    private void ClearUnknownState()
    {
        m_WeaponInstance = null;
        m_Lvl.text = "??";
        m_healthNumber.text = "???";
        m_damageNumber.text = "???";

        if (m_weaponItemButton != null)
        {
            m_weaponItemButton.ClearClassList();
            m_weaponItemButton.AddToClassList(WeaponItemComponentStyleClasses.GetUnknownCardStyle());
        }

        m_classIcon?.ClearClassList();

        if (m_weaponImage != null)
            m_weaponImage.style.backgroundImage = new StyleBackground(m_unknownWeaponIcon);
    }

    public void UpdateAttackPreview(float newAttackValue, bool hasAdvantage, bool hasDisadvantage)
    {
        if (m_damageNumber == null) return;

        if (hasAdvantage)
            m_damageNumber.style.color = Color.green;
        else if (hasDisadvantage)
            m_damageNumber.style.color = Color.red;
        else
            m_damageNumber.style.color = Color.white;

        m_damageNumber.text = Mathf.RoundToInt(newAttackValue).ToString();
    }

    public void ResetAttackHudData()
    {
        if (m_damageNumber == null) return;

        if (m_WeaponInstance != null)
            m_damageNumber.text = m_WeaponInstance.GetDamage().ToString();

        m_damageNumber.style.color = Color.white;
    }

    public void RegisterButtonCallbacks(bool useCustomClick = false)
    {
        if (m_weaponItemButton == null) return;

        m_weaponItemButton.UnregisterCallback<ClickEvent>(OnButtonClicked);
        m_weaponItemButton.UnregisterCallback<ClickEvent>(OnCustomButtonClicked);

        if (useCustomClick && OnCustomClick != null)
            m_weaponItemButton.RegisterCallback<ClickEvent>(OnCustomButtonClicked);
        else
            m_weaponItemButton.RegisterCallback<ClickEvent>(OnButtonClicked);
    }

    public void UnRegisterButtonCallbacks()
    {
        m_weaponItemButton.UnregisterCallback<ClickEvent>(OnButtonClicked);
        m_weaponItemButton.UnregisterCallback<ClickEvent>(OnCustomButtonClicked);
    }

    public void UpdateHealth(float newHealth)
    {
        if (m_healthNumber == null) return;

        newHealth = Mathf.Max(0, newHealth);
        m_healthNumber.text = newHealth.ToString("0");
    }

    public void SetChance(float newChance)
    {
        m_chanceLabel.text = $"{newChance:0.#}%";
        m_chanceLabel.style.display = DisplayStyle.Flex;
    }

    private void StartCooldownTimer(WeaponInstance playerWeapon)
    {
        m_cooldownRoutine = CoroutineRunner.Start(UpdateCooldown(playerWeapon));
    }

    private IEnumerator UpdateCooldown(WeaponInstance playerWeapon)
    {
        while (playerWeapon != null && playerWeapon.IsOnCooldown)
        {
            var remaining = TimeSpan.FromSeconds(playerWeapon.RemainingCooldownSeconds);
            m_cooldownTimer.text = $"{(int)remaining.TotalHours}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
            yield return new WaitForSeconds(1f);
        }

        m_cooldownOverlay.style.display = DisplayStyle.None;
        m_cooldownTimer.text = "";
        m_watchRewardedAdButton.style.display = DisplayStyle.None;
        EnableInteractions();
    }


    public void ApplyDeadCardStyle()
    {
        m_weaponItemButton.ClearClassList();
        m_weaponItemButton.AddToClassList(WeaponItemComponentStyleClasses.GetDeadCardStyle());
    }

    private void OnButtonClicked(ClickEvent evt)
    {
        ArsenalEvents.WeaponItemClicked?.Invoke(this);
    }

    private void OnAdsButtonClikced(ClickEvent evt)
    {
        OnAdsButtonClick?.Invoke();
    }

    private void OnCustomButtonClicked(ClickEvent evt)
    {
        OnCustomClick?.Invoke();
    }

    public void DisableInteractions()
    {
        m_weaponItemButton.SetEnabled(false);
    }

    public void EnableInteractions()
    {
        m_weaponItemButton.SetEnabled(true);
    }

    public WeaponInstanceBase GetWeaponInstance() => m_WeaponInstance;
}
