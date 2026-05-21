using UnityEngine;

public class SlopeAlign : MonoBehaviour
{
    [Header("Настройки выравнивания")]
    [Tooltip("На каком расстоянии от низа объекта проверять землю")]
    [SerializeField] private float rayDistance = 3f;

    [Tooltip("Скорость поворота (чем больше, тем быстрее)")]
    [SerializeField] private float smoothSpeed = 10f;

    [Tooltip("Смещение луча относительно центра объекта (вниз)")]
    [SerializeField] private float rayOffset = 2f;

    [Header("Отладка")]
    [SerializeField] private bool showDebugRay = true;

    void LateUpdate()
    {
        Vector3 rayOrigin = transform.position - Vector3.up * rayOffset;

        RaycastHit hit;
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, rayDistance))
        {
            Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothSpeed * Time.deltaTime);
        }

        if (showDebugRay)
        {
            Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.red);
        }
    }
}