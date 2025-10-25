using PlayFab.EconomyModels;
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

    private Label m_lootBoxName;
    private Label m_lootBoxOpenPrompt;
    private ScrollView m_lootBoxContentScrollView;

    private VisualTreeAsset m_shopItemAsset;
    private VisualTreeAsset m_weaponItemAsset;

    private Button m_closeDetailsPanelButton;
    private Button m_claimLootBoxRewardButton;

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
        m_lootBoxName = m_TopElement.Q<Label>("loot-box-title");
        m_lootBoxOpenPrompt = m_TopElement.Q<Label>("loot-box-open-title");
        m_lootBoxContentScrollView = m_TopElement.Q<ScrollView>("loot-box-content-scrollview");
        m_closeDetailsPanelButton = m_TopElement.Q<Button>("close-details-btn");
        m_claimLootBoxRewardButton = m_TopElement.Q<Button>("claim-reward-btn");
    }

    public override void Show()
    {
        base.Show();
        ShopEvents.ScreenEnabled?.Invoke();
        m_lootBoxesContentContainer.style.display = DisplayStyle.None;
    }

    public override void Hide()
    {
        base.Hide();
    }

    public override void Dispose()
    {
        ResetLootBoxOpenState();
        UnRegisterButtonCallbacls();
    }

    protected override void RegisterButtonCallbacks()
    {
        m_closeDetailsPanelButton.RegisterCallback<ClickEvent>(CloseDetailsPanel);
        m_claimLootBoxRewardButton.RegisterCallback<ClickEvent>(HandleAfterClaimReward);
    }

    private void UnRegisterButtonCallbacls()
    {
        m_closeDetailsPanelButton.UnregisterCallback<ClickEvent>(CloseDetailsPanel);
        m_claimLootBoxRewardButton.UnregisterCallback<ClickEvent>(HandleAfterClaimReward);
    }

    public void PopulateDiamondBundles(IReadOnlyList<DiamondBundleData> bundles)
    {
        m_diamondBundlesContainer.Clear();
        foreach (var bundle in bundles)
        {
            TemplateContainer bundleUI = m_shopItemAsset.Instantiate();
            var shopItem = new ShopItemComponent();
            shopItem.SetVisualElements(bundleUI);
            shopItem.Configure(ShopItemType.DiamondBundle);
            shopItem.SetAmount(bundle.Amount);

            string price = PlayFabManager.Instance.IAPService?.GetProductPrice(bundle.MarketplaceId) ?? "";
            shopItem.SetPrice(price);

            shopItem.OnPurchaseClicked = () => ShopEvents.DiamondPurchased?.Invoke(bundle.MarketplaceId);
            shopItem.RegisterButtonCallbacks();
            m_diamondBundlesContainer.Add(bundleUI);
        }
    }

    public void PopulateLootBoxes(IReadOnlyList<LootBoxData> lootBoxes)
    {
        m_lootboxesContainer.Clear();

        foreach (var lootBox in lootBoxes)
        {
            TemplateContainer lootBoxUI = m_shopItemAsset.Instantiate();
            var shopItem = new ShopItemComponent();
            shopItem.SetVisualElements(lootBoxUI);

            shopItem.Configure(ShopItemType.LootBox);
            shopItem.SetPrice(lootBox.cost.ToString());

            shopItem.EnableButton();
            shopItem.OnPurchaseClicked = () => ShopEvents.LootBoxPurchaseIntiated?.Invoke(lootBox);
            
            shopItem.OnDetailsClicked = () => ShopEvents.LootBoxDeatailsInspected?.Invoke(lootBox);

            m_lootboxesContainer.Add(lootBoxUI);
            shopItem.RegisterButtonCallbacks();
        }
    }

    public void PopulateLootBoxDetailsPanel(List<LootBoxWeaponEntry> weaponEntries , string lootBoxName)
    {
        m_lootBoxName.text = lootBoxName;
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

    public void SetupAndShowOpenLootBoxContainer()
    {
        m_lootBoxOpenContainer.style.display = DisplayStyle.Flex;
        ShopEvents.LootBoxPurchased?.Invoke();

        m_lootBox.RegisterCallback<TransitionEndEvent>(OnTransitionEnd);
        m_lootBox.RegisterCallback<ClickEvent>(evt => ShopEvents.LootBoxClicked?.Invoke());

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

    public void OpenLootBox(CatalogItem reward)
    {
        StopShaking();

        m_lootBox.RemoveFromClassList("loot-box-shake-right");
        m_lootBox.RemoveFromClassList("loot-box-shake-left");
        m_lootBox.RemoveFromClassList("loot-box-middle");
        m_lootBox.AddToClassList("loot-box-opened");
        m_claimLootBoxRewardButton.style.display = DisplayStyle.Flex;
        m_lootBoxOpenPrompt.style.display = DisplayStyle.None;

        ShowLootBoxReward(reward);
    }

    private void ShowLootBoxReward(CatalogItem reward)
    {
        m_lootBoxResultContainer.Clear();

        TemplateContainer weaponUI = m_weaponItemAsset.Instantiate();
        var weaponComponent = new WeaponItemComponent();

        weaponComponent.SetVisualElements(weaponUI, WeaponItemComponentDisplayContext.LootBoxDetails);

        weaponComponent.SetGameData(new CatalogWeaponInstance(reward));

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

    public void ResetLootBoxOpenState()
    {
        m_isShaking = true;
        m_lootBox?.UnregisterCallback<TransitionEndEvent>(OnTransitionEnd);
        m_lootBox?.UnregisterCallback<ClickEvent>(evt => ShopEvents.LootBoxClicked?.Invoke());

        if (m_lootBox != null)
        {
            m_lootBox.RemoveFromClassList("loot-box-shake-middle");
            m_lootBox.RemoveFromClassList("loot-box-shake-right");
            m_lootBox.RemoveFromClassList("loot-box-shake-left");
            m_lootBox.RemoveFromClassList("loot-box-opened");
            m_lootBox.AddToClassList("loot-box");
        }
        m_claimLootBoxRewardButton.style.display = DisplayStyle.None;
        m_lootBoxOpenPrompt.style.display = DisplayStyle.Flex;
        m_lootBoxOpenContainer.style.display = DisplayStyle.None;
        m_lootBoxResultContainer.Clear();

        m_lootBoxShakeCurrentState = "loot-box";
    }

    public void ShowLootBoxDetailsContainer()
    {
        m_lootBoxesContentContainer.style.display = DisplayStyle.Flex;
        m_lootBoxesContentContainer.transform.scale = new Vector3(0.1f, 0.1f, 0.1f);
        m_lootBoxesContentContainer.experimental.animation.Scale(1f, 200);
    }

    private void CloseDetailsPanel(ClickEvent evt)
    {
        m_lootBoxesContentContainer.style.display = DisplayStyle.None;
    }

    private void HandleAfterClaimReward(ClickEvent evt)
    {
        ResetLootBoxOpenState();
        ShopEvents.LootBoxRewardClaimed?.Invoke();
    }

}