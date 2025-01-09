using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponDetails wepDetails;
    public int wepListPosition;
    public float wepReloadTimer;
    public int wepClipRemainingAmmo;
    public int wepRemainingAmmo;
    public bool isReloading;
}
