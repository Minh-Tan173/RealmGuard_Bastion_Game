using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class SaveData
{

    public const string VOLUME_SAVE_DATA_FILE_PATH = "VolumeData_PojectB.json";
    public const string MAP_DATA_PATH = "MapData_ProjectB.json";
    public const string ABILITY_DATA_PATH = "AbilityData_ProjectB.json";
    public const string GAME_MANGEMENT_PATH = "GameMangement_ProjectB.json";

    public static GameDataTemplateSO cachedGameData;

    public static SoundData soundData;
    public static MapCollection mapCollection;
    public static AbilityStatCollection abilityStatusCollection;
    public static GameProgression gameProgression;

    public static Dictionary<IAbility.AbilityType, AbilityStatus> abilitiesStatusDict = new Dictionary<IAbility.AbilityType, AbilityStatus>();
    public static Dictionary<ITowerObject.TowerType, TowerStatus> towersStatusDict = new Dictionary<ITowerObject.TowerType, TowerStatus>();
    public static Dictionary<string, EnemyStatus> enemiesStatusDict = new Dictionary<string, EnemyStatus>();

    public static float defaultVolume = 1f;
    public static int defaultPoint = 1000;

    #region First Time access game and New Game
    public static void InitializeIfFirstTime(GameDataTemplateSO gameDataTemplateSO) {

        string volumeDataPath = Path.Combine(Application.persistentDataPath, VOLUME_SAVE_DATA_FILE_PATH);
        string mapCollectionPath = Path.Combine(Application.persistentDataPath, MAP_DATA_PATH);
        string abilityStatCollectionPath = Path.Combine(Application.persistentDataPath, ABILITY_DATA_PATH);
        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        Debug.Log(gameStatusPath);

        // Khởi tạo ban đầu
        soundData = new SoundData();
        mapCollection = new MapCollection();
        abilityStatusCollection = new AbilityStatCollection();
        gameProgression = new GameProgression();

        // 0. Loaded gameDataTemplate into cached
        cachedGameData = gameDataTemplateSO;

        // 1. Sound volume load Saved
        if (!File.Exists(volumeDataPath)) {
            // File not exists before

            soundData.musicVolume = defaultVolume;
            soundData.sfxVolume = defaultVolume;

            soundData.isMutedMusic = false;
            soundData.isMutedSFX = false;

            string newData = JsonUtility.ToJson(soundData);
            File.WriteAllText(volumeDataPath, newData);

        }
        else {
            // File exists before

            string oldData = File.ReadAllText(volumeDataPath);
            soundData = JsonUtility.FromJson<SoundData>(oldData); // Sound có thể thay mới data

        }

        // 2. GridMap load Saved
        if (!File.Exists(mapCollectionPath)) {
            // File not exists before

            // Forest Biome
            mapCollection = new MapCollection();

            // Load Data in resource folder
            AsyncOperationHandle<TextAsset> handle = gameDataTemplateSO.mapCollection.LoadAssetAsync<TextAsset>();
            handle.Completed += (handle) => {

                TextAsset textJson = handle.Result;

                string dataLoaded = textJson.text;
                mapCollection = JsonUtility.FromJson<MapCollection>(dataLoaded);

                // Save Data into persistentDataPath
                string newData = JsonUtility.ToJson(mapCollection);
                File.WriteAllText(mapCollectionPath, newData);

                // After Load data --> realise
                gameDataTemplateSO.mapCollection.ReleaseAsset();
            };

        }
        else {
            // File exists before

            string oldData = File.ReadAllText(mapCollectionPath);
            JsonUtility.FromJsonOverwrite(oldData, mapCollection); // Biome Map chỉ được update data chứ ko thay mới data

        }

        // 3. Ability load Saved
        if (!File.Exists(abilityStatCollectionPath)) {
            // File not exists before

            List<AbilitySO> abilitySOList = new List<AbilitySO>();

            foreach (AbilitySO abilitySO in gameDataTemplateSO.abilitySOList) {

                IAbility.AbilityType currentType = abilitySO.abilityType;
                AbilityStatus abilityStat = new AbilityStatus(currentType, IAbility.AbilityLevel.Level1, isLocked: true); // Mặc định khởi tạo mới là Level 1 và khóa

                abilityStatusCollection.abilityStatusList.Add(abilityStat);

            }

            string newData = JsonUtility.ToJson(abilityStatusCollection);
            File.WriteAllText(abilityStatCollectionPath, newData);

        }
        else {
            // File exists before

            string oldData = File.ReadAllText(abilityStatCollectionPath);
            JsonUtility.FromJsonOverwrite(oldData, abilityStatusCollection); // Ability chỉ được update data chứ ko thay mới data

        }

        // ---- Load Ability Stats into Dict
        abilitiesStatusDict.Clear();
        foreach (AbilityStatus abilityStatus in abilityStatusCollection.abilityStatusList) {
            
            if (!abilitiesStatusDict.ContainsKey(abilityStatus.abilityType)) {

                // Load AbilitySO into current abilityStatus
                foreach (AbilitySO abilitySO in gameDataTemplateSO.abilitySOList) {

                    if (abilitySO.abilityType == abilityStatus.abilityType) {
                        // Tìm đúng AbilitySO với type tương ứng thì dừng

                        abilityStatus.abilitySO = abilitySO;
                        break;  
                    }
                }

                abilitiesStatusDict.Add(abilityStatus.abilityType, abilityStatus);
            }

        }


        // 4. Game Status
        if (!File.Exists(gameStatusPath)) {
            // File not exists before

            // Update Biome
            foreach (ILevelManager.BiomeType biomeType in Enum.GetValues(typeof(ILevelManager.BiomeType))) {
                // Duyệt qua từng biome

                if (biomeType == ILevelManager.BiomeType.Default) { continue; }

                List<LevelStatus> levelStatusListTemp = new List<LevelStatus>();
                foreach (ILevelManager.GameLevel gameLevel in Enum.GetValues(typeof(ILevelManager.GameLevel))) {
                    // Duyệt qua từng level

                    LevelStatus levelStatus = new LevelStatus(gameLevel, isUnlockedLevel: false, isCompleted: false);

                    if (biomeType == ILevelManager.BiomeType.Forest && gameLevel == ILevelManager.GameLevel.Level1) {
                        // Màn chơi khởi đầu toàn bộ game

                        levelStatus.isUnlockedLevel = true;

                    }

                    levelStatusListTemp.Add(levelStatus);

                }

                BiomeStatus newBiomeStatus = new BiomeStatus(biomeType, levelStatusListTemp);

                gameProgression.biomeStatusList.Add(newBiomeStatus);
            }

            // Update toltal point
            gameProgression.totalPoints = defaultPoint;
            gameProgression.hasSavedGame = false;

            // Update tower status
            foreach (ITowerObject.TowerType towerType in Enum.GetValues(typeof(ITowerObject.TowerType))) {

                if (towerType == ITowerObject.TowerType.Default) { continue; }

                TowerStatus towerStatus = new TowerStatus(towerType, isUnLockedTower: false); // Mặc định khi khởi tạo mới

                gameProgression.towerStatusList.Add(towerStatus);

            }

            // Update enemy status
            foreach (EnemySO enemySO in gameDataTemplateSO.enemySOList) {

                EnemyStatus enemyStatus = new EnemyStatus(enemySO.enemyName, hasIntroduced: false, isUnlockAnomaly: false); // Mặc định khi khởi tạo mới

                gameProgression.enemyStatusList.Add(enemyStatus);

            }

            string newData = JsonUtility.ToJson(gameProgression);
            File.WriteAllText(gameStatusPath, newData);
        }
        else {
            // File exists before

            string data = File.ReadAllText(gameStatusPath);
            JsonUtility.FromJsonOverwrite(data, gameProgression);
        }

        // ---- Load Tower Stats into Dict
        towersStatusDict.Clear();
        foreach(TowerStatus towerStatus in gameProgression.towerStatusList) {

            if (!towersStatusDict.ContainsKey(towerStatus.towerType)) {

                towersStatusDict.Add(towerStatus.towerType, towerStatus); // Load Tower Data into Dict for searching
            }
        }

        // ---- Load Enemy Stats into Dict
        enemiesStatusDict.Clear();
        foreach (EnemyStatus enemyStatus in gameProgression.enemyStatusList) {

            if (!enemiesStatusDict.ContainsKey(enemyStatus.enemyID)) {

                enemiesStatusDict.Add(enemyStatus.enemyID, enemyStatus); // Load Enemy Data into Dict for searching
            }

        }
    }

    public static void ResetDataForNewGame(GameDataTemplateSO gameDataTemplateSO) {
        // When Clicked new Game Button

        string abilityStatCollectionPath = Path.Combine(Application.persistentDataPath, ABILITY_DATA_PATH);
        string gameManagerPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        // 1. Reset Ability saved back to default
        foreach (AbilityStatus abilityStatus in abilityStatusCollection.abilityStatusList) {

            abilityStatus.currentLevel = IAbility.AbilityLevel.Level1;
            abilityStatus.isLocked = true; 

        }

        string newAbilityData = JsonUtility.ToJson(abilityStatusCollection);
        File.WriteAllText(abilityStatCollectionPath, newAbilityData);

        // 2. Reset toàn bộ tiến trình game về default
        gameProgression.biomeStatusList.Clear();
        gameProgression.totalPoints = defaultPoint;
        gameProgression.hasSavedGame = false;

        foreach (ILevelManager.BiomeType biomeType in Enum.GetValues(typeof(ILevelManager.BiomeType))) {
            // Duyệt qua từng biome

            if (biomeType == ILevelManager.BiomeType.Default) { continue; }

            List<LevelStatus> levelStatusListTemp = new List<LevelStatus>();
            foreach (ILevelManager.GameLevel gameLevel in Enum.GetValues(typeof(ILevelManager.GameLevel))) {
                // Duyệt qua từng level

                LevelStatus levelStatus = new LevelStatus(gameLevel, isUnlockedLevel: false, isCompleted: false);


                if (biomeType == ILevelManager.BiomeType.Forest && gameLevel == ILevelManager.GameLevel.Level1) {
                    // Màn chơi khởi đầu toàn bộ game

                    levelStatus.isUnlockedLevel = true;

                }

                levelStatusListTemp.Add(levelStatus);

            }

            BiomeStatus newBiomeStatus = new BiomeStatus(biomeType, levelStatusListTemp);

            gameProgression.biomeStatusList.Add(newBiomeStatus);

            // Reset Tower Status
            foreach(TowerStatus towerStatus in gameProgression.towerStatusList) {
                towerStatus.isUnLockedTower = false;
            }

            // Reset Enemy Status
            foreach (EnemyStatus enemyStatus in gameProgression.enemyStatusList) {

                enemyStatus.hasIntroduced = false;
                enemyStatus.isUnlockAnomaly = false;
            }
        }

        string newGameData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameManagerPath, newGameData);
    }
    #endregion

    #region Update and Get sound data
    public static void SetVolumeData(float volume, bool hasChangedMusic, bool hasChangedSFX) {

        string volumeDataPath = Path.Combine(Application.persistentDataPath, VOLUME_SAVE_DATA_FILE_PATH);

        if (hasChangedMusic) {
            SaveData.soundData.SetMusicVolume(volume);
        }

        if (hasChangedSFX) {
            SaveData.soundData.SetSFXVolume(volume);
        }

        string soundDataUpdate = JsonUtility.ToJson(SaveData.soundData);
        File.WriteAllText(volumeDataPath, soundDataUpdate);
    }

    public static void SetMutedMusic(bool isMuted) {

        string volumeDataPath = Path.Combine(Application.persistentDataPath, VOLUME_SAVE_DATA_FILE_PATH);

        SaveData.soundData.isMutedMusic = isMuted;

        string soundDataUpdate = JsonUtility.ToJson(SaveData.soundData);
        File.WriteAllText(volumeDataPath, soundDataUpdate);
    }

    public static void SetMutedSFX(bool isMuted) {

        string volumeDataPath = Path.Combine(Application.persistentDataPath, VOLUME_SAVE_DATA_FILE_PATH);

        SaveData.soundData.isMutedSFX = isMuted;

        string soundDataUpdate = JsonUtility.ToJson(SaveData.soundData);
        File.WriteAllText(volumeDataPath, soundDataUpdate);
    }

    public static float GetMusicVolumeSaved() {

        return soundData.musicVolume;
    }

    public static float GetSFXVolumeSaved() {

        return soundData.sfxVolume;
    }

    public static bool IsMutedMusic() {

        return soundData.isMutedMusic;
    }

    public static bool IsMutedSFX() {

        return soundData.isMutedSFX;
    }

    #endregion

    #region Update and Get Ability Data
    public static void UnLockAbility(IAbility.AbilityType abilityType) {

        string abilityStatCollectionPath = Path.Combine(Application.persistentDataPath, ABILITY_DATA_PATH);

        abilitiesStatusDict[abilityType].isLocked = false;

        string newData = JsonUtility.ToJson(abilityStatusCollection);
        File.WriteAllText(abilityStatCollectionPath, newData);

    }

    public static void UpdateLevelAbility(IAbility.AbilityType abilityType) {

        string abilityStatCollectionPath = Path.Combine(Application.persistentDataPath, ABILITY_DATA_PATH);

        abilitiesStatusDict[abilityType].currentLevel = (IAbility.AbilityLevel)((int)abilitiesStatusDict[abilityType].currentLevel + 1); // Tăng thêm 1 Level

        string newData = JsonUtility.ToJson(abilityStatusCollection);
        File.WriteAllText(abilityStatCollectionPath, newData);
    }

    public static AbilityStatus GetAbilityStatusByType(IAbility.AbilityType abilityType) {
        return abilitiesStatusDict[abilityType];
    }

    public static AbilitySO GetAbilitySOByType(IAbility.AbilityType abilityType) {

        return abilitiesStatusDict[abilityType].abilitySO;
    }

    public static AbilityLevelData GetAbilityLevelDataByLevelAndType(IAbility.AbilityLevel abilityLevel, IAbility.AbilityType abilityType) {

        foreach(AbilityLevelData abilityLevelData in GetAbilitySOByType(abilityType).abilityLevelDataList) {

            if (abilityLevelData.level == abilityLevel) {

                return abilityLevelData;
            }
        }

        return null;
    }
    #endregion

    #region Game and Level Status
    public static void UnlockNextLevel(ILevelManager.BiomeType biomeType, ILevelManager.GameLevel gameLevel, int pointIncrease) {

        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        int currentLevelIndex = (int)gameLevel;
        int nextLevelIndex = currentLevelIndex + 1;

        LevelStatus currentLevel = GetLevelStatusByBiomeAndLevel(biomeType, gameLevel);
        LevelStatus nextLevel = GetLevelStatusByBiomeAndLevel(biomeType, (ILevelManager.GameLevel)nextLevelIndex);

        // Hoàn thành 1 màn thì màn kế tiếp được mở khóa
        currentLevel.isCompleted = true;
        nextLevel.isUnlockedLevel = true;

        gameProgression.totalPoints += pointIncrease;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);
    }

    public static void UnlockNextBiome(ILevelManager.BiomeType oldBiome, ILevelManager.GameLevel gameLevel, int pointIncrease) {

        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        ILevelManager.GameLevel nextLevelIndex = ILevelManager.GameLevel.Level1; // Sang biome mới thì mặc định về level 1 của biome đó
        int nextBiomeIndex = (int)oldBiome + 1;

        LevelStatus currentLevel = GetLevelStatusByBiomeAndLevel(oldBiome, gameLevel);
        LevelStatus nextLevel = GetLevelStatusByBiomeAndLevel((ILevelManager.BiomeType)nextBiomeIndex, nextLevelIndex);

        currentLevel.isCompleted = true;
        nextLevel.isUnlockedLevel = true;

        gameProgression.totalPoints += pointIncrease;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);
    }

    public static void SetNewTotalPoint(int newTotalPoints) {

        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        gameProgression.totalPoints = newTotalPoints;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);

    }

    public static void SetHasSavedGame() {

        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);
        gameProgression.hasSavedGame = true;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);
    }

    public static void UnlockNewTower(ITowerObject.TowerType towerType) {
        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        towersStatusDict[towerType].isUnLockedTower = true;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);
    }

    public static void SetEnemyIntroduced(EnemySO enemySO) {
        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        enemiesStatusDict[enemySO.enemyName].hasIntroduced = true;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);
    }

    public static void UnlockAnomalyEnemy(EnemySO enemySO) {

        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        enemiesStatusDict[enemySO.enemyName].isUnlockAnomaly = true;

        string newData = JsonUtility.ToJson(gameProgression);
        File.WriteAllText(gameStatusPath, newData);

    }

    public static LevelStatus GetLevelStatusByBiomeAndLevel(ILevelManager.BiomeType biomeType, ILevelManager.GameLevel gameLevel) {

        string gameStatusPath = Path.Combine(Application.persistentDataPath, GAME_MANGEMENT_PATH);

        List<BiomeStatus> biomeStatusList = gameProgression.biomeStatusList;

        foreach (BiomeStatus biomeStatus in biomeStatusList) {

            if (biomeStatus.currentBiome == biomeType) {
                // Tìm đúng biome cần get level

                List<LevelStatus> levelStatusList = biomeStatus.levelStatusList;

                foreach (LevelStatus levelStatus in levelStatusList) {

                    if (levelStatus.currentLevel == gameLevel) {
                        // Nếu đúng Level cần get
                        return levelStatus;
                    }
                }

            }

        }

        return null;
    }

    public static GameProgression GetGameStatus() {
        return gameProgression;
    }

    public static bool IsTowerUnlocked(ITowerObject.TowerType towerType) {
        return towersStatusDict[towerType].isUnLockedTower;
    }

    public static bool HasIntroduced(EnemySO enemySO) {
        return enemiesStatusDict[enemySO.enemyName].hasIntroduced;
    }

    public static bool IsUnlockAnomaly(EnemySO enemySO) {
        return enemiesStatusDict[enemySO.enemyName].isUnlockAnomaly;
    }

    #endregion

    #region Get Level Data Base On Biome and Level

    public static LevelData GetLevelDataByBiomeAndLevel(ILevelManager.BiomeType biomeType, ILevelManager.GameLevel gameLevel) {

        List<LevelData> levelDataList = cachedGameData.levelDataList;

        foreach (LevelData levelData in levelDataList) {

            if (levelData.biomeType == biomeType) {

                if (levelData.gameLevel == gameLevel) {

                    return levelData;
                }
                else {
                    continue;
                }
            }
            else {
                continue;
            }

        }

        return null;
    }

    public static LevelData GetNextLevelData(LevelManagerSO currentLevelSO) {

        ILevelManager.BiomeType currentBiome = currentLevelSO.biomeType;
        ILevelManager.GameLevel currentLevel = currentLevelSO.gameLevel;

        LevelData nextLevelData = null;

        if (currentLevel != ILevelManager.GameLevel.Level5) {
            // Not reached last level of current biome

            ILevelManager.GameLevel nextLevel = (ILevelManager.GameLevel)((int)currentLevel + 1);

            nextLevelData = GetLevelDataByBiomeAndLevel(currentBiome, nextLevel);

        }
        else {
            // Reached last level of current biome --> Move to next Biome and Restart to Level 1

            ILevelManager.BiomeType nextBiome = (ILevelManager.BiomeType)((int)currentBiome + 1);
            ILevelManager.GameLevel nextLevel = ILevelManager.GameLevel.Level1;

            nextLevelData = GetLevelDataByBiomeAndLevel(nextBiome, nextLevel);
        }

        return nextLevelData;
    }

    #endregion
}

