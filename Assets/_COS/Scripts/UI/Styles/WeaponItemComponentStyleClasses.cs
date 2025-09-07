using System.Collections.Generic;
using UnityEngine;

public class WeaponItemComponentStyleClasses : MonoBehaviour
{
    private static readonly Dictionary<string, string> RarityClasses = new()
    {
        { "Common", "weapon-scroll-item-common" },
        { "Rare", "weapon-scroll-item-epic" },
        { "Legendary", "weapon-scroll-item-legendary" }
    };

    private static readonly Dictionary<string, string> ClassTypeClasses = new()
    {
        { "Sword", "class-icon-sword" },
        { "Shield", "class-icon-shield" },
        { "Hammer", "class-icon-hammer" }
    };

    public static string GetRarityClass(string rarity) =>
    RarityClasses.TryGetValue(rarity, out var cls) ? cls : string.Empty;

    public static string GetClassTypeClass(string className) =>
        ClassTypeClasses.TryGetValue(className, out var cls) ? cls : string.Empty;
}
