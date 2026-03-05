using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ArcherTowerSO : TowerSO
{

    [Header("Layer")]
    public LayerMask canAttackLayer;

    [Header("Soldier Base Data")]
    public float priceArcher;
    public float viewAngle;

    [Header("Tower Stats")]
    public List<ArcherTowerLevelData> towerLevelDataList;

    [Header("Arrow Stats")]
    public float arrowSpeed;
}

[System.Serializable]
public class ArcherTowerLevelData {
    
    public ITowerObject.LevelTower levelTower;

    public Sprite currentSprite;

    [Header("Upgrade Progress")]
    public float upgradeTimer;
    public float upgradeCost;

    [Header("Status")]
    public float attackDamage;
    public float attackRange;
    public float cooldownTimer;
    public Vector3 archerSpawnPos;

}