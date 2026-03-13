using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuyArcherUI : MonoBehaviour, IHasFunctionButton
{
    public event EventHandler OnFunctionButton;
    public event EventHandler<IHasFunctionButton.OffFunctionButtonEventArgs> OffFunctionButton;

    [Header("Arrow Button")]
    [SerializeField] private Button upArrowButton;
    [SerializeField] private Button rightArrowButton;
    [SerializeField] private Button downArrowButton;
    [SerializeField] private Button leftArrowButton;

    [Header("Text")]
    [SerializeField] private List<TextMeshProUGUI> priceTextList;

    private ArcherTower archerTower;
    private ArcherTowerUI archerTowerUI;

    private void Awake() {

        this.archerTower = GetComponentInParent<ArcherTower>();
        this.archerTowerUI = this.archerTower.GetComponentInChildren<ArcherTowerUI>();

        // Spawn Archer Button
        upArrowButton.onClick.AddListener(() => {
            BuyArcher(SoldierSO.SoldierDirection.Up);
        });

        rightArrowButton.onClick.AddListener(() => {
            BuyArcher(SoldierSO.SoldierDirection.Right);
        });

        downArrowButton.onClick.AddListener(() => {

            BuyArcher(SoldierSO.SoldierDirection.Down);
        });

        leftArrowButton.onClick.AddListener(() => {

            BuyArcher(SoldierSO.SoldierDirection.Left);

        });

    }

    private void Start() {

        archerTower.UnBuyArcherUI += ArcherTower_UnBuyArcherUI;
        archerTower.DeselectedAllUI += ArcherTower_DeselectedAllUI;

        // After spawn
        OnFunctionButton?.Invoke(this, EventArgs.Empty);

        UpdateVisual();
    }

    private void OnDestroy() {
        archerTower.UnBuyArcherUI -= ArcherTower_UnBuyArcherUI;
        archerTower.DeselectedAllUI -= ArcherTower_DeselectedAllUI;
    }

    private void ArcherTower_DeselectedAllUI(object sender, EventArgs e) {
        // When deselected all UI

        if (this.gameObject.activeSelf) {
            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = OnDestroySelf });
        }

    }

    private void ArcherTower_UnBuyArcherUI(object sender, EventArgs e) {
        OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = OnDestroySelf });
    }


    private void OnDestroySelf() {
        Destroy(this.gameObject);
    }

    private void BuyArcher(SoldierSO.SoldierDirection archerDirection) {

        if (LevelManager.Instance.GetCurrentCoin() >= archerTower.GetArcherTowerSO().priceArcher) {

            Archer archer = Archer.SpawnArcher(archerTower.GetArcherTowerSO().soldierManagerSO, archerTower.GetSpawnPoint(archerDirection), archerDirection);

            archerTower.AddToArcherList(archer);

            UpdateVisual();

            LevelManager.Instance.ChangedCoinTo(ILevelManager.CoinChangedState.Decrease, archerTower.GetArcherTowerSO().priceArcher);
        }

    }

    private void UpdateVisual() {

        List<Archer> archerList = archerTower.GetArcherList();

        if (archerList.Count > 0 && archerList.Count < 4) {
            // If number of archer in list > 0 and < 4

            foreach (Archer archer in archerList) {

                if (archer.GetArcherDirection() == SoldierSO.SoldierDirection.Up) {
                    // Nếu đã có archer hướng Up

                    upArrowButton.gameObject.SetActive(false);

                }

                if (archer.GetArcherDirection() == SoldierSO.SoldierDirection.Right) {
                    // Nếu đã có archer hướng Right

                    rightArrowButton.gameObject.SetActive(false);

                }

                if (archer.GetArcherDirection() == SoldierSO.SoldierDirection.Down) {
                    // Nếu đã có archer hướng Down

                    downArrowButton.gameObject.SetActive(false);

                }

                if (archer.GetArcherDirection() == SoldierSO.SoldierDirection.Left) {
                    // Nếu đã có archer hướng Left

                    leftArrowButton.gameObject.SetActive(false);

                }

            }
        }
        else if (archerList.Count == 4) {
            // If number of archer in list = 4

            archerTowerUI.ShowUI();

            OffFunctionButton?.Invoke(this, new IHasFunctionButton.OffFunctionButtonEventArgs { callbackAction = OnDestroySelf });

        }
        else {
            // If number of archer in list = 0

            upArrowButton.gameObject.SetActive(true);
            rightArrowButton.gameObject.SetActive(true);
            downArrowButton.gameObject.SetActive(true);
            leftArrowButton.gameObject.SetActive(true);

        }

        foreach (TextMeshProUGUI priceText in priceTextList) {
            priceText.text = $"{this.archerTower.GetArcherTowerSO().priceArcher}$";
        }
    }

    public static BuyArcherUI SpawnBuyArcherUI(Transform buyArcherUIPrefab, Transform parent) {

        Transform buyArcherUITranform = Instantiate(buyArcherUIPrefab, parent);
        buyArcherUITranform.localPosition = Vector3.zero;

        BuyArcherUI buyArcherUI = buyArcherUITranform.GetComponent<BuyArcherUI>();

        return buyArcherUI;
    }

}
