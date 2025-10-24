using System.Collections.Generic;

public static class LocalWeaponProgressionCache
{
    private static readonly Dictionary<string, int> _localLevels = new();

    public static void SetLocalLevel(string weaponId, int level)
    {
        _localLevels[weaponId] = level;
    }

    public static bool TryGetLocalLevel(string weaponId, out int level)
    {
        return _localLevels.TryGetValue(weaponId, out level);
    }

    public static void ClearLocalLevel(string weaponId)
    {
        _localLevels.Remove(weaponId);
    }

    public static void ClearAll()
    {
        _localLevels.Clear();
    }
}
