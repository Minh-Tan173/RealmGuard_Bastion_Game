using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultTower : BaseTower, IHasClockTimer, ITowerObject
{
    public event EventHandler OnBuildingSFX;
    public event EventHandler UnBuildingSFX;

    public event EventHandler OnAttackZone;
    public event EventHandler UnAttackZone;
    public event EventHandler<ITowerObject.UpgradeAttackZoneEventArgs> UpdateAttackZone;

    public event EventHandler OnCatapultTowerUI;
    public event EventHandler UnCatapultTowerUI;
    public event EventHandler DeselectedAllUI;

    public event EventHandler<IHasClockTimer.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnUpgradeLevelProgress;
    public event EventHandler UnUpgradeLevelProgress;

    // Animator Event
    public event EventHandler OnUpgradeLevel1;
    public event EventHandler OnUpgradeLevel2;
    public event EventHandler OnUpgradeLevel3;
    public event EventHandler OnUpgradeLevel4;

    [Header("Data")]
    [SerializeField] private CatapultTowerSO catapulTowerSO;

    [Header("Child")]
    [SerializeField] private Catapult catapult;

    [Header("UI")]
    [SerializeField] private CatapultTowerUI catapultTowerUI;

    [Header("Spawn Point")]
    [SerializeField] private Transform catapulSpawnPoint;

    //private Collider2D[] enemyDetectedArray;
    private Dictionary<ITowerObject.LevelTower, CatapultTowerLevelData> catapulTowerDataDict;

    private CatapultTowerLevelData currentTowerStatus;
    private ITowerObject.LevelTower currentTowerLevel;

    private CatapultTowerLifeControl catapultTowerLifeControl;

    private float currentAttackZone;
    private bool isUpgrading;

    #region Radar Scan behavior
    private List<Collider2D> enemyDetectedList;
    private ContactFilter2D filter2D;
    private float scanTimer = 0f;
    #endregion

    #region UI
    private bool isOnArcherTowerUI = false;
    #endregion


    private void Awake() {

        catapultTowerLifeControl = GetComponent<CatapultTowerLifeControl>();

        catapulTowerDataDict = new Dictionary<ITowerObject.LevelTower, CatapultTowerLevelData>();

        // Setup radar
        enemyDetectedList = new List<Collider2D>();
        filter2D = new ContactFilter2D();
        filter2D.useTriggers = true;
        filter2D.useLayerMask = true;
        filter2D.SetLayerMask(catapulTowerSO.canAttackLayer);

        // Load Level Data into dictionary
        foreach (CatapultTowerLevelData catapulTowerLevelData in catapulTowerSO.towerLevelDataList) {

            ITowerObject.LevelTower currentKey = catapulTowerLevelData.levelTower;

            catapulTowerDataDict[currentKey] = catapulTowerLevelData;

        }

    }

    private void Start() {

        // After spawn
        ChangeStateTo(ITowerObject.LevelTower.Spawn);

    }

    private void Update() {

        scanTimer += Time.deltaTime;

        if (scanTimer >= catapulTowerSO.scanTimerMax) {
            scanTimer = 0f;

            Physics2D.OverlapCircle(this.transform.position, currentAttackZone, filter2D, enemyDetectedList);

            // Command catapult
            catapult.OnTowerScannedNewData();
        }

    }

    private IEnumerator UpgradeCoroutine() {

        float upgradeTimer = 0f;
        float upgradeTimerMax = currentTowerStatus.upgradeTimer;

        // First pos of upgrade progress
        OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = upgradeTimer / upgradeTimerMax });

        OnUpgradeLevelProgress?.Invoke(this, EventArgs.Empty);

        catapulSpawnPoint.localPosition = currentTowerStatus.catapulSpawnPos;

        yield return new WaitForSeconds(catapulTowerSO.placedSFXTimer);
        OnBuildingSFX?.Invoke(this, EventArgs.Empty);

        yield return null;

        // Upgrage progress
        while (upgradeTimer < upgradeTimerMax) {

            float elapsed = Mathf.Clamp01(upgradeTimer / upgradeTimerMax);
            float smoothTimer = Mathf.SmoothStep(0f, 1f, elapsed);

            OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = elapsed });

            upgradeTimer += Time.deltaTime;

            yield return null;

        }

        yield return null;

        // Last pos of upgrade progress
        OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = 1f });

        UnUpgradeLevelProgress?.Invoke(this, EventArgs.Empty);

        UnBuildingSFX?.Invoke(this, EventArgs.Empty);

        isUpgrading = false;

        if (currentTowerLevel != ITowerObject.LevelTower.Level1) {
            // Chuyển từ spawn sang Level1 thì ko on UI
            isOnArcherTowerUI = true;
            OnCatapultTowerUI?.Invoke(this, EventArgs.Empty);
        }


    }

    public override void HandleLeftClicked() {

        if (isUpgrading || isOnArcherTowerUI) {
            return;
        }

        // Attack zone
        OnAttackZone?.Invoke(this, EventArgs.Empty);
        UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });


        // ArcherTowerUI control
        isOnArcherTowerUI = true;
        OnCatapultTowerUI?.Invoke(this, EventArgs.Empty);

    }

    public override void HandleRightClicked() {

        if (isOnArcherTowerUI) {

            UnAttackZone?.Invoke(this, EventArgs.Empty);
            UnCatapultTowerUI?.Invoke(this, EventArgs.Empty);

            isOnArcherTowerUI = false;

        }

    }

    public override void HandleDeselectedAll() {

        DeselectedAllUI?.Invoke(this, EventArgs.Empty);
        isOnArcherTowerUI = false;

        if (!isUpgrading) {
            // If method was call when not upgrading

            UnAttackZone?.Invoke(this, EventArgs.Empty);

        }
        else {
            // If method was call when upgrading

        }
    }

    public override void HitDamage(float damageGet) {
        catapultTowerLifeControl.TakeDamage(damageGet);
    }

    public override bool IsDeselectLastUI() {
        // If last ui was deselect
        return isOnArcherTowerUI == false;
    }

    public override bool IsRecoveringHealth() {
        return catapultTowerLifeControl.IsRecoveringHealth();
    }

    public void ChangeStateTo(ITowerObject.LevelTower levelTower) {

        this.currentTowerLevel = levelTower;

        isUpgrading = true;
        HandleDeselectedAll();

        // Update Data
        switch (this.currentTowerLevel) {

            case ITowerObject.LevelTower.Spawn:

                OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = 0f });

                ChangeStateTo(ITowerObject.LevelTower.Level1);

                break;

            case ITowerObject.LevelTower.Level1:

                currentTowerStatus = catapulTowerDataDict[currentTowerLevel];
                currentAttackZone = catapulTowerSO.baseRange; // Mặc định level 1 dùng baseRange

                OnUpgradeLevel1?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = this.currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level2:

                currentTowerStatus = catapulTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel2?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level3:

                currentTowerStatus = catapulTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel3?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level4:

                currentTowerStatus = catapulTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel4?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                break;

        }

        // Start upgrade
        StartCoroutine(UpgradeCoroutine());


    }

    public float GetCurrentPrice() {
        return catapulTowerSO.price; ;
    }

    public CatapultTowerSO GetCatapultTowerSO() {
        return this.catapulTowerSO;
    }

    public List<Collider2D> GetEnemyDetectedList() {
        return this.enemyDetectedList;
    }

    public CatapultTowerLevelData GetCurrentTowerStatus() {
        return this.currentTowerStatus;
    }

    public float GetCurrentAttackRange() {
        return this.currentAttackZone;
    }

    public Transform GetSpawnPoint(SoldierSO.SoldierDirection catapultDirection) {
        return catapulSpawnPoint;
    }


    public CatapultTowerLifeControl GetArcherTowerLifeControl() {
        return this.catapultTowerLifeControl;
    }

    public CatapultTowerUI GetCatapultTowerUI() {
        return this.catapultTowerUI;
    }

    public Dictionary<ITowerObject.LevelTower, CatapultTowerLevelData> GetCatapulTowerDataDict() {
        return catapulTowerDataDict;
    }

    public bool IsUpgrading() {
        return this.isUpgrading;
    }


    // ----- VISUALIZE -----

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentAttackZone); // viền
    }

}
