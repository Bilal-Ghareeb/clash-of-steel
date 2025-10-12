using System;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponItemComponent
{
    private VisualElement m_weaponItemButton;
    private WeaponInstanceBase m_WeaponInstance;
    private VisualElement m_weaponImage;
    private Label m_Lvl;
    private Label m_healthNumber;
    private Label m_damageNumber;
    private VisualElement m_classIcon;
    private Texture2D m_unknownWeaponIcon;
    private bool m_isSelected = false;

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

    public void SetVisualElements(TemplateContainer weaponItemUXMLTemplate)
    {
        if (weaponItemUXMLTemplate == null) return;

        m_weaponItemButton = weaponItemUXMLTemplate.Q<VisualElement>("Weapon-item-button");
        m_Lvl = weaponItemUXMLTemplate.Q<Label>("weapon-scroll-item-lvl");
        m_weaponImage = weaponItemUXMLTemplate.Q<VisualElement>("weapon-scroll-item-icon");
        m_unknownWeaponIcon = m_weaponImage?.resolvedStyle.backgroundImage.texture;
        m_classIcon = weaponItemUXMLTemplate.Q<VisualElement>("Class-icon");
        m_healthNumber = weaponItemUXMLTemplate.Q<Label>("health-number");
        m_damageNumber = weaponItemUXMLTemplate.Q<Label>("damage-number");
    }

    public async void SetGameData(WeaponInstanceBase weaponInstance = null)
    {
        if (weaponInstance == null)
        {
            ClearUnknownState();
            return;
        }

        m_WeaponInstance = weaponInstance;

        if (m_Lvl != null) m_Lvl.text = $"{weaponInstance.Level}";
        if (m_healthNumber != null) m_healthNumber.text = weaponInstance.GetHealth().ToString();
        if (m_damageNumber != null) m_damageNumber.text = weaponInstance.GetDamage().ToString();

        if (m_weaponImage != null)
        {
            await weaponInstance.EnsureIconLoadedAsync();

            m_weaponImage.style.backgroundImage = new StyleBackground(weaponInstance.IconTexture);
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

    public void UpdateHealth(float newHealth)
    {
        if (m_healthNumber == null) return;

        newHealth = Mathf.Max(0, newHealth);
        m_healthNumber.text = newHealth.ToString("0");
    }


    private void OnButtonClicked(ClickEvent evt)
    {
        ArsenalEvents.WeaponItemClicked?.Invoke(this);
    }

    private void OnCustomButtonClicked(ClickEvent evt)
    {
        OnCustomClick?.Invoke();
    }

    public WeaponInstanceBase GetWeaponInstance() => m_WeaponInstance;
}
