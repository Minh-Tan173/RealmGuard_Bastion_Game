using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTowerVisual : MonoBehaviour
{
    [SerializeField] private CatapultTower catapultTower;
    [SerializeField] private Transform attackZoneVisual;

    private const string TRIGGER_UPGRADE_LEVEL_1 = "TriggerUpagradeLevel1";
    private const string TRIGGER_UPGRADE_LEVEL_2 = "TriggerUpagradeLevel2";
    private const string TRIGGER_UPGRADE_LEVEL_3 = "TriggerUpagradeLevel3";
    private const string TRIGGER_UPGRADE_LEVEL_4 = "TriggerUpagradeLevel4";

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {
        catapultTower.OnUpgradeLevel2 += CatapultTower_OnUpgradeLevel2;
        catapultTower.OnUpgradeLevel3 += CatapultTower_OnUpgradeLevel3;
        catapultTower.OnUpgradeLevel4 += CatapultTower_OnUpgradeLevel4;
    }

    private void OnDestroy() {

        catapultTower.OnUpgradeLevel2 -= CatapultTower_OnUpgradeLevel2;
        catapultTower.OnUpgradeLevel3 -= CatapultTower_OnUpgradeLevel3;
        catapultTower.OnUpgradeLevel4 -= CatapultTower_OnUpgradeLevel4;
    }

    private void CatapultTower_OnUpgradeLevel4(object sender, System.EventArgs e) {
        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_4);
    }

    private void CatapultTower_OnUpgradeLevel3(object sender, System.EventArgs e) {
        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_3);
    }

    private void CatapultTower_OnUpgradeLevel2(object sender, System.EventArgs e) {
        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_2);
    }
}
