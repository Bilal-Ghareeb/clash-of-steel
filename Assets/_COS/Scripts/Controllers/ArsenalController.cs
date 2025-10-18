using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ArsenalController : MonoBehaviour
{
    private void OnEnable()
    {
        ArsenalEvents.ScreenEnabled += OnArsenalScreenEnabled;
        ArsenalEvents.GearFiltered += OnGearFiltered;
    }

    private void OnDisable()
    {
        ArsenalEvents.ScreenEnabled -= OnArsenalScreenEnabled;
        ArsenalEvents.GearFiltered -= OnGearFiltered;
    }

    private void Awake()
    {
    }

    private void OnArsenalScreenEnabled()
    {
        UpdateInventory(PlayFabManager.Instance.EconomyService.PlayerWeapons);
    }

    private void OnGearFiltered(Rarity rarity, WeaponType weaponType)
    {
        IReadOnlyList<WeaponInstance> filteredGear = new List<WeaponInstance>();

        filteredGear = FilterGearList(PlayFabManager.Instance.EconomyService.PlayerWeapons, rarity, weaponType);

        UpdateInventory(filteredGear);
    }

    private IReadOnlyList<WeaponInstance> FilterGearList(IReadOnlyList<WeaponInstance> weaponList, Rarity rarity, WeaponType gearType)
    {
        IReadOnlyList<WeaponInstance> filteredWeaponList = weaponList;

        if (rarity != Rarity.All)
        {
            filteredWeaponList = filteredWeaponList.Where(x => x.CatalogData.rarity == rarity.ToString()).ToList();
        }

        if (gearType != WeaponType.All)
        {
            filteredWeaponList = filteredWeaponList.Where(x => x.CatalogData.@class == gearType.ToString()).ToList();
        }

        return filteredWeaponList;
    }

    private void UpdateInventory(IReadOnlyList<WeaponInstance> gearToShow)
    {
        ArsenalEvents.ArsenalUpdated?.Invoke(gearToShow);
    }
}
