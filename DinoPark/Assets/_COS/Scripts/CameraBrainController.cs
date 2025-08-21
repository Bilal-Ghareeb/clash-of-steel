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
        transform.rotation = Quaternion.Euler(m_arsenalRotationAngles);
    }

    private void RotateCameraTowardsEntrance()
    {
        transform.rotation = Quaternion.Euler(m_entranceRotationAngles);
    }

}
