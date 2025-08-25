using PlayFab.EconomyModels;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponItemComponent : MonoBehaviour
{
    private VisualElement m_weaponItemVisual;
    private Label m_Lvl;
    private Label m_Name;

    public Label Lvl => m_Lvl;
    public Label Name => m_Name;

    public WeaponItemComponent(InventoryItem m_weaponInstanceData)
    {

    }

    public void SetVisualElements(TemplateContainer gearElement)
    {
        if (gearElement == null)
            return;

        m_weaponItemVisual = gearElement.Q("gear-item__icon");
    }

    public void SetGameData(TemplateContainer gearElement)
    {
        if (gearElement == null)
            return;
    }

    public void RegisterButtonCallbacks()
    {
        m_weaponItemVisual.RegisterCallback<ClickEvent>(ClickGearItem);
    }

    public void ClickGearItem(ClickEvent evt)
    {
        ArsenalEvents.WeaponItemClicked?.Invoke(this);
    }

}
