using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Transform target;
    public float followSpeed;
    public Vector3 offset;

	// Use this for initialization
	void Start ()
    {
        offset = transform.position - target.position;
	}
	
	// Update is called once per frame
	void LateUpdate ()
    {
        if (target != null)
        {
            Follow();
        }
	}

    void Follow()
    {
        Vector3 targetPos = target.position + offset;

        Vector3 lerpPos = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);

        transform.position = lerpPos;
    }
}
