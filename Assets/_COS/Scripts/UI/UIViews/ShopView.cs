using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UIElements.Experimental;

public class ShopView : UIView
{
    private VisualElement m_diamondBundlesContainer;
    private VisualElement m_lootboxesContainer;
    private VisualElement m_lootBoxesContentContainer;
    private VisualElement m_lootBoxOpenContainer;

    private VisualElement m_lootBox;
    private VisualElement m_lootBoxResultContainer;
    private bool m_isShaking = true;
    private string m_lootBoxShakeCurrentState = "loot-box-shake-middle";

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
        m_lootBoxOpenContainer = m_TopElement.Q<VisualElement>("loot-box-opening-container");
        m_lootBoxResultContainer = m_TopElement.Q<VisualElement>("loot-box-result-container");
        m_lootBox = m_TopElement.Q<VisualElement>("loot-box");
        m_lootBoxTitle = m_TopElement.Q<Label>("loot-box-title");
        m_lootBoxContentScrollView = m_TopElement.Q<ScrollView>("loot-box-content-scrollview");
    }

    public override void Show()
    {
        base.Show();
        m_lootBoxesContentContainer.style.display = DisplayStyle.None;
        ResetLootBoxOpenState();
        PopulateDiamondBundles();
        PopulateLootBoxes();
    }

    public override void Hide()
    {
        base.Hide();
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

            shopItem.OnPurchaseClicked = () =>
            {
                SetupAndShowOpenLootBoxContainer();
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

    private void SetupAndShowOpenLootBoxContainer()
    {
        m_lootBoxOpenContainer.style.display = DisplayStyle.Flex;

        m_lootBox.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        m_lootBox.RegisterCallback<ClickEvent>(OnLootBoxClicked);

        m_TopElement.schedule.Execute(() =>
        {
            m_lootBox.AddToClassList("loot-box-shake-middle");
            StartNextShake();
        }).StartingIn(50);
    }

    private void OnTransitionEnd(TransitionEndEvent evt)
    {
        if (!m_isShaking || evt.target != m_lootBox) return;
        StartNextShake();
    }

    private void StartNextShake()
    {
        m_lootBox.RemoveFromClassList("loot-box-shake-middle");
        m_lootBox.RemoveFromClassList("loot-box-shake-right");
        m_lootBox.RemoveFromClassList("loot-box-shake-left");

        if (m_lootBoxShakeCurrentState == "loot-box-shake-middle")
        {
            m_lootBox.AddToClassList("loot-box-shake-right");
            m_lootBoxShakeCurrentState = "loot-box-shake-right";
        }
        else if (m_lootBoxShakeCurrentState == "loot-box-shake-right")
        {
            m_lootBox.AddToClassList("loot-box-shake-left");
            m_lootBoxShakeCurrentState = "loot-box-shake-left";
        }
        else
        {
            m_lootBox.AddToClassList("loot-box-shake-middle");
            m_lootBoxShakeCurrentState = "loot-box-shake-middle";
        }
    }


    public void StopShaking()
    {
        m_isShaking = false;
        m_lootBox.RemoveFromClassList("loot-box-shake-right");
        m_lootBox.RemoveFromClassList("loot-box-shake-left");
        m_lootBox.AddToClassList("loot-box-middle");
    }

    private void OnLootBoxClicked(ClickEvent evt)
    {
        StopShaking();

        m_lootBox.RemoveFromClassList("loot-box-shake-right");
        m_lootBox.RemoveFromClassList("loot-box-shake-left");
        m_lootBox.RemoveFromClassList("loot-box-middle");

        m_lootBox.AddToClassList("loot-box-opened");

        ShowLootBoxReward();
    }

    private void ShowLootBoxReward()
    {
        m_lootBoxResultContainer.Clear();

        TemplateContainer weaponUI = m_weaponItemAsset.Instantiate();
        var weaponComponent = new WeaponItemComponent();
        weaponComponent.SetVisualElements(weaponUI, WeaponItemComponentDisplayContext.LootBoxDetails);

        m_lootBoxResultContainer.Add(weaponUI);

        m_TopElement.schedule.Execute(() =>
        {
            weaponUI.experimental.animation
                .Scale(1.1f, 300)
                .Ease(Easing.OutBack)
                .OnCompleted(() =>
                {
                    weaponUI.experimental.animation
                        .Position(new Vector2(0, -150), 400)
                        .Ease(Easing.OutCubic);
                });
        }).StartingIn(100);
    }

    private void ResetLootBoxOpenState()
    {
        m_isShaking = true;
        m_lootBox?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        m_lootBox?.UnregisterCallback<ClickEvent>(OnLootBoxClicked);

        if (m_lootBox != null)
        {
            m_lootBox.RemoveFromClassList("loot-box-shake-middle");
            m_lootBox.RemoveFromClassList("loot-box-shake-right");
            m_lootBox.RemoveFromClassList("loot-box-shake-left");
            m_lootBox.RemoveFromClassList("loot-box-opened");
            m_lootBox.AddToClassList("loot-box");
        }

        m_lootBoxOpenContainer.style.display = DisplayStyle.None;
        m_lootBoxResultContainer.Clear();

        m_lootBoxShakeCurrentState = "loot-box";
    }

}