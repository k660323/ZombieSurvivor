using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class STZombieG : MonoBehaviour
{
    public float health;
    bool isDead;
    SkinnedMeshRenderer sMesh;
    Animator anim;
    Rigidbody rb;
    public Weapon equitWeapon;
    public AudioClip[] audioClip;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        sMesh = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {     
        StartCoroutine("Attack");
    }

    private void FixedUpdate()
    {
        FreezeRotation();
    }

    void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
    }

    IEnumerator Attack()
    {
        yield return new WaitForSeconds(0.2f);
        anim.SetTrigger("doThrow");
        yield return new WaitForSeconds(0.7f);
        SoundManager.instance.SFXPlay(audioClip[0], "ThrowKnife");
        GameObject throwWeapon = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        throwWeapon.GetComponent<Bullet>().damage = equitWeapon.damage;
        throwWeapon.GetComponent<Rigidbody>().AddForce(transform.forward * 25f, ForceMode.Impulse);
        yield return new WaitForSeconds(3f);
        if (health > 0)
        {
            health = 0;
            StartCoroutine(OnDamage(Vector3.zero));
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            health -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));

            if (bullet.Penetrate == 0)
                Destroy(other.gameObject);
            else
                bullet.Penetrate--;
        }
        else if (other.CompareTag("Melee"))
        {
            Weapon weapon = other.GetComponent<Weapon>();
            SoundManager.instance.SFXPlay(weapon.clip[Random.Range(0, 5)], "axeAttack");
            health -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));
        }
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        sMesh.sharedMaterial.color = Color.red;

        if (health > 0)
        {
            yield return new WaitForSeconds(0.1f);
            sMesh.sharedMaterial.color = Color.white;
        }
        else
        {
            if (gameObject.layer != 13)
            {
                gameObject.layer = 13;
                StartCoroutine("OnDie", reactVec);
            }
        }
    }

    IEnumerator OnDie(Vector3 reactVec)
    {
        isDead = true;
        sMesh.sharedMaterial.color = Color.gray;

        anim.SetInteger("dieMotion", Random.Range(0, 2));
        anim.SetTrigger("doDie");
        rb.velocity = Vector3.zero;
        reactVec = reactVec.normalized;
        reactVec += Vector3.up;
        rb.AddForce(reactVec * 5, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);
        Destroy(gameObject, 4.0f);
    }
}
