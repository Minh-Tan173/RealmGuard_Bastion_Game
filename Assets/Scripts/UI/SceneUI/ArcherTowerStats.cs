using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ArcherTowerStats : MonoBehaviour
{
    public static ArcherTowerStats Instance { get; private set; }

    [Header("Parent")]
    [SerializeField] private DataTowerUI dataTowerUI;

    [Header("Price")]
    [SerializeField] private TextMeshProUGUI soldierPrice;

    [Header("Status Tower Text")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    [Header("Status Archer Text")]
    [SerializeField] private TextMeshProUGUI viewAngleText;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI cdAttack;

    private void Awake() {
        Instance = this;
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

        // 2. Update Price Data
        soldierPrice.text = $"{archerTowerSO.priceArcher}";

        // 2. Update Tower Status
        priceText.text = $"{archerTowerSO.price}";
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

        // 3. Update Soldier Status
        viewAngleText.text = $"{archerTowerSO.viewAngle}";
        damageText.text = $"{currentData.attackDamage}";
        cdAttack.text = $"{currentData.cooldownTimer}";
    }
}
