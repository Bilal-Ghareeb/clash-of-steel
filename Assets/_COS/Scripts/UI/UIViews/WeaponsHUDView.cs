using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class WeaponsHUDView : UIView
{
    private VisualTreeAsset m_WeaponItemAsset;
    private VisualElement m_PlayerContainer;
    private VisualElement m_EnemyContainer;

    public WeaponsHUDView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        m_WeaponItemAsset = Resources.Load<VisualTreeAsset>("WeaponItem");
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_PlayerContainer = m_TopElement.Q<VisualElement>("PlayerWeaponsContainer");
        m_EnemyContainer = m_TopElement.Q<VisualElement>("EnemyWeaponsContainer");
    }

    public void ClearContainer(bool isPlayer)
    {
        var container = isPlayer ? m_PlayerContainer : m_EnemyContainer;
        container?.Clear();
    }

    public void AddWeaponItemToContainer(WeaponItemComponent weaponItem, bool isPlayer, float scale, int durationMs = 500)
    {
        if (weaponItem == null) return;
        var container = isPlayer ? m_PlayerContainer : m_EnemyContainer;
        VisualElement root = weaponItem.Root;
        root.experimental.animation.Scale(scale, durationMs).Ease(Easing.OutCubic);
        container.Add(root);
    }

    public WeaponItemComponent CreateWeaponItem(Combatant combatant, WeaponItemComponentDisplayContext context)
    {
        if (combatant?.InstanceData == null || m_WeaponItemAsset == null) return null;

        TemplateContainer itemTemplate = m_WeaponItemAsset.Instantiate();
        var weaponItem = new WeaponItemComponent();
        weaponItem.SetVisualElements(itemTemplate, context);
        weaponItem.SetGameData(combatant.InstanceData);
        return weaponItem;
    }

    public void SetWeaponItemClickCallback(WeaponItemComponent weaponItem, Action onClick, bool useCustomClick)
    {
        if (weaponItem == null) return;
        weaponItem.OnCustomClick = onClick;
        weaponItem.RegisterButtonCallbacks(useCustomClick: useCustomClick);
    }

    public void RemoveWeaponItemClickCallback(WeaponItemComponent weaponItem)
    {
        if (weaponItem == null) return;
        weaponItem.UnRegisterButtonCallbacks();
        weaponItem.OnCustomClick = null;
    }

    public void UpdateAttackPreview(WeaponItemComponent weaponItem, float newAttackValue, bool hasAdvantage, bool hasDisadvantage)
    {
        weaponItem?.UpdateAttackPreview(newAttackValue, hasAdvantage, hasDisadvantage);
    }

    public void ResetAttackHudData(WeaponItemComponent weaponItem)
    {
        weaponItem?.ResetAttackHudData();
    }

    public void UpdateHealth(WeaponItemComponent weaponItem, int newHealth)
    {
        weaponItem?.UpdateHealth(newHealth);
    }

    public void ApplyDeadCardStyle(WeaponItemComponent weaponItem)
    {
        weaponItem?.ApplyDeadCardStyle();
    }
}