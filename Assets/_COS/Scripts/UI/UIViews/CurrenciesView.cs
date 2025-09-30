using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CurrenciesView : UIView
{
    private VisualElement m_goldCurrencyButton;
    private Label m_playerGoldCountLabel;

    public CurrenciesView(VisualElement topElement, bool hideOnAwake = true) : base(topElement, hideOnAwake)
    {
        PlayFabManager.Instance.OnCurrenciesUpdated += UpdateCurrencies;

        UpdateCurrencies(PlayFabManager.Instance.PlayerCurrencies);
    }

    public override void Show()
    {
        base.Show();
    }

    public override void Dispose()
    {
        base.Dispose();
        PlayFabManager.Instance.OnCurrenciesUpdated -= UpdateCurrencies;
    }

    protected override void SetVisualElements()
    {
        base.SetVisualElements();
        m_goldCurrencyButton = m_TopElement.Q<VisualElement>("Gold-Btn");
        m_playerGoldCountLabel = m_TopElement.Q<Label>("Gold-Count");
    }

    protected override void RegisterButtonCallbacks()
    {
        m_goldCurrencyButton.RegisterCallback<ClickEvent>(GoToShop);
    }

    protected void UnregisterButtonCallbacks()
    {
        m_goldCurrencyButton.UnregisterCallback<ClickEvent>(GoToShop);
    }


    private void UpdateCurrencies(IReadOnlyDictionary<string, int> currencies)
    {
        if (m_playerGoldCountLabel == null) return;

        currencies.TryGetValue("GD", out var gold);
        m_playerGoldCountLabel.text = gold.ToString();
    }

    private void GoToShop(ClickEvent evt)
    {
        Debug.Log("Go To Shop !!");
    }
}