[System.Serializable]
public class SoundData {

    public float musicVolume;
    public float sfxVolume;
    public bool isMutedMusic;
    public bool isMutedSFX;

    public void SetMusicVolume(float musicVolume) {
        this.musicVolume = musicVolume;
    }

    public void SetSFXVolume(float soundEffectVolume) {
        this.sfxVolume = soundEffectVolume;
    }

}

#region Saved Gridmap Datatype
[System.Serializable]
public class MapCollection {

    public ForestBiomeMapCollection forestBiomeMapCollection = new ForestBiomeMapCollection();
}


[System.Serializable]
public class ForestBiomeMapCollection {

    public List<SavedGridMap> forestGridMapCollectionLevel1 = new List<SavedGridMap>();
    public List<SavedGridMap> forestGridMapCollectionLevel2 = new List<SavedGridMap>();
    public List<SavedGridMap> forestGridMapCollectionLevel3 = new List<SavedGridMap>();
    public List<SavedGridMap> forestGridMapCollectionLevel4 = new List<SavedGridMap>();
    public List<SavedGridMap> forestGridMapCollectionLevel5 = new List<SavedGridMap>();

}

[System.Serializable]
public class SavedGridMap {
    public List<GridNode> gridMap = new List<GridNode>();
    public List<NodePath2x2> nodePath2X2s = new List<NodePath2x2>();

