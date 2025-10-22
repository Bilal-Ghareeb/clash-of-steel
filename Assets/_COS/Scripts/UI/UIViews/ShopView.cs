using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ShopView : UIView
{
    private VisualElement m_diamondBundlesContainer;
    private VisualElement m_lootboxesContainer;
    private VisualElement m_lootBoxesContentContainer;
    private Label m_lootBoxTitle;
    private ScrollView m_lootBoxContentScrollView;

    private VisualTreeAsset m_shopItemAsset;
    private VisualTreeAsset m_weaponItemAsset;


    public ShopView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        m_shopItemAsset = Resources.Load<VisualTreeAsset>("ShopItem");
        m_weaponItemAsset = Resources.Load<VisualTreeAsset>("WeaponItem");
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_diamondBundlesContainer = m_TopElement.Q<VisualElement>("diamond-bundles-container");
        m_lootboxesContainer = m_TopElement.Q<VisualElement>("lootboxes-bundles-container");
        m_lootBoxesContentContainer = m_TopElement.Q<VisualElement>("loot-box-content-container");
        m_lootBoxTitle = m_TopElement.Q<Label>("loot-box-title");
        m_lootBoxContentScrollView = m_TopElement.Q<ScrollView>("loot-box-content-scrollview");
    }

    public override void Show()
    {
        base.Show();
        m_lootBoxesContentContainer.style.display = DisplayStyle.None;
        PopulateDiamondBundles();
        PopulateLootBoxes();
    }

    private void PopulateDiamondBundles()
    {
        m_diamondBundlesContainer.Clear();

        var bundles = PlayFabManager.Instance.EconomyService.DiamondBundlesCatalog;

        foreach (var bundle in bundles)
        {

            TemplateContainer bundleUI = m_shopItemAsset.Instantiate();
            var shopItem = new ShopItemComponent();
            shopItem.SetVisualElements(bundleUI);

            shopItem.Configure(ShopItemType.DiamondBundle);

            if (bundle.Amount > 0)
            {
                shopItem.SetAmount(bundle.Amount);
            }

            if (PlayFabManager.Instance.IAPService != null)
            {
                string price = PlayFabManager.Instance.IAPService.GetProductPrice(bundle.MarketplaceId);
                if (!string.IsNullOrEmpty(price))
                {
                    shopItem.SetPrice(price);
                }
                else
                {
                    shopItem.SetPrice("Price not available");
                }
            }

            if (PlayFabManager.Instance.IAPService != null)
            {
                string marketplaceId = bundle.MarketplaceId;
                shopItem.OnPurchaseClicked = () => PlayFabManager.Instance.IAPService.BuyProduct(marketplaceId);
                shopItem.RegisterButtonCallbacks();
            }

            m_diamondBundlesContainer.Add(bundleUI);
        }
    }

    private void PopulateLootBoxes()
    {
        m_lootboxesContainer.Clear();

        var lootBoxes = PlayFabManager.Instance.EconomyService.LootBoxes;

        foreach (var lootBox in lootBoxes)
        {
            TemplateContainer lootBoxUI = m_shopItemAsset.Instantiate();
            var shopItem = new ShopItemComponent();
            shopItem.SetVisualElements(lootBoxUI);

            shopItem.Configure(ShopItemType.LootBox);
            shopItem.SetPrice(lootBox.cost.ToString());

            shopItem.OnDetailsClicked = () =>
            {
                var weaponEntries = PlayFabManager.Instance.EconomyService.GetWeaponsCatalogItemsInLootBox(lootBox.id);
                PopulateLootBoxDetailsPanel(weaponEntries , lootBox.id);
                m_lootBoxesContentContainer.style.display = DisplayStyle.Flex;
                m_lootBoxesContentContainer.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
                m_lootBoxesContentContainer.experimental.animation.Scale(1f, 200);
            };


            shopItem.RegisterButtonCallbacks();
            m_lootboxesContainer.Add(lootBoxUI);
        }
    }


    private void PopulateLootBoxDetailsPanel(List<LootBoxWeaponEntry> weaponEntries , string lootBoxName)
    {
        m_lootBoxTitle.text = lootBoxName;
        m_lootBoxContentScrollView.Clear();

        foreach (var entry in weaponEntries)
        {
            var weaponCatalogItem = entry.WeaponCatalogItem;
            float dropWeight = entry.Weight;

            TemplateContainer weaponUI = m_weaponItemAsset.Instantiate();
            var weaponLootBoxEntryComponent = new WeaponItemComponent();
            weaponLootBoxEntryComponent.SetVisualElements(weaponUI, WeaponItemComponentDisplayContext.LootBoxDetails);

            var previewInstance = new CatalogWeaponInstance(weaponCatalogItem);
            weaponLootBoxEntryComponent.SetGameData(previewInstance);
            weaponLootBoxEntryComponent.SetChance(dropWeight);

            m_lootBoxContentScrollView.Add(weaponUI);
        }
    }




}