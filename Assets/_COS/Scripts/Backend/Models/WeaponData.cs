using System.Collections.Generic;

[System.Serializable]
public class WeaponData
{
    public string @class;
    public Dictionary<string, string> name;
    public Dictionary<string, string> description;
    public int baseDamage;
    public int baseHealth;
    public int level;
    public string rarity;
    public string progressionId;

    public string GetLocalizedName(string langCode = "en")
    {
        return name != null && name.TryGetValue(langCode, out var value) ? value : name?["en"];
    }

    public string GetLocalizedDescription(string langCode = "en")
    {
        return description != null && description.TryGetValue(langCode, out var value) ? value : description?["en"];
    }
}
