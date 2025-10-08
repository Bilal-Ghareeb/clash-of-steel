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

    public WeaponItemComponent()
    {
    }

    public void SetVisualElements(TemplateContainer weaponItemUXMLTemplate)
    {
        if (weaponItemUXMLTemplate == null) return;

        m_weaponItemButton = weaponItemUXMLTemplate.Q<VisualElement>("Weapon-item-button");
        m_Lvl = weaponItemUXMLTemplate.Q<Label>("weapon-scroll-item-lvl");
        m_weaponImage = weaponItemUXMLTemplate.Q<VisualElement>("weapon-scroll-item-icon");
        m_classIcon = weaponItemUXMLTemplate.Q<VisualElement>("Class-icon");
        m_healthNumber = weaponItemUXMLTemplate.Q<Label>("health-number");
        m_damageNumber = weaponItemUXMLTemplate.Q<Label>("damage-number");

    }

    public void SetGameData(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null) return;

        m_WeaponInstance = weaponInstance;

        WeaponProgressionData progression = PlayFabManager.Instance.ProgressionFormulas[weaponInstance.CatalogData.progressionId];

        m_Lvl.text = $"{m_WeaponInstance.InstanceData.level}";
        m_healthNumber.text = WeaponProgressionCalculator.GetDamage(weaponInstance.CatalogData.baseHealth, weaponInstance.InstanceData.level, progression).ToString();
        m_damageNumber.text = WeaponProgressionCalculator.GetDamage(weaponInstance.CatalogData.baseDamage, weaponInstance.InstanceData.level, progression).ToString();
        m_weaponImage.style.backgroundImage = weaponInstance.IconTexture;

        m_weaponItemButton.AddToClassList(WeaponItemComponentStyleClasses.GetRarityClass(m_WeaponInstance.CatalogData.rarity));

        m_classIcon.ClearClassList();
        m_classIcon.AddToClassList(WeaponItemComponentStyleClasses.GetClassTypeClass(m_WeaponInstance.CatalogData.@class));
    }

    public void RegisterButtonCallbacks()
    {
        m_weaponItemButton?.RegisterCallback<ClickEvent>(OnButtonClicked);
    }

    private void OnButtonClicked(ClickEvent evt)
    {
        ArsenalEvents.WeaponItemClicked?.Invoke(this);
    }

    public WeaponInstance GetWeaponInstance() => m_WeaponInstance;
}
