using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class WeaponFiredEvent : MonoBehaviour
{
    public event Action<WeaponFiredEvent, WeaponFiredEventArgs> OnWeaponFired;

    public void CallWeaponFiredEvent(Weapon wep)
    {
        OnWeaponFired?.Invoke(this, new WeaponFiredEventArgs() { weapon = wep });
    }
}

public class WeaponFiredEventArgs : EventArgs
{
    public Weapon weapon;
}