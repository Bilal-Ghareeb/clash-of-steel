using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Weapon Asset")]
public class WeaponAsset : ScriptableObject
{
    public string weaponId;
    public GameObject WeaponPrefab;

    [Header("Cinematic Timelines")]
    public WeaponTimelineSet Timelines;
}
