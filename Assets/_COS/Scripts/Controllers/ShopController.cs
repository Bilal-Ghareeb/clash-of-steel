using PlayFab.EconomyModels;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] private SoundData m_shopEntranceSound;
    [SerializeField] private SoundData m_purchaseConfirmedSound;
    [SerializeField] private SoundData m_boxShakingSound;
    [SerializeField] private SoundData m_openChestSound;

    private ShopView m_view;
    private CatalogItem m_currentLootBoxReward;

    public void Setup(ShopView view)
    {
        m_view = view;
    }

    private void OnEnable()
    {
        ShopEvents.ScreenEnabled += HandleScreenEnabled;
        ShopEvents.DiamondPurchased += HandleDiamondPurchased;
        ShopEvents.LootBoxPurchaseIntiated += HandlePurchaseLootBox;
        ShopEvents.LootBoxDeatailsInspected += HandleShowLootBoxDetails;
        ShopEvents.LootBoxClicked += HandleLootBoxClicked;
        ShopEvents.LootBoxRewardClaimed += HandleRewardClaimed;
    }

    private void OnDisable()
    {
        ShopEvents.ScreenEnabled -= HandleScreenEnabled;
        ShopEvents.DiamondPurchased -= HandleDiamondPurchased;
        ShopEvents.LootBoxPurchaseIntiated -= HandlePurchaseLootBox;
        ShopEvents.LootBoxDeatailsInspected -= HandleShowLootBoxDetails;
        ShopEvents.LootBoxClicked -= HandleLootBoxClicked;
        ShopEvents.LootBoxRewardClaimed -= HandleRewardClaimed;
    }

    private void HandleScreenEnabled()
    {
        AudioManager.Instance.PlaySFX(m_shopEntranceSound);
        PopulateShopData();
    }

    public void PopulateShopData()
    {
        var bundles = PlayFabManager.Instance.EconomyService.DiamondBundlesCatalog;
        var lootBoxes = PlayFabManager.Instance.EconomyService.LootBoxes;
        m_view.PopulateDiamondBundles(bundles);
        m_view.PopulateLootBoxes(lootBoxes);
    }

    private void HandleDiamondPurchased(string productID)
    {
        PlayFabManager.Instance.IAPService.BuyProduct(productID);
    }

    private async void HandlePurchaseLootBox(LootBoxData lootBox)
    {
        m_currentLootBoxReward = await PlayFabManager.Instance.AzureService.GrantLootBoxRewardAsync(lootBox);
        m_view.SetupAndShowOpenLootBoxContainer();
    }

    private void HandleShowLootBoxDetails(LootBoxData lootBox)
    {
        var weaponEntries = PlayFabManager.Instance.EconomyService.GetWeaponsCatalogItemsInLootBox(lootBox.id);
        m_view.PopulateLootBoxDetailsPanel(weaponEntries, lootBox.id);
        m_view.ShowLootBoxDetailsContainer();
    }

    private void HandleLootBoxClicked()
    {
        m_view.OpenLootBox(m_currentLootBoxReward);
    }

    private void HandleRewardClaimed()
    {
        m_currentLootBoxReward = null;
    }
}
