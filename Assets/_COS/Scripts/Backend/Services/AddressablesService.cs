using System.Threading.Tasks;
using UnityEngine.AddressableAssets;

public class AddressablesService
{
    private bool m_isInitialized;

    public bool IsInitialized => m_isInitialized;

    public async Task Init()
    {
        if (m_isInitialized)
        {
            return;
        }

        try
        {
            await Addressables.InitializeAsync().Task;
            m_isInitialized = true;
        }
        catch (System.Exception ex)
        {
        }
    }

}
