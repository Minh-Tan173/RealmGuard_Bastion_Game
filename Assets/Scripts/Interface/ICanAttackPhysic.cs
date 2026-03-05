using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICanAttackPhysic
{
    public void SetLockedTarget(bool isLockedTarget, Guardian guardian);

    public bool HasLockedTarget();

}
