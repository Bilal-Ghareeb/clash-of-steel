using UnityEngine.UIElements;

public class InspectView : UIView
{
    private VisualElement m_backButton;
    private VisualElement m_levelUpButton;

    private Label m_weaponName, m_weaponDescription, m_health, m_damage, m_cost, m_max, m_level;


    public InspectView(VisualElement root) : base(root)
    {
        SetVisualElements();
        RegisterButtonCallbacks();
    }

    public override void Show()
    {
        base.Show();
        InspectWeaponEvents.ScreenEnabled?.Invoke();
    }

    protected override void SetVisualElements()
    {
        m_backButton = m_TopElement.Q<VisualElement>("Back-btn");
        m_levelUpButton = m_TopElement.Q<VisualElement>("Lvlup-btn");
        m_weaponName = m_TopElement.Q<Label>("Weapon-name");
        m_weaponDescription = m_TopElement.Q<Label>("Weapon-description");
        m_health = m_TopElement.Q<Label>("Health-number");
        m_damage = m_TopElement.Q<Label>("Damage-number");
        m_level = m_TopElement.Q<Label>("Lvl-text");
        m_cost = m_TopElement.Q<Label>("Currency-text");
        m_max = m_TopElement.Q<Label>("Max-text");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_backButton.RegisterCallback<ClickEvent>(_ => InspectWeaponEvents.BackButtonClicked?.Invoke());
        m_levelUpButton.RegisterCallback<ClickEvent>(_ => InspectWeaponEvents.LevelUpWeaponClicked?.Invoke());
    }

    public void SetWeaponData(WeaponInstanceBase weapon)
    {
        m_weaponName.text = weapon.CatalogData.name;
        m_weaponDescription.text = weapon.CatalogData.description;
        RefreshWeaponStats(weapon);
    }

    public void RefreshWeaponStats(WeaponInstanceBase weapon)
    {
        var progression = PlayFabManager.Instance.EconomyService.ProgressionFormulas[weapon.CatalogData.progressionId];
        int level = (weapon is WeaponInstance instance) ? instance.InstanceData.level : 1;

        m_level.text = level.ToString();
        m_health.text = WeaponProgressionCalculator.GetHealth(weapon.CatalogData.baseHealth, level, progression).ToString();
        m_damage.text = WeaponProgressionCalculator.GetDamage(weapon.CatalogData.baseDamage, level, progression).ToString();

        int cost = WeaponProgressionCalculator.GetCostForLevelUp(level, progression);
        m_cost.text = cost.ToString();

        bool isMax = level >= progression.maxLevel;
        m_max.style.display = isMax ? DisplayStyle.Flex : DisplayStyle.None;
        m_cost.style.display = isMax ? DisplayStyle.None : DisplayStyle.Flex;
    }

    public void SetLevelUpInteractable(bool state)
    {
        m_levelUpButton.SetEnabled(state);
        m_levelUpButton.style.opacity = state ? 1f : 0.5f;
    }

    public void PlayLevelUpAnimation()
    {
        BounceLabel(m_health);
        BounceLabel(m_damage);
        BounceLabel(m_level);
    }

    private void BounceLabel(VisualElement label)
    {
        if (label == null) return;

        label.experimental.animation
            .Scale(2f, 100)
            .OnCompleted(() =>
            {
                label.experimental.animation.Scale(1f, 150);
            });
    }
}
