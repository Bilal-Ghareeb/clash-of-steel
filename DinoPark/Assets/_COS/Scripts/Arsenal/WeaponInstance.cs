using Newtonsoft.Json;
using PlayFab.EconomyModels;

public class WeaponInstance
{
    public InventoryItem Item { get; }
    public WeaponData Data { get; }

    public WeaponInstance(InventoryItem item)
    {
        Item = item;

        string json = JsonConvert.SerializeObject(item.DisplayProperties);
        Data = JsonConvert.DeserializeObject<WeaponData>(json);
        
    }
}
