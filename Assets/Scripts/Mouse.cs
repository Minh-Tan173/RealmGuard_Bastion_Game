using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse : MonoBehaviour
{
    [SerializeField] private GameInput gameInput;
    [SerializeField] private MouseVisual mouseVisual;

    [Header("Layer")]
    [SerializeField] private LayerMask towerLayer;
    [SerializeField] private LayerMask enemyLayer;

    private BaseTower currentBaseTower;

    private void Awake() {

        currentBaseTower = null;

    }

    private void Start() {

        gameInput.OnLeftClicked += GameInput_OnLeftClicked;
        gameInput.OnRightClicked += GameInput_OnRightClicked;

        LevelManagerUI.Instance.OnClickedButton += LevelManagerUI_OnClickedButton;

    }

    private void OnDestroy() {
        gameInput.OnLeftClicked -= GameInput_OnLeftClicked;
        gameInput.OnRightClicked -= GameInput_OnRightClicked;

        LevelManagerUI.Instance.OnClickedButton -= LevelManagerUI_OnClickedButton;
    }

    private void LevelManagerUI_OnClickedButton(object sender, System.EventArgs e) {

        if (this.currentBaseTower != null) {
            this.currentBaseTower.HandleDeselectedAll();
            this.currentBaseTower = null;
        }

    }

    private void GameInput_OnRightClicked(object sender, System.EventArgs e) {
        // When Right clicked

        if (this.currentBaseTower != null) {
            // Has Left Click on a BaseTower

            if (this.currentBaseTower.TryGetComponent<BaseTower>(out BaseTower baseTower)) {

                this.currentBaseTower.HandleRightClicked();

                if (baseTower.IsDeselectLastUI()) {

                    this.currentBaseTower = null;

                }

            }

        }

    }

    private void GameInput_OnLeftClicked(object sender, System.EventArgs e) {
        // When Left clicked

        RaycastHit2D isDetectedTower = Physics2D.Raycast(gameInput.GetMouseWorldPos(), Vector2.zero, Mathf.Infinity, towerLayer);
        
        if (isDetectedTower) {
            // If Left click on a Tower

            if (isDetectedTower.collider.TryGetComponent<BaseTower>(out BaseTower newBaseTower)) {

                if (this.currentBaseTower != newBaseTower) {
                    // If currentBaseTower is not newBaseTower

                    if (this.currentBaseTower != null) {
                        // Only happen is currentBaseTowe != null --> Deselected old Tower

                        this.currentBaseTower.HandleDeselectedAll();
                    }

                    // Change current to newTower
                    this.currentBaseTower = newBaseTower;
                }

                // Selected new Tower
                this.currentBaseTower.HandleLeftClicked();

            }

        }
        else {
            // If not Left click on a Tower

            if (this.currentBaseTower != null) {
                // If have a Tower is selected

                this.currentBaseTower.HandleDeselectedAll();

                this.currentBaseTower = null;
            }

        }

    }

}
