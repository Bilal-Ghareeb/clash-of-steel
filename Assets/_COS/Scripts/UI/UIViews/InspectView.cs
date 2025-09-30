using UnityEngine.UIElements;

public class InspectView : UIView
{
    private WeaponInstance m_currentWeapon;

    private VisualElement m_backButton;
    private VisualElement m_weaponLevelUpButton;

    private Label m_weaponName;
    private Label m_weaponDescription;

    private Label m_currentHealth;
    private Label m_currentDamage;
    private Label m_nextLvlCost;
    private Label m_weaponLvl;

    private WeaponInspectPresenter m_presenter;


    public InspectView(VisualElement topElement , WeaponInspectPresenter presenter) : base(topElement) 
    {
        m_presenter = presenter;
        InspectWeaponEvents.WeaponSelectedForInspect += OnWeaponSelectedForInspect;
    }

    public override void Show()
    {
        base.Show();
        InspectWeaponEvents.InspectWeaponViewShown?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        InspectWeaponEvents.WeaponSelectedForInspect -= OnWeaponSelectedForInspect;
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_backButton = m_TopElement.Q<VisualElement>("Back-btn");
        m_weaponLevelUpButton = m_TopElement.Q<VisualElement>("Lvlup-btn");
        m_weaponName = m_TopElement.Q<Label>("Weapon-name");
        m_weaponDescription = m_TopElement.Q<Label>("Weapon-description");
        m_currentHealth = m_TopElement.Q<Label>("Health-number");
        m_currentDamage = m_TopElement.Q<Label>("Damage-number");
        m_weaponLvl = m_TopElement.Q<Label>("Lvl-text");
        m_nextLvlCost = m_TopElement.Q<Label>("Currency-text");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_backButton.RegisterCallback<ClickEvent>(ReturnToArsenal);
        m_weaponLevelUpButton.RegisterCallback<ClickEvent>(LevelUpWeapon);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_backButton.UnregisterCallback<ClickEvent>(ReturnToArsenal);
        m_weaponLevelUpButton.UnregisterCallback<ClickEvent>(LevelUpWeapon);
    }

    private void OnWeaponSelectedForInspect(WeaponInstance weapon)
    {
        m_currentWeapon = weapon;

        WeaponProgressionData progression = PlayFabManager.Instance.ProgressionFormulas[weapon.CatalogData.progressionId];

        m_weaponName.text = weapon.CatalogData.name;
        m_weaponDescription.text = weapon.CatalogData.description;

        m_weaponLvl.text = weapon.InstanceData.level.ToString();

        m_currentHealth.text = WeaponProgressionCalculator.GetDamage(weapon.CatalogData.baseHealth, weapon.InstanceData.level, progression).ToString();
        m_currentDamage.text = WeaponProgressionCalculator.GetDamage(weapon.CatalogData.baseDamage, weapon.InstanceData.level, progression).ToString();
        m_nextLvlCost.text = WeaponProgressionCalculator.GetCostForLevelUp(weapon.InstanceData.level, progression).ToString();

        m_presenter.ShowWeapon(weapon);
    }

    private void ReturnToArsenal(ClickEvent evt)
    {
        m_presenter.ClearCurrent();
        InspectWeaponEvents.BackToArsenalButtonPressed?.Invoke();
    }

    private void LevelUpWeapon(ClickEvent evt)
    {
        if (m_currentWeapon == null) return;

        WeaponProgressionData progression = PlayFabManager.Instance.ProgressionFormulas[m_currentWeapon.CatalogData.progressionId];

        PlayFabManager.Instance.LevelWeapon(m_currentWeapon.Item.Id, progression.currencyId, WeaponProgressionCalculator.GetCostForLevelUp(m_currentWeapon.InstanceData.level, progression));
    }
}
