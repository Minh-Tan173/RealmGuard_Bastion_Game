using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    public enum LevelState {

        StartGame,
        GameRunning,
        EndTurn,
        WinGame,
        LostGame

    }

    public event EventHandler<UnLockTowerButtonEventArgs> UnLockTowerButton;
    public class UnLockTowerButtonEventArgs : EventArgs {
        public ITowerObject.TowerType towerType;
    }

    public event EventHandler GameSetupDone;

    public event EventHandler HeartChanged;

    public event EventHandler OnGamePause;
    public event EventHandler UnGamePause;

    public event EventHandler OnStartGameState;
    public event EventHandler OnGameRunningState;
    public event EventHandler OnEndTurnState;

    [Header("Level Data")]
    [SerializeField] private LevelManagerSO levelManagerSO;

    private LevelState currentLevelState;

    private float currentCoin;
    private float currentHeart;

    private WaveScript currentWaveScript;
    private int currentWave;

    private bool isFirstStart;
    private bool isGamePaused;

    private void Awake() {

        Instance = this;
            
        // Load Database
        if (Loader.levelDataLoaded != null) {
            levelManagerSO = Loader.levelDataLoaded;
        }

        Time.timeScale = 1f;

        currentCoin = levelManagerSO.coinStart;
        currentHeart = levelManagerSO.numberOfHeart;
        currentWave = 0;
        isGamePaused = false;
        isFirstStart = true;

        // Spawn UI
        SceneUIManager.SpawnUI(SceneUIManager.TypeUI.LevelManagerUI);
    }

    private void Start() {


        PathGenerator.Instance.PathVisualShowAll += PathGenerator_PathVisualShowAll;
        GameInput.Instance.OnPauseGameAction += GameInput_OnPauseGameAction;
        NextTurnManager.Instance.OnNextTurn += NextTurnControl_OnNextTurn;

        // When start game
        ChangeLevelStateTo(LevelState.StartGame);
    }

    private void OnDestroy() {
        PathGenerator.Instance.PathVisualShowAll -= PathGenerator_PathVisualShowAll;
        GameInput.Instance.OnPauseGameAction -= GameInput_OnPauseGameAction;
        NextTurnManager.Instance.OnNextTurn -= NextTurnControl_OnNextTurn;
    }

    private void NextTurnControl_OnNextTurn(object sender, EventArgs e) {
        ChangeLevelStateTo(LevelState.GameRunning);
    }

    private void GameInput_OnPauseGameAction(object sender, EventArgs e) {

        if (currentLevelState == LevelState.StartGame) {
            return;
        }

        ToggleGamePause();
    }

    private void PathGenerator_PathVisualShowAll(object sender, System.EventArgs e) {
        // After create path

        GameSetupDone?.Invoke(this, EventArgs.Empty);

        ChangeLevelStateTo(LevelState.EndTurn);

    }

    private void Update() {
        
        if (currentLevelState == LevelState.GameRunning) {
            // Game is running

            if (currentHeart <= 0) {
                ChangeLevelStateTo(LevelState.LostGame);
            }
        }

    }

    private void HandleIntroduceNewEnemy() {

        // 1. Introduce New Enemy if has
        if (currentWaveScript.newEnemy != null && !SaveData.HasIntroduced(currentWaveScript.newEnemy)) {
            // If this wave has a new enemy and hasnt introduced this enemy before

            EnemyIntroductionUI.SpawnEnemyIntroductionUI(currentWaveScript.newEnemy, SceneUIManager.Instance.transform, HandleIntroduceNewTower);
        }
        else if (currentWaveScript.newEnemyAnomaly != null && !SaveData.IsUnlockAnomaly(currentWaveScript.newEnemyAnomaly)) {
            // If this wave has unlocked new anomaly of enemy

            EnemyIntroductionUI.SpawnEnemyIntroductionUI(currentWaveScript.newEnemyAnomaly, SceneUIManager.Instance.transform, HandleIntroduceNewTower);
        }
        else {
            // If dont have new enemy to Introduce or Unlock Anomaly --> Check if have Introduce new Tower

            HandleIntroduceNewTower();
        }
    }

    private void HandleIntroduceNewTower() {

        if (isFirstStart && levelManagerSO.newTower != ITowerObject.TowerType.Default && !SaveData.IsTowerUnlocked(levelManagerSO.newTower)) {
            // Nếu mới vào level và có Tower mới chưa được unlock trong kịch bản

            SaveData.UnlockNewTower(levelManagerSO.newTower);

            UnLockTowerButton?.Invoke(this, new UnLockTowerButtonEventArgs { towerType = levelManagerSO.newTower});

            isFirstStart = false;
        }
    }

    public void ChangeLevelStateTo(LevelState levelState) {

        this.currentLevelState = levelState;

        switch (this.currentLevelState) {

            case LevelState.StartGame:

                OnStartGameState?.Invoke(this, EventArgs.Empty);

                break;

            case LevelState.GameRunning:
                // khi bắt đầu Turn

                OnGameRunningState?.Invoke(this, EventArgs.Empty);

                currentWave += 1;

                LevelManagerUI.Instance.UpdateVisual();

                break;

            case LevelState.EndTurn:
                // Khi kết thúc Turn

                if (currentWave == levelManagerSO.waveScriptList.Count) {
                    // If complete last wave

                    ChangeLevelStateTo(LevelState.WinGame);
                    return;
                }
                else {
                    // If not complete last wave

                    currentWaveScript = levelManagerSO.waveScriptList[currentWave];

                }

                HandleIntroduceNewEnemy();
                

                OnEndTurnState?.Invoke(this, EventArgs.Empty);

                break;

            case LevelState.WinGame:
                // Show Win GUI

                if (levelManagerSO.gameLevel == ILevelManager.GameLevel.Level5) {
                    // Reached last level of current biome

                    if (levelManagerSO.biomeType == ILevelManager.BiomeType.Swamp) {
                        // Reached last biome

                        // Todo: Show Special Win Game UI

                    }
                    else {
                        // Move to next biome
                        SaveData.UnlockNextBiome(levelManagerSO.biomeType, levelManagerSO.gameLevel, levelManagerSO.pointGetWhenWin);

                        SceneUIManager.SpawnUI(SceneUIManager.TypeUI.GameWinUI);

                    }
                }
                else {
                    // Not reached last level of current biome

                    SaveData.UnlockNextLevel(levelManagerSO.biomeType, levelManagerSO.gameLevel, levelManagerSO.pointGetWhenWin);

                    SceneUIManager.SpawnUI(SceneUIManager.TypeUI.GameWinUI);
                }

                break;

            case LevelState.LostGame:
                // Show Lost GUI


                SceneUIManager.SpawnUI(SceneUIManager.TypeUI.GameOverUI);

                break;

        }

    }

    public void ToggleGamePause() {

        isGamePaused = !isGamePaused;

        if (isGamePaused) {

            Time.timeScale = 0f;
            OnGamePause?.Invoke(this, EventArgs.Empty);
        }
        else {
            UnGamePause?.Invoke(this, EventArgs.Empty);
            Time.timeScale = 1f;
        }


    }

    public void ChangedHeartTo(ILevelManager.HeartChangedState heartChangedState) {

        if (heartChangedState == ILevelManager.HeartChangedState.Decrase) {
            currentHeart -= 1;
            HeartChanged?.Invoke(this, EventArgs.Empty);
        }
        
        if (heartChangedState == ILevelManager.HeartChangedState.Increase) {
            currentHeart += 1;
            HeartChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void ChangedCoinTo(ILevelManager.CoinChangedState coinChangedState, float coinValue) {

        if (coinChangedState == ILevelManager.CoinChangedState.Decrase) {
            currentCoin -= coinValue;
        }

        if (coinChangedState == ILevelManager.CoinChangedState.Increase) {
            currentCoin += coinValue;
        }

        LevelManagerUI.Instance.UpdateVisual();
    }


    public LevelState GetCurrentLevelState() {
        return this.currentLevelState;
    }

    public LevelManagerSO GetLevelManagerSO() {
        return this.levelManagerSO;
    }

    public float GetCurrentCoin() {
        return this.currentCoin;
    }

    public float GetCurrentHeart() {
        return this.currentHeart;
    }

    public float GetCurrentWave() {
        return this.currentWave;
    }

    public bool IsNotSetUpGame() {
        return this.currentLevelState != LevelState.StartGame;
    }
}
