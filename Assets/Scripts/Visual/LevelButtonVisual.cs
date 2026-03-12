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

        ILevelManager.GameLevel buttonLevel = levelButton.GetLevelData().gameLevel;

        if (buttonLevel != ILevelManager.GameLevel.Level5) {

            ShowLevelText();

            levelText.text = $"{(int)buttonLevel}";
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
