using UnityEngine;

public class CameraBrainController : MonoBehaviour
{
    [SerializeField] private Vector3 m_arsenalRotationAngles;
    [SerializeField] private Vector3 m_entranceRotationAngles;

    private void OnEnable()
    {
        MainTabBarEvents.ArsenalViewShown += RotateCameraTowardsArsenal;
        MainTabBarEvents.PlayScreenShown += RotateCameraTowardsEntrance;
    }

    private void OnDisable()
    {
        MainTabBarEvents.ArsenalViewShown -= RotateCameraTowardsArsenal;
        MainTabBarEvents.PlayScreenShown -= RotateCameraTowardsEntrance;
    }

    private void RotateCameraTowardsArsenal()
    {
        LeanTween.rotate(gameObject, m_arsenalRotationAngles, 0.5f)
            .setEaseOutQuad();
    }

    private void RotateCameraTowardsEntrance()
    {
        LeanTween.rotate(gameObject, m_entranceRotationAngles, 0.5f)
            .setEaseOutQuad();
    }
}
