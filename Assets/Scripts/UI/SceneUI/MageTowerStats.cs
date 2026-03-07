using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MageTowerStats : MonoBehaviour
{
    public static MageTowerStats Instance { get; private set; }

    [Header("Parent")]
    [SerializeField] private DataTowerUI dataTowerUI;

    [Header("Price")]

    [Header("Status Tower Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI fixedCostText;

    [Header("Status Mage Text")]
    [SerializeField] private TextMeshProUGUI viewAngleText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI cdAttack;

    private void Awake() {

        Instance = this;

        dataTowerUI.UpdateLevelData += DataTowerUI_UpdateLevelData;
    }

    private void OnDestroy() {

        dataTowerUI.UpdateLevelData -= DataTowerUI_UpdateLevelData;
    }

    private void DataTowerUI_UpdateLevelData(object sender, DataTowerUI.UpdateLevelDataEventArgs e) {
        
        if (e.towerType == ITowerObject.TowerType.MageTower) {

            UpdateVisual();
        }

    }

    public void UpdateVisual() {

        // 1. Get Data 
        MageTowerSO mageTowerSO = dataTowerUI.GetCurrentTowerDictionary().towerSO as MageTowerSO;
        ITowerObject.LevelTower currentLevel = dataTowerUI.GetCurrentLevelIndex();

        MageTowerLevelData currentData = new MageTowerLevelData();

        foreach (MageTowerLevelData mageTowerLevelData in mageTowerSO.towerLevelDataList) {

            if (mageTowerLevelData.levelTower == currentLevel) {

                currentData = mageTowerLevelData;
                break;
            }

        }

        // 1. Update Tower Status
        healthText.text = $"{mageTowerSO.healthTower}";
        levelText.text = $"{(int)currentLevel}";

        if (currentLevel == ITowerObject.LevelTower.Level1) {
            rangeText.text = $"{mageTowerSO.baseRange}";
            upgradeCostText.text = $"";
        }
        else {
            rangeText.text = $"{currentData.attackRange}";
            upgradeCostText.text = $"{currentData.upgradeCost}";
        }

        upgradeTimerText.text = $"{currentData.upgradeTimer}";
        fixedCostText.text = $"{mageTowerSO.fixedCost}$";

        // 2. Update Soldier Status
        viewAngleText.text = $"{mageTowerSO.viewAngle}";
        damageText.text = $"{currentData.attackDamage}";
        cdAttack.text = $"{currentData.recoilTimer}";
    }

}
