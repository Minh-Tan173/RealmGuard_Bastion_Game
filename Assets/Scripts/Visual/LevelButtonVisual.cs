using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonVisual : MonoBehaviour
{
    [SerializeField] private LevelButton levelButton;

    [Header("Child visual")]
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private Image bossImage;

    private void Awake() {

    }

    private void Start() {

        UpdateVisual();
    }

    private void UpdateVisual() {

        LevelManagerSO levelManagerSO = levelButton.GetLevelManagerSO();
        ILevelManager.GameLevel gameLevel = levelManagerSO.gameLevel;


        if (gameLevel != ILevelManager.GameLevel.Level5) {
            ShowLevelText();

            if (gameLevel == ILevelManager.GameLevel.Level1) {
                levelText.text = "1";
            }
            if (gameLevel == ILevelManager.GameLevel.Level2) {
                levelText.text = "2";
            }
            if (gameLevel == ILevelManager.GameLevel.Level3) {
                levelText.text = "3";
            }
            if (gameLevel == ILevelManager.GameLevel.Level4) {
                levelText.text = "4";
            }

        }
        else {
            ShowLevelBoss();
        }

    }

    private void ShowLevelText() {
        levelText.gameObject.SetActive(true);
        bossImage.gameObject.SetActive(false);
    }
    private void ShowLevelBoss() {
        levelText.gameObject.SetActive(false);
        bossImage.gameObject.SetActive(true);
    }
}
