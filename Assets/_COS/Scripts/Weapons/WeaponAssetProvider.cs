using UnityEngine;

public class WeaponAssetProvider
{
    private static WeaponAssetDatabase _db;

    public static WeaponAssetDatabase Database
    {
        get
        {
            if (_db == null)
                _db = Resources.Load<WeaponAssetDatabase>("WeaponAssetDatabase");
            return _db;
        }
    }
}
