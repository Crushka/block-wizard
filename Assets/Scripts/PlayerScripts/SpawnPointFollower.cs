using UnityEngine;

public class SpawnPointFollower : MonoBehaviour
{
    [SerializeField] private Transform wandTip;
    [SerializeField] private Camera aimCamera;

    void LateUpdate()
    {
        transform.position = wandTip.position;
        transform.rotation = aimCamera.transform.rotation;
    }


}