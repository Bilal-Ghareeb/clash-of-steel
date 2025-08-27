using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ArsenalView : UIView
{
    public static readonly string[] RarityKeys = { "All", "Common", "Rare", "Legendary" };
    public static readonly string[] ClassTypeKeys = { "All", "Sword", "Shield", "Hammer"};

    private ScrollView m_ScrollViewParent;

    private DropdownField m_InventoryRarityDropdown;
    private DropdownField m_InventoryClassTypeDropdown;

    private VisualTreeAsset m_WeaponItemAsset;

    public ArsenalView(VisualElement topElement) : base(topElement)
    {
        ArsenalEvents.WeaponItemClicked += OnWeaponItemClicked;
        ArsenalEvents.ArsenalSetup += OnArsenalSetup;
        ArsenalEvents.ArsenalUpdated += OnArsenalUpdated;

        m_WeaponItemAsset = Resources.Load("WeaponItem") as VisualTreeAsset;
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

        m_InventoryRarityDropdown = m_TopElement.Q<DropdownField>("Rarity-dropdown");
        m_InventoryClassTypeDropdown = m_TopElement.Q<DropdownField>("Class-dropdown");

        m_ScrollViewParent = m_TopElement.Q<ScrollView>("ScrollView");
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

    private void ShowWeaponItems(IReadOnlyList<WeaponInstance> waeponsToShow)
    {
        VisualElement contentContainer = m_ScrollViewParent.Q<VisualElement>("unity-content-container");
        contentContainer.Clear();

        for (int i = 0; i < waeponsToShow.Count; i++)
        {
            CreateGearItemButton(waeponsToShow[i], contentContainer);
        }
    }

    private void CreateGearItemButton(WeaponInstance weaponData, VisualElement container)
    {
        if (container == null)
        {
            Debug.Log("InventoryScreen.CreateGearItemButton: missing parent element");
            return;
        }

        TemplateContainer weaponUIElement = m_WeaponItemAsset.Instantiate();

        WeaponItemComponent weaponItem = new();

        weaponItem.SetVisualElements(weaponUIElement);
        weaponItem.SetGameData(weaponData);
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

   private void OnArsenalUpdated(IReadOnlyList<WeaponInstance> weaponsToLoad)
    {
        ShowWeaponItems(weaponsToLoad);
    }

    private void OnWeaponItemClicked(WeaponItemComponent weaponComponent)
    {
        Debug.Log(weaponComponent.GetWeaponInstance().Data.name + " Is Clicked");
    }
}
