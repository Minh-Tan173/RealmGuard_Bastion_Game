using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianTowerVisual : MonoBehaviour {
    [SerializeField] private GuardianTower guardianTower;

    private const string TRIGGER_UPGRADE_LEVEL_2 = "TriggerUpagradeLevel2";
    private const string TRIGGER_UPGRADE_LEVEL_3 = "TriggerUpagradeLevel3";
    private const string TRIGGER_UPGRADE_LEVEL_4 = "TriggerUpagradeLevel4";
    private const string TRIGGER_SPAWN_GUARDIAN = "TriggerSpawnGuardian"; // Todo: Chỉ Kích hoạt ở Level 3, 4

    private Animator animator;


    private void Awake() {
        animator = GetComponent<Animator>();
    }

    private void Start() {

        guardianTower.OnUpgradeLevel2 += ArcherTower_OnUpgradeLevel2;
        guardianTower.OnUpgradeLevel3 += ArcherTower_OnUpgradeLevel3;
        guardianTower.OnUpgradeLevel4 += ArcherTower_OnUpgradeLevel4;

    }

    private void OnDestroy() {

        guardianTower.OnUpgradeLevel2 -= ArcherTower_OnUpgradeLevel2;
        guardianTower.OnUpgradeLevel3 -= ArcherTower_OnUpgradeLevel3;
        guardianTower.OnUpgradeLevel4 -= ArcherTower_OnUpgradeLevel4;
    }

    private void ArcherTower_OnUpgradeLevel4(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_4);
    }

    private void ArcherTower_OnUpgradeLevel3(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_3);

    }

    private void ArcherTower_OnUpgradeLevel2(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_2);

    }

}
