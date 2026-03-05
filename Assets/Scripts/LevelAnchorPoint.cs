using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelAnchorPoint : MonoBehaviour
{
    public ILevelManager.BiomeType biomeType;
    public ILevelManager.GameLevel gameLevel;
    [SerializeField] private Loader.Scene sceneLoaded;
    [SerializeField] private LevelManagerSO levelManagerSO;

    public Loader.Scene GetSceneLoaded() {
        return sceneLoaded;
    }

    public LevelManagerSO GetLevelManagerSO() {
        return levelManagerSO;
    }

}
