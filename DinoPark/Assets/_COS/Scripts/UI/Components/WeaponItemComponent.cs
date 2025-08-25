using UnityEngine.UIElements;

public class WeaponItemComponent
{
    private VisualElement m_weaponItemButton;

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
    }

    public void SetGameData(WeaponInstance weaponInstance)
    {
        if (weaponInstance == null) return;

        m_Lvl.text = $"Lv {weaponInstance.Data.level}";
    }

    public void RegisterButtonCallbacks()
    {
        m_weaponItemButton?.RegisterCallback<ClickEvent>(ClickGearItem);
    }

    private void ClickGearItem(ClickEvent evt)
    {
        ArsenalEvents.WeaponItemClicked?.Invoke(this);
    }
}
