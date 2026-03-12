using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu()]
public class GameDataTemplateSO : ScriptableObject
{
    [Header("Map Saved")]
    public AssetReferenceT<TextAsset> mapCollection;

    [Header("Level Data")]
    public List<LevelData> levelDataList;

    [Header("Ability Data")]
    public List<AbilitySO> abilitySOList;

    [Header("List EnemySO")]
    public List<EnemySO> enemySOList;
}

[System.Serializable]
public class LevelData {

    [Header("Key search")]
    public ILevelManager.BiomeType biomeType;
    public ILevelManager.GameLevel gameLevel;

    [Header("Value result")]
    public LevelManagerSO levelManagerSO;
    public Loader.Scene sceneToLoad;
}
    