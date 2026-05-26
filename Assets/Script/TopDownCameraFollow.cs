using UnityEngine;

public class TopDownCameraFollow : MonoBehaviour
{
    [SerializeField] Vector3 offset = new Vector3(0f, 10f, -8f);
    [SerializeField] float followSpeed = 10f;

    private Transform target;
    public void SetTarget(Transform newTarget)
    {   
        target = newTarget;
    }
    private void LateUpdate()
    {
        if(target == null) {return; }
        Vector3 desiredPosition = target.position + offset;
    transform.position =
    Vector3.Lerp(transform.position, desiredPosition, followSpeed *
    Time.deltaTime);
    transform.LookAt(transform.position);
    }
}