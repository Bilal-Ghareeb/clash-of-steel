using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PreparingForBattleStageView : UIView
{
    private VisualTreeAsset m_WeaponItemAsset;

    private Label m_stageNumber;
    private Button m_leavePreparingForBattleButton;
    private Button m_beginBattleStageButton;
    private VisualElement m_opponentTeamContainer;
    private VisualElement m_playerTeamContainer;
    private ScrollView m_ScrollViewParent;

    private WeaponItemComponent[] m_playerTeamSlotComponents = new WeaponItemComponent[2];
    private List<WeaponItemComponent> m_arsenalWeaponComponents = new List<WeaponItemComponent>();

    public PreparingForBattleStageView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        m_WeaponItemAsset = Resources.Load<VisualTreeAsset>("WeaponItem");
    }

    public override void Show()
    {
        base.Show();

        PreparingForBattleStageEvents.ScreenEnabled?.Invoke();
        PreparingForBattleStageEvents.RequestFetchPlayerArsenal?.Invoke();
        PreparingForBattleStageEvents.RequestStageInfo?.Invoke();
        SpawnPlayerTeamHolders();
        EnableButtons();
    }

    public override void Hide()
    {
        base.Hide();
        PreparingForBattleStageEvents.ClearContainers?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        UnregisterButtonCallbacks();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_stageNumber = m_TopElement.Q<Label>("stage-number");
        m_leavePreparingForBattleButton = m_TopElement.Q<Button>("Back-btn");
        m_beginBattleStageButton = m_TopElement.Q<Button>("Battle-btn");
        m_opponentTeamContainer = m_TopElement.Q<VisualElement>("OponentTeamContainer");
        m_playerTeamContainer = m_TopElement.Q<VisualElement>("PlayerTeamContainer");
        m_ScrollViewParent = m_TopElement.Q<ScrollView>("ScrollView");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_leavePreparingForBattleButton.RegisterCallback<ClickEvent>(evt => PreparingForBattleStageEvents.LeavePreparingForBattle?.Invoke());
        m_beginBattleStageButton.RegisterCallback<ClickEvent>(evt => PreparingForBattleStageEvents.RequestBeginBattle?.Invoke());
    }

    protected void UnregisterButtonCallbacks()
    {
        m_leavePreparingForBattleButton.UnregisterCallback<ClickEvent>(evt => PreparingForBattleStageEvents.LeavePreparingForBattle?.Invoke());
        m_beginBattleStageButton.UnregisterCallback<ClickEvent>(evt => PreparingForBattleStageEvents.RequestBeginBattle?.Invoke());
    }

    private void SpawnPlayerTeamHolders()
    {
        if (m_playerTeamContainer == null)
        {
            Debug.LogError("Team container is null.");
            return;
        }
        m_playerTeamContainer.Clear();

        for (int i = 0; i < m_playerTeamSlotComponents.Length; i++)
        {
            TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
            m_playerTeamSlotComponents[i] = new WeaponItemComponent();
            m_playerTeamSlotComponents[i].SetVisualElements(weaponUIElement, WeaponItemComponentDisplayContext.PrepareForBattle);
            m_playerTeamSlotComponents[i].SetGameData();

            int slotIndex = i;
            m_playerTeamSlotComponents[i].OnCustomClick = () => PreparingForBattleStageEvents.TeamSlotClicked?.Invoke(slotIndex);
            m_playerTeamSlotComponents[i].RegisterButtonCallbacks(useCustomClick: true);

            m_playerTeamContainer.Add(weaponUIElement);
        }
    }

    public void UpdateTeamSlot(int slotIndex, WeaponInstance weapon)
    {
        if (slotIndex < 0 || slotIndex >= m_playerTeamSlotComponents.Length) return;
        m_playerTeamSlotComponents[slotIndex].SetGameData(weapon);
    }

    public void UpdatePlayerArsenal(IReadOnlyList<WeaponInstance> weapons)
    {
        if (m_ScrollViewParent == null)
        {
            Debug.LogError("ScrollView parent is null.");
            return;
        }

        VisualElement contentContainer = m_ScrollViewParent.Q<VisualElement>("unity-content-container");
        if (contentContainer == null)
        {
            Debug.LogError("Could not find ScrollView content container.");
            return;
        }

        contentContainer.Clear();
        m_arsenalWeaponComponents.Clear();

        foreach (var weapon in weapons)
        {
            TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
            WeaponItemComponent weaponItem = new WeaponItemComponent();
            weaponItem.SetVisualElements(weaponUIElement, WeaponItemComponentDisplayContext.PrepareForBattle);

            int levelToDisplay = LocalWeaponProgressionCache.TryGetLocalLevel(weapon.Item.Id, out int localLevel)
                ? localLevel
                : weapon.InstanceData.level;

            weaponItem.SetGameData(weapon, levelToDisplay);
            m_arsenalWeaponComponents.Add(weaponItem);

            weaponItem.OnCustomClick = () => PreparingForBattleStageEvents.ArsenalWeaponClicked?.Invoke(weapon);
            weaponItem.RegisterButtonCallbacks(useCustomClick: true);

            if (weapon.IsOnCooldown)
            {
                weaponItem.OnAdsButtonClick = () =>
                {
                    PlayFabManager.Instance.ADService.ShowRewardedAd(async () =>
                    {
                        weapon.RemoveCooldownLocally();
                        weaponItem.EnableInteractions();
                        await PlayFabManager.Instance.AzureService.ClearWeaponCooldownAsync(weapon.Item.Id);
                    });
                };
                weaponItem.DisableInteractions();
            }

            contentContainer.Add(weaponUIElement);
        }
    }

    public void UpdateEnemyTeam(List<EnemyWeaponInstance> enemies)
    {
        if (m_opponentTeamContainer == null)
        {
            Debug.LogError("Opponent team container is null.");
            return;
        }

        m_opponentTeamContainer.Clear();

        foreach (var enemy in enemies)
        {
            TemplateContainer enemyUIElement = m_WeaponItemAsset.Instantiate();
            var enemyItemComponent = new WeaponItemComponent();
            enemyItemComponent.SetVisualElements(enemyUIElement, WeaponItemComponentDisplayContext.PrepareForBattle);
            enemyItemComponent.SetGameData(enemy);
            m_opponentTeamContainer.Add(enemyUIElement);
        }
    }

    public void UpdateStageInfo(int stageId)
    {
        m_stageNumber.text = stageId.ToString() ?? "?";
    }

    public void ClearContainers()
    {
        m_playerTeamContainer?.Clear();
        m_opponentTeamContainer?.Clear();
        m_ScrollViewParent?.Q<VisualElement>("unity-content-container")?.Clear();
        m_arsenalWeaponComponents.Clear();
    }

    public void UpdateWeaponSelection(WeaponInstance weapon, bool isSelected)
    {
        var weaponItem = m_arsenalWeaponComponents.Find(component => component.GetWeaponInstance() == weapon);
        if (weaponItem != null)
        {
            weaponItem.IsSelected = isSelected;
        }
    }

    public void CloseButtonsWhileLoadingBattle()
    {
        m_beginBattleStageButton.SetEnabled(false);
        m_leavePreparingForBattleButton.SetEnabled(false);
    }

    public void EnableButtons()
    {
        m_beginBattleStageButton.SetEnabled(true);
        m_leavePreparingForBattleButton.SetEnabled(true);
    }
}