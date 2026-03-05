using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {

    private Loader.Scene sceneLoaded;
    private LevelManagerSO levelManagerSO;

    private void Awake() {
        Button levelButton = this.GetComponent<Button>();

        levelButton.onClick.AddListener(() => {
            Loader.Load(this.sceneLoaded, this.levelManagerSO);
        });
    }

    private void Start() {

        LevelStatus currentLevelStatus = SaveData.GetLevelStatusByBiomeAndLevel(this.levelManagerSO.biomeType, this.levelManagerSO.gameLevel);

        if (currentLevelStatus.isUnlockedLevel) {
            // If current level attach to this button is unlocked

            Show();

        }
        else {
            Hide();
        }

    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }

    public LevelManagerSO GetLevelManagerSO() {
        return this.levelManagerSO;
    }

    public static LevelButton SpawnLevelButton(Transform prefab, Transform parent, Vector3 spawnPos ,Loader.Scene sceneLoaded, LevelManagerSO levelManagerSO) {
        
        Transform buttonTransform = Instantiate(prefab, parent);

        buttonTransform.transform.position = spawnPos;

        LevelButton levelButton = buttonTransform.GetComponent<LevelButton>();

        levelButton.levelManagerSO = levelManagerSO;
        levelButton.sceneLoaded = sceneLoaded;

        return levelButton;
    }
}
