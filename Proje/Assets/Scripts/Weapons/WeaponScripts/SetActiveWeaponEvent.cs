using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveWeapon;

    public void CallSetActiveWeaponEvent(Weapon wep)
    {
        OnSetActiveWeapon?.Invoke(this, new SetActiveWeaponEventArgs() { wep = wep });
    }
}


public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon wep;
}