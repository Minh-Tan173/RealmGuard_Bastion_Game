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
    [SerializeField] private TextMeshProUGUI soldierPrice;

    [Header("Status Tower Text")]
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI rangeText;
    [SerializeField] private TextMeshProUGUI upgradeTimerText;
    [SerializeField] private TextMeshProUGUI upgradeCostText;

    [Header("Status Mage Text")]
    [SerializeField] private TextMeshProUGUI viewAngleText;
    [SerializeField] private TextMeshProUGUI damageText;
    //[SerializeField] private TextMeshProUGUI boltSpeed;
    //[SerializeField] private TextMeshProUGUI boltSize;
    [SerializeField] private TextMeshProUGUI cdAttack;

    private void Awake() {

        Instance = this;
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

        // 2. Update Price Mage
        soldierPrice.text = $"{mageTowerSO.priceMage}";

        // 2. Update Tower Status
        priceText.text = $"{mageTowerSO.price}";
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

        // 3. Update Soldier Status
        viewAngleText.text = $"{mageTowerSO.viewAngle}";
        damageText.text = $"{currentData.attackDamage}";
        //boltSpeed.text = $"{currentData.projectileSpeed}";
        //boltSize.text = $"{currentData.projectileSize}";
        cdAttack.text = $"{currentData.recoilTimer}";
    }

}
