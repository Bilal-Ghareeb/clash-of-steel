using System;
using UnityEngine.UIElements;

public class ShopItemComponent 
{
    public VisualElement Root { get; private set; }

    private VisualElement m_shopItemButton;
    private Label m_shopItemPrice;
    private Label m_shopItemAmount;

    public Action OnPurchaseClicked;


    public void SetVisualElements(TemplateContainer shopItemUXMLTemplate)
    {
        if (shopItemUXMLTemplate == null) return;

        Root = shopItemUXMLTemplate;
        m_shopItemButton = Root.Q<Button>("shop-item");
        m_shopItemPrice = Root.Q<Label>("price-label");
        m_shopItemAmount = Root.Q<Label>("amount-label");
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

    public void RegisterButtonCallbacks()
    {
        m_shopItemButton?.RegisterCallback<ClickEvent>(OnShopItemClicked);
    }

    private void OnShopItemClicked(ClickEvent evt)
    {
        OnPurchaseClicked?.Invoke();
    }
}
