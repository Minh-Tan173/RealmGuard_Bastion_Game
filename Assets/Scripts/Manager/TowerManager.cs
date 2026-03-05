using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerManager : MonoBehaviour
{
    public static TowerManager Instance { get; private set; }

    [SerializeField] private TowerManagerSO towerManagerSO;

    private void Awake() {
        Instance = this;
    }

    public TowerSO GetTowerSOByType(ITowerObject.TowerType towerType) {

        if (towerType == ITowerObject.TowerType.ArcherTower) {
            return towerManagerSO.archerTowerSO;
        }

        if (towerType == ITowerObject.TowerType.GuardianTower) {
            return towerManagerSO.guardianTowerSO;
        }

        if (towerType == ITowerObject.TowerType.MageTower) {
            return towerManagerSO.mageTowerSO;
        }

        if (towerType == ITowerObject.TowerType.CatapultTower) {
            return towerManagerSO.catapultTowerSO;
        }

        return null;
    }
}
