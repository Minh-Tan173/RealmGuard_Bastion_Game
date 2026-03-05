using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class MageTowerSO : TowerSO
{
    public enum MageType {
        Ground = 0,
        Sky = 1,
        River = 2
    }

    [Header("Layer")]
    public LayerMask groundEnemyLayer;
    public LayerMask flyEnemyLayer;

    [Header("Soldier Base Data")]
    public float priceMage;
    public float viewAngle;

    [Header("Tower Stats")]
    public List<MageTowerLevelData> towerLevelDataList;

    [Header("Magic Bolt Stats")]
    public float magicBoltSpeed;
}

[System.Serializable]
public class MageTowerLevelData {

    public ITowerObject.LevelTower levelTower;

    public Sprite currentSprite;

    [Header("Upgrade Progress")]
    public float upgradeTimer;
    public float upgradeCost;

    [Header("Status")]
    public float attackDamage;
    public float attackRange;
    public float recoilTimer;
    public Vector3 mageSpawnPos;

}
