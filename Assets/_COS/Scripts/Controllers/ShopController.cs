using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] private SoundData m_shopEntranceSound;

    private void OnEnable()
    {
        ShopEvents.ScreenEnabled += OnShopScreenEnabled;
    }

    private void OnDisable()
    {
        ShopEvents.ScreenEnabled -= OnShopScreenEnabled;
    }


    private void OnShopScreenEnabled()
    {
        AudioManager.Instance.PlaySFX(m_shopEntranceSound);
    }
}
