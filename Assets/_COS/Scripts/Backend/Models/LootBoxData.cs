using System;
using System.Collections.Generic;

[Serializable]
public class LootBoxData
{
    public string id;
    public List<LootBoxReward> rewards;
    public int target;
    public int cost;
}

[System.Serializable]
public class LootBoxReward
{
    public string id;  
    public int weight;    
}
