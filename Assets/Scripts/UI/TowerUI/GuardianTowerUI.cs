using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GuardianTowerUI : MonoBehaviour, IHasFunctionButton
{

    public event EventHandler OnFunctionButton;
    public event EventHandler<IHasFunctionButton.OffFunctionButtonEventArgs> OffFunctionButton;
    public event EventHandler OnFixingTower;

    [Header("Parent object")]
    [SerializeField] private GuardianTower guardianTower;

    [Header("Button")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Button assignPositionButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button fixButton;

    private void Awake() {

        upgradeButton.onClick.AddListener(() => {

            if (guardianTower.IsUpgrading()) {
                // If is in upgrade progress
                return;
            }

            int currentLevelIndex = (int)guardianTower.GetCurrentTowerStatus().levelTower;
            int nextLevelIndex = currentLevelIndex + 1;

            guardianTower.ChangeStateTo((ITowerObject.LevelTower)nextLevelIndex);

        });

        assignPositionButton.onClick.AddListener(() => {

            guardianTower.ToggleAssignPositionZone();

            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });
        });


        sellButton.onClick.AddListener(() => {

            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Increase, guardianTower.GetGuardianTowerSO().price);

            GridManager.Instance.SetHasItemArea2x2(this.transform.position, isSetHasItemON: false);

            Destroy(guardianTower.transform.gameObject);

        });

        fixButton.onClick.AddListener(() => {

            // Start fixing
            OnFixingTower?.Invoke(this, EventArgs.Empty);

        });
    }


    private void Start() {

        guardianTower.OnGuardianTowerUI += GuardianTower_OnGuardianTowerUI;
        guardianTower.UnGuardianTowerUI += GuardianTower_UnGuardianTowerUI;
        guardianTower.DeselectedAllUI += GuardianTower_DeselectedAllUI;

        guardianTower.OnDeployPorgress += GuardianTower_OnDeployPorgress;
        guardianTower.UnDeployProgress += GuardianTower_UnDeployProgress;

        HideUI();
    }

    private void OnDestroy() {

        guardianTower.OnGuardianTowerUI -= GuardianTower_OnGuardianTowerUI;
        guardianTower.UnGuardianTowerUI -= GuardianTower_UnGuardianTowerUI;
        guardianTower.DeselectedAllUI -= GuardianTower_DeselectedAllUI;

        guardianTower.OnDeployPorgress -= GuardianTower_OnDeployPorgress;
        guardianTower.UnDeployProgress -= GuardianTower_UnDeployProgress;

    }

    private void GuardianTower_UnDeployProgress(object sender, EventArgs e) {

        // Khi Deploy Gurdiann xong thì tái kích hoạt assign pos button
        assignPositionButton.interactable = true;
    }

    private void GuardianTower_OnDeployPorgress(object sender, EventArgs e) {

        // Khi đang Deploy Gurdian thì vô hiệu hóa assign pos button
        assignPositionButton.interactable = false;

    }

    private void GuardianTower_UnGuardianTowerUI(object sender, EventArgs e) {

        OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });
    }

    private void GuardianTower_OnGuardianTowerUI(object sender, EventArgs e) {
        ShowUI();
    }

    private void GuardianTower_DeselectedAllUI(object sender, EventArgs e) {
        // When deselected all UI

        if (this.gameObject.activeSelf) {
            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = HideUI });

        }
    }

    private void HideUI() {
        this.gameObject.SetActive(false);
    }

    public void ShowUI() {

        this.gameObject.SetActive(true);

        OnFunctionButton?.Invoke(this, EventArgs.Empty);
    }
}
