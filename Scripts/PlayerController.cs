using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[System.Serializable]
public class Score
{
    public int Maxscore;
    public float SFXSound;
    public float BGSound;
}

public class PlayerController : MonoBehaviour
{
    public GameManager manager;
    public Camera camera;
    public Canvas mycanvas;
    private GraphicRaycaster gr;
    PointerEventData ped;
    List<RaycastResult> results;

    Ray cameraRay;
    Ray ray;
    RaycastHit hit;

    public Joystick moveJoystick;
    public Joystick attackJoystick;

    // 방향
    Vector3 moveDir;
    Vector3 dodgeDir;
    float h;
    float v;
    
    // 스피드 점프 파워
    public float moveSpeed;
    public float runSpeed;
   // public float jumpPower;
   // public int maxJumpCount;
    //public int curJumpCount;

    //키 입력
    bool fire1Down;
    bool lShiftDown;
    bool sDown;
    bool eDown;
    bool rDown;
    bool gDown;
    bool xDown;
    bool _1Down;
    bool _2Down;
    bool _3Down;
    bool _4Down;
    bool _5Down;

    //플레이어 정보
    public int hpLevel = 0;
    public int speedLevel = 0;
    
    public float maxHealth;
    public float health;
    public int curArrow;
    public int curAmmo;
    public int coin;
    public int curScore;
    public GameObject[] havingWeaponsG;
    public Transform weaponPos;
    public int curGrenade;
    float fireDelay;
    bool isFireReady=true;
    public float dodgeRate;
    float dodgeDelay;
    bool isDodgeReady = true;
    public Weapon equipWeapon;
    public GameObject handWeapon;
    public GameObject[] weapons;
    public GameObject[] Items;
    public GameObject LaserSite;
    public Transform bulletPos;
    public Transform bulletCase;

    //상태
    bool isAttack;
    bool isDamage;
    bool isDead;
    bool isReload;
    bool isDodge;
    bool isShop;
    bool isBorder;

    public GameObject nearObject;

    Animator anim;
    Rigidbody rb;
    SkinnedMeshRenderer sMesh;
    public Score score;
    [SerializeField]
    AudioClip[] audioClips;
    [SerializeField]
    AudioClip[] dodgeClips;
    [SerializeField]
    AudioClip[] reloadClips;
    [SerializeField]
    AudioClip itemAcheive;
    [SerializeField]
    AudioClip coinClip;
    [SerializeField]
    AudioClip hpUpClip;

    AudioSource audioSource;

    Touch touch;

    // Start is called before the first frame update
    void Awake()
    {
        gr = mycanvas.GetComponent<GraphicRaycaster>();
        results = new List<RaycastResult>();
        ped = new PointerEventData(null);
        score = new Score();
        havingWeaponsG = new GameObject[Items.Length];

        havingWeaponsG[3] = Instantiate(weapons[FindItem("Axe")], weaponPos);
        havingWeaponsG[3].name = "Axe";
        havingWeaponsG[3].SetActive(false);

        if (havingWeaponsG[3] == null)
            return;
        if (equipWeapon != null)
            equipWeapon.gameObject.SetActive(false);

        equipWeapon = havingWeaponsG[3].GetComponent<Weapon>();
        havingWeaponsG[3].SetActive(true);
        fireDelay = (1f / equipWeapon.rate); ;

        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        sMesh = GetComponentInChildren<SkinnedMeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Dodge();
        Interection();
        Swap();
        Attack();
        Reload();
    }

