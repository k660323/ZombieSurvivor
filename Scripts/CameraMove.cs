using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform target;
    public Vector3 offSet;
    public float trackSpeed;

    void Awake()
    {
        transform.position = target.position + offSet;
    }

    void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, target.position + offSet, trackSpeed * Time.deltaTime);
    }
}
