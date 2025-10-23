using System.Collections.Generic;
using System.Linq;
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

    private WeaponInstance[] m_playerSelectedWeapons = new WeaponInstance[2];
    private WeaponItemComponent[] m_playerTeamSlotComponents = new WeaponItemComponent[2];
    private List<WeaponItemComponent> m_arsenalWeaponComponents = new List<WeaponItemComponent>();


    public PreparingForBattleStageView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        m_WeaponItemAsset = Resources.Load<VisualTreeAsset>("WeaponItem");
    }

    public override void Show()
    {
        base.Show();
        PreparingForBattleStageEvents.PreparingForBattleStageShown?.Invoke();

        SetVisualElements();
        RegisterButtonCallbacks();

        SpawnPlayerTeamHolders();
        SpawnEnemyTeamHolders();

        FetchPlayerArsenal();

        UpdateStageInfo();
    }

    public override void Hide()
    {
        base.Hide();
        ClearContainers();
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
        m_leavePreparingForBattleButton.RegisterCallback<ClickEvent>(LeavePreparingForBattle);
        m_beginBattleStageButton.RegisterCallback<ClickEvent>(BeginBattle);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_leavePreparingForBattleButton.UnregisterCallback<ClickEvent>(LeavePreparingForBattle);
        m_beginBattleStageButton.UnregisterCallback<ClickEvent>(BeginBattle);
    }

    private void LeavePreparingForBattle(ClickEvent evt)
    {
        PreparingForBattleStageEvents.LeavePreparingForBattle?.Invoke();
    }

    private async void BeginBattle(ClickEvent evt)
    {
        var playerTeam = m_playerSelectedWeapons.Where(w => w != null).ToList();
        if (playerTeam.Count == 0) return;

        var enemies = PlayFabManager.Instance.PlayerService?.CurrentStage.enemies;
        if (enemies == null) return;

        foreach (var weapon in playerTeam)
        {
            await PlayFabManager.Instance.AzureService.StartWeaponCooldownAsync(
                weapon.Item.Id,
                weapon.Level,
                weapon.CatalogData.rarity,
                weapon.CatalogData.progressionId
            );
        }

        PreparingForBattleStageEvents.RequestBeginBattle?.Invoke(playerTeam, enemies);
    }

    private void SpawnPlayerTeamHolders()
    {
        if (m_playerTeamContainer == null)
        {
            Debug.LogError("Team container is null.");
            return;
        }
        m_playerTeamContainer.Clear();

        for (int i = 0; i < m_playerSelectedWeapons.Length; i++)
        {
            TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
            m_playerTeamSlotComponents[i] = new WeaponItemComponent();
            m_playerTeamSlotComponents[i].SetVisualElements(weaponUIElement, WeaponItemComponentDisplayContext.PrepareForBattle);
            m_playerTeamSlotComponents[i].SetGameData();

            int slotIndex = i;
            m_playerTeamSlotComponents[i].OnCustomClick = () => OnTeamSlotClicked(slotIndex);
            m_playerTeamSlotComponents[i].RegisterButtonCallbacks(useCustomClick: true);

            m_playerTeamContainer.Add(weaponUIElement);
        }
    }

    private void SpawnEnemyTeamHolders()
    {
        var currentStage = PlayFabManager.Instance.PlayerService?.CurrentStage;
        if (currentStage == null)
        {
            Debug.LogError("Cannot spawn enemies: current stage not found.");
            return;
        }

        if (m_opponentTeamContainer == null)
        {
            Debug.LogError("Opponent team container is null.");
            return;
        }

        m_opponentTeamContainer.Clear();

        foreach (var enemy in currentStage.enemies)
        {
            var weaponData = PlayFabManager.Instance.EconomyService.GetWeaponDataByFriendlyId(enemy.weaponId);
            if (weaponData == null)
            {
                Debug.LogWarning($"No WeaponData found for friendly ID: {enemy.weaponId}");
                continue;
            }

            var enemyInstance = new EnemyWeaponInstance(enemy.weaponId, weaponData, enemy.level);

            TemplateContainer enemyUIElement = m_WeaponItemAsset.Instantiate();
            var enemyItemComponent = new WeaponItemComponent();
            enemyItemComponent.SetVisualElements(enemyUIElement, WeaponItemComponentDisplayContext.PrepareForBattle);
            enemyItemComponent.SetGameData(enemyInstance);

            m_opponentTeamContainer.Add(enemyUIElement);
        }
    }


    private void FetchPlayerArsenal()
    {
        if (m_ScrollViewParent == null)
        {
            Debug.LogError("ScrollView parent is null in PreparingForBattleStageView.");
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

        var playerWeapons = PlayFabManager.Instance.EconomyService.PlayerWeapons;

        if (playerWeapons == null || playerWeapons.Count == 0)
        {
            Debug.Log("Player has no weapons in arsenal.");
            return;
        }

        foreach (var weapon in playerWeapons)
        {
            TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
            WeaponItemComponent weaponItem = new WeaponItemComponent();

            weaponItem.SetVisualElements(weaponUIElement, WeaponItemComponentDisplayContext.PrepareForBattle);
            weaponItem.SetGameData(weapon);

            m_arsenalWeaponComponents.Add(weaponItem);

            weaponItem.OnCustomClick = () => OnArsenalWeaponClicked(weaponItem,weapon);
            weaponItem.RegisterButtonCallbacks(useCustomClick: true);

            if (weapon.IsOnCooldown)
            {
                weaponItem.OnAdsButtonClick = ()=> PlayFabManager.Instance.ADService.ShowRewardedAd();
                weaponItem.DisableInteractions();
            }

            contentContainer.Add(weaponUIElement);
        }
    }

    private void OnArsenalWeaponClicked(WeaponItemComponent weaponItem, WeaponInstance weapon)
    {
        for (int i = 0; i < m_playerSelectedWeapons.Length; i++)
        {
            if (m_playerSelectedWeapons[i] == null)
            {
                weaponItem.IsSelected = true;
                m_playerSelectedWeapons[i] = weapon;
                UpdateTeamSlot(i, weapon);
                break;
            }
        }
    }


    private void OnTeamSlotClicked(int slotIndex)
    {
        if (m_playerSelectedWeapons[slotIndex] != null)
        {
            WeaponItemComponent selectedWeaponComponent = m_arsenalWeaponComponents.Find(
                component => component.GetWeaponInstance() == m_playerSelectedWeapons[slotIndex]
            );

            if (selectedWeaponComponent != null)
            {
                selectedWeaponComponent.IsSelected = false;
            }

            m_playerSelectedWeapons[slotIndex] = null;
            UpdateTeamSlot(slotIndex, null);
        }
    }

    private void UpdateTeamSlot(int slotIndex, WeaponInstance weapon)
    {
        if (weapon != null)
        {
            m_playerTeamSlotComponents[slotIndex].SetGameData(weapon);
        }
        else
        {
            m_playerTeamSlotComponents[slotIndex].SetGameData(null);
        }
    }

    private void UpdateStageInfo()
    {
        var currentStage = PlayFabManager.Instance.PlayerService?.CurrentStage;
        if (currentStage == null)
        {
            m_stageNumber.text = "?";
            return;
        }

        m_stageNumber.text = $"{currentStage.id}";
    }


    private void ClearContainers()
    {
        m_playerTeamContainer?.Clear();
        m_opponentTeamContainer?.Clear();
    }
}
