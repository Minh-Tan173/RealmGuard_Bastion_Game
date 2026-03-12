using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour {

    private LevelData levelData;

    private void Awake() {

        Button levelButton = GetComponent<Button>();

        levelButton.onClick.AddListener(() => {

            Loader.Load(levelData);
        });
    }

    private void Start() {

        LevelStatus currentLevelStatus = SaveData.GetLevelStatusByBiomeAndLevel(levelData.biomeType, levelData.gameLevel);

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

    public LevelData GetLevelData() {
        return this.levelData;
    }

    public static LevelButton SpawnLevelButton(Transform prefab, Transform parent, LevelAnchorPoint levelAnchorPoint) {
        
        Transform buttonTransform = Instantiate(prefab, parent);

        buttonTransform.transform.position = levelAnchorPoint.transform.position;

        LevelButton levelButton = buttonTransform.GetComponent<LevelButton>();

        levelButton.levelData = levelAnchorPoint.GetLevelData();

        return levelButton;
    }
}
