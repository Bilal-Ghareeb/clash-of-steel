using PlayFab.EconomyModels;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ArsenalView : UIView
{
    public static readonly string[] RarityKeys = { "All", "Common", "Rare", "Legendary" };
    public static readonly string[] ClassTypeKeys = { "All", "Sword", "Shield", "Spear"};

    private ScrollView m_ScrollViewParent;
    private VisualElement m_ArsenalPanel;

    private DropdownField m_InventoryRarityDropdown;
    private DropdownField m_InventoryClassTypeDropdown;

    private VisualTreeAsset m_WeaponItemAsset;

    public ArsenalView(VisualElement topElement) : base(topElement)
    {
        ArsenalEvents.WeaponItemClicked += OnWeaponItemClicked;
        ArsenalEvents.ArsenalSetup += OnArsenalSetup;
        ArsenalEvents.ArsenalUpdated += OnArsenalUpdated;
    }

    public override void Dispose()
    {
        base.Dispose();
        ArsenalEvents.WeaponItemClicked -= OnWeaponItemClicked;
        ArsenalEvents.ArsenalSetup -= OnArsenalSetup;
        ArsenalEvents.ArsenalUpdated -= OnArsenalUpdated;

        UnregisterButtonCallbacks();
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();

        m_ArsenalPanel = m_TopElement.Q("ArsenalView");

        m_InventoryRarityDropdown = m_ArsenalPanel.Q<DropdownField>("Rarity-dropdown");
        m_InventoryClassTypeDropdown = m_ArsenalPanel.Q<DropdownField>("Class-dropdown");

        m_ScrollViewParent = m_ArsenalPanel.Q<ScrollView>("ScrollView-Container");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_InventoryRarityDropdown.RegisterValueChangedCallback(UpdateFilters);
        m_InventoryClassTypeDropdown.RegisterValueChangedCallback(UpdateFilters);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_InventoryRarityDropdown.UnregisterValueChangedCallback(UpdateFilters);
        m_InventoryClassTypeDropdown.UnregisterValueChangedCallback(UpdateFilters);
    }

    private Rarity GetRarity(string rarityString)
    {

        Rarity rarity = Rarity.Common;

        if (!Enum.TryParse<Rarity>(rarityString, out rarity))
        {
            Debug.Log("String " + rarityString + " failed to convert");
        }
        return rarity;
    }

    private WeaponType GetWeaponType(string weaponTypeString)
    {

        WeaponType weaponType = WeaponType.Sword;

        if (!Enum.TryParse<WeaponType>(weaponTypeString, out weaponType))
        {
            Debug.LogWarning("Converted " + weaponTypeString + " failed to convert");
        }
        return weaponType;
    }

    private void UpdateFilters(ChangeEvent<string> evt)
    {
        string weaponTypeKey = ClassTypeKeys[m_InventoryClassTypeDropdown.index];
        string rarityKey = RarityKeys[m_InventoryRarityDropdown.index];

        WeaponType gearType = GetWeaponType(weaponTypeKey);
        Rarity rarity = GetRarity(rarityKey);

        Debug.Log("Geras Filtered To :" + rarity +" and "+ gearType);

        ArsenalEvents.GearFiltered?.Invoke(rarity, gearType);
    }

    private void ShowWeaponItems(List<InventoryItem> waeponsToShow)
    {
        VisualElement contentContainer = m_ScrollViewParent.Q<VisualElement>("unity-content-container");
        contentContainer.Clear();

        for (int i = 0; i < waeponsToShow.Count; i++)
        {
            CreateGearItemButton(waeponsToShow[i], contentContainer);
        }
    }

    private void CreateGearItemButton(InventoryItem weaponData, VisualElement container)
    {
        if (container == null)
        {
            Debug.Log("InventoryScreen.CreateGearItemButton: missing parent element");
            return;
        }

        TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();
        weaponUIElement.AddToClassList("gear-item-spacing");

        WeaponItemComponent weaponItem = new WeaponItemComponent(weaponData);

        weaponItem.SetVisualElements(weaponUIElement);
        weaponItem.SetGameData(weaponUIElement);
        weaponItem.RegisterButtonCallbacks();

        container.Add(weaponUIElement);
    }

    public override void Show()
    {
        base.Show();

        ArsenalEvents.ScreenEnabled?.Invoke();
        UpdateFilters(null);
    }


    public override void Hide()
    {
        base.Hide();
    }

    private void OnArsenalSetup()
    {
        SetVisualElements();
        RegisterButtonCallbacks();
    }

   private void OnArsenalUpdated(List<InventoryItem> weaponsToLoad)
    {
        ShowWeaponItems(weaponsToLoad);
    }

    private void OnWeaponItemClicked(WeaponItemComponent weaponComponent)
    {
        Debug.Log("Weapon Clicked");
    }
}
