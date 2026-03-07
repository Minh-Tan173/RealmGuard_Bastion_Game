using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CatapultTowerStats : MonoBehaviour
{
    public static CatapultTowerStats Instance { get; private set; }

    [Header("Parent")]
    [SerializeField] private DataTowerUI dataTowerUI;

    [Header("Status Tower Text")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI fixedCostText;

    [Header("Status Catapult Text")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI pebblePerVolley;
    [SerializeField] private TextMeshProUGUI splashRadius;
    [SerializeField] private TextMeshProUGUI cdAttack;


    private void Awake() {

        Instance = this;

        dataTowerUI.UpdateLevelData += DataTowerUI_UpdateLevelData;
    }

    private void OnDestroy() {

        dataTowerUI.UpdateLevelData -= DataTowerUI_UpdateLevelData;
    }

    private void DataTowerUI_UpdateLevelData(object sender, DataTowerUI.UpdateLevelDataEventArgs e) {
        
        if (e.towerType == ITowerObject.TowerType.CatapultTower) {

            UpdateVisual();
        }
    }

    public void UpdateVisual() {
        // 1. Get Data 
        CatapultTowerSO catapultTowerSO = dataTowerUI.GetCurrentTowerDictionary().towerSO as CatapultTowerSO;
        ITowerObject.LevelTower currentLevel = dataTowerUI.GetCurrentLevelIndex();

        CatapultTowerLevelData currentData = new CatapultTowerLevelData();

        foreach (CatapultTowerLevelData catapultTowerLevelData in catapultTowerSO.towerLevelDataList) {

            if (catapultTowerLevelData.levelTower == currentLevel) {

                currentData = catapultTowerLevelData;
                break;
            }

        }


        // 1. Update Tower Status
        healthText.text = $"{catapultTowerSO.healthTower}";
        levelText.text = $"{(int)currentLevel}";

        if (currentLevel == ITowerObject.LevelTower.Level1) {
            rangeText.text = $"{catapultTowerSO.baseRange}";
            upgradeCostText.text = $"";
        }
        else {
            rangeText.text = $"{currentData.attackRange}";
            upgradeCostText.text = $"{currentData.upgradeCost}$";
        }

        upgradeTimerText.text = $"{currentData.upgradeTimer}";
        fixedCostText.text = $"{catapultTowerSO.fixedCost}$";

        // 3. Update Soldier Status
        damageText.text = $"{currentData.attackDamage}";
        pebblePerVolley.text = $"{currentData.pebbleSplashRadius}";
        splashRadius.text = $"{currentData.pebbleSplashRadius}";
        cdAttack.text = $"{currentData.recoilTimer}";
    }

}
