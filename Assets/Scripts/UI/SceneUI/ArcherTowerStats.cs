using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArcherTowerStats : MonoBehaviour
{

    [Header("Parent")]
    [SerializeField] private DataTowerUI dataTowerUI;

    [Header("Status Tower Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI fixedCostText;

    [Header("Status Archer Text")]
    [SerializeField] private TextMeshProUGUI viewAngleText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI cdAttack;

    private void Awake() {

        dataTowerUI.UpdateLevelData += DataTowerUI_UpdateLevelData;
    }

    private void OnDestroy() {

        dataTowerUI.UpdateLevelData -= DataTowerUI_UpdateLevelData;
    }

    private void DataTowerUI_UpdateLevelData(object sender, DataTowerUI.UpdateLevelDataEventArgs e) {

        if (e.towerType == ITowerObject.TowerType.ArcherTower) {

            UpdateVisual();
        }
    }

    public void UpdateVisual() {

        // 1. Get Data 
        ArcherTowerSO archerTowerSO = dataTowerUI.GetCurrentTowerDictionary().towerSO as ArcherTowerSO;
        ITowerObject.LevelTower currentLevel = dataTowerUI.GetCurrentLevelIndex();

        ArcherTowerLevelData currentData = new ArcherTowerLevelData();

        foreach (ArcherTowerLevelData archerTowerLevelData in archerTowerSO.towerLevelDataList) {

            if (archerTowerLevelData.levelTower == currentLevel) {

                currentData = archerTowerLevelData;
                break;
            }

        }

        // 1. Update Tower Status
        healthText.text = $"{archerTowerSO.healthTower}";
        levelText.text = $"{(int)currentLevel}";

        if (currentLevel == ITowerObject.LevelTower.Level1) {
            rangeText.text = $"{archerTowerSO.baseRange}";
            upgradeCostText.text = $"";
        }
        else {
            rangeText.text = $"{currentData.attackRange}";
            upgradeCostText.text = $"{currentData.upgradeCost}";
        }

        upgradeTimerText.text = $"{currentData.upgradeTimer}";
        fixedCostText.text = $"{archerTowerSO.fixedCost}$";

        // 2. Update Soldier Status
        viewAngleText.text = $"{archerTowerSO.viewAngle}";
        damageText.text = $"{currentData.attackDamage}";
        cdAttack.text = $"{currentData.cooldownTimer}";
    }
}
