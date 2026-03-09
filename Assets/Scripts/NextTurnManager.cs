using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextTurnManager : MonoBehaviour, IHasProgressBar
{
    public static NextTurnManager Instance { get; private set; }

    public event EventHandler OnNextTurn;
    public event EventHandler<IHasProgressBar.OnChangeProgressEventArgs> OnChangeProgress;

    [SerializeField] private Button nextTurnButton;
    [SerializeField] private float nextTurnTimerMax;

    private int currentAlivesEnemy;

    private Coroutine currentCoroutine;
    private bool canClickedButton = false;

    private void Awake() {

        Instance = this;

        nextTurnButton.onClick.AddListener(() => {

            if (currentCoroutine != null) {
                StopCoroutine(currentCoroutine);
                currentCoroutine = null;
            }

            NextTurn();
        });
    }

    private void Start() {

        PathGenerator.Instance.PathVisualShowAll += PathGenerator_PathVisualShowAll;

        LevelManager.Instance.OnEndTurnState += LevelManager_EndTurn;

        BaseEnemy.OnActiveEnemy += BaseEnemy_OnActiveEnemy;
        BaseEnemy.UnActiveEnemy += BaseEnemy_UnActiveEnemy;

        this.gameObject.SetActive(false);
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
        // After Path created done --> Set nextTurnManager pos

        Vector3 firstWaypointPos = PathGenerator.Instance.GetWaypointList()[0].transform.position;
        this.transform.position = new Vector3(firstWaypointPos.x, firstWaypointPos.y - 1.5f, firstWaypointPos.z);
        
    }

    private void LevelManager_EndTurn(object sender, System.EventArgs e) {

        this.gameObject.SetActive(true);

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);    
            currentCoroutine = null;    
        }

        currentCoroutine = StartCoroutine(ChangedToNextTurnCoroutine());

    }

    private IEnumerator ChangedToNextTurnCoroutine() {

        canClickedButton = false;

        // 1. Show button anim
        RectTransform nextTurnButtonRect = nextTurnButton.GetComponent<RectTransform>();
        nextTurnButtonRect.localScale = Vector3.zero;
        

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 0f });

        float duration = 0.3f;
        Sequence showScaleSequence = DOTween.Sequence();

        showScaleSequence.Append(nextTurnButtonRect.DOScale(Vector3.one, duration).SetEase(Ease.OutBack));

        yield return showScaleSequence.WaitForCompletion();

        // 2. Next turn progress

        canClickedButton = true;
        float nextTurnTimer = 0f;
        while (nextTurnTimer <= nextTurnTimerMax) {

            float smoothT = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(nextTurnTimer / nextTurnTimerMax));

            OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = smoothT });

            nextTurnTimer += Time.deltaTime;

            yield return null;
        }

        OnChangeProgress?.Invoke(this, new IHasProgressBar.OnChangeProgressEventArgs { progressNormalized = 1f });

        NextTurn();
    }

    private void NextTurn() {

        currentAlivesEnemy = 0;

        OnNextTurn?.Invoke(this, EventArgs.Empty);

        canClickedButton = false;
        RectTransform nextTurnButtonRect = nextTurnButton.GetComponent<RectTransform>();

        float duration = 0.3f;
        nextTurnButtonRect.DOScale(Vector3.zero, duration).OnComplete(() => {

            this.gameObject.SetActive(false);
        });
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
