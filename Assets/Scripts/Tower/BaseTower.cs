using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseTower : MonoBehaviour
{

    public virtual void HandleLeftClicked() {
        Debug.LogError("Trigger baseTower");
    }

    public virtual void HandleRightClicked() {
        Debug.LogError("Trigger baseTower");
    }

    public virtual void HandleDeselectedAll() {
        Debug.LogError("Trigger baseTower");
    }

    public virtual void HitDamage(float damageGet) {
        Debug.LogError("Trigger baseTower");
    }

    public virtual bool IsDeselectLastUI() {
        Debug.LogError("Trigger baseTower");
        return true;
    }

    public virtual bool IsRecoveringHealth() {
        Debug.LogError("Trigger baseTower");
        return true;
    }

    public static void SpawnTower(Transform tower, Vector3 towerPosition) {

        Transform towerTransform = Instantiate(tower);

        towerTransform.transform.position = towerPosition;

    }
}
