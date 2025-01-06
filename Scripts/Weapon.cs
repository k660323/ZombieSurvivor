using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type
    {
        Melee,
        Range,
        SubRange,
        Bow,
        Throw
    };
    [Header("무기 능력치")]
    public Type type;
    public float damage;
    public int curAmmo;
    public int maxAmmo;
    public float rate;
    public int ammoLevel = 0;
    public int damageLevel = 0;
    public int rateLevel = 0;
    public bool isAnim;
    public float upgradeRate;
    public float upgradeDamage;
    [Header("근접")]
    public BoxCollider meleeArea;
    [Header("원거리")]
    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;
    [Header("이펙트")]
    public TrailRenderer trailEffect;
    [Header("사운드")]
    public AudioClip[] clip;
    public AudioClip[] SwingClips;

    IEnumerator coroutine;

    public void Cancel()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);

            StopCoroutine("disAble");
            StartCoroutine("disAble");
        }
    }

    IEnumerator disAble()
    {
        isAnim = false;
        if (meleeArea != null)
            meleeArea.enabled = false;
        if (trailEffect != null)
        {
            yield return new WaitForSeconds(0.2f);
            trailEffect.enabled = false;
        }
    }

    public void Use()
    {
        if (Type.Melee == type)
        {
            coroutine = Swing();
            StopCoroutine(coroutine);
            StartCoroutine(coroutine);
        }
        else if ((Type.Range == type || Type.Bow == type || Type.SubRange == type) && curAmmo > 0)
        {
            coroutine = Shot();
            StopCoroutine(coroutine);
            StartCoroutine(coroutine);
            curAmmo--;
        }
        else if (Type.Throw == type)
        {
            coroutine = Throw();
            StopCoroutine(coroutine);
            StartCoroutine(coroutine);
        }
    }

    IEnumerator Swing()
    {
        SoundManager.instance.SFXPlay(SwingClips[Random.Range(0,SwingClips.Length)], "Swing");
        isAnim = true;
        if (trailEffect != null)
            trailEffect.enabled = true;
        yield return new WaitForSeconds(((1f / rate) * (0.32f)));
        meleeArea.enabled = true;
        yield return new WaitForSeconds(((1f / rate) * (0.7f)) - ((1f / rate) * (0.32f)));
        meleeArea.enabled = false;
        yield return new WaitForSeconds(((1f / rate) * (0.9f)) - ((1f / rate) * (0.7f))); 
        isAnim = false;
        if (trailEffect != null)
            trailEffect.enabled = false;
    }


    IEnumerator Shot()
    {
        isAnim = true;
        if (Type.Range == type || Type.SubRange == type)
        {
            SoundManager.instance.SFXPlay(clip[0], "GunShot");

            GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            instantBullet.GetComponent<Bullet>().damage = damage;
            Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
            bulletRigid.velocity = bulletPos.forward * 50;
            yield return null;

            GameObject instantBulletCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
            Rigidbody bulletCasetRigid = instantBulletCase.GetComponent<Rigidbody>();
            Vector3 caseVec = bulletCasePos.forward * Random.Range(-2, -3) + Vector3.up * Random.Range(2, 3);
            bulletCasetRigid.AddForce(caseVec, ForceMode.Impulse);
            bulletCasetRigid.AddTorque(Vector3.up, ForceMode.Impulse);
            Destroy(instantBulletCase, 5.0f);
        }
        else if (Type.Bow == type)
        {
            SoundManager.instance.SFXPlay(clip[0], "BowStringPull");
            yield return new WaitForSeconds(((1f / rate) * (0.5f))); 
            GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
            instantBullet.GetComponent<Bullet>().damage = damage;
            Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
            bulletRigid.velocity = bulletPos.forward * 50;
            SoundManager.instance.SFXPlay(clip[1], "Bow");
        }
        isAnim = false;
    }

    IEnumerator Throw()
    {
        SoundManager.instance.SFXPlay(clip[0], "Throw");
        isAnim = true;
        yield return new WaitForSeconds(((1f / rate) * (0.7f)));
        GameObject InstantThrow = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        InstantThrow.GetComponent<Boom>().damage = damage;
        Rigidbody throwRigid = InstantThrow.GetComponent<Rigidbody>();
        throwRigid.velocity = bulletPos.forward * 5f;
        yield return new WaitForSeconds((1f / rate) - ((1f / rate) * (0.7f)));
        isAnim = false;
    }
}