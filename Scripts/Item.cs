using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public enum Type
    {
        Heart,
        Ammo,
        Coin,
        MeleeWeapon,
        RangeWeapon,
        BowWeapon,
        subRangeWeapon,
        ThrowWeapon,
        ArrowAmmo
    }
    public Type type;
    public string itemName;
    public int curAmmo;
    public int heart;
    public int coin;


    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * 30 * Time.deltaTime,Space.World);
    }
}
