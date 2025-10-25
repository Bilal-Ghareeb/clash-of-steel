using UnityEngine;

public class GemPurchaseMagnet : MonoBehaviour
{
    [SerializeField] private GameObject m_gemBurstPrefab;
    [SerializeField] private SoundData m_purchaseConfirmedSound;
    private ParticleSystem m_particleSystem;

    private void OnEnable()
    {
        ShopEvents.DiamondPurchased += PlayGemPurchaseEffect;
        m_particleSystem = m_gemBurstPrefab.GetComponent<ParticleSystem>();
    }

    private void OnDisable()
    {
        ShopEvents.DiamondPurchased -= PlayGemPurchaseEffect;
    }

    [ContextMenu("VFX")]
    public void PlayGemPurchaseEffect()
    {
        m_particleSystem.Play();
        AudioManager.Instance.PlaySFX(m_purchaseConfirmedSound);
    }

}
