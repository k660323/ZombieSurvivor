using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieController : MonoBehaviour
{
    public enum Type
    {
        A,
        B,
        C,
        D,
        E,
    }
    public Type enemyType;
    public Transform target;
    public int score;
    public float health;
    //public float damage;
    public GameObject[] dropItem;
    protected Rigidbody rb;
    protected NavMeshAgent nav;
    protected Animator anim;
    public Weapon equitWeapon;
    protected Material mat;
    protected Color originColor;
    public GameManager manager;
    public bool isChase;
    public bool isAttack;
    public bool isDead;
    public float hitUp = 1;
    Vector3 dir;

    [SerializeField]
    AudioClip[] audioClips;
    [SerializeField]
    AudioClip DieClip;
    [SerializeField]
    AudioSource audioSource;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        originColor = mat.color;
        Invoke("ChaseStart", 1.0f);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true); 
    }

    // Update is called once per frame
    void Update()
    {
        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }  

    void FixedUpdate()
    {
        Targeting();
        FreezeRotation();
    }

    protected void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
    }

    void Targeting() // 공격 감지 범위 
    {
        if (!isDead)
        {
            float targerRadius = 0f; // 두께
            float targetRange = 0f; // 길이

            switch (enemyType)
            {
                case Type.A: // 일반 좀비
                    targerRadius = 1f;
                    targetRange = 1.5f;
                    break;
                case Type.B: // 돌진 좀비
                    targerRadius = 1f;
                    targetRange = 9f;
                    break;
                case Type.C: // 소녀 좀비
                    targerRadius = 1f;
                    targetRange = 1.5f;
                    break;
                case Type.D: // 식칼 좀비
                    targerRadius = 1f;
                    targetRange = 2f;
                    break;
                case Type.E: // 경찰 좀비
                    targerRadius = 1.5f;
                    targetRange = 20f;
                    break;
                default:
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targerRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine("Attack");
            }
        }
    }

    public float AttackRate(float WeaponRate, float waiteTime)
    {     
        return (1 / equitWeapon.rate) * waiteTime;
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        if (target.GetComponent<PlayerController>().health > 0)
        {
            switch (enemyType)
            {
                case Type.A:
                    anim.SetFloat("attackAnimSpeed", equitWeapon.rate);
                    anim.SetBool("isAttack", true);
                    SoundManager.instance.SFXPlay(audioClips[0], "attack");
                    yield return new WaitForSeconds(AttackRate(equitWeapon.rate, 0.42f));
                    equitWeapon.meleeArea.enabled = true;
                    yield return new WaitForSeconds(AttackRate(equitWeapon.rate, 0.67f) - AttackRate(equitWeapon.rate, 0.42f));
                    equitWeapon.meleeArea.enabled = false;
                    yield return new WaitForSeconds((1 / equitWeapon.rate) * 1f - ((1 / equitWeapon.rate) * 0.67f));
                    break;
                case Type.B:
                    anim.SetTrigger("doCrouch");
                    yield return new WaitForSeconds(1f);
                    anim.SetBool("isAttack", true);
                    SoundManager.instance.SFXPlay(audioClips[0], "attack");
                    rb.AddForce(transform.forward * 25f, ForceMode.Impulse);
                    equitWeapon.meleeArea.enabled = true;
                    yield return new WaitForSeconds(1f);
                    rb.velocity = Vector3.zero;
                    equitWeapon.meleeArea.enabled = false;
                    break;
                case Type.C:
                    anim.SetFloat("attackAnimSpeed", equitWeapon.rate);
                    anim.SetBool("isAttack", true);
                    SoundManager.instance.SFXPlay(audioClips[0], "attack");
                    yield return new WaitForSeconds((1 / equitWeapon.rate) * 0.42f);
                    equitWeapon.meleeArea.enabled = true;
                    yield return new WaitForSeconds(((1 / equitWeapon.rate) * 0.67f) - ((1 / equitWeapon.rate) * 0.42f));
                    equitWeapon.meleeArea.enabled = false;
                    yield return new WaitForSeconds((1 / equitWeapon.rate) * 1f - ((1 / equitWeapon.rate) * 0.67f));
                    yield return new WaitForSeconds(1f);
                    break;
                case Type.D:
                    anim.SetFloat("attackAnimSpeed", equitWeapon.rate);
                    anim.SetBool("isAttack", true);
                    SoundManager.instance.SFXPlay(audioClips[0], "attack");
                    yield return new WaitForSeconds((1 / equitWeapon.rate) * 0.3f);
                    equitWeapon.meleeArea.enabled = true;
                    yield return new WaitForSeconds(((1 / equitWeapon.rate) * 0.6f) - ((1 / equitWeapon.rate) * 0.3f));
                    equitWeapon.meleeArea.enabled = false;
                    yield return new WaitForSeconds(((1 / equitWeapon.rate) * 1f) - ((1 / equitWeapon.rate) * 0.6f));
                    break;
                case Type.E:
                    anim.SetFloat("attackAnimSpeed", equitWeapon.rate);
                    anim.SetBool("isAttack", true);
                    SoundManager.instance.SFXPlay(audioClips[0], "attack");
                    dir = (target.position - transform.position).normalized;
                    dir.y = 0;
                    transform.LookAt(transform.position + dir);
                    yield return new WaitForSeconds((1 / equitWeapon.rate) * 0.17f);
                    GameObject instantBullet = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
                    instantBullet.GetComponent<Bullet>().damage = equitWeapon.damage;
                    Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
                    bulletRigid.velocity = equitWeapon.bulletPos.forward * 10;

                    GameObject instantBulletCase = Instantiate(equitWeapon.bulletCase, equitWeapon.bulletCasePos.position, equitWeapon.bulletCasePos.rotation);
                    Rigidbody bulletCasetRigid = instantBulletCase.GetComponent<Rigidbody>();
                    Vector3 caseVec = equitWeapon.bulletCasePos.forward * Random.Range(-2, -3) + Vector3.up * Random.Range(2, 3);
                    bulletCasetRigid.AddForce(caseVec, ForceMode.Impulse);
                    bulletCasetRigid.AddTorque(Vector3.up, ForceMode.Impulse);
                    Destroy(instantBulletCase, 5.0f);
                    yield return new WaitForSeconds(((1 / equitWeapon.rate) * 1f) - ((1 / equitWeapon.rate) * 0.17f));
                    break;
                default:
                    break;
            }
            isChase = true;
            isAttack = false;
            anim.SetBool("isAttack", false);
        }
        else // 플레이어가 죽으면 하는 행동
        { 
            switch (enemyType)
            {
                case Type.A:
                    rb.isKinematic = true;
                    isChase = true;
                    yield return new WaitForSeconds(0.5f);
                    isChase = false;
                    nav.enabled = false;
                    anim.SetTrigger("doEat");
                    break;
                case Type.B:
                    rb.isKinematic = true;
                    isChase = true;
                    yield return new WaitForSeconds(0.5f);
                    isChase = false;
                    nav.enabled = false;
                    anim.SetTrigger("doEat");
                    break;
                case Type.C:
                    yield return new WaitForSeconds(0.5f);
                    nav.enabled = false;
                    anim.SetTrigger("doSexy");
                    break;
                case Type.D:
                    yield return new WaitForSeconds(0.5f);
                    nav.enabled = false;
                    break;
                default:
                    yield return new WaitForSeconds(0.5f);
                    nav.enabled = false;
                    break;
            }
        }
    }

    public void HitGrenade(Vector3 explosionPos ,float damage)
    {
        health -= (damage * hitUp);
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec));
    }

    IEnumerator OnDamage(Vector3 reactVec)
    {
        mat.color = Color.red;

        if(health > 0)
        {
            yield return new WaitForSeconds(0.1f);
            mat.color = originColor;
        }
        else
        {
            if(gameObject.layer != 13)
            {
                gameObject.layer = 13;
                StartCoroutine("OnDie", reactVec);
            }
        }
    }

    IEnumerator OnDie(Vector3 reactVec)
    {
        StopCoroutine("Attack");
        if (equitWeapon.meleeArea != null)
        {
            equitWeapon.meleeArea.enabled = false;
        }
        isChase = false;
        nav.enabled = false;
        isDead = true;
        mat.color = Color.gray;

        anim.SetInteger("dieMotion", Random.Range(0, 2));
        anim.SetTrigger("doDie");
        rb.velocity = Vector3.zero;
        reactVec = reactVec.normalized;
        reactVec += Vector3.up;
        rb.AddForce(reactVec * 5, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);
        int random = Random.Range(0, dropItem.Length);
        target.GetComponent<PlayerController>().coin += dropItem[random].GetComponent<Item>().coin;
        target.GetComponent<PlayerController>().curScore += score;
        manager.zombieCount--;
        SoundManager.instance.SFXPlay(DieClip, "Die");
        Destroy(gameObject, 4.0f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            health -= (bullet.damage * hitUp);
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
            SoundManager.instance.SFXPlay(weapon.clip[Random.Range(0,5)], "hitAttack");
            health -= (weapon.damage * hitUp);
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec));
        }
    }
}
