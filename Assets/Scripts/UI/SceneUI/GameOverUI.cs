using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUI : MonoBehaviour
{
    public event EventHandler OnGameOverSFX;
    public event EventHandler OnClickedButtonSFX;

    [Header("Main Panel")]
    [SerializeField] private RectTransform gameOverPanelRect;

    [Header("Position")]
    [SerializeField] private Vector3 firstPosition;
    [SerializeField] private Vector3 endPosition;

    [Header("Button")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

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

        // 1.
        gameOverPanelRect.anchoredPosition = firstPosition;
        canvasGroup.interactable = false;

        OnGameOverSFX?.Invoke(this, EventArgs.Empty);

        // 2. Dotween anim
        Sequence spawnSequence = DOTween.Sequence();
        spawnSequence.Append(gameOverPanelRect.DOAnchorPos(endPosition, dropDuration).SetEase(Ease.InQuad));
        spawnSequence.Append(gameOverPanelRect.DOPunchScale(punchStrength, punchDuration)).AppendCallback(() => {
            canvasGroup.interactable = true;
        });
    }

    private void OnClickedButton(Action action) {

        // 1.
        canvasGroup.interactable = false;
        gameOverPanelRect.anchoredPosition = endPosition;

        OnClickedButtonSFX?.Invoke(this, EventArgs.Empty);

        // 2. Dotween anim
        Sequence resetGameSequence = DOTween.Sequence();
        resetGameSequence.Append(gameOverPanelRect.DOPunchScale(punchStrength, punchDuration));
        resetGameSequence.Append(gameOverPanelRect.DOAnchorPos(firstPosition, dropDuration).SetEase(Ease.InQuad)).OnComplete(() => {
            action?.Invoke();
        });

    }
}