    public SavedGridMap(List<GridNode> gridMap, List<NodePath2x2> nodePath2x2s) {
        this.gridMap = new List<GridNode>(gridMap);
        this.nodePath2X2s = new List<NodePath2x2>(nodePath2x2s);
    }
}
#endregion

#region Saved Ability Datatype
[System.Serializable]
public class AbilityStatCollection {
    public List<AbilityStatus> abilityStatusList = new List<AbilityStatus>();
}

[System.Serializable]
public class AbilityStatus {
    public IAbility.AbilityType abilityType;
    public IAbility.AbilityLevel currentLevel;
    public bool isLocked;

    [System.NonSerialized] public AbilitySO abilitySO;

    public AbilityStatus(IAbility.AbilityType abilityType, IAbility.AbilityLevel abilityLevel ,bool isLocked) {
        this.abilityType = abilityType;
        this.currentLevel = abilityLevel;
        this.isLocked = isLocked;

       // this.abilitySO = abilitySO;
    }
}
#endregion

#region Saved Game Datatype

[System.Serializable]
public class TowerStatus {
    public ITowerObject.TowerType towerType;
    public bool isUnLockedTower;

    public TowerStatus(ITowerObject.TowerType towerType, bool isUnLockedTower) {
        this.towerType = towerType;
        this.isUnLockedTower = isUnLockedTower;
    }
}

