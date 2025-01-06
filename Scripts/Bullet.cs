using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Rigidbody rb;
    public float damage;
    public int Penetrate;

    void Start()
    {
       Destroy(gameObject, 5.0f);
    }

    //자신 리지드바디에 따른 호출
    void OnTriggerEnter(Collider other)
    {
        if (other.transform.CompareTag("Wall") || other.transform.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("충돌");
            Destroy(gameObject);
        }
    }
}
