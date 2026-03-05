using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    public event EventHandler<OnActiveEnemyEventArgs> ActiveEnemy;
    public class OnActiveEnemyEventArgs : EventArgs {
        public BaseEnemy baseEnemy;
    }

    public event EventHandler PrepareDone; 

    public Dictionary<EnemySO, List<BaseEnemy>> globalPool; // Tổng lưu trữ số lượng enemy từ đầu tới giờ

    public Dictionary<EnemyGroup, List<BaseEnemy>> currentWavePool; // Key đại diện cho Nhóm Enemy có cùng EnemySO, Value đại diện cho list 
    public Dictionary<EnemyGroup, List<BaseEnemy>> nextWavePool; // Key đại diện cho Nhóm Enemy có cùng EnemySO, Value đại diện cho list 

    private int activeSpawningCoroutines;
    private int nextWavePrepareIndex;
    private WaveScript nextWavePrepare;



    private void Awake() {

        Instance = this;
        globalPool = new Dictionary<EnemySO, List<BaseEnemy>>();
        currentWavePool = new Dictionary<EnemyGroup, List<BaseEnemy>>();
        nextWavePool = new Dictionary<EnemyGroup, List<BaseEnemy>>();

        nextWavePrepareIndex = 0;
    }

    private void Start() {

        LevelManager.Instance.OnGameRunningState += LevelManager_OnGameRunningState;
        LevelManager.Instance.OnEndTurnState += LevelManager_OnEndTurnState;

        // When start game - Prepare Wave 1 (index 0)
        PrepareNextWave();

    }

    private void OnDestroy() {

        LevelManager.Instance.OnGameRunningState -= LevelManager_OnGameRunningState;
        LevelManager.Instance.OnEndTurnState -= LevelManager_OnEndTurnState;

    }


    private void LevelManager_OnGameRunningState(object sender, EventArgs e) {

        HandleActiveEnemy();

    }

    private void LevelManager_OnEndTurnState(object sender, EventArgs e) {
        // When end a turn

        // 1. Add next wave pool script for current wave pool

        currentWavePool = new Dictionary<EnemyGroup, List<BaseEnemy>>(nextWavePool);

        // 2. Counting how many coroutine this wave have
        activeSpawningCoroutines = currentWavePool.Count;

        // 3. Prepare next Wave
        nextWavePool.Clear();
        nextWavePrepareIndex += 1;

        PrepareNextWave();
    }

    private void PrepareNextWave() {

        if (nextWavePrepareIndex >= LevelManager.Instance.GetLevelManagerSO().waveScriptList.Count) { return; }

        nextWavePrepare = LevelManager.Instance.GetLevelManagerSO().waveScriptList[nextWavePrepareIndex];

        // 1. Tính tổng số lượng enemy ứng với mỗi type có trong từng group của kịch bản của wave này
        Dictionary<EnemySO, int> requiredByType = new Dictionary<EnemySO, int>();

        foreach(EnemyGroup enemyGroup in nextWavePrepare.enemyGroupList) {

            if (!requiredByType.ContainsKey(enemyGroup.enemyTypeSO)) {
                // Nếu chưa có Type enemy này thì phải thêm vào dict total và khởi tạo value ban đầu  = 0
                requiredByType.Add(enemyGroup.enemyTypeSO, 0);
            }

            requiredByType[enemyGroup.enemyTypeSO] += enemyGroup.numberEnemy;
        }

        // 2. Đảm bảo globalPool có đủ số lượng enemy ứng với từng Type Enemy một
        foreach (KeyValuePair<EnemySO, int> entry in requiredByType) {

            EnemySO currentEnemyType = entry.Key;
            int toltalRequired = entry.Value;

            if (!globalPool.ContainsKey(currentEnemyType)) {
                // If globalPool dont contain this key befored

                List<BaseEnemy> noneList = new List<BaseEnemy>();
                globalPool.Add(currentEnemyType, noneList);

            }

            while (globalPool[currentEnemyType].Count < toltalRequired) {
                // If the spawned count of this enemy type (key: enemySO) hasn't reached the amount required by nextWaveScript

                SpawnEnemy(currentEnemyType);
            }
        }

        // 3. Phân phối Enemy cho từng GroupEnemy trong kịch bản next wave
        Dictionary<EnemySO, List<BaseEnemy>> tempPool = new Dictionary<EnemySO, List<BaseEnemy>>();

        foreach (KeyValuePair<EnemySO, List<BaseEnemy>> entry in globalPool) {
            tempPool[entry.Key] = new List<BaseEnemy>(entry.Value);
        }

        foreach (EnemyGroup enemyGroup in nextWavePrepare.enemyGroupList) {
            EnemySO type = enemyGroup.enemyTypeSO;

            // Get enemies from temPool
            nextWavePool[enemyGroup] = tempPool[type].GetRange(0, enemyGroup.numberEnemy);

            // Remove enemies was put in nextWavePool
            tempPool[type].RemoveRange(0, enemyGroup.numberEnemy);
        }

        PrepareDone?.Invoke(this, EventArgs.Empty);
    }

    private void HandleActiveEnemy() {

        foreach (EnemyGroup enemyGroup in currentWavePool.Keys) {
            // Duyệt qua mỗi group Enemy ở wavePool hiện tại

            StartCoroutine(ActiveEnemyWithType(enemyGroup));
        }

    }

    private IEnumerator ActiveEnemyWithType(EnemyGroup enemyGroup) {

        List<BaseEnemy> baseEnemieList = currentWavePool[enemyGroup];
        float spawnTimer = enemyGroup.cdSpawnTimer;

        int currentActiveIndex = 0;

        while (currentActiveIndex < baseEnemieList.Count) {

            ActiveEnemy?.Invoke(this, new OnActiveEnemyEventArgs { baseEnemy = baseEnemieList[currentActiveIndex] });

            currentActiveIndex += 1;

            yield return new WaitForSeconds(spawnTimer);
        }

        // When active full enemy of this enemyGroup
        activeSpawningCoroutines -= 1;
        NextTurnManager.Instance.TryEndTurn();
    }

    private void SpawnEnemy(EnemySO typeEnemySO) {
        BaseEnemy currentEnemy = BaseEnemy.SpawnEnemy(typeEnemySO, this.transform);

        globalPool[typeEnemySO].Add(currentEnemy);

    }

    public bool IsSpawningWave() {

        return activeSpawningCoroutines > 0;
    }

}
