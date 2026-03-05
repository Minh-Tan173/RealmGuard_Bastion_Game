using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class LevelManagerSO : ScriptableObject
{
    [Header("Level Data")]
    public ILevelManager.GameMode gameMode;
    public ILevelManager.BiomeType biomeType;
    public ILevelManager.GameLevel gameLevel;

    [Header("Timer")]
    public float startGameTimer;
    public float endTurnTimer;

    [Header("Scene Data")]
    public float coinStart;
    public float numberOfHeart;

    [Header("Wave Script")]
    public List<WaveScript> waveScriptList;

    [Header("Tower Script")]
    public ITowerObject.TowerType newTower;

    public int pointGetWhenWin;

}

[System.Serializable]
public class WaveScript {
    // Kịch bản mỗi Wave
    public string waveName;

    #region Enemy script
    public List<EnemyGroup> enemyGroupList;
    public EnemySO newEnemy;
    public EnemySO newEnemyAnomaly;
    #endregion
}

[System.Serializable]
public class EnemyGroup {

    public EnemySO enemyTypeSO;
    public int numberEnemy;
    public float cdSpawnTimer;

}