[System.Serializable]
public class EnemyStatus {
    public string enemyID;
    public bool hasIntroduced;
    public bool isUnlockAnomaly;

    public EnemyStatus(string enemySO, bool hasIntroduced, bool isUnlockAnomaly) {
        this.enemyID = enemySO;
        this.hasIntroduced = hasIntroduced;
        this.isUnlockAnomaly = isUnlockAnomaly;
    }
}


[System.Serializable]
public class LevelStatus {

    public ILevelManager.GameLevel currentLevel;
    public bool isUnlockedLevel;
    public bool isCompleted;

    public LevelStatus(ILevelManager.GameLevel gameLevel, bool isUnlockedLevel, bool isCompleted) {
        this.currentLevel = gameLevel;
        this.isUnlockedLevel = isUnlockedLevel;
        this.isCompleted = isCompleted;
    }

}

[System.Serializable]
public class BiomeStatus {
    public string biomeName;
    public ILevelManager.BiomeType currentBiome;
    public List<LevelStatus> levelStatusList = new List<LevelStatus>();

    public BiomeStatus(ILevelManager.BiomeType currentBiome, List<LevelStatus> levelStatusList) {
        this.biomeName = currentBiome.ToString();
        this.currentBiome = currentBiome;
        this.levelStatusList = levelStatusList;
    }
}

[System.Serializable]
public class GameProgression {
    public bool hasSavedGame;
    public List<BiomeStatus> biomeStatusList = new List<BiomeStatus>();
    public int totalPoints;

    public List<TowerStatus> towerStatusList = new List<TowerStatus>();
    public List<EnemyStatus> enemyStatusList = new List<EnemyStatus>();

}
#endregion

#region Saved Enemy Datatype
#endregion