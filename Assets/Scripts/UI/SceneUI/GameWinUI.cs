using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWinUI : MonoBehaviour
{

    public event EventHandler OnGameWinSFX;
    public event EventHandler OnClickedButtonSFX;

    [Header("Main Panel")]
    [SerializeField] private RectTransform gameWinPanelRect;

    [Header("Position")]
    [SerializeField] private Vector3 firstPosition;
    [SerializeField] private Vector3 endPosition;

    [Header("Button")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI rewardValueText;

    [Header("Dotween Data")]
    [SerializeField] private float dropDuration;
    [SerializeField] private Vector3 punchStrength;
    [SerializeField] private float punchDuration;

    private CanvasGroup canvasGroup;

    private void Awake() {

        canvasGroup = GetComponent<CanvasGroup>();

        restartButton.onClick.AddListener(() => {

            OnClickedButton(() => {

                Loader.Load(SceneManager.GetActiveScene().ToString());
            });

        });

        mainMenuButton.onClick.AddListener(() => {

            OnClickedButton(() => {

                Loader.Load(Loader.Scene.MainMenu);
            });
        });
    }

    private void Start() {

        GlobalVolumeManager.Instance.SetOnBlurBackground();

        // After Spawn
        SpawnAnimation();
    }

    private void SpawnAnimation() {

        // 0. Update Total Point
        rewardValueText.text = $"{LevelManager.Instance.GetLevelManagerSO().pointGetWhenWin}";

        int currentPoint = SaveData.GetGameStatus().totalPoints;
        int newTotalPoint = currentPoint + LevelManager.Instance.GetLevelManagerSO().pointGetWhenWin;

        SaveData.SetNewTotalPoint(newTotalPoint);

        // 1. Setup
        gameWinPanelRect.anchoredPosition = firstPosition;
        canvasGroup.interactable = false;

        OnGameWinSFX?.Invoke(this, EventArgs.Empty);

        // 2. Dotween anim
        Sequence spawnSequence = DOTween.Sequence();
        spawnSequence.Append(gameWinPanelRect.DOAnchorPos(endPosition, dropDuration).SetEase(Ease.InQuad));
        spawnSequence.Append(gameWinPanelRect.DOPunchScale(punchStrength, punchDuration)).AppendCallback(() => {
            canvasGroup.interactable = true;
        });
    }

    private void OnClickedButton(Action action) {

        // 1.
        canvasGroup.interactable = false;
        gameWinPanelRect.anchoredPosition = endPosition;

        OnClickedButtonSFX?.Invoke(this, EventArgs.Empty);

        // 2. Dotween anim
        Sequence resetGameSequence = DOTween.Sequence();
        resetGameSequence.Append(gameWinPanelRect.DOPunchScale(punchStrength, punchDuration));
        resetGameSequence.Append(gameWinPanelRect.DOAnchorPos(firstPosition, dropDuration).SetEase(Ease.InQuad)).OnComplete(() => {
            action?.Invoke();
        });

    }

}
