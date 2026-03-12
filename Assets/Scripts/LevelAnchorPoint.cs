using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAnchorPoint : MonoBehaviour
{
    [SerializeField] private ILevelManager.BiomeType biomeType;
    [SerializeField] private ILevelManager.GameLevel gameLevel;

    private LevelData levelData;

    private void Awake() {

        levelData = SaveData.GetLevelDataByBiomeAndLevel(biomeType, gameLevel);
    }

    public LevelData GetLevelData() {
        return this.levelData;
    }

    public ILevelManager.BiomeType GetBiomeType() {
        return this.biomeType;
    }

    public ILevelManager.GameLevel GetGameLevel() {
        return this.gameLevel;
    }

}
