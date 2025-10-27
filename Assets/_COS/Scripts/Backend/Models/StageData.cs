using System.Collections.Generic;

[System.Serializable]
public class StageList
{
    public List<StageData> stages;
}

[System.Serializable]
public class StageData
{
    public int id;
    public string name;
    public List<StageEnemyData> enemies;
    public StageRewardData rewards;
}

[System.Serializable]
public class StageEnemyData
{
    public string weaponId;
    public int level;
}

[System.Serializable]
public class StageRewardData
{
    public int GD;
}

