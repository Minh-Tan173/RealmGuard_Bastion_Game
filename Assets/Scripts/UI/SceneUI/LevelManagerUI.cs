using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelManagerUI : MonoBehaviour, IHasProgressBar
{
    public static LevelManagerUI Instance { get; private set; }

    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler<OnClickedButtonEventArgs> OnClickedButton;
    public class OnClickedButtonEventArgs : EventArgs {
        public Button buttonClicked;
    }

    [Header("Data")]
    [SerializeField] private TowerManagerSO towerManagerSO;
    [SerializeField] private AbilityManagerSO abilitySO;

    [Header("Menu Button")]
    [SerializeField] private Button gamePauseButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI numberOfWaveText;
    [SerializeField] private TextMeshProUGUI numberOfCoinText;
    [SerializeField] private TextMeshProUGUI heathText;

    private CanvasGroup canvasGroup;

    private void Awake() {

        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();

        // After spawn - Hide
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        // Menu Button
        gamePauseButton.onClick.AddListener(() => {
            LevelManager.Instance.ToggleGamePause();
        });

    }

    private void Start() {

        LevelManager.Instance.GameSetupDone += LevelManager_GameSetupDone;
        LevelManager.Instance.HeartChanged += LevelManager_HeartChanged;
        LevelManager.Instance.OnGameRunningState += LevelManager_OnGameRunningState;

    }

    private void OnDestroy() {
        LevelManager.Instance.GameSetupDone -= LevelManager_GameSetupDone;
        LevelManager.Instance.HeartChanged -= LevelManager_HeartChanged;
        LevelManager.Instance.OnGameRunningState -= LevelManager_OnGameRunningState;
    }


    private void LevelManager_GameSetupDone(object sender, EventArgs e) {

        this.gameObject.SetActive(true);

        StartCoroutine(ShowCoroutine());
    }

    private void LevelManager_OnGameRunningState(object sender, EventArgs e) {
        // After new turn start

        UpdateVisual();
    }

    private void LevelManager_HeartChanged(object sender, EventArgs e) {

        float currentHeart = LevelManager.Instance.GetCurrentHeart();
        float maxNumberOfHeart = LevelManager.Instance.GetLevelManagerSO().numberOfHeart;

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = currentHeart / maxNumberOfHeart});

        UpdateVisual();
        
    }

    private IEnumerator ShowCoroutine() {
        
        // After Show
        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 1f });

        UpdateVisual();

        yield return null;

        float timerShow = 0f;
        float timerShowMax = 0.1f;

        while (timerShow <= timerShowMax) {

            float t = Mathf.Clamp01(timerShow / timerShowMax);

            canvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);

            timerShow += Time.unscaledDeltaTime;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }


    public void UpdateVisual() {

        // Health
        int currentHealth = Mathf.FloorToInt(LevelManager.Instance.GetCurrentHeart());
        string currentHealthText = "";

        if (currentHealth >= 10) {

            currentHealthText = $"{currentHealth} / {LevelManager.Instance.GetLevelManagerSO().numberOfHeart}";
        }
        else if (currentHealth > 0 && currentHealth < 10){

            currentHealthText = $"0{currentHealth} / {LevelManager.Instance.GetLevelManagerSO().numberOfHeart}";
        }
        else {

            currentHealthText = $"00 / {LevelManager.Instance.GetLevelManagerSO().numberOfHeart}";
        }

        heathText.text = currentHealthText;

        // Coin
        float numberOfCoin = LevelManager.Instance.GetCurrentCoin();
        numberOfCoinText.text = $"{numberOfCoin}";

        // Wave
        numberOfWaveText.text = $"Wave \n {LevelManager.Instance.GetCurrentWave()} / {LevelManager.Instance.GetLevelManagerSO().waveScriptList.Count}";
    }

}
