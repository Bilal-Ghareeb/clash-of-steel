using UnityEngine.UIElements;

public class InspectView : UIView
{
    private VisualElement m_backButton;
    private VisualElement m_weaponLevelUpButton;

    private Label m_weaponName;
    private Label m_weaponDescription;

    private Label m_currentHealth;
    private Label m_currentDamage;
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
        m_weaponName.text = weapon.Data.name;
        m_weaponDescription.text = weapon.Data.description;

        m_weaponLvl.text = weapon.Data.level.ToString();
        m_currentHealth.text = (weapon.Data.level * weapon.Data.scaling.healthPerLevel).ToString();
        m_currentDamage.text = (weapon.Data.baseDamage * weapon.Data.scaling.damagePerLevel).ToString();

        m_presenter.ShowWeapon(weapon);
    }

    private void ReturnToArsenal(ClickEvent evt)
    {
        m_presenter.ClearCurrent();
        InspectWeaponEvents.BackToArsenalButtonPressed?.Invoke();
    }

    private void LevelUpWeapon(ClickEvent evt)
    {
        // i want playfab to execute the level up weapon here 
    }
}
