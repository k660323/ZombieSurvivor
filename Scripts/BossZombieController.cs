using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossZombieController : ZombieController
{
    bool isLook;
    Vector3 lookVec;

    public AudioClip[] bossClip;
    public GameObject[] HavingWeapon;
    public Sprite boosImage;
    float distance;
    public float maxHealth;
    public string name;
    public GameObject STzombieG;
    public BoxCollider rushMelee;
    float h;
    float v;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mat = GetComponentInChildren<SkinnedMeshRenderer>().material;
        originColor = mat.color;
        nav.isStopped = false;
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
        if (isDead)
        {
            return;
        }

        if (nav.enabled)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }

        
        if (isLook) // 돌진
        {
            h = Input.GetAxisRaw("Horizontal");
            v = Input.GetAxisRaw("Vertical");
            if (h + v == 0)
            {
                transform.LookAt(target.position);
            }
            else
            {
                if (enemyType == Type.A)
                {
                    lookVec = new Vector3(h, 0, v) * 5f;
                }
                else if (enemyType == Type.B)
                {
                    lookVec = new Vector3(h, 0, v) * 3f;
                }
                transform.LookAt(target.position + lookVec);
            }
        }
    }

    private void FixedUpdate()
    {
        Targeting();
        base.FreezeRotation();
    }

    void Targeting() // 공격 감지 범위 
    {
        if (!isDead)
        {
            float targerRadius = 0f; // 두께
            float targetRange = 0f; // 길이

            switch (enemyType)
            {
                case Type.A: // 군인 보스 좀비
                    targerRadius = 2f;
                    targetRange = 15f;
                    break;
                case Type.B: // 돌진 좀비
                    targerRadius = 2f;
                    targetRange = 15f;
                    break;
                default:
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targerRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine("Think");
            }
            if(!isAttack)
            {
                anim.SetBool("isWalk", true);
            }
        }
    }

    IEnumerator Think()
    {
        anim.SetBool("isWalk", false);
        isChase = false;
        isAttack = true;
        yield return new WaitForSeconds(0.1f);
        distance = (target.position - transform.position).sqrMagnitude;
        int ranAction = Random.Range(0,10);
        if (enemyType == Type.A)
        {
            switch (ranAction)
            {
                //난사
                case 0:
                case 1:
                case 2:
                case 3:
                    StartCoroutine("Rampage");
                    break;
                case 4:
                case 5:
                    StartCoroutine("RPGShot");
                    break;
                case 6:
                case 7:
                    StartCoroutine("Rush");
                    break;
                case 8:
                case 9:
                    StartCoroutine("Grenade");
                    break;
            }
        }
        else if (enemyType == Type.B)
        {
            switch (ranAction)
            {
                //난사
                case 0:
                case 1:
                case 2:
                case 3:
                    if (distance < Mathf.Pow(7, 2))
                        StartCoroutine("comboAttack");
                    else { 
                        isChase = true;
                        isAttack = false;
                    }
                    break;
                case 4:
                case 5:
                    StartCoroutine("createStone");
                    break;
                case 6:
                case 7:
                    StartCoroutine("shadowAttack");
                    break;
                case 8:
                case 9:
                    StartCoroutine("boomRash");
                    break;
            }
        }
    }

    void Swap(int i)
    {
        if (equitWeapon != HavingWeapon[i].GetComponent<Weapon>())
        {
            equitWeapon.gameObject.SetActive(false);
            equitWeapon = HavingWeapon[i].GetComponent<Weapon>();
            equitWeapon.gameObject.SetActive(true);
        }
    }

    #region 군인보스
    IEnumerator Rampage()
    {
        Swap(0);
        yield return new WaitForSeconds(1.5f);
        hitUp = 0.8f;
        anim.SetBool("isMiniGun", true);
        isLook = true;
        for (int i = 0; i < 50; i++)
        {
            if (i % 2 == 0)
                SoundManager.instance.SFXPlay(bossClip[0], "Rampage");
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
            rb.velocity = transform.forward * -2f;
            yield return new WaitForSeconds(0.15f);
        }
        isLook = false;
        anim.SetBool("isMiniGun", false);
        yield return new WaitForSeconds(5f);
        hitUp = 1f;
        isChase = true;
        isAttack = false;
    }

    IEnumerator RPGShot()
    {
        Swap(1);
        yield return new WaitForSeconds(1f);
        anim.SetBool("isRPGShoot", true);
        yield return new WaitForSeconds(2f);
        SoundManager.instance.SFXPlay(bossClip[4], "FireRPG");
        GameObject instantBullet = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        instantBullet.GetComponent<BossMissile>().damage = equitWeapon.damage;
        instantBullet.GetComponent<BossMissile>().target = target;
        //Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        //bulletRigid.velocity = equitWeapon.bulletPos.forward * 10;
        yield return new WaitForSeconds(2f);
        anim.SetBool("isRPGShoot", false);
        isChase = true;
        isAttack = false;
    }

    IEnumerator Rush()
    {
        transform.LookAt(target.position);
        anim.SetTrigger("doCrouch");
        float originDamage = equitWeapon.damage;
        equitWeapon.damage = 30;
        yield return new WaitForSeconds(0.5f);
        SoundManager.instance.SFXPlay(bossClip[1], "Rush");
        yield return new WaitForSeconds(0.5f);
        anim.SetBool("isAttack", true);
        SoundManager.instance.SFXPlay(bossClip[2], "Swing");
        rb.AddForce(transform.forward * 20f, ForceMode.Impulse);
        yield return new WaitForSeconds(0.12f);
        equitWeapon.meleeArea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        equitWeapon.meleeArea.enabled = false;
        yield return new WaitForSeconds(0.5f);
        equitWeapon.damage = originDamage;
        anim.SetBool("isAttack", false);
        rb.velocity = Vector3.zero;

        isChase = true;
        isAttack = false;
    }

    IEnumerator Grenade()
    {
        Swap(2);
        GameObject instanceBoom;
        Rigidbody boomRig;
        Vector3 dir;
        yield return new WaitForSeconds(1f);

        transform.Rotate(new Vector3(0, -90, 0));
        anim.SetTrigger("doThrow");
        SoundManager.instance.SFXPlay(bossClip[3],"finOut");
        yield return new WaitForSeconds(0.35f);
        instanceBoom = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        instanceBoom.GetComponent<Boom>().damage = equitWeapon.damage;
        instanceBoom.GetComponent<Boom>().isEnemy = true;
        boomRig = instanceBoom.GetComponent<Rigidbody>();
        boomRig.velocity = equitWeapon.bulletPos.forward * 5f;
        SoundManager.instance.SFXPlay(bossClip[2], "Throw");
        yield return new WaitForSeconds(0.25f);

        transform.Rotate(new Vector3(0, +90, 0));
        anim.SetTrigger("doThrow");
        SoundManager.instance.SFXPlay(bossClip[3], "finOut");
        yield return new WaitForSeconds(0.35f);
        instanceBoom = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        instanceBoom.GetComponent<Boom>().damage = equitWeapon.damage;
        instanceBoom.GetComponent<Boom>().isEnemy = true;
        boomRig = instanceBoom.GetComponent<Rigidbody>();
        boomRig.velocity = equitWeapon.bulletPos.forward * 5f;
        SoundManager.instance.SFXPlay(bossClip[2], "Throw");
        yield return new WaitForSeconds(0.25f);

        transform.Rotate(new Vector3(0, +90, 0));
        anim.SetTrigger("doThrow");
        SoundManager.instance.SFXPlay(bossClip[3], "finOut");
        yield return new WaitForSeconds(0.35f);
        instanceBoom = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        instanceBoom.GetComponent<Boom>().damage = equitWeapon.damage;
        instanceBoom.GetComponent<Boom>().isEnemy = true;
        boomRig = instanceBoom.GetComponent<Rigidbody>();
        boomRig.velocity = equitWeapon.bulletPos.forward * 5f;
        SoundManager.instance.SFXPlay(bossClip[2], "Throw");
        yield return new WaitForSeconds(0.25f);

        transform.Rotate(new Vector3(0, +90, 0));
        anim.SetTrigger("doThrow");
        SoundManager.instance.SFXPlay(bossClip[3], "finOut");
        yield return new WaitForSeconds(0.35f);
        instanceBoom = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        instanceBoom.GetComponent<Boom>().damage = equitWeapon.damage;
        instanceBoom.GetComponent<Boom>().isEnemy = true;
        boomRig = instanceBoom.GetComponent<Rigidbody>();
        boomRig.velocity = equitWeapon.bulletPos.forward * 5f;
        SoundManager.instance.SFXPlay(bossClip[2], "Throw");
        yield return new WaitForSeconds(0.25f);

        isChase = true;
        isAttack = false;
    } 
    #endregion

    #region 삐에로 보스
    IEnumerator comboAttack()
    {
        Swap(0);
        transform.LookAt(target.position);
        yield return new WaitForSeconds(1.5f);
        hitUp = 0.5f;
        SoundManager.instance.SFXPlay(bossClip[4], "attackReady");
        yield return new WaitForSeconds(0.5f);
        anim.SetTrigger("doAttack");
        SoundManager.instance.SFXPlay(bossClip[1], "attack");
        yield return new WaitForSeconds(0.5f);
        transform.LookAt(target.position);
        transform.position += transform.forward * 2f;
        equitWeapon.meleeArea.enabled = true;
        yield return new WaitForSeconds(0.34f);
        equitWeapon.meleeArea.enabled = false;

        anim.SetTrigger("doAttack");
        SoundManager.instance.SFXPlay(bossClip[1], "attack");
        yield return new WaitForSeconds(0.5f);
        transform.LookAt(target.position);
        transform.position += transform.forward * 2f;
        equitWeapon.meleeArea.enabled = true;
        yield return new WaitForSeconds(0.34f);
        equitWeapon.meleeArea.enabled = false;

        yield return new WaitForSeconds(0.1f);

        anim.SetTrigger("doFinalAttack");
        SoundManager.instance.SFXPlay(bossClip[1], "attack");
        yield return new WaitForSeconds(0.5f);
        transform.LookAt(target.position);
        transform.position += transform.forward * 2f;
        equitWeapon.meleeArea.enabled = true;
        yield return new WaitForSeconds(0.34f);
        equitWeapon.meleeArea.enabled = false;
        hitUp = 1f;
        isChase = true;
        isAttack = false;
    }

    IEnumerator createStone()
    {
        hitUp = 0.5f;
        GameObject instanceBoom;
        Swap(0);
        anim.SetTrigger("doCharge");
        SoundManager.instance.SFXPlay(bossClip[0], "charge");
        instanceBoom = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        instanceBoom.GetComponent<BossStone>().damage = 80;
        yield return new WaitForSeconds(2.2f);
        anim.SetTrigger("doThrow");
        SoundManager.instance.SFXPlay(bossClip[1], "ThrowStone");
        yield return new WaitForSeconds(2f);
        hitUp = 1f;
        isChase = true;
        isAttack = false;
    }
    IEnumerator shadowAttack()
    {
        Swap(1);
        GameObject stZombie;
        int realPos = Random.Range(0,4);
        yield return new WaitForSeconds(2f);
        hitUp = 1.5f;
        // 0 앞 // 1 오른쪽 // 2 뒤쪽 // 3 왼쪽
        for (int i=0; i<4; i++)
        {
            if (realPos == i)
                continue;
            switch (i)
            {
                case 0:
                    RaycastHit[] hit;
                    //hit = Physics.SphereCastAll(target.position, 1.5f, target.forward, 10f, LayerMask.GetMask("Wall"));
                    hit = Physics.RaycastAll(target.position, target.forward, 10f, LayerMask.GetMask("Wall"));
                    if (hit.Length > 0)
                    {
                        stZombie = Instantiate(STzombieG, hit[0].transform.position, Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    else
                    {
                        stZombie = Instantiate(STzombieG, target.position + (target.forward * 10f), Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    break;
                case 1:
                    RaycastHit[] hit1;
                    hit1 = Physics.SphereCastAll(target.position, 1.5f, target.right, 10f, LayerMask.GetMask("Wall"));
                    if (hit1.Length > 0)
                    {
                        stZombie = Instantiate(STzombieG, hit1[0].transform.position, Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    else
                    {
                        stZombie = Instantiate(STzombieG, target.position + (target.right * 10f), Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    break;
                case 2:
                    RaycastHit[] hit2;
                    hit2 = Physics.SphereCastAll(target.position, 1.5f, -target.forward, 10f, LayerMask.GetMask("Wall"));
                    if (hit2.Length > 0)
                    {
                        stZombie = Instantiate(STzombieG, hit2[0].transform.position, Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    else
                    {
                        stZombie = Instantiate(STzombieG, target.position + (-target.forward * 10f), Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    break;
                case 3:
                    RaycastHit[] hit3;
                    hit3 = Physics.SphereCastAll(target.position, 1.5f, -target.right, 10f, LayerMask.GetMask("Wall"));
                    if (hit3.Length > 0)
                    {
                        stZombie = Instantiate(STzombieG, hit3[0].transform.position, Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    else
                    {
                        stZombie = Instantiate(STzombieG, target.position + (-target.right * 10f), Quaternion.identity);
                        stZombie.transform.LookAt(target);
                    }
                    break;
            }
        }

        switch (realPos)
        {
            case 0:
                RaycastHit[] hit;
                hit = Physics.SphereCastAll(target.position, 1.5f, target.forward, 10f, LayerMask.GetMask("Wall"));
                if (hit.Length > 0)
                {
                    transform.position = hit[0].transform.position;
                    transform.LookAt(target);
                }
                else
                {
                    transform.position = target.position + (target.forward * 10f);
                    transform.LookAt(target);
                }
                break;
            case 1:
                RaycastHit[] hit1;
                hit1 = Physics.SphereCastAll(target.position, 1.5f, target.right, 10f, LayerMask.GetMask("Wall"));
                if (hit1.Length > 0)
                {
                    transform.position = hit1[0].transform.position;
                    transform.LookAt(target);
                }
                else
                {
                    transform.position = target.position + (target.right * 10f);               
                    transform.LookAt(target);
                }
                break;
            case 2:
                RaycastHit[] hit2;
                hit2 = Physics.SphereCastAll(target.position, 1.5f, -target.forward, 10f, LayerMask.GetMask("Wall"));
                if (hit2.Length > 0)
                {
                    transform.position = hit2[0].transform.position;
                    transform.LookAt(target);
                }
                else
                {
                    transform.position = target.position + (-target.forward * 10f);
                    transform.LookAt(target);
                }
                break;
            case 3:
                RaycastHit[] hit3;
                hit3 = Physics.SphereCastAll(target.position, 1.5f, -target.right, 10f, LayerMask.GetMask("Wall"));
                if (hit3.Length > 0)
                {
                    transform.position = hit3[0].transform.position;
                    transform.LookAt(target);
                }
                else
                {
                    transform.position = target.position + (-target.right * 10f);
                    transform.LookAt(target);
                }
                break;
        }
        yield return new WaitForSeconds(0.2f);
        anim.SetTrigger("doThrow");
        yield return new WaitForSeconds(0.7f);
        GameObject throwWeapon = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
        throwWeapon.GetComponent<Bullet>().damage = equitWeapon.damage;
        throwWeapon.GetComponent<Rigidbody>().AddForce(transform.forward * 25f, ForceMode.Impulse);
        SoundManager.instance.SFXPlay(bossClip[2], "ThrowKnife");
        yield return new WaitForSeconds(3f);
        hitUp = 1;
        isChase = true;
        isAttack = false;
    }

    IEnumerator boomRash()
    {
        GameObject instanceBoom;
        Rigidbody boomRig;
        Swap(2);
        yield return new WaitForSeconds(1f);
        transform.LookAt(target.position);
        anim.SetTrigger("doCrouch");
        SoundManager.instance.SFXPlay(bossClip[4], "rash");
        yield return new WaitForSeconds(1f);
        rushMelee.enabled = true;
        anim.SetTrigger("doAttack");
        rb.AddForce(transform.forward * 50f, ForceMode.Impulse);
        for (int i = 0; i < 5; i++)
        {
            SoundManager.instance.SFXPlay(bossClip[3], "finOut");
            instanceBoom = Instantiate(equitWeapon.bullet, equitWeapon.bulletPos.position, equitWeapon.bulletPos.rotation);
            instanceBoom.GetComponent<Boom>().damage = equitWeapon.damage;
            instanceBoom.GetComponent<Boom>().isEnemy = true;
            boomRig = instanceBoom.GetComponent<Rigidbody>();
            boomRig.velocity = transform.up * 5f;
            yield return new WaitForSeconds(0.2f);
        }
        rushMelee.enabled = false;
        isChase = true;
        isAttack = false;
    }

    #endregion

    IEnumerator OnDie(Vector3 reactVec)
    {
        if(enemyType == Type.A)
        {
            StopCoroutine("Rampage");
            StopCoroutine("RPGShot");
            StopCoroutine("Rush");
            StopCoroutine("Grenade");
        }
        else if(enemyType == Type.B)
        {
            StopCoroutine("comboAttack");
            StopCoroutine("createStone");
            StopCoroutine("shadowAttack");
            StopCoroutine("boomRash");
        }
        if (equitWeapon.meleeArea != null)
        {
            equitWeapon.meleeArea.enabled = false;
        }
        isChase = false;
        nav.enabled = false;
        isDead = true;
        mat.color = Color.gray;

        anim.SetBool("isWalk", false);
        anim.SetInteger("dieMotion", Random.Range(0, 2));
        anim.SetTrigger("doDie");
        rb.velocity = Vector3.zero;
        reactVec = reactVec.normalized;
        reactVec += Vector3.up;
        rb.AddForce(reactVec * 5, ForceMode.Impulse);

        yield return new WaitForSeconds(0.3f);
        for(int i=0; i< 10; i++)
        {
            int random = Random.Range(0, dropItem.Length);
            target.GetComponent<PlayerController>().coin += dropItem[random].GetComponent<Item>().coin;
        }
        target.GetComponent<PlayerController>().curScore += score;
        manager.zombieCount--;

        manager.BossInfoGroup.SetActive(false);
        manager.Boss = null;
        SoundManager.instance.bgSound.Stop();
        SoundManager.instance.BgSoundPlay(SoundManager.instance.bglist[4]);
        SoundManager.instance.bgSound.Play();
        Destroy(gameObject, 4.0f);
    }
}
