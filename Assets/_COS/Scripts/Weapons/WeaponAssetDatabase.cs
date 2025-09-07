using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Asset Database")]
public class WeaponAssetDatabase : ScriptableObject
{
    public WeaponAsset[] assets;

    public WeaponAsset GetAssetFor(string weaponID)
    {
        return Array.Find(assets , a=>a.weaponId == weaponID);
    }
}
