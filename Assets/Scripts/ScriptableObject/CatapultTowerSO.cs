using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class CatapultTowerSO : TowerSO {

    [Header("Layer")]
    public LayerMask canAttackLayer;

    [Header("Blind Spot Litmit")]
    public float blindSpotRange;

    [Header("Status by Level")]
    public List<CatapultTowerLevelData> towerLevelDataList;
}

[System.Serializable]
public class CatapultTowerLevelData {

    [Header("Current Level")]
    public ITowerObject.LevelTower levelTower;

    [Header("Upgrade progress")]
    public float upgradeTimer;
    public float upgradeCost;

    [Header("Status")]
    public float attackDamage;
    public float attackRange;
    public float pebbleSpeed;
    public int pebblePerVolley;
    public float pebbleSplashRadius;
    public float cooldownTimer;
    public Vector3 catapulSpawnPos;
}
