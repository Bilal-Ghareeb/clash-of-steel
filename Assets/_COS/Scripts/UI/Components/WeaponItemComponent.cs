using System;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponItemComponent
{
    private VisualElement m_weaponItemButton;
    private WeaponInstance m_WeaponInstance;
    private VisualElement m_weaponImage;
    public VisualElement WeaponImage => m_weaponImage;
    private Label m_Lvl;
    private Label m_healthNumber;
    private Label m_damageNumber;
    public Label Lvl => m_Lvl;
    private VisualElement m_classIcon;
    public VisualElement ClassIcon => m_classIcon;
    private Texture2D m_defaultWeaponIcon;
    private bool m_isSelected = false;

    public bool IsSelected
    {
        get { return m_isSelected; }
        set
        {
            m_isSelected = value;
            if (m_weaponItemButton != null)
            {
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
    }

    public Action OnCustomClick { get; set; }

    public WeaponItemComponent()
    {
    }

    public void SetVisualElements(TemplateContainer weaponItemUXMLTemplate)
    {
        if (weaponItemUXMLTemplate == null) return;

        m_weaponItemButton = weaponItemUXMLTemplate.Q<VisualElement>("Weapon-item-button");
        m_Lvl = weaponItemUXMLTemplate.Q<Label>("weapon-scroll-item-lvl");

        m_weaponImage = weaponItemUXMLTemplate.Q<VisualElement>("weapon-scroll-item-icon");
        m_defaultWeaponIcon = m_weaponImage.resolvedStyle.backgroundImage.texture;

        m_classIcon = weaponItemUXMLTemplate.Q<VisualElement>("Class-icon");
        m_healthNumber = weaponItemUXMLTemplate.Q<Label>("health-number");
        m_damageNumber = weaponItemUXMLTemplate.Q<Label>("damage-number");
    }

    public void SetGameData(WeaponInstance weaponInstance = null)
    {
        if (weaponInstance == null)
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

            if(m_classIcon != null)
            {
                m_classIcon.ClearClassList();
            }

            if (m_weaponImage != null)
            {
                m_weaponImage.style.backgroundImage = new StyleBackground(m_defaultWeaponIcon);
            }

            return;
        }

        m_WeaponInstance = weaponInstance;
        WeaponProgressionData progression = PlayFabManager.Instance.ProgressionFormulas[weaponInstance.CatalogData.progressionId];

        if (m_Lvl != null) m_Lvl.text = $"{m_WeaponInstance.InstanceData.level}";
        if (m_healthNumber != null) m_healthNumber.text = WeaponProgressionCalculator.GetDamage(weaponInstance.CatalogData.baseHealth, weaponInstance.InstanceData.level, progression).ToString();
        if (m_damageNumber != null) m_damageNumber.text = WeaponProgressionCalculator.GetDamage(weaponInstance.CatalogData.baseDamage, weaponInstance.InstanceData.level, progression).ToString();

        if (m_weaponImage != null && weaponInstance.IconTexture != null)
        {
            m_weaponImage.style.backgroundImage = weaponInstance.IconTexture;
        }

        if (m_weaponItemButton != null)
        {
            m_weaponItemButton.ClearClassList();
            m_weaponItemButton.AddToClassList(WeaponItemComponentStyleClasses.GetRarityClass(m_WeaponInstance.CatalogData.rarity));
        }

        if (m_classIcon != null)
        {
            m_classIcon.ClearClassList();
            m_classIcon.AddToClassList(WeaponItemComponentStyleClasses.GetClassTypeClass(m_WeaponInstance.CatalogData.@class));
        }

        IsSelected = false;
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


    private void OnButtonClicked(ClickEvent evt)
    {
        ArsenalEvents.WeaponItemClicked?.Invoke(this);
    }

    private void OnCustomButtonClicked(ClickEvent evt)
    {
        OnCustomClick?.Invoke();
    }

    public WeaponInstance GetWeaponInstance() => m_WeaponInstance;
}
