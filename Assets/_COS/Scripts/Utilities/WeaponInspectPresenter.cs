using UnityEngine;

public class WeaponInspectPresenter : MonoBehaviour
{
    [SerializeField] private Transform m_inspectSpawnPoint;
    private GameObject m_currentWeaponModel;

    public void ShowWeapon(WeaponInstance instance)
    {
        ClearCurrent();

        if (instance.Asset.WeaponPrefab != null)
        {
            m_currentWeaponModel = Instantiate(
                instance.Asset.WeaponPrefab,
                m_inspectSpawnPoint.position,
                new Quaternion(0,0,0,0),
                m_inspectSpawnPoint
            );

            m_currentWeaponModel.AddComponent<InspectRotator>();
        }
    }

    public void ClearCurrent()
    {
        if (m_currentWeaponModel != null)
        {
            Destroy(m_currentWeaponModel);
        }
    }
}
