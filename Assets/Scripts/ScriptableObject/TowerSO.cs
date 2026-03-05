using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TowerSO : ScriptableObject
{
    [Header("Base Info")]
    public ITowerObject.TowerType towerType;
    public string nameTower;
    public string description;

    [Header("Visuals & Prefabs")]
    public Transform prefab;
    public Transform icon;

    [Header("Economy")]
    public float price;
    public float fixedCost;

    [Header("Core Stats")]
    public float healthTower;
    public float baseRange; // Range of Tower at Level 1

    [Header("Spawned Units")]
    public SoldierManagerSO soldierManagerSO;

    [Header("Timer")]
    public float scanTimerMax;
    public float placedSFXTimer;
}
