using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance { get; private set; }

    //public event EventHandler EndScene;

    [SerializeField] private SceneMangerSO sceneMangerSO;
    [SerializeField] private GameDataTemplateSO gameDataTemplateSO;

    [Header("Button")]
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueGameButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button quitGameButton;
    [SerializeField] private Button sFXControlButton;
    [SerializeField] private Button musicControlButton;

    [Header("Fade Anim")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration;

    [Header("Name Game Show Anim")]
    [SerializeField] private RectTransform nameGameRect;
    [SerializeField] private float firstPosYNameGame;
    [SerializeField] private float endPosYNameGame;

    [Header("Button Show Anim")]
    [SerializeField] private GridLayoutGroup buttonGroup;
    [SerializeField] private float showButtonDuration;
    [SerializeField] private float endPosXButton;

    [Header("Mute Image Control")]
    [SerializeField] private Image muteSFX;
    [SerializeField] private Image muteMusic;

    private void Awake() {

        Instance = this;

        SaveData.InitializeIfFirstTime(gameDataTemplateSO);

        // Select Button
        newGameButton.onClick.AddListener(() => {

            ModeSelectUI.SpawnModeSelectUI(sceneMangerSO.modeSelectUI, this);

        });

        continueGameButton.onClick.AddListener(() => {

            Instantiate(sceneMangerSO.campaignMapUI, this.transform.parent);

        });

        creditsButton.onClick.AddListener(() => {

            Instantiate(sceneMangerSO.creditsUI, this.transform.parent);

        });

        quitGameButton.onClick.AddListener(() => {
            Application.Quit();
        });

        // Option Button
        sFXControlButton.onClick.AddListener(ToggleSFXControlButton);
        musicControlButton.onClick.AddListener(ToggleMusicControlButton);

    }

    private void Start() {

        // When start scene
        UpdateVisual();

        StartScene();
    }

    private void StartScene() {

        RectTransform newGameButtonRect = newGameButton.GetComponent<RectTransform>();
        RectTransform continueButtonRect = continueGameButton.GetComponent<RectTransform>();
        RectTransform creditButtonRect = creditsButton.GetComponent<RectTransform>();
        RectTransform quitGameButtonRect = quitGameButton.GetComponent<RectTransform>();


        Sequence startSceneSequence = DOTween.Sequence();

        // 1. Start

        Color tempColor = fadeImage.color;
        tempColor.a = 1f;
        fadeImage.color = tempColor;

        nameGameRect.anchoredPosition = new Vector3(0f, firstPosYNameGame, 0f);
        float showNameGameDuration;

        if (HasSavedGame()) {
            showNameGameDuration = showButtonDuration * 4f / 2f;
        }
        else {
            showNameGameDuration = showButtonDuration * 3f / 2f;
        }

        // 2. Sequence Progress

        startSceneSequence.Append(fadeImage.DOFade(0f, fadeDuration).SetEase(Ease.Linear));

        startSceneSequence.AppendCallback(() => {

            nameGameRect.DOAnchorPosY(endPosYNameGame, showNameGameDuration).SetEase(Ease.OutBack);

            buttonGroup.enabled = false;

        });

        startSceneSequence.Append(newGameButtonRect.DOAnchorPosX(endPosXButton, showButtonDuration).SetEase(Ease.OutBack));

        if (HasSavedGame()) {

            startSceneSequence.Append(continueButtonRect.DOAnchorPosX(endPosXButton, showButtonDuration).SetEase(Ease.OutBack));
        }

        startSceneSequence.Append(creditButtonRect.DOAnchorPosX(endPosXButton, showButtonDuration).SetEase(Ease.OutBack));
        startSceneSequence.Append(quitGameButtonRect.DOAnchorPosX(endPosXButton, showButtonDuration).SetEase(Ease.OutBack));

        startSceneSequence.OnComplete(() => {

            fadeImage.gameObject.SetActive(false);

        });
    }

    private void ToggleSFXControlButton() {

        SaveData.SetMutedSFX(!SaveData.IsMutedSFX());

        muteSFX.gameObject.SetActive(SaveData.IsMutedSFX());

        if (!SaveData.IsMutedSFX() && SaveData.GetSFXVolumeSaved() <= 0f) {
            // Khi UnMute SFX mà SFX <= 0f (Do bật tắt game bất thường)

            SaveData.SetVolumeData(SaveData.defaultVolume, hasChangedMusic: false, hasChangedSFX: true);
        }
    }

    private void ToggleMusicControlButton() {

        SaveData.SetMutedMusic(!SaveData.IsMutedMusic());

        muteMusic.gameObject.SetActive(SaveData.IsMutedMusic());

        if (!SaveData.IsMutedMusic() && SaveData.GetMusicVolumeSaved() <= 0f) {
            // Khi tuy UnMute Music nhưng mà Music <= 0f (Do bật tắt game bất thường - chỉ xảy ra khi thiết kế game)

            SaveData.SetVolumeData(SaveData.defaultVolume, hasChangedMusic: true, hasChangedSFX: false);
        }
    }

    private bool HasSavedGame() {
        return SaveData.GetGameStatus().hasSavedGame;
    }

    public SceneMangerSO GetSceneMangerSO() {
        return this.sceneMangerSO;
    }

    public GameDataTemplateSO GetGameDataTemplateSO() {
        return this.gameDataTemplateSO;
    }

    public void UpdateVisual() {

        // Continue Button 
        continueGameButton.gameObject.SetActive(HasSavedGame());

        // Option Button

        if (!SaveData.IsMutedSFX() && SaveData.GetSFXVolumeSaved() <= 0f) {
            // Khi tuy UnMute SFX nhưng mà SFX <= 0f (Do bật tắt game bất thường - chỉ xảy ra khi thiết kế game)

            SaveData.SetMutedSFX(true);
        }

        if (!SaveData.IsMutedMusic() && SaveData.GetMusicVolumeSaved() <= 0f) {
            // Khi tuy UnMute Music nhưng mà Music <= 0f (Do bật tắt game bất thường - chỉ xảy ra khi thiết kế game)

            SaveData.SetMutedMusic(true);
        }

        muteSFX.gameObject.SetActive(SaveData.IsMutedSFX());
        muteMusic.gameObject.SetActive(SaveData.IsMutedMusic());
    }
}
