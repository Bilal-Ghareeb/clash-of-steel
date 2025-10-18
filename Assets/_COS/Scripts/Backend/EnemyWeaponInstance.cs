using System.Linq;

public class EnemyWeaponInstance : WeaponInstanceBase
{
    public string EnemyId { get; private set; }

    public EnemyWeaponInstance(string enemyId, WeaponData catalogData, int level)
    {
        EnemyId = enemyId;
        Initialize(catalogData, level);

        var catalogItem = PlayFabManager.Instance.EconomyService.GetCatalogItemByFriendlyId(enemyId);
        IconUrl = catalogItem?.Images?.FirstOrDefault()?.Url;
    }
}
