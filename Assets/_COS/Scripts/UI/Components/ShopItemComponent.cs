using System;
using UnityEngine;
using UnityEngine.UIElements;

public enum ShopItemType
{
    DiamondBundle,
    LootBox
}

public class ShopItemComponent 
{
    public VisualElement Root { get; private set; }

    private Button m_shopItemButton;
    private Button m_detailsButton;
    private VisualElement m_shopItemImage;
    private Label m_shopItemPrice;
    private Label m_shopItemAmount;
    private VisualElement m_diamondPriceIcon;

    private ShopItemType m_itemType;

    public Action OnPurchaseClicked;
    public Action OnDetailsClicked;


    public void SetVisualElements(TemplateContainer shopItemUXMLTemplate)
    {
        if (shopItemUXMLTemplate == null) return;

        Root = shopItemUXMLTemplate;
        m_shopItemButton = Root.Q<Button>("shop-item");
        m_detailsButton = Root.Q<Button>("details_btn");
        m_shopItemPrice = Root.Q<Label>("price-label");
        m_shopItemAmount = Root.Q<Label>("amount-label");
        m_diamondPriceIcon = Root.Q<VisualElement>("diamond-icon");
        m_shopItemImage = Root.Q<VisualElement>("shop-item-image");
    }

    public void Configure(ShopItemType itemType)
    {
        m_itemType = itemType;

        switch (m_itemType)
        {
            case ShopItemType.DiamondBundle:
                m_shopItemAmount.style.display = DisplayStyle.Flex;
                m_diamondPriceIcon.style.display = DisplayStyle.None;
                m_detailsButton.style.display = DisplayStyle.None;
                m_shopItemButton.AddToClassList("shop-item-diamond");
                SetItemImage("UI/shop_diamond_icon");
                break;

            case ShopItemType.LootBox:
                m_shopItemAmount.style.display = DisplayStyle.None;
                m_diamondPriceIcon.style.display = DisplayStyle.Flex;
                m_detailsButton.style.display = DisplayStyle.Flex;
                m_shopItemButton.AddToClassList("shop-item-box");
                SetItemImage("UI/loot_box_icon");
                break;
        }
    }

    public void SetPrice(string priceText)
    {
        if (m_shopItemPrice != null)
            m_shopItemPrice.text = priceText;
    }

    public void SetAmount(int amountText)
    {
        if (m_shopItemAmount != null)
            m_shopItemAmount.text = amountText.ToString();
    }

    private void SetItemImage(string resourcePath)
    {
        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture != null && m_shopItemImage != null)
        {
            m_shopItemImage.style.backgroundImage = new StyleBackground(texture);
        }
        else
        {
            Debug.LogWarning($"ShopItem image not found at: {resourcePath}");
        }
    }


    public void RegisterButtonCallbacks()
    {
        m_shopItemButton?.RegisterCallback<ClickEvent>(OnShopItemClicked);
        m_detailsButton?.RegisterCallback<ClickEvent>(OnDetailsButtonClicked);
    }

    private void OnShopItemClicked(ClickEvent evt)
    {
        OnPurchaseClicked?.Invoke();
    }

    private void OnDetailsButtonClicked(ClickEvent evt)
    {
        OnDetailsClicked?.Invoke();
    }
}
