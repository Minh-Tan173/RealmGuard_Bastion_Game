using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static ModeSelectUI;

public class ModeSelectUI : MonoBehaviour
{
    public static ModeSelectUI Instance { get; private set; }

    public enum GameMode {
        Campaign,
        Custom
    }

    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button campaignButton;
    [SerializeField] private Button customButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Check Box")]
    [SerializeField] private Transform checkBox;

    private MainMenuUI mainMenuUI;
    private CanvasGroup canvasGroup;
    private GameMode currentGameMode;

    private void Awake() {

        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();

        closeButton.onClick.AddListener(() => {

            StartCoroutine(DespawnCoroutine());

        });

        campaignButton.onClick.AddListener(() => {
            HandleModeClicked(GameMode.Campaign);
        });

        customButton.onClick.AddListener(() => {
            HandleModeClicked(GameMode.Custom);
        });

        yesButton.onClick.AddListener(() => {

            if (this.currentGameMode == GameMode.Campaign) {
                // Show CampaignMapUI

                SaveData.ResetDataForNewGame(mainMenuUI.GetGameDataTemplateSO());

                SaveData.SetHasSavedGame();

                MainMenuUI.Instance.UpdateVisual();

                Instantiate(MainMenuUI.Instance.GetSceneMangerSO().campaignMapUI, MainMenuUI.Instance.transform.parent);
            }

            if (this.currentGameMode == GameMode.Custom) {
                // Show CustomMapUI


            }

            HideCheckBox();
            StartCoroutine(DespawnCoroutine());

        });

        noButton.onClick.AddListener(() => {
            HideCheckBox();
        });

    }

    private void Start() {

        // After spawn
        StartCoroutine(SpawnCoroutine());

        checkBox.gameObject.SetActive(false);
    }

    private void HandleModeClicked(GameMode gameMode) {
        bool isAlreadyOpen = checkBox.gameObject.activeSelf;

        if (isAlreadyOpen && this.currentGameMode != gameMode) {
            // If CheckBox was open and switch to other gameMode

            this.currentGameMode = gameMode;

            RefreshCheckBox();

        }
        else if(!isAlreadyOpen) {
            // If CheckBox isn't opened

            this.currentGameMode = gameMode;

            ShowCheckBox();
        }
    }

    private IEnumerator SpawnCoroutine() {

        float spawnTimer = 0f;
        float spawnTimerMax = 0.2f;

        // 1. Start
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;

        // 2. Progress
        while(spawnTimer <= spawnTimerMax) {

            float alphaValue = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(spawnTimer / spawnTimerMax));

            canvasGroup.alpha = alphaValue;

            spawnTimer += Time.deltaTime;

            yield return null;
        }

        // 3. End
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    private IEnumerator DespawnCoroutine() {

        float despawnTimerMax = 0.2f;
        float despawnTimer = despawnTimerMax;

        // 1. Start
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = false;

        // 2. Progress
        while (despawnTimer >= 0f) {

            float alphaValue = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(despawnTimer / despawnTimerMax));

            canvasGroup.alpha = alphaValue;

            despawnTimer -= Time.deltaTime;

            yield return null;
        }

        // 3. End
        canvasGroup.alpha = 0f;

        yield return null;

        Destroy(this.gameObject);
    }

    private void ShowCheckBox() {
        checkBox.gameObject.SetActive(true);

        checkBox.DOKill();

        // DOTween Animation: Từ 0 bung lên 1 (Nảy nhẹ)
        checkBox.localScale = Vector3.zero;
        checkBox.DOScale(0.7f, 0.4f).SetEase(Ease.OutBack);
    }

    private void HideCheckBox() {
        checkBox.DOKill();

        // DOTween Animation: Từ 1 thu về 0 (Nhanh gọn)
        checkBox.DOScale(0f, 0.2f).SetEase(Ease.InBack).OnComplete(() => checkBox.gameObject.SetActive(false));
    }

    private void RefreshCheckBox() {

        checkBox.DOKill();

        // Sequence: Thu nhỏ nhanh -> Bung ra lại
        Sequence seq = DOTween.Sequence();
        seq.Append(checkBox.DOScale(0f, 0.1f).SetEase(Ease.Linear)); // Thu vào
        seq.Append(checkBox.DOScale(0.7f, 0.3f).SetEase(Ease.OutBack)); // Bung ra
    }

    public void SetMainMenuUI(MainMenuUI mainMenuUI) {
        this.mainMenuUI = mainMenuUI;
    }

    public static void SpawnModeSelectUI(Transform prefab, MainMenuUI mainMenuUI) {

        Transform modeSelectUITransform = Instantiate(prefab, mainMenuUI.transform.parent);

        ModeSelectUI modeSelectUI = modeSelectUITransform.GetComponent<ModeSelectUI>();

        modeSelectUI.SetMainMenuUI(mainMenuUI);

    }
}
