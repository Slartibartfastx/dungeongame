using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int startHP;
    private int currentHP;

    public void setStartingHP(int startHP)
    {
        this.startHP = startHP;
        currentHP = startHP;
    }

    public int getStartingHP()
    {
        return startHP;

    }

}