using UnityEngine.UIElements;

public class WeaponItemComponent
{
    private VisualElement m_weaponItemButton;
    private WeaponInstance m_WeaponInstance;

    private VisualElement m_weaponImage;
    public VisualElement WeaponImage => m_weaponImage;

    private Label m_Lvl;
    public Label Lvl => m_Lvl;

    public WeaponItemComponent()
    {
    }

    public void SetVisualElements(TemplateContainer weaponItemUXMLTemplate)
    {
        if (weaponItemUXMLTemplate == null) return;

        m_weaponItemButton = weaponItemUXMLTemplate;
        m_Lvl = weaponItemUXMLTemplate.Q<Label>("weapon-scroll-item-lvl");
        m_weaponImage = weaponItemUXMLTemplate.Q<VisualElement>("weapon-scroll-item-icon");
    }

    public void SetGameData(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null) return;

        m_WeaponInstance = weaponInstance;

        m_Lvl.text = $"Lv {m_WeaponInstance.Data.level}";
        m_weaponImage.style.backgroundImage = weaponInstance.IconTexture;
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
