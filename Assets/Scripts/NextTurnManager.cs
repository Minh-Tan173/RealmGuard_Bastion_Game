using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnManager : MonoBehaviour
{
    public static NextTurnManager Instance { get; private set; }

    public event EventHandler OnNextTurn;

    [SerializeField] private Button nextTurnButton;

    private int currentAlivesEnemy;

    private void Awake() {

        Instance = this;

        nextTurnButton.onClick.AddListener(() => {

            currentAlivesEnemy = 0;

            OnNextTurn?.Invoke(this, EventArgs.Empty);

            Hide();
        });
    }

    private void Start() {

        PathGenerator.Instance.PathVisualShowAll += PathGenerator_PathVisualShowAll;

        LevelManager.Instance.OnEndTurnState += LevelManager_EndTurn;

        BaseEnemy.OnActiveEnemy += BaseEnemy_OnActiveEnemy;
        BaseEnemy.UnActiveEnemy += BaseEnemy_UnActiveEnemy;

        Hide();
    }

    private void OnDestroy() {
        PathGenerator.Instance.PathVisualShowAll -= PathGenerator_PathVisualShowAll;

        LevelManager.Instance.OnEndTurnState -= LevelManager_EndTurn;

        BaseEnemy.OnActiveEnemy -= BaseEnemy_OnActiveEnemy;
        BaseEnemy.UnActiveEnemy -= BaseEnemy_UnActiveEnemy;

    }



    private void BaseEnemy_UnActiveEnemy(object sender, EventArgs e) {
        // When has enemy destroy or hide

        currentAlivesEnemy -= 1;

        // Check End Wave condition
        TryEndTurn();

    }

    private void BaseEnemy_OnActiveEnemy(object sender, EventArgs e) {
        // When has enemy active

        currentAlivesEnemy += 1;
    }

    private void PathGenerator_PathVisualShowAll(object sender, System.EventArgs e) {

        // After Path created done
        this.transform.position = PathGenerator.Instance.GetWaypointList()[0].transform.position;
        
    }

    private void LevelManager_EndTurn(object sender, System.EventArgs e) {

        Show();

    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }

    public void TryEndTurn() {

        if (LevelManager.Instance.GetCurrentLevelState() != LevelManager.LevelState.GameRunning) {
            //  When game is not running, dont run this method
            return;
        }

        if (!EnemySpawner.Instance.IsSpawningWave() && currentAlivesEnemy <= 0) {
            // If spawn all enemy in script and currentAlives <= 0

            currentAlivesEnemy = 0;
            LevelManager.Instance.ChangeLevelStateTo(LevelManager.LevelState.EndTurn);
        }

    }
}
