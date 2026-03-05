using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MageTowerVisual : MonoBehaviour
{
    [SerializeField] private MageTower mageTower;
    [SerializeField] private Transform attackZoneVisual;

    private const string TRIGGER_UPGRADE_LEVEL_2 = "TriggerUpagradeLevel2";
    private const string TRIGGER_UPGRADE_LEVEL_3 = "TriggerUpagradeLevel3";
    private const string TRIGGER_UPGRADE_LEVEL_4 = "TriggerUpagradeLevel4";

    private Animator animator;

    private void Awake() {
        animator = GetComponent<Animator>();

    }

    private void Start() {

        mageTower.OnUpgradeLevel2 += MageTower_OnUpgradeLevel2;
        mageTower.OnUpgradeLevel3 += MageTower_OnUpgradeLevel3;
        mageTower.OnUpgradeLevel4 += MageTower_OnUpgradeLevel4;

    }

    private void OnDestroy() {

        mageTower.OnUpgradeLevel2 -= MageTower_OnUpgradeLevel2;
        mageTower.OnUpgradeLevel3 -= MageTower_OnUpgradeLevel3;
        mageTower.OnUpgradeLevel4 -= MageTower_OnUpgradeLevel4;

    }

    private void MageTower_OnUpgradeLevel4(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_4);
    }

    private void MageTower_OnUpgradeLevel3(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_3);

    }

    private void MageTower_OnUpgradeLevel2(object sender, System.EventArgs e) {

        animator.SetTrigger(TRIGGER_UPGRADE_LEVEL_2);

    }
}
