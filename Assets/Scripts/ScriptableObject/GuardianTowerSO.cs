using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class GuardianTowerSO : TowerSO
{
    [Header("Layer")]
    public LayerMask canAttackLayer;

    [Header("Soldier Base Data")]
    public SoldierSO guardianSO;
    public float damageBonusRate;

    [Header("Tower Status")]
    public float delayDeployGuardianTimer;
    public List<GuardianTowerLevelData> towerLevelDataList;

}

[System.Serializable]
public class GuardianTowerLevelData {

    public ITowerObject.LevelTower levelTower;

    public Sprite currentSprite;

    [Header("Upgrade Progress")]
    public float upgradeTimer;
    public float upgradeCost;

    [Header("Status")]
    public int countOfGuardian;
    public float healthOfGuardian;
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float respawnTimer;
}