    void GetInput()
    {
        h = Input.GetAxis("Horizontal");
        v = Input.GetAxis("Vertical");
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (Input.touchCount == 0)
                return;

            for (int i = 0; i < Input.touchCount; i++)
            {
                touch = Input.GetTouch(i);
            }
            ped.position = touch.position;
            results = new List<RaycastResult>();//여기에 히트된 개체 저장
            gr.Raycast(ped, results);
        }
        else
        {
            fire1Down = Input.GetButton("Fire1");
            lShiftDown = Input.GetButton("LShift");
            sDown = Input.GetButtonDown("Dodge");
            eDown = Input.GetButtonDown("E");
            rDown = Input.GetButtonDown("R");
            gDown = Input.GetButtonDown("G");
            xDown = Input.GetButtonDown("X");
            _1Down = Input.GetButtonDown("1");
            _2Down = Input.GetButtonDown("2");
            _3Down = Input.GetButtonDown("3");
            _4Down = Input.GetButtonDown("4");
            _5Down = Input.GetButtonDown("5");
        }
    }

    void Move()
    {
        if (!isDead)
        {
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                moveDir = new Vector3(moveJoystick.Horizontal, 0, moveJoystick.Vertical).normalized;
            else
                moveDir = new Vector3(h, 0, v).normalized;

            if (isDodge)
                moveDir = dodgeDir;

            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (System.Math.Abs(moveJoystick.Horizontal) >= 0.7 || System.Math.Abs(moveJoystick.Vertical) >= 0.7)
                {
                    runSpeed = 1.4f;
                    lShiftDown = true;
                }
                else
                {
                    runSpeed = 1;
                    lShiftDown = false;
                }
            }
            else
            {
                runSpeed = lShiftDown ? 1.4f : 1;
            }

            if (isAttack || isReload)
                runSpeed = 1.0f;

            if (!isBorder)
                transform.position += moveDir * moveSpeed * runSpeed * Time.deltaTime;

            if (moveDir != Vector3.zero && !audioSource.isPlaying && !lShiftDown)
            {
                audioSource.pitch = 1.5f;
                audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                audioSource.Play();
            }
            else if (moveDir != Vector3.zero && !audioSource.isPlaying && lShiftDown)
            {
                audioSource.pitch = 2f;
                audioSource.clip = audioClips[Random.Range(0, audioClips.Length)];
                audioSource.Play();
            }
            anim.SetBool("isWalk", moveDir != Vector3.zero);
            anim.SetBool("isRun", lShiftDown);
        }
    }

    void Turn()
    {
        if (isShop)
            return;

        if (equipWeapon != null)
        {
            if (!equipWeapon.GetComponent<Weapon>().isAnim)
            {
                transform.LookAt(transform.position + moveDir);
            }
        }
        else
        {
            transform.LookAt(transform.position + moveDir);
        }

        if (!isDead && Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (attackJoystick.Horizontal + attackJoystick.Vertical != 0)
            {
                moveDir = new Vector3(attackJoystick.Horizontal, 0, attackJoystick.Vertical).normalized;
                transform.LookAt(transform.position + moveDir);
            }
        }
        else if (fire1Down && !isDead)
        {
            cameraRay = camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(cameraRay, out hit, 100))
            {
                Vector3 dir = (hit.point - transform.position).normalized;
                dir.y = 0;
                // Quaternion quaternion = Quaternion.LookRotation(dir);
                //transform.rotation = Quaternion.RotateTowards(transform.rotation, quaternion, 10000.0f);
                //transform.rotation = Quaternion.LookRotation(dir);
                transform.LookAt(transform.position + dir);
            }
        }
    }

    void Dodge()
    {
        if (isShop)
            return;

        if (!isDodge)
            dodgeDelay += Time.deltaTime;

        isDodgeReady = dodgeDelay > dodgeRate;

        //  if (equipWeapon != null && equipWeapon.type == Weapon.Type.Melee)
        //    isAttack = false;      


        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (touch.phase == TouchPhase.Began && isDodgeReady && !isDodge && !isAttack && !isReload && !isDead)
            {
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.name == "Dodge Button")
                        {
                            if (equipWeapon != null && equipWeapon.type == Weapon.Type.Melee)
                                equipWeapon.Cancel();
                            audioSource.pitch = 1.5f;
                            audioSource.clip = dodgeClips[Random.Range(0, dodgeClips.Length)];
                            audioSource.Play();
                            SoundManager.instance.SFXPlay(dodgeClips[Random.Range(0, dodgeClips.Length)]);
                            isDodge = true;
                            dodgeDelay = 0;
                            moveSpeed *= 1.8f;
                            dodgeDir = moveDir;
                            anim.SetTrigger("doDodge");
                            Invoke("DodgeOut", 0.5f);
                        }
                    }
                }
            }
        }
        else 
        {
            if (sDown && isDodgeReady && !isDodge && !isAttack && !isReload && !isDead)
            {
                if (equipWeapon != null && equipWeapon.type == Weapon.Type.Melee)
                    equipWeapon.Cancel();

                audioSource.pitch = 1.5f;
                audioSource.clip = dodgeClips[Random.Range(0, dodgeClips.Length)];
                audioSource.Play();
                SoundManager.instance.SFXPlay(dodgeClips[Random.Range(0, dodgeClips.Length)]);
                isDodge = true;
                dodgeDelay = 0;
                moveSpeed *= 1.8f;
                dodgeDir = moveDir;
                anim.SetTrigger("doDodge");
                Invoke("DodgeOut", 0.5f);
            }
        }
    }

    void DodgeOut()
    {
        isDodge = false;
        moveSpeed /= 1.8f;
        
    }
    /*
    void Jump()
    {
        if (sDown && maxJumpCount > curJumpCount && !isDead)
        {
            curJumpCount++;
            rb.velocity = new Vector3(0, jumpPower, 0);
            //rb.AddForce(transform.up * jumpPower, ForceMode.Impulse);

            anim.SetTrigger("doJump");
        }
    }
    */
    int FindItem(string itemName)
    {
        for(int i = 0; i < Items.Length; i++)
        {
            if(weapons[i].name == itemName)
            {
                return i;
            }
        }
        return -1;
    }

    void Interection()
    {
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (touch.phase == TouchPhase.Began && nearObject != null && !isDead && !isDodge && !isAttack && !isReload)
            {
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.name == "Interection Button")
                        {
                            SoundManager.instance.SFXPlay(itemAcheive, "Acheive");
                            if (nearObject.CompareTag("Weapon"))
                            {
                                if (nearObject.GetComponent<Item>().type == Item.Type.RangeWeapon || nearObject.GetComponent<Item>().type == Item.Type.BowWeapon)
                                {
                                    if (havingWeaponsG[0] == null)
                                    {
                                        Item item = nearObject.GetComponent<Item>();
                                        havingWeaponsG[0] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                                        havingWeaponsG[0].name = item.itemName;
                                        havingWeaponsG[0].GetComponent<Weapon>().bulletPos = bulletPos;
                                        havingWeaponsG[0].GetComponent<Weapon>().bulletCasePos = bulletCase;

                                        if (Weapon.Type.Range == havingWeaponsG[0].GetComponent<Weapon>().type)
                                        {
                                            havingWeaponsG[0].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                            havingWeaponsG[0].SetActive(false);
                                            Destroy(nearObject);
                                            return;
                                        }
                                        else if (Weapon.Type.Bow == havingWeaponsG[0].GetComponent<Weapon>().type)
                                        {
                                            havingWeaponsG[0].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                            havingWeaponsG[0].SetActive(false);
                                            Destroy(nearObject);
                                            return;
                                        }
                                    }
                                    else if (havingWeaponsG[1] == null)
                                    {
                                        Item item = nearObject.GetComponent<Item>();
                                        havingWeaponsG[1] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                                        havingWeaponsG[1].name = item.itemName;
                                        havingWeaponsG[1].GetComponent<Weapon>().bulletPos = bulletPos;
                                        havingWeaponsG[1].GetComponent<Weapon>().bulletCasePos = bulletCase;

                                        if (Weapon.Type.Range == havingWeaponsG[1].GetComponent<Weapon>().type)
                                        {
                                            havingWeaponsG[1].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                            havingWeaponsG[1].SetActive(false);
                                            Destroy(nearObject);
                                            return;
                                        }
                                        else if (Weapon.Type.Bow == havingWeaponsG[1].GetComponent<Weapon>().type)
                                        {
                                            havingWeaponsG[1].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                            havingWeaponsG[1].SetActive(false);
                                            Destroy(nearObject);
                                            return;
                                        }
                                    }
                                }
                                else if (nearObject.GetComponent<Item>().type == Item.Type.subRangeWeapon)
                                {
                                    if (havingWeaponsG[2] == null)
                                    {
                                        Item item = nearObject.GetComponent<Item>();
                                        havingWeaponsG[2] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                                        havingWeaponsG[2].name = item.itemName;
                                        havingWeaponsG[2].GetComponent<Weapon>().bulletPos = bulletPos;
                                        havingWeaponsG[2].GetComponent<Weapon>().bulletCasePos = bulletCase;
                                        havingWeaponsG[2].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                        havingWeaponsG[2].SetActive(false);
                                        Destroy(nearObject);
                                        return;
                                    }
                                }
                                else if (nearObject.GetComponent<Item>().type == Item.Type.MeleeWeapon)
                                {
                                    if (havingWeaponsG[3] == null)
                                    {
                                        Item item = nearObject.GetComponent<Item>();
                                        havingWeaponsG[3] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                                        havingWeaponsG[3].name = item.itemName;
                                        havingWeaponsG[3].SetActive(false);
                                        Destroy(nearObject);
                                        return;
                                    }
                                }
                                else if (nearObject.GetComponent<Item>().type == Item.Type.ThrowWeapon)
                                {
                                    if (curGrenade == 0)
                                    {
                                        Item item = nearObject.GetComponent<Item>();
                                        havingWeaponsG[4] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                                        havingWeaponsG[4].name = item.itemName;
                                        havingWeaponsG[4].GetComponent<Weapon>().bulletPos = bulletPos;
                                        havingWeaponsG[4].SetActive(false);
                                    }
                                    curGrenade++;
                                    Destroy(nearObject);
                                }
                            }
                            else if (nearObject.CompareTag("Shop"))
                            {
                                ShopManager shop = nearObject.GetComponent<ShopManager>();
                                shop.Enter(this);
                                isShop = true;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (eDown && nearObject != null && !isDead && !isDodge && !isAttack && !isReload)
            {
                SoundManager.instance.SFXPlay(itemAcheive, "Acheive");
                if (nearObject.CompareTag("Weapon"))
                {
                    if (nearObject.GetComponent<Item>().type == Item.Type.RangeWeapon || nearObject.GetComponent<Item>().type == Item.Type.BowWeapon)
                    {
                        if (havingWeaponsG[0] == null)
                        {
                            Item item = nearObject.GetComponent<Item>();
                            havingWeaponsG[0] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                            havingWeaponsG[0].name = item.itemName;
                            havingWeaponsG[0].GetComponent<Weapon>().bulletPos = bulletPos;
                            havingWeaponsG[0].GetComponent<Weapon>().bulletCasePos = bulletCase;

                            if (Weapon.Type.Range == havingWeaponsG[0].GetComponent<Weapon>().type)
                            {
                                havingWeaponsG[0].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                havingWeaponsG[0].SetActive(false);
                                Destroy(nearObject);
                                return;
                            }
                            else if (Weapon.Type.Bow == havingWeaponsG[0].GetComponent<Weapon>().type)
                            {
                                havingWeaponsG[0].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                havingWeaponsG[0].SetActive(false);
                                Destroy(nearObject);
                                return;
                            }
                        }
                        else if (havingWeaponsG[1] == null)
                        {
                            Item item = nearObject.GetComponent<Item>();
                            havingWeaponsG[1] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                            havingWeaponsG[1].name = item.itemName;
                            havingWeaponsG[1].GetComponent<Weapon>().bulletPos = bulletPos;
                            havingWeaponsG[1].GetComponent<Weapon>().bulletCasePos = bulletCase;

                            if (Weapon.Type.Range == havingWeaponsG[1].GetComponent<Weapon>().type)
                            {
                                havingWeaponsG[1].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                havingWeaponsG[1].SetActive(false);
                                Destroy(nearObject);
                                return;
                            }
                            else if (Weapon.Type.Bow == havingWeaponsG[1].GetComponent<Weapon>().type)
                            {
                                havingWeaponsG[1].GetComponent<Weapon>().curAmmo = item.curAmmo;
                                havingWeaponsG[1].SetActive(false);
                                Destroy(nearObject);
                                return;
                            }
                        }
                    }
                    else if (nearObject.GetComponent<Item>().type == Item.Type.subRangeWeapon)
                    {
                        if (havingWeaponsG[2] == null)
                        {
                            Item item = nearObject.GetComponent<Item>();
                            havingWeaponsG[2] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                            havingWeaponsG[2].name = item.itemName;
                            havingWeaponsG[2].GetComponent<Weapon>().bulletPos = bulletPos;
                            havingWeaponsG[2].GetComponent<Weapon>().bulletCasePos = bulletCase;
                            havingWeaponsG[2].GetComponent<Weapon>().curAmmo = item.curAmmo;
                            havingWeaponsG[2].SetActive(false);
                            Destroy(nearObject);
                            return;
                        }
                    }
                    else if (nearObject.GetComponent<Item>().type == Item.Type.MeleeWeapon)
                    {
                        if (havingWeaponsG[3] == null)
                        {
                            Item item = nearObject.GetComponent<Item>();
                            havingWeaponsG[3] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                            havingWeaponsG[3].name = item.itemName;
                            havingWeaponsG[3].SetActive(false);
                            Destroy(nearObject);
                            return;
                        }
                    }
                    else if (nearObject.GetComponent<Item>().type == Item.Type.ThrowWeapon)
                    {
                        if (curGrenade == 0)
                        {
                            Item item = nearObject.GetComponent<Item>();
                            havingWeaponsG[4] = Instantiate(weapons[FindItem(item.itemName)], weaponPos);
                            havingWeaponsG[4].name = item.itemName;
                            havingWeaponsG[4].GetComponent<Weapon>().bulletPos = bulletPos;
                            havingWeaponsG[4].SetActive(false);
                        }
                        curGrenade++;
                        Destroy(nearObject);
                    }
                }
                else if (nearObject.CompareTag("Shop"))
                {

                    ShopManager shop = nearObject.GetComponent<ShopManager>();
                    shop.Enter(this);
                    isShop = true;
                }

            }
        }

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (touch.phase == TouchPhase.Began && equipWeapon != null && !isDead && !isShop)
            {
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.name == "Throw Button")
                        {
                            if (equipWeapon.gameObject.name == "Hand")
                                return;
                            for (int i = 0; i < Items.Length; i++)
                            {
                                if (Items[i].GetComponent<Item>().itemName == equipWeapon.name)
                                {
                                    GameObject throwItem = Instantiate(Items[i], transform.position + new Vector3(0, 0.5f, 0), transform.rotation);
                                    throwItem.GetComponent<Item>().itemName = equipWeapon.name;
                                    throwItem.GetComponent<Item>().curAmmo = equipWeapon.curAmmo;
                                    isAttack = false;

                                    if (LaserSite.activeInHierarchy == true)
                                        LaserSite.SetActive(false);
                                    if (equipWeapon.type == Weapon.Type.Bow)
                                    {
                                        anim.SetBool("isHasBow", false);
                                    }
                                    else if (equipWeapon.type == Weapon.Type.Range)
                                    {
                                        anim.SetBool("isFire1", false);
                                    }
                                    else if (equipWeapon.type == Weapon.Type.SubRange)
                                        anim.SetBool("isFire2", false);
                                    Destroy(equipWeapon.gameObject);
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (gDown && equipWeapon != null && !isDead && !isShop)
            {
                if (equipWeapon.gameObject.name == "Hand")
                    return;

                for (int i = 0; i < Items.Length; i++)
                {
                    if (Items[i].GetComponent<Item>().itemName == equipWeapon.name)
                    {
                        GameObject throwItem = Instantiate(Items[i], transform.position + new Vector3(0, 0.5f, 0), transform.rotation);
                        throwItem.GetComponent<Item>().itemName = equipWeapon.name;
                        throwItem.GetComponent<Item>().curAmmo = equipWeapon.curAmmo;
                        isAttack = false;

                        if (LaserSite.activeInHierarchy == true)
                            LaserSite.SetActive(false);
                        if (equipWeapon.type == Weapon.Type.Bow)
                        {
                            anim.SetBool("isHasBow", false);
                        }
                        else if (equipWeapon.type == Weapon.Type.Range)
                        {
                            anim.SetBool("isFire1", false);
                        }
                        else if (equipWeapon.type == Weapon.Type.SubRange)
                            anim.SetBool("isFire2", false);

                        Destroy(equipWeapon.gameObject);
                        return;
                    }
                }
            }
        }

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (touch.phase == TouchPhase.Began && equipWeapon != null && !isDead && !isShop)
            {
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.name == "Noequit Button")
                        {
                            if (equipWeapon.gameObject.name == "Hand")
                                return;

                            if (equipWeapon.type == Weapon.Type.Bow)
                            {
                                anim.SetBool("isHasBow", false);
                            }
                            else if (equipWeapon.type == Weapon.Type.Range)
                            {
                                anim.SetBool("isFire1", false);
                            }
                            else if (equipWeapon.type == Weapon.Type.SubRange)
                                anim.SetBool("isFire2", false);

                            equipWeapon.transform.gameObject.SetActive(false);
                            equipWeapon = handWeapon.GetComponent<Weapon>();
                            equipWeapon.transform.gameObject.SetActive(true);
                        }
                    }
                }
            }
        }
        else
        {
            if (xDown && equipWeapon != null && !isDead && !isShop)
            {
                if (equipWeapon.gameObject.name == "Hand")
                    return;

                if (equipWeapon.type == Weapon.Type.Bow)
                {
                    anim.SetBool("isHasBow", false);
                }
                else if (equipWeapon.type == Weapon.Type.Range)
                {
                    anim.SetBool("isFire1", false);
                }
                else if (equipWeapon.type == Weapon.Type.SubRange)
                    anim.SetBool("isFire2", false);

                LaserSite.SetActive(false);
                isAttack = false;
                equipWeapon.transform.gameObject.SetActive(false);
                equipWeapon = handWeapon.GetComponent<Weapon>();
                equipWeapon.transform.gameObject.SetActive(true);
            }
        }
    }

    void Swap()
    {
        if (isDead || isDodge || isAttack || isReload || isShop )
            return;

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (touch.phase == TouchPhase.Began)
            {
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.name == "Weapon Button0")
                        {
                            if (havingWeaponsG[0] == null)
                                return;
                            if (equipWeapon != null)
                                equipWeapon.gameObject.SetActive(false);

                            equipWeapon = havingWeaponsG[0].GetComponent<Weapon>();
                            havingWeaponsG[0].SetActive(true);
                            fireDelay = (1f/equipWeapon.rate);
                            return;
                        }
                        else if (ray.gameObject.name == "Weapon Button1")
                        {
                            if (havingWeaponsG[1] == null)
                                return;
                            if (equipWeapon != null)
                                equipWeapon.gameObject.SetActive(false);

                            equipWeapon = havingWeaponsG[1].GetComponent<Weapon>();
                            havingWeaponsG[1].SetActive(true);
                            fireDelay = (1f / equipWeapon.rate); ;
                            return;
                        }
                        else if (ray.gameObject.name == "Weapon Button2")
                        {
                            if (havingWeaponsG[2] == null)
                                return;
                            if (equipWeapon != null)
                                equipWeapon.gameObject.SetActive(false);

                            equipWeapon = havingWeaponsG[2].GetComponent<Weapon>();
                            havingWeaponsG[2].SetActive(true);
                            fireDelay = (1f / equipWeapon.rate); ;
                            return;
                        }
                        else if (ray.gameObject.name == "Weapon Button3")
                        {
                            if (havingWeaponsG[3] == null)
                                return;
                            if (equipWeapon != null)
                                equipWeapon.gameObject.SetActive(false);

                            equipWeapon = havingWeaponsG[3].GetComponent<Weapon>();
                            havingWeaponsG[3].SetActive(true);
                            fireDelay = (1f / equipWeapon.rate); ;
                            return;
                        }
                        else if (ray.gameObject.name == "Weapon Button4")
                        {
                            if (havingWeaponsG[4] == null)
                                return;
                            if (equipWeapon != null)
                                equipWeapon.gameObject.SetActive(false);

                            equipWeapon = havingWeaponsG[4].GetComponent<Weapon>();
                            havingWeaponsG[4].SetActive(true);
                            fireDelay = (1f / equipWeapon.rate); ;
                            return;
                        }
                    }
                }
            }
        }
        else
        {
            if (_1Down && havingWeaponsG[0] != null)
            {
                // if (equipWeapon != null && havingWeapons[0] == equipWeapon.gameObject.name)
                //     return;
                if (equipWeapon != null)
                    equipWeapon.gameObject.SetActive(false);

                equipWeapon = havingWeaponsG[0].GetComponent<Weapon>();
                havingWeaponsG[0].SetActive(true);
                fireDelay = (1f / equipWeapon.rate); ;
                return;
            }
            else if (_2Down && havingWeaponsG[1] != null)
            {
                // if (equipWeapon != null && havingWeapons[1] == equipWeapon.gameObject.name)
                //   return;
                if (equipWeapon != null)
                    equipWeapon.gameObject.SetActive(false);

                equipWeapon = havingWeaponsG[1].GetComponent<Weapon>();
                havingWeaponsG[1].SetActive(true);
                fireDelay = (1f / equipWeapon.rate); ;
            }
            else if (_3Down && havingWeaponsG[2] != null)
            {
                if (equipWeapon != null && havingWeaponsG[2].name == equipWeapon.gameObject.name)
                    return;
                if (equipWeapon != null)
                    equipWeapon.gameObject.SetActive(false);

                equipWeapon = havingWeaponsG[2].GetComponent<Weapon>();
                havingWeaponsG[2].SetActive(true);
                fireDelay = (1f / equipWeapon.rate); ;
            }
            else if (_4Down && havingWeaponsG[3] != null)
            {
                if (equipWeapon != null && havingWeaponsG[3].name == equipWeapon.gameObject.name)
                    return;
                if (equipWeapon != null)
                    equipWeapon.gameObject.SetActive(false);

                equipWeapon = havingWeaponsG[3].GetComponent<Weapon>();
                havingWeaponsG[3].SetActive(true);
                fireDelay = (1f / equipWeapon.rate); ;
            }
            else if (_5Down && havingWeaponsG[4] != null)
            {
                if (equipWeapon != null && havingWeaponsG[4].name == equipWeapon.gameObject.name)
                    return;
                if (equipWeapon != null)
                    equipWeapon.gameObject.SetActive(false);

                equipWeapon = havingWeaponsG[4].GetComponent<Weapon>();
                havingWeaponsG[4].SetActive(true);
                fireDelay = (1f / equipWeapon.rate); ;
            }
        }

        if (equipWeapon != null)
        {
            if (equipWeapon.type == Weapon.Type.Bow && (_1Down || _2Down))
            {
                anim.SetBool("isHasBow", true);
            }
            else if (equipWeapon.type != Weapon.Type.Bow && (_1Down || _2Down || _3Down || _4Down || _5Down))
            {
                anim.SetBool("isHasBow", false);
            }

            if (equipWeapon.type == Weapon.Type.Melee || equipWeapon.type == Weapon.Type.Throw)
            {
                LaserSite.SetActive(false);
            }
            else
            {
                LaserSite.SetActive(true);
            }
        }     
    }
    void Attack()
    {
        if (equipWeapon == null)
            return;
        if (isDead || isShop || isReload)
            return;

        fireDelay += Time.deltaTime;
        isFireReady = (1f / equipWeapon.rate) <= fireDelay;

        //isFireReady 공격 시작을 알림 isAttack =true;
        //!equipWeapon.isAnim 공격 끝을 알림 isAttack=false;

        //공격속도에 따른 애니메이션 스피드
        if (equipWeapon.type == Weapon.Type.Melee || equipWeapon.type == Weapon.Type.Throw)
        {
            if (equipWeapon.rate > 1)
                anim.SetFloat("attackSpeed", equipWeapon.rate);
            else
                anim.SetFloat("attackSpeed", 1);
        }
        else if(equipWeapon.type == Weapon.Type.Bow)
        {
                anim.SetFloat("attackSpeed", equipWeapon.rate);
        }
        
        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if ((System.Math.Abs(attackJoystick.Horizontal) + System.Math.Abs(attackJoystick.Vertical)) != 0 && isFireReady)
            {
                if (equipWeapon.curAmmo == 0 && equipWeapon.type != Weapon.Type.Melee && equipWeapon.type != Weapon.Type.Throw)
                {
                    rDown = true;
                    return;
                }

                if (equipWeapon.type != Weapon.Type.Melee)
                    isAttack = true;
                else
                    isAttack = false;

                if (equipWeapon.type == Weapon.Type.Melee)
                {
                    anim.SetTrigger("doSwing1");
                }
                else if (equipWeapon.type == Weapon.Type.Bow)
                    anim.SetTrigger("doPull");
                else if (equipWeapon.type == Weapon.Type.Range)
                    anim.SetBool("isFire1", true);//anim.SetTrigger("doFire1");
                else if (equipWeapon.type == Weapon.Type.SubRange)
                    anim.SetBool("isFire2", true); //anim.SetTrigger("doFire2");
                else if (equipWeapon.type == Weapon.Type.Throw)
                {
                    anim.SetTrigger("doThrow");
                    curGrenade--;
                }


                if (equipWeapon.type != Weapon.Type.Melee || equipWeapon.type != Weapon.Type.Throw)
                {
                    //if (Physics.BoxCast(transform.position, transform.lossyScale / 2, transform.forward, out hit, transform.rotation, 15f, LayerMask.GetMask("Enemy")))
                    if (Physics.SphereCast(transform.position, 2f, transform.forward, out hit, 15f, LayerMask.GetMask("Enemy")))
                    {
                        if (hit.transform.CompareTag("Enemy"))
                        {
                            Vector3 dir = (hit.transform.position - transform.position).normalized;
                            dir.y = 0;
                            transform.LookAt(transform.position + dir);
                        }
                    }
                }

                equipWeapon.Use();
                fireDelay = 0;
                if (curGrenade == 0 && equipWeapon.type == Weapon.Type.Throw)
                    Invoke("noitem", ((1f / equipWeapon.rate)));
            }         
            else if ((System.Math.Abs(attackJoystick.Horizontal) + System.Math.Abs(attackJoystick.Vertical)) == 0 && !equipWeapon.isAnim)
            {
                isAttack = false;
                if (equipWeapon.type == Weapon.Type.Range)
                    anim.SetBool("isFire1", false);
                else if (equipWeapon.type == Weapon.Type.SubRange)
                    anim.SetBool("isFire2", false);
            }
        }
        else
        {
            if (fire1Down)
            {
                ped.position = Input.mousePosition;
                List<RaycastResult> results = new List<RaycastResult>();//여기에 히트된 개체 저장
                gr.Raycast(ped, results);
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.CompareTag("UI"))
                        {
                            return;
                        }
                    }
                }
            }

            if (fire1Down && isFireReady)
            {
                if (equipWeapon.curAmmo == 0 && equipWeapon.type != Weapon.Type.Melee && equipWeapon.type != Weapon.Type.Throw)
                {
                    rDown = true;
                    return;
                }

                if (equipWeapon.type != Weapon.Type.Melee)
                    isAttack = true;
                else
                    isAttack = false;

                if (equipWeapon.type == Weapon.Type.Melee)
                    anim.SetTrigger("doSwing1");
                else if (equipWeapon.type == Weapon.Type.Bow)
                    anim.SetTrigger("doPull");
                else if (equipWeapon.type == Weapon.Type.Range)
                {
                    anim.SetBool("isFire1", true);//anim.SetTrigger("doFire1");
                }
                else if (equipWeapon.type == Weapon.Type.SubRange)
                    anim.SetBool("isFire2", true); //anim.SetTrigger("doFire2");
                else if (equipWeapon.type == Weapon.Type.Throw)
                {
                        anim.SetTrigger("doThrow");
                        curGrenade--;
                }

                if (equipWeapon.type != Weapon.Type.Melee || equipWeapon.type != Weapon.Type.Throw)
                {
                    //if (Physics.BoxCast(transform.position, transform.lossyScale / 2, transform.forward, out hit, transform.rotation, 15f, LayerMask.GetMask("Enemy")))
                    if (Physics.SphereCast(transform.position, 2f, transform.forward, out hit, 15f,LayerMask.GetMask("Enemy")))
                    {
                        if (hit.transform.CompareTag("Enemy"))
                        {
                            Vector3 dir = (hit.transform.position - transform.position).normalized;
                            dir.y = 0;
                            transform.LookAt(transform.position + dir);
                        }
                    }
                }

                equipWeapon.Use();
                fireDelay = 0;
                if (curGrenade == 0 && equipWeapon.type == Weapon.Type.Throw)
                    Invoke("noitem", ((1f / equipWeapon.rate)));
            }
            else if (!fire1Down && !equipWeapon.isAnim)
            {
                isAttack = false;

                if (equipWeapon.type == Weapon.Type.Range)
                {
                    anim.SetBool("isFire1", false);
                }
                else if (equipWeapon.type == Weapon.Type.SubRange)
                    anim.SetBool("isFire2", false);
            }
        }
    }

    void noitem()
    {
        Destroy(equipWeapon.gameObject);
        isAttack = false;
        equipWeapon = null;
        havingWeaponsG[4] = null;
    }

    void Reload()
    {
        if (isDead)
            return;
        if (equipWeapon == null)
            return;
        if (equipWeapon.type == Weapon.Type.Melee)
            return;
        if (equipWeapon.type == Weapon.Type.Range || equipWeapon.type == Weapon.Type.SubRange)
        {       
            if (curAmmo == 0)
                return;
        }
        else if (equipWeapon.type == Weapon.Type.Bow)
        {
            if (curArrow == 0)
                return;
        }

        if (equipWeapon.curAmmo == equipWeapon.maxAmmo)
            return;

        if (rDown && !isReload)
        {
            if (equipWeapon.type == Weapon.Type.Range)
                anim.SetBool("isFire1", false);
            else if (equipWeapon.type == Weapon.Type.SubRange)
                anim.SetBool("isFire2", false);
            
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
            {
                if (equipWeapon.type == Weapon.Type.Range)
                {
                    anim.SetTrigger("doReload");
                }
                else if (equipWeapon.type == Weapon.Type.SubRange)
                {
                    anim.SetTrigger("doReload2");
                }
                else if (equipWeapon.type == Weapon.Type.Bow)
                {
                    anim.SetTrigger("doBowReload");
                }
                Invoke("ReloadOut", 1.0f);
                isReload = true;
                rDown = false;
            }
        }

       

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            if (touch.phase == TouchPhase.Began && !isReload)
            {
                if (results.Count != 0)
                {
                    foreach (RaycastResult ray in results)
                    {
                        if (ray.gameObject.name == "Reload Button")
                        {
                            if (equipWeapon.type == Weapon.Type.Range)
                            {
                                anim.SetTrigger("doReload");
                            }
                            else if (equipWeapon.type == Weapon.Type.SubRange)
                            {
                                anim.SetTrigger("doReload2");
                            }
                            else if (equipWeapon.type == Weapon.Type.Bow)
                            {
                                anim.SetTrigger("doBowReload");
                            }
                            Invoke("ReloadOut", 1.0f);
                            isReload = true;
                        }
                    }
                }
            }
        }
        else
        {
            if (rDown && !isReload)
            {
                if (equipWeapon.type == Weapon.Type.Range)
                {
                    anim.SetTrigger("doReload");
                }
                else if (equipWeapon.type == Weapon.Type.SubRange)
                {
                    anim.SetTrigger("doReload2");
                }
                else if (equipWeapon.type == Weapon.Type.Bow)
                {
                    anim.SetTrigger("doBowReload");
                }
                Invoke("ReloadOut", 1.0f);
                isReload = true;
            }
        }
    }

    void ReloadOut()
    {
        audioSource.pitch = 1.0f;
        audioSource.clip = reloadClips[0];
        audioSource.Play();
        int reloadAmmo = equipWeapon.maxAmmo - equipWeapon.curAmmo;
        if ((equipWeapon.type == Weapon.Type.Bow ? curArrow : curAmmo) > reloadAmmo)
        {
            equipWeapon.curAmmo += reloadAmmo;
            if (equipWeapon.type == Weapon.Type.Range || equipWeapon.type == Weapon.Type.SubRange)
            {
                curAmmo -= reloadAmmo;
            }
            else
            {
                curArrow -= reloadAmmo;
            }
        }
        else
        {
            equipWeapon.curAmmo += (equipWeapon.type == Weapon.Type.Range ? curAmmo : curArrow);
            if(equipWeapon.type == Weapon.Type.Range)
            {
                curAmmo = 0;
            }
            else
            {
                curArrow = 0;
            }           
        }
        isReload = false;
    }

    void FixedUpdate()
    {
        StopToWall();
        FreezeRotation();
    }

    void FreezeRotation()
    {
        rb.angularVelocity = Vector3.zero;
    }

    void StopToWall()
    {
        //Debug.DrawRay(transform.position, transform.forward * 0.5f, Color.green);
        isBorder = Physics.Raycast(transform.position, moveDir, 0.5f, LayerMask.GetMask("Wall"));
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Item"))
        {
            Item item = other.GetComponent<Item>();
            switch (item.type)
            {
                case Item.Type.Ammo:
                    curAmmo += item.curAmmo;
                    Destroy(other.gameObject);
                    break;
                case Item.Type.Coin:
                    coin += item.coin;
                    SoundManager.instance.SFXPlay(coinClip, "coin");
                    Destroy(other.gameObject);
                    break;
                case Item.Type.Heart:
                    if(maxHealth == health)                   
                        break;                  
                    if (maxHealth > health + item.heart)
                    {
                        SoundManager.instance.SFXPlay(hpUpClip, "hpUp");
                        health += item.heart;
                        Destroy(other.gameObject);
                    }
                    else
                    {
                        SoundManager.instance.SFXPlay(hpUpClip, "hpUp");
                        health = maxHealth;
                        Destroy(other.gameObject);
                    }
                    break;
                case Item.Type.ArrowAmmo:
                    curArrow += item.curAmmo;
                    Destroy(other.gameObject);
                    break;
            }
        }
        else if (other.CompareTag("EnemyMelee"))
        {
            if (!isDamage && !isDead)
            {
                Weapon attack = other.GetComponent<Weapon>();
                health -= attack.damage;
                StartCoroutine(OnDamage());
            }
        }    
        else if (other.CompareTag("EnemyBullet"))
        {
            if (!isDamage && !isDead)
            {
                Bullet attack = other.GetComponent<Bullet>();
                health -= attack.damage;
                Destroy(other.gameObject);
                StartCoroutine(OnDamage());
            }
        }
        else if (other.CompareTag("EnemyMissile"))
        {
            if (!isDamage && !isDead)
            {
                BossMissile attack = other.GetComponent<BossMissile>();
                health -= attack.damage;
                Destroy(other.gameObject);
                StartCoroutine(OnDamage());
            }
        }   
    }

    public void HitGrenade(Vector3 explosionPos, float damage)
    {
        health -= damage;
        //Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage());
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Weapon")|| other.CompareTag("Shop"))
        {
            nearObject = other.gameObject;
        }     
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Weapon"))
        {
            nearObject = null;
        }
        else if (other.CompareTag("Shop"))
        {
            ShopManager shop = nearObject.GetComponent<ShopManager>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
         if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            if (!isDamage && !isDead)
            {
                BossStone attack = collision.gameObject.GetComponent<BossStone>();
                health -= attack.damage;
                Destroy(collision.gameObject.gameObject);
                StartCoroutine(OnDamage());
            }
        }
    }

    IEnumerator OnDamage()
    {
        isDamage = true;
        if (health > 0)
        {
            sMesh.sharedMaterial.color = Color.gray;
            yield return new WaitForSeconds(1.0f);
            sMesh.sharedMaterial.color = Color.white;
        }
        else
        {
            isDead = true;
            rb.isKinematic = true;
            if (equipWeapon != null)
                equipWeapon.StopAllCoroutines();
            if (equipWeapon.GetComponent<BoxCollider>() != null)
                equipWeapon.GetComponent<BoxCollider>().enabled = false;
            anim.SetInteger("dieMotion", Random.Range(0, 2));
            anim.SetTrigger("doDie");
            yield return new WaitForSeconds(2.0f);
            manager.GameOver();
        }
        isDamage = false;
    }
}
