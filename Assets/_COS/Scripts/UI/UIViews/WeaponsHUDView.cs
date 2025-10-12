using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class WeaponsHUDView : UIView
{
    private BattleManager m_Battle;

    private VisualTreeAsset m_WeaponItemAsset;
    private VisualElement m_PlayerContainer;
    private VisualElement m_EnemyContainer;

    private readonly Dictionary<Combatant, WeaponItemComponent> m_CombatantsUI = new();


    public WeaponsHUDView(VisualElement topElement,bool hideOnAwake = true)
        : base(topElement, hideOnAwake)
    {
        m_WeaponItemAsset = Resources.Load<VisualTreeAsset>("WeaponItem");
    }

    public void InitializeBattleManager(BattleManager battle)
    {
        m_Battle = battle;
        m_Battle.OnBattleStarted += OnBattleStarted;
        m_Battle.OnCombatantDamaged += OnCombatantDamaged;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (m_Battle != null)
        {
            m_Battle.OnBattleStarted -= OnBattleStarted;
            m_Battle.OnCombatantDamaged -= OnCombatantDamaged;
        }
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_PlayerContainer = m_TopElement.Q<VisualElement>("PlayerWeaponsContainer");
        m_EnemyContainer = m_TopElement.Q<VisualElement>("EnemyWeaponsContainer");
    }

    private void OnBattleStarted()
    {
        if (m_Battle == null) return;

        RefreshWeaponList(m_Battle.PlayerTeam, m_PlayerContainer);
        RefreshWeaponList(m_Battle.EnemyTeam, m_EnemyContainer);
    }

    private void RefreshWeaponList(Combatant[] combatants, VisualElement container)
    {
        container.Clear();

        if (combatants == null || combatants.Length == 0)
            return;

        foreach (var combatant in combatants)
        {
            if (combatant?.InstanceData == null)
                continue;

            TemplateContainer itemTemplate = m_WeaponItemAsset.Instantiate();
            var weaponItem = new WeaponItemComponent();
            weaponItem.SetVisualElements(itemTemplate);
            weaponItem.SetGameData(combatant.InstanceData);

            bool isFirst = combatant == combatants[0];
            float scale = isFirst ? 1f : 0.7f;
            itemTemplate.style.scale = new StyleScale(new Vector2(scale, scale));
            if (!isFirst)
                itemTemplate.style.opacity = 0.7f;

            container.Add(itemTemplate);
            m_CombatantsUI[combatant] = weaponItem;

        }
    }

    private void OnCombatantDamaged(Combatant combatant, float newHealth)
    {
        if (combatant == null)
            return;

        if (m_CombatantsUI.TryGetValue(combatant, out var uiItem))
        {
            uiItem.UpdateHealth(newHealth);
        }
    }


}
