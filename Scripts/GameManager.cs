using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    bool isBattle;
    public int curRound;
    string filePath;
    //카메라
    public GameObject mainCamera;
    public GameObject menuCamera;

    //패널
    public GameObject MenuPanel;
    public Text highScoreText;

    public GameObject GameOverPanel;
    public Text curScore;

    [Header("보스 정보")]
    public GameObject BossInfoGroup;
    public Image BossHpImage;
    public Text BossNameText;
    public Image BossImage;
    public GameObject Boss;
    [Header("게임 패널")]
    public GameObject GamePanel;
    public GameObject helpGroup;
    public Animator waveAnim;
    public Animator scoreAnim;
    //게임 패널
    public Text RoundText;
    public Text playTimeText;
    public Text curZombieText;
    public Text playerCoinText;
    public Text playerAmmoText;
    public Text playerWeaponText;
    public Text scoreText;
    public Text waveText;
    public Text speedUpText;
    public Image playerHPImage;
    public RawImage playerWeaponImage;

    public GameObject returnBase;
    public GameObject clearObject;
    public GameObject mainObject;
    [Header("아이템 이미지")]
    public RenderTexture[] WeaponImage;
    public Sprite defaultWeaponSprite;

    public Transform GameStartPos;
    public Transform ShopPos;
    public Transform[] spawnPoint;
    public Transform bossSpawnPoint;
    public GameObject[] zomList;
    List<int> enemyList;
    public int zombieCount;
    public GameObject player;
    int totalZombie;

    [Header("옵션")]
    public GameObject optionObject;
    bool isPause;
    public Slider[] VolumeSlider;
    public float playTime;
    public GameObject mobileUI;
    // Start is called before the first frame update
    void Start()
    {
        //Screen.SetResolution(1920, 1080, true);
        Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
        Application.targetFrameRate = 60;
        filePath = Application.persistentDataPath + "/zombieDefense.txt";
        Load();
        highScoreText.text = player.GetComponent<PlayerController>().score.Maxscore.ToString();
        VolumeSlider[0].value = player.GetComponent<PlayerController>().score.BGSound;
        VolumeSlider[1].value = player.GetComponent<PlayerController>().score.SFXSound;
        enemyList = new List<int>();
    }

    public void Save()
    {
        if (player.GetComponent<PlayerController>().score.Maxscore < player.GetComponent<PlayerController>().curScore)
        {
            player.GetComponent<PlayerController>().score.Maxscore = player.GetComponent<PlayerController>().curScore;
        }
        string jdata = JsonUtility.ToJson(player.GetComponent<PlayerController>().score);
        File.WriteAllText(filePath, jdata);
    }

    void Load()
    {
        if (!File.Exists(filePath))
        {
            Reset();
            return;
        }

        string jdata = File.ReadAllText(filePath);
        player.GetComponent<PlayerController>().score = JsonUtility.FromJson<Score>(jdata);
    }

    void Reset()
    {
        player.GetComponent<PlayerController>().score.Maxscore = 0;
        player.GetComponent<PlayerController>().score.BGSound = 0f;
        player.GetComponent<PlayerController>().score.SFXSound = 0f;
        Save();
    }

    // Update is called once per frame
    void Update()
    {
        if (isBattle)
        {
            playTime += Time.deltaTime;
        }
        if (Input.GetKeyDown(KeyCode.Escape) && mainCamera.activeSelf == true)
        {
            GamePause(!isPause);
        }
    }

    void LateUpdate()
    {
        scoreText.text = "점수 : " + player.GetComponent<PlayerController>().curScore;
        if (isBattle)
        {
            int hour = (int)(playTime / 3600);
            int min = (int)((playTime - hour * 3600) / 60);
            int second = (int)(playTime % 60);
            playTimeText.text = string.Format("{0:00}", hour) + ":" + string.Format("{0:00}", min) + ":" + string.Format("{0:00}", second);
        }
        curZombieText.text = zombieCount.ToString() + " / " + totalZombie;
        playerCoinText.text = player.GetComponent<PlayerController>().coin.ToString();
        playerHPImage.fillAmount = player.GetComponent<PlayerController>().health / player.GetComponent<PlayerController>().maxHealth;

        if (player.GetComponent<PlayerController>().equipWeapon)
        {
            playerWeaponText.text = player.GetComponent<PlayerController>().equipWeapon.name;
            SwitchWeaponImage(playerWeaponText.text);

            switch (player.GetComponent<PlayerController>().equipWeapon.type)
            {
                case Weapon.Type.Melee:
                    playerAmmoText.text = "-";
                    break;
                case Weapon.Type.Range:
                case Weapon.Type.SubRange:
                    playerAmmoText.text = player.GetComponent<PlayerController>().equipWeapon.curAmmo + "/" + player.GetComponent<PlayerController>().curAmmo;
                    break;
                case Weapon.Type.Bow:
                    playerAmmoText.text = player.GetComponent<PlayerController>().equipWeapon.curAmmo + "/" + player.GetComponent<PlayerController>().curArrow;
                    break;
                case Weapon.Type.Throw:
                    playerAmmoText.text = player.GetComponent<PlayerController>().curGrenade + "/" + player.GetComponent<PlayerController>().curGrenade;
                    break;
            }
        }
        else
        {
            playerWeaponText.text = "맨손";
            playerAmmoText.text = "-";
            SwitchWeaponImage(playerWeaponText.text);
        }
        
        if(Boss != null)
        {         
            BossHpImage.fillAmount = Boss.GetComponent<BossZombieController>().health / Boss.GetComponent<BossZombieController>().maxHealth;          
        }

        player.GetComponent<PlayerController>().score.BGSound = VolumeSlider[0].value;
        player.GetComponent<PlayerController>().score.SFXSound = VolumeSlider[1].value;
    }

    void SwitchWeaponImage(string name)
    {
        switch (name)
        {
            case "Axe":
                playerWeaponImage.texture = WeaponImage[0];
                break;
            case "M4":
                playerWeaponImage.texture = WeaponImage[1];
                break;
            case "AK_47":
                playerWeaponImage.texture = WeaponImage[2];
                break;
            case "Revolver":
                playerWeaponImage.texture = WeaponImage[3];
                break;
            case "CrossBow":
                playerWeaponImage.texture = WeaponImage[4];
                break;
            case "Grenade":
                playerWeaponImage.texture = WeaponImage[5];
                break;
            default:
                playerWeaponImage.texture = defaultWeaponSprite.texture;
                break;
        }
    }

    public void GameStart()
    {
        mainCamera.SetActive(true);
        menuCamera.SetActive(false);

        MenuPanel.SetActive(false);
        GameOverPanel.SetActive(false);
        GamePanel.SetActive(true);

        if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
        {
            mobileUI.SetActive(true);
            helpGroup.SetActive(false);
        }
        else
        {
            mobileUI.SetActive(false);
            helpGroup.SetActive(true);
        }

        SoundManager.instance.bgSound.Stop();
        SoundManager.instance.BgSoundPlay(SoundManager.instance.bglist[4]);
        SoundManager.instance.bgSound.Play();
    }

    public void StageStart()
    {
        isBattle = true;
        RoundText.text = "Round " + ++curRound;
        player.transform.position = GameStartPos.position;

        foreach (Transform spawn in spawnPoint)
            spawn.gameObject.SetActive(true);
        bossSpawnPoint.gameObject.SetActive(true);

        waveText.text = "Wave " + curRound;
        waveAnim.SetTrigger("doShow");

        StartCoroutine(InBattle());
    }

    IEnumerator InBattle()
    {
        //if (curRound % 5 == 0 && !isBattle)
        //{
        //    int ranZom = Random.Range(0, zomList.Length);
        //    int ranZone = Random.Range(0, 4);
        //    GameObject instantEnemy = Instantiate(zomList[ranZom], spawnPoint[ranZone].position, spawnPoint[ranZone].rotation);
        //    ZombieController zombie = instantEnemy.GetComponent<ZombieController>();
        //    zombie.target = player.transform;
        //    zombie.manager = this;
        //    zombieCount++;
        //    yield return new WaitForSeconds(4.0f);
        //}
        //else
       // {       
       // A 근접 B 돌격 C 소좀 D 칼빵좀 E 총좀 F 군좀 G 삐좀
            switch (curRound)
            {
                case 1:
                    SetZombieCount(10, 5, 5, 0, 0, 0, 0);
                    break;
                case 2:
                    SetZombieCount(10, 5, 8, 0, 2, 0, 0);
                    break;
                case 3:
                    SetZombieCount(10, 5, 5, 1, 3, 0, 0);
                    break;
                case 4:
                    SetZombieCount(10, 5, 5, 3, 5, 0, 0);
                    break;
                case 5: // 보스
                    SetZombieCount(0, 0, 0, 2, 4, 1, 0);
                    break;
                case 6:
                    SetZombieCount(10, 7, 5, 3, 5, 0, 0);
                    break;
                case 7:
                    SetZombieCount(10, 7, 7, 3, 7, 0, 0);
                    break;
                case 8:
                    SetZombieCount(10, 7, 7, 7, 5, 0, 0);
                    break;
                case 9:
                    SetZombieCount(10, 10, 7, 7, 5, 0, 0);
                    break;
                case 10: // 보스
                    SetZombieCount(0, 0, 0, 5, 2, 0, 1);
                    break;
            }

            StartCoroutine("ZombieSpawn");
        //}


        while (zombieCount > 0)
        {
            yield return null;
        }
        yield return new WaitForSeconds(4.0f);
        StageEnd();
    }

    void SetZombieCount(int A,int B,int C, int D,int E,int F,int G)
    {
        totalZombie = 0;
        if (A != 0)
            for (int i = A; i > 0; i--)
                enemyList.Add(0);

        if (B != 0)
            for (int i = B; i > 0; i--)
                enemyList.Add(1);

        if (C != 0)
            for (int i = C; i > 0; i--)
                enemyList.Add(2);

        if (D != 0)
            for (int i = D; i > 0; i--)
                enemyList.Add(3);

        if (E != 0)
            for (int i = E; i > 0; i--)
                enemyList.Add(4);

        if (F != 0)
            for (int i = F; i > 0; i--)
                enemyList.Add(5);
        if (G != 0)
            for (int i = G; i > 0; i--)
                enemyList.Add(6);
        totalZombie = A + B + C + D + E + F + G;
    }

    IEnumerator ZombieSpawn()
    {
        while (enemyList.Count > 0)
        {
            if (enemyList[0] != 5 && enemyList[0] != 6)
            {
                int ranZone = Random.Range(0, 4);
                GameObject instantEnemy = Instantiate(zomList[enemyList[0]], spawnPoint[ranZone].position, spawnPoint[ranZone].rotation);
                ZombieController zombie = instantEnemy.GetComponent<ZombieController>();
                zombie.target = player.transform;
                zombie.manager = this;
                enemyList.RemoveAt(0);
                zombieCount++;
                yield return new WaitForSeconds(2f);
            }
            else
            {              
                Boss  = Instantiate(zomList[enemyList[0]], bossSpawnPoint.position, bossSpawnPoint.rotation);
                BossZombieController zombie = Boss.GetComponent<BossZombieController>();
                zombie.target = player.transform;
                zombie.manager = this;
                BossInfoGroup.SetActive(true);
                BossImage.sprite = zombie.boosImage;
                BossNameText.text = Boss.GetComponent<BossZombieController>().name;
                enemyList.RemoveAt(0);
                zombieCount++;

                switch (zombie.enemyType)
                {
                    case ZombieController.Type.A:
                        SoundManager.instance.BgSoundPlay(SoundManager.instance.bglist[2]);
                        break;
                    case ZombieController.Type.B:
                        SoundManager.instance.BgSoundPlay(SoundManager.instance.bglist[3]);
                        break;
                }               
                yield return new WaitForSeconds(1f);
            }
        }
    }

    void StageEnd()
    {
        zombieCount = 0;
        if (curRound == 10)
        {
            clearObject.SetActive(true);
            mainObject.SetActive(true);
            player.GetComponent<PlayerController>().curScore += player.GetComponent<PlayerController>().coin;
            scoreAnim.SetBool("isMove", true);
            SoundManager.instance.BgSoundPlay(SoundManager.instance.bglist[5]);
        }
        else
        {
            returnBase.SetActive(true);
        }
        foreach (Transform spawn in spawnPoint)
            spawn.gameObject.SetActive(false);
        bossSpawnPoint.gameObject.SetActive(false);
        isBattle = false;
    }

    public void ReturnBasePos()
    {
        returnBase.SetActive(false);
        player.transform.position = ShopPos.position;
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }

    public void GameOver()
    {
        GamePanel.SetActive(false);
        GameOverPanel.SetActive(true);

        if (player.GetComponent<PlayerController>().score.Maxscore < player.GetComponent<PlayerController>().curScore)
        {
            player.GetComponent<PlayerController>().score.Maxscore = player.GetComponent<PlayerController>().curScore;
            curScore.text = player.GetComponent<PlayerController>().score.Maxscore.ToString();
            Save();
        }
        else
        {
            curScore.text = player.GetComponent<PlayerController>().score.Maxscore.ToString();
        }
    }

    public void GameExit()
    {
        Save();
        Application.Quit();
    }

    public void GamePause(bool state)
    {
        optionObject.SetActive(state);
        isPause = state;

        if (isPause)
            Time.timeScale = 0;
        else
            Time.timeScale = 1;
    }

    public void GameSpeed()
    {
        if(Time.timeScale == 1)
        {
            Time.timeScale = 2f;
            speedUpText.text = "2x 배속";
        }
        else
        {
            Time.timeScale = 1f;
            speedUpText.text = "1x 배속";
        }
    }
}
