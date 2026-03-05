using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GamePauseUI : MonoBehaviour
{
    [Header("Slider")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Button")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button quitGameButton;

    [Header("Position")]
    [SerializeField] private Vector3 firstPosition;
    [SerializeField] private Vector3 endPosition;

    [Header("Animation Config")]
    [SerializeField] private float animDuration = 0.3f;

    private RectTransform rectTransform;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();

        // Slider
        musicSlider.onValueChanged.AddListener((float musicVolume) => {
            MusicManager.Instance.SetVolume(musicVolume);
        });
        
        sfxSlider.onValueChanged.AddListener((float sfxVolume) => {
            SoundManager.Instance.SetSFXVolume(sfxVolume);
        });

        // Button
        closeButton.onClick.AddListener(() => {

            LevelManager.Instance.ToggleGamePause();

        });

        mainMenuButton.onClick.AddListener(() => {

            Loader.Load(Loader.Scene.MainMenu);

        });

        restartButton.onClick.AddListener(() => {

            Loader.Load(SceneManager.GetActiveScene().name);

        });

        quitGameButton.onClick.AddListener(() => {

            Application.Quit();

        });

    }

    private void Start() {

        LevelManager.Instance.OnGamePause += LevelManager_OnGamePause;
        LevelManager.Instance.UnGamePause += LevelManager_UnGamePause;

        rectTransform.anchoredPosition = firstPosition;
        Hide();
    }

    private void OnDestroy() {

        LevelManager.Instance.OnGamePause -= LevelManager_OnGamePause;
        LevelManager.Instance.UnGamePause -= LevelManager_UnGamePause;

    }

    private void LevelManager_UnGamePause(object sender, System.EventArgs e) {

        HideAnim();
    }

    private void LevelManager_OnGamePause(object sender, System.EventArgs e) {

        ShowAnim();
    }

    private void ShowAnim() {

        Show();

        UpdateVisualSlider();

        // 1. Kill animation cũ 
        rectTransform.DOKill();

        // 2. Đặt ngay vị trí bắt đầu
        rectTransform.anchoredPosition = firstPosition;

        // 3. Chạy Tween
        rectTransform.DOAnchorPos(endPosition, animDuration).SetUpdate(true).SetEase(Ease.OutBack);
    }

    private void HideAnim() {

        // 1. Kill animation cũ
        rectTransform.DOKill();

        // 2. Chạy Tween về vị trí ẩn
        rectTransform.DOAnchorPos(firstPosition, animDuration).SetUpdate(true).SetEase(Ease.InBack).OnComplete(() => {
            // Chỉ tắt GameObject khi chạy xong animation
            Hide();
        });

    }

    private void UpdateVisualSlider() {

        // 1. Music Slider Visual
        if (SaveData.IsMutedMusic()) {

            musicSlider.SetValueWithoutNotify(0f);
        }
        else {

            musicSlider.SetValueWithoutNotify(SaveData.GetMusicVolumeSaved());

        }

        // 2. SFX Slider Visual
        if (SaveData.IsMutedSFX()) {

            sfxSlider.SetValueWithoutNotify(0f);
        }
        else {

            sfxSlider.SetValueWithoutNotify(SaveData.GetSFXVolumeSaved());
        }

    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }
}
