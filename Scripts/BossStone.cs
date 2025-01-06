using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStone : Bullet
{
    float angularPower = 2;
    float scalueValue = 0.1f;
    bool isShoot;
    public SphereCollider sphereCollider;
    void Start()
    {
        StartCoroutine(GainPowerTimer());
        StartCoroutine(GainPower());
    }

    IEnumerator GainPowerTimer()
    {
        yield return new WaitForSeconds(2.2f);
        rb.useGravity = true;
        rb.AddForce(transform.forward * -50, ForceMode.Impulse);
        isShoot = true;
        sphereCollider.enabled = true;
        Destroy(gameObject, 7f);
    }

    IEnumerator GainPower()
    {
        while (!isShoot)
        {
            angularPower += 100f;
            scalueValue += 0.0014f;
            transform.localScale = Vector3.one * scalueValue;
            rb.AddTorque(transform.right * angularPower, ForceMode.Acceleration);
            yield return null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Destroy(gameObject, 7f);
        }
    }
}
