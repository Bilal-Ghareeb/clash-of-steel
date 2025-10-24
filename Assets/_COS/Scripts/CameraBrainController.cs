using UnityEngine;

public class CameraBrainController : MonoBehaviour
{
    [Header("Arsenal Camera Data")]
    [SerializeField] private Vector3 m_arsenalRotationAngles;

    [Header("Entrance Camera Data")]
    [SerializeField] private Vector3 m_entranceRotationAngles;

    [Header("Shop Camera Data")]
    [SerializeField] private Vector3 m_shopRotationAngles;

    [Header("Inspect Weapon Camera Data")]
    [SerializeField] private Vector3 m_inspectWeaponCamerPosition;
    [SerializeField] private Vector3 m_inspectWeaponRotationAngles;

    private void OnEnable()
    {
        MainTabBarEvents.ArsenalViewShown += RotateCameraTowardsArsenal;
        MainTabBarEvents.PlayScreenShown += RotateCameraTowardsEntrance;
        MainTabBarEvents.ShopViewShown += RotateCameraTowardsShop;

        InspectWeaponEvents.BackButtonClicked += RotateCameraTowardsArsenal;
        InspectWeaponEvents.ScreenEnabled += RotateCameraTowardsInspectWeapon;
    }

    private void OnDisable()
    {
        MainTabBarEvents.ArsenalViewShown -= RotateCameraTowardsArsenal;
        MainTabBarEvents.PlayScreenShown -= RotateCameraTowardsEntrance;
        MainTabBarEvents.ShopViewShown -= RotateCameraTowardsShop;


        InspectWeaponEvents.BackButtonClicked -= RotateCameraTowardsArsenal;
        InspectWeaponEvents.ScreenEnabled -= RotateCameraTowardsInspectWeapon;
    }

    private void RotateCameraTowardsArsenal()
    {

        LeanTween.rotate(gameObject, m_arsenalRotationAngles, 0.5f)
        .setEaseOutQuad()
        .setOnComplete(() =>
        {
            LeanTween.move(gameObject, Vector3.up, 0.5f)
            .setEaseOutQuad();
        });

    }

    private void RotateCameraTowardsEntrance()
    {
        LeanTween.rotate(gameObject, m_entranceRotationAngles, 0.5f)
            .setEaseOutQuad();
    }

    private void RotateCameraTowardsShop()
    {
        LeanTween.rotate(gameObject, m_shopRotationAngles, 0.5f)
            .setEaseOutQuad();
    }

    private void RotateCameraTowardsInspectWeapon()
    {
        LeanTween.move(gameObject, m_inspectWeaponCamerPosition, 0.5f)
        .setEaseOutQuad().setOnComplete(() =>
        {
            LeanTween.rotate(gameObject, m_inspectWeaponRotationAngles, 0.5f)
            .setEaseOutQuad();
        });
    }

}
