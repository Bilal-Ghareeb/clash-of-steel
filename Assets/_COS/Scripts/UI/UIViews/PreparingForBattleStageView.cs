using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PreparingForBattleStageView : UIView
{
    private VisualTreeAsset m_WeaponItemAsset;

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

        SpawnPlayerTeamHolders(m_playerTeamContainer);
        FetchPlayerArsenal();
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

    private void BeginBattle(ClickEvent evt)
    {
        Debug.Log("Battle is Starting...");
    }

    private void SpawnPlayerTeamHolders(VisualElement container)
    {
        if (container == null)
        {
            Debug.LogError("Team container is null.");
            return;
        }
        container.Clear();

        for (int i = 0; i < m_playerSelectedWeapons.Length; i++)
        {
            TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
            m_playerTeamSlotComponents[i] = new WeaponItemComponent();
            m_playerTeamSlotComponents[i].SetVisualElements(weaponUIElement);
            m_playerTeamSlotComponents[i].SetGameData();

            int slotIndex = i;
            m_playerTeamSlotComponents[i].OnCustomClick = () => OnTeamSlotClicked(slotIndex);
            m_playerTeamSlotComponents[i].RegisterButtonCallbacks(useCustomClick: true);

            container.Add(weaponUIElement);
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

        var playerWeapons = PlayFabManager.Instance.PlayerWeapons;

        if (playerWeapons == null || playerWeapons.Count == 0)
        {
            Debug.Log("Player has no weapons in arsenal.");
            return;
        }

        foreach (var weapon in playerWeapons)
        {
            TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
            WeaponItemComponent weaponItem = new WeaponItemComponent();

            weaponItem.SetVisualElements(weaponUIElement);
            weaponItem.SetGameData(weapon);

            m_arsenalWeaponComponents.Add(weaponItem);

            weaponItem.OnCustomClick = () => OnArsenalWeaponClicked(weaponItem,weapon);
            weaponItem.RegisterButtonCallbacks(useCustomClick: true);

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


    private void ClearContainers()
    {
        m_playerTeamContainer?.Clear();
        m_opponentTeamContainer?.Clear();
    }
}
