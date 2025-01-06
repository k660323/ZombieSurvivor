using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    public GameManager gameManager;
    public GameObject UIObject;
    PlayerController enterplayer;
    public GameObject[] itemObject;
    public int[] itemOriginPrice;
    public int[] totalitemPrice;
    public Transform[] itemPos;
    public Text[] itemPriceText;
    public Text[] itemLevelText;
    public string[] talkData;
    public Text talkText;
    IEnumerator Coroutine;
    [SerializeField]
    AudioClip[] audioClip;

    public bool isUpgradeShop;

    public void Awake()
    {
        totalitemPrice = new int[itemOriginPrice.Length];
        for (int i = 0; i < itemOriginPrice.Length; i++)
            totalitemPrice[i] = itemOriginPrice[i];
    }

    public void Enter(PlayerController player)
    {
        enterplayer = player;
        if (isUpgradeShop)
            InitUpgradePrice();
        UIObject.SetActive(true);
    }

    public void Exit()
    {
        SoundManager.instance.SFXPlay(audioClip[2]);
        enterplayer = null;
        UIObject.SetActive(false);
    }

    public void PurchaseItem(int index)
    {
        int price = totalitemPrice[index];
        if(price > enterplayer.coin)
        {
            Coroutine = Talk(1);
            StopCoroutine(Coroutine);
            StartCoroutine(Coroutine);
            return;
        }
        SoundManager.instance.SFXPlay(audioClip[0]);
        enterplayer.coin -= price;
        Vector3 ranVec = Vector3.right * Random.Range(-3, 3)
                       + Vector3.forward * Random.Range(-3, 3);
        int spawn = Random.Range(0, 3);
        Instantiate(itemObject[index], itemPos[spawn].position + ranVec, itemPos[spawn].rotation);
    }

    public void EmergencyArmorPurcahse()
    {
        enterplayer = gameManager.player.GetComponent<PlayerController>();
        if (enterplayer == null)
            return;

        int price = totalitemPrice[1] * 2;
        if (price > enterplayer.coin)
        {
            return;
        }

        if (enterplayer.equipWeapon.type == Weapon.Type.Range || enterplayer.equipWeapon.type == Weapon.Type.SubRange)
        {
            enterplayer.coin -= price;
            Item item = itemObject[1].GetComponent<Item>();
            enterplayer.curAmmo += item.curAmmo;
            SoundManager.instance.SFXPlay(audioClip[0]);
        }
        else if(enterplayer.equipWeapon.type == Weapon.Type.Bow)
        {
            enterplayer.coin -= price;
            Item item = itemObject[1].GetComponent<Item>();
            enterplayer.curArrow += item.curAmmo;
            SoundManager.instance.SFXPlay(audioClip[0]);
        }      
    }

    public void InitUpgradePrice()
    {
        if(enterplayer.hpLevel == 10)
        {
            itemObject[0].GetComponent<Button>().interactable=false;
        }
        if(enterplayer.speedLevel == 5)
        {
            itemObject[1].GetComponent<Button>().interactable = false;
        }    

        totalitemPrice[0] = itemOriginPrice[0] + (enterplayer.hpLevel * 500);
        itemPriceText[0].text = totalitemPrice[0].ToString();
        itemLevelText[0].text = "Lv "+enterplayer.hpLevel;

        totalitemPrice[1] = itemOriginPrice[1] + (enterplayer.speedLevel * 500);
        itemPriceText[1].text = totalitemPrice[1].ToString();
        itemLevelText[1].text = "Lv " + enterplayer.speedLevel;

        if (enterplayer.equipWeapon != null && enterplayer.equipWeapon.type != Weapon.Type.Throw)
        {
            if (enterplayer.equipWeapon.ammoLevel == 3 || enterplayer.equipWeapon.type == Weapon.Type.Melee || enterplayer.equipWeapon.type == Weapon.Type.Bow)
            {
                itemObject[2].GetComponent<Button>().interactable = false;
            }
            else
            {
                itemObject[2].GetComponent<Button>().interactable = true;
            }

            if (enterplayer.equipWeapon.damageLevel == 10)
            {
                itemObject[3].GetComponent<Button>().interactable = false;
            }
            else
            {
                itemObject[3].GetComponent<Button>().interactable = true;
            }
            if (enterplayer.equipWeapon.rateLevel == 10)
            {
                itemObject[4].GetComponent<Button>().interactable = false;
            }
            else
            {
                itemObject[4].GetComponent<Button>().interactable = true;
            }

            totalitemPrice[2] = itemOriginPrice[2] + (enterplayer.equipWeapon.ammoLevel * 1000);
            itemPriceText[2].text = totalitemPrice[2].ToString();
            itemLevelText[2].text = "Lv " + enterplayer.equipWeapon.ammoLevel;

            totalitemPrice[3] = itemOriginPrice[3] + (enterplayer.equipWeapon.damageLevel * 500);
            itemPriceText[3].text = totalitemPrice[3].ToString();
            itemLevelText[3].text = "Lv " + enterplayer.equipWeapon.damageLevel;

            totalitemPrice[4] = itemOriginPrice[4] + (enterplayer.equipWeapon.rateLevel * 500);
            itemPriceText[4].text = totalitemPrice[4].ToString();
            itemLevelText[4].text = "Lv " + enterplayer.equipWeapon.rateLevel;
        }
        else
        {
            itemObject[2].GetComponent<Button>().interactable = false;

            itemObject[3].GetComponent<Button>().interactable = false;

            itemObject[4].GetComponent<Button>().interactable = false;
        }
    }

    public void UpgradeItem(int index)
    {
        int price = totalitemPrice[index];
        if (price > enterplayer.coin)
        {
            Coroutine = Talk(1);
            StopCoroutine(Coroutine);
            StartCoroutine(Coroutine);
            return;
        }
        
        SoundManager.instance.SFXPlay(audioClip[0]);
        enterplayer.coin -= price;

        switch (index)
        {
            case 0:
                enterplayer.hpLevel++;
                enterplayer.maxHealth += 10;
                break;
            case 1:
                enterplayer.speedLevel++;
                enterplayer.moveSpeed += 0.1f;
                break;
            case 2:
                if (enterplayer.equipWeapon == null && enterplayer.equipWeapon.type == Weapon.Type.Throw)
                {
                    Coroutine = Talk(2);
                    StopCoroutine(Coroutine);
                    StartCoroutine(Coroutine);
                    InitUpgradePrice();
                    return;
                }
                enterplayer.equipWeapon.ammoLevel++;
                enterplayer.equipWeapon.maxAmmo += 5;
                break;
            case 3:
                if (enterplayer.equipWeapon == null && enterplayer.equipWeapon.type == Weapon.Type.Throw)
                {
                    Coroutine = Talk(2);
                    StopCoroutine(Coroutine);
                    StartCoroutine(Coroutine);
                    InitUpgradePrice();
                    return;
                }
                enterplayer.equipWeapon.damageLevel++;
                enterplayer.equipWeapon.damage += (enterplayer.equipWeapon.damage * enterplayer.equipWeapon.upgradeDamage);
                break;
            case 4:
                if (enterplayer.equipWeapon == null && enterplayer.equipWeapon.type == Weapon.Type.Throw)
                {
                    Coroutine = Talk(2);
                    StopCoroutine(Coroutine);
                    StartCoroutine(Coroutine);
                    InitUpgradePrice();
                    return;
                }
                enterplayer.equipWeapon.rateLevel++;
                enterplayer.equipWeapon.rate += enterplayer.equipWeapon.upgradeRate;
                break;
        }
        InitUpgradePrice();
    }

    IEnumerator Talk(int index)
    {
        SoundManager.instance.SFXPlay(audioClip[1]);
        talkText.text = talkData[index];
        yield return new WaitForSeconds(2.0f);
        talkText.text = talkData[0];
    }
}
