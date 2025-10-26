using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;
using static LTGUI;

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

        var rootElement = weaponItem.Root;
        if (rootElement != null)
        {
            List<TimeValue> durations = new List<TimeValue>();
            durations.Add(new TimeValue(0.1f, TimeUnit.Second));

            List<EasingFunction> easing = new List<EasingFunction>();
            easing.Add(new EasingFunction(EasingMode.EaseOutCubic));

            rootElement.style.transitionDuration = new StyleList<TimeValue>(durations);
            rootElement.style.transitionTimingFunction = new StyleList<EasingFunction>(easing);
        }

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

    public void AnimateAttackerCardDashAndDefenderCardReaction(WeaponItemComponent attacker, WeaponItemComponent defender, bool isPlayer)
    {
        var attackerRoot = attacker.Root;
        var defenderRoot = defender.Root;

        var attackerAnimationClass = isPlayer ? "dash-to-enemy-card" : "dash-to-player-card";
        var defenderAnimationClass = "react-to-attack";

        void OnDefenderTransitionEnd(TransitionEndEvent evt)
        {
            defenderRoot.RemoveFromClassList(defenderAnimationClass);
            defenderRoot.UnregisterCallback<TransitionEndEvent>(OnDefenderTransitionEnd);
        }

        void OnAttackerTransitionEnd(TransitionEndEvent evt)
        {
            defenderRoot.AddToClassList(defenderAnimationClass);
            defenderRoot.RegisterCallback<TransitionEndEvent>(OnDefenderTransitionEnd);

            attackerRoot.RemoveFromClassList(attackerAnimationClass);
            attackerRoot.UnregisterCallback<TransitionEndEvent>(OnAttackerTransitionEnd);
        }

        attackerRoot.AddToClassList(attackerAnimationClass);
        attackerRoot.RegisterCallback<TransitionEndEvent>(OnAttackerTransitionEnd);
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