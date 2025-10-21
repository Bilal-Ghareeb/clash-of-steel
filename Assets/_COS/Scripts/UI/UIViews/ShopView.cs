using UnityEngine;
using UnityEngine.UIElements;

public class ShopView : UIView
{
    private VisualElement m_diamondBundlesContainer;
    private VisualTreeAsset m_shopItemAsset;

    public ShopView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        m_shopItemAsset = Resources.Load<VisualTreeAsset>("ShopItem");
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_diamondBundlesContainer = m_TopElement.Q<VisualElement>("diamond-bundles-container");
    }

    public override void Show()
    {
        base.Show();
        PopulateDiamondBundles();
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
            else
            {
                Debug.LogError("IAPService is null!");
                shopItem.SetPrice("Price not available");
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
}