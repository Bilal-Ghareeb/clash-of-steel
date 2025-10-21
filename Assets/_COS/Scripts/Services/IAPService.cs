using PlayFab;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;

public class IAPService
{
    #region Properties
    public bool IsInitialized { get; private set; } = false;
    #endregion

    #region Fields
    private StoreController m_storeController;
    private List<ProductDefinition> m_productDefinitions;
    private List<Product> m_products;
    #endregion

    public async Task InintIAP()
    {
        try
        {
            var env = new InitializationOptions().SetEnvironmentName("production");
            await UnityServices.InitializeAsync(env);

            m_storeController = UnityIAPServices.StoreController();

            m_storeController.OnProductsFetched += OnProductsFetched;
            m_storeController.OnProductsFetchFailed += OnProductsFetchFailed;
            m_storeController.OnPurchasesFetched += OnPurchasesFetched;
            m_storeController.OnPurchasePending += OnPurchasePending;
            m_storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            m_storeController.OnPurchaseFailed += OnPurchaseFailed;
            m_storeController.OnStoreDisconnected += OnStoreDisconnected;

            await m_storeController.Connect();

            m_productDefinitions = BuildProductDefinitions();
            m_storeController.FetchProducts(m_productDefinitions);

        }
        catch(Exception ex)
        {
            Debug.LogError($"IAP initialization failed: {ex.Message}");
        }
    }

    private List<ProductDefinition> BuildProductDefinitions()
    {
        var initialProductsToFetch = new List<ProductDefinition>
        {
            new("dm_pack_1000", ProductType.Consumable),
            new("dm_pack_5000" , ProductType.Consumable),
        };
        return initialProductsToFetch;
    }

    private void OnProductsFetched(List<Product> products)
    {
        Debug.Log($"Products fetched successfully. Count: {products?.Count ?? 0}");
        m_products = products;
        m_storeController.FetchPurchases();
    }

    private void OnProductsFetchFailed(ProductFetchFailed failed)
    {
        Debug.LogError($"Products fetch failed - Reason: {failed.FailureReason}");
    }

    private void OnPurchasesFetched(Orders orders)
    {
        Debug.Log("IAP DONE INIT ! Purchases fetched successfully.");
        IsInitialized = true;
    }

    public void BuyProduct(string productID)
    {
        if(IsInitialized)
            m_storeController.PurchaseProduct(productID);
    }

    private void OnPurchasePending(PendingOrder order)
    {
        m_storeController.ConfirmPurchase(order);
    }

    private async void OnPurchaseConfirmed(Order order)
    {
        try
        {
            var wrapper = (Dictionary<string, object>)MiniJson.JsonDecode(order.Info.Receipt);
            if (null == wrapper)
            {
                return;
            }

            var store = (string)wrapper["Store"];
            var payload = (string)wrapper["Payload"];


            if (Application.platform == RuntimePlatform.Android)
            {
                var gpDetails = (Dictionary<string, object>)MiniJson.JsonDecode(payload);
                var gpJson = (string)gpDetails["json"];
                var gpSig = (string)gpDetails["signature"];

                var gpJsonDict = (Dictionary<string, object>)MiniJson.JsonDecode(gpJson);
                var productId = (string)gpJsonDict["productId"];

                await PlayFabManager.Instance.AzureService.ValidateAndGrantPurchaseAsync(productId, gpJson, gpSig);
            }

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error during parsing and creating the request: {ex.Message}");
        }
    }


    public string GetProductPrice(string productID)
    {
        if (!IsInitialized)
        {
            return null;
        }

        foreach (var product in m_products)
        {
            if (product.definition.id == productID)
            {
                return product.metadata.localizedPriceString;
            }
        }
        return string.Empty;
    }


    private void OnPurchaseFailed(FailedOrder failedOrder)
    {

    }

    private void OnStoreDisconnected(StoreConnectionFailureDescription description)
    {
        Debug.LogError($"Store disconnected: {description.message}");
    }

}
