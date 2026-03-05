using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GuardianTowerStats : MonoBehaviour
{

    public static GuardianTowerStats Instance { get; private set; }

    [Header("Parent")]
    [SerializeField] private DataTowerUI dataTowerUI;

    [Header("Soldier's Price")]
    [SerializeField] private TextMeshProUGUI soldierCount;

    [Header("Status's Tower Text")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;
    [SerializeField] private TextMeshProUGUI fixedCostText;

    [Header("Status's Guardian Text")]
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI attackSpeedText;
    [SerializeField] private TextMeshProUGUI healthGuardianText;
    [SerializeField] private TextMeshProUGUI respawnTimerText;

    private void Awake() {
        Instance = this;
    }

    public void UpdateVisual() {

        // 0. Get Data 
        GuardianTowerSO guardianTowerSO = dataTowerUI.GetCurrentTowerDictionary().towerSO as GuardianTowerSO;
        ITowerObject.LevelTower currentLevel = dataTowerUI.GetCurrentLevelIndex();

        GuardianTowerLevelData currentData = new GuardianTowerLevelData();

        foreach (GuardianTowerLevelData guardianTowerLevelData in guardianTowerSO.towerLevelDataList) {

            if (guardianTowerLevelData.levelTower == currentLevel) {

                currentData = guardianTowerLevelData;
                break;
            }

        }

        // 1. Update count of guardian base on level
        soldierCount.text = $"Count: {currentData.countOfGuardian}";

        // 2. Update Tower Data
        priceText.text = $"{guardianTowerSO.price}$";
        healthText.text = $"{guardianTowerSO.healthTower}";
        levelText.text = $"{currentLevel}";

        if (currentLevel == ITowerObject.LevelTower.Level1) {
            rangeText.text = $"{guardianTowerSO.baseRange}";
            upgradeCostText.text = $"Default";
        }
        else {
            rangeText.text = $"{currentData.attackRange}";
            upgradeCostText.text = $"{currentData.upgradeCost}$";
        }

        rangeText.text = $"{currentData.attackRange}";
        upgradeTimerText.text = $"{currentData.upgradeTimer}s";
        fixedCostText.text = $"{guardianTowerSO.fixedCost}";

        // 3. Update Guardian Data
        damageText.text = $"{currentData.attackDamage}";
        attackSpeedText.text = $"{currentData.attackSpeed}";
        healthGuardianText.text = $"{currentData.healthOfGuardian}";
        respawnTimerText.text = $"{currentData.RespawnTimer}s";
    }
}
