using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFireable
{
    void InitialiseAmmo(AmmoDetails ammoDetalis, float angle, float aimAngle, float speed, Vector3 aimDirVec, bool overrideAmmoMovement = false);

    GameObject GetGameObject();
}


