using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArcherTowerVisual : MonoBehaviour
{
    [SerializeField] private ArcherTower archerTower;
    [SerializeField] private Transform attackZoneVisual;


    private const string TRIGGER_UPGRADE_LEVEL_2 = "TriggerUpagradeLevel2";
    private const string TRIGGER_UPGRADE_LEVEL_3 = "TriggerUpagradeLevel3";
    private const string TRIGGER_UPGRADE_LEVEL_4 = "TriggerUpagradeLevel4";
    
    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();

    }

    private void Start() {

        archerTower.OnUpgradeLevel2 += ArcherTower_OnUpgradeLevel2;
        archerTower.OnUpgradeLevel3 += ArcherTower_OnUpgradeLevel3;
        archerTower.OnUpgradeLevel4 += ArcherTower_OnUpgradeLevel4;

    }

    private void OnDestroy() {

        archerTower.OnUpgradeLevel2 -= ArcherTower_OnUpgradeLevel2;
        archerTower.OnUpgradeLevel3 -= ArcherTower_OnUpgradeLevel3;
        archerTower.OnUpgradeLevel4 -= ArcherTower_OnUpgradeLevel4;

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
