using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneUIManager : MonoBehaviour
{
    public static SceneUIManager Instance { get; private set; }

    public enum TypeUI {
        LevelManagerUI,
        GamePauseUI,
        GameOverUI,
        GameWinUI
    }

    [SerializeField] private SceneMangerSO sceneMangerSO;

    private void Awake() {
        Instance = this;
    }

    public Transform GetUIByType(TypeUI typeUI) {

        if (typeUI == TypeUI.LevelManagerUI) {
            return sceneMangerSO.levelManagerUI;
        }

        if (typeUI == TypeUI.GamePauseUI) {
            return sceneMangerSO.gamePauseUI;
        }
        
        if (typeUI == TypeUI.GameOverUI) {
            return sceneMangerSO.gameOverUI;
        }

        if (typeUI == TypeUI.GameWinUI) {
            return sceneMangerSO.gameWinUI;
        }

        return null;
    }

    public static void SpawnUI(TypeUI typeUI) {

        Transform uiPrefab = SceneUIManager.Instance.GetUIByType(typeUI);
        Transform parentCanvas = SceneUIManager.Instance.transform;

        Transform uiTranform = Instantiate(uiPrefab, parentCanvas);

    }
}
