using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;

public class ArcherTower : BaseTower, IHasClockTimer, ITowerObject, IHasFieldOfView
{
    public event EventHandler OnBuildingSFX;
    public event EventHandler UnBuildingSFX;

    public event EventHandler OnAttackZone;
    public event EventHandler UnAttackZone;
    public event EventHandler<ITowerObject.UpgradeAttackZoneEventArgs> UpdateAttackZone;

    public event EventHandler OnArcherTowerUI;
    public event EventHandler UnArcherTowerUI;
    public event EventHandler UnBuyArcherUI;
    public event EventHandler DeselectedAllUI;

    public event EventHandler<IHasClockTimer.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnUpgradeLevelProgress;
    public event EventHandler UnUpgradeLevelProgress;

    // Animator Event
    public event EventHandler OnUpgradeLevel1;
    public event EventHandler OnUpgradeLevel2;
    public event EventHandler OnUpgradeLevel3;
    public event EventHandler OnUpgradeLevel4;

    public event EventHandler<IHasFieldOfView.OnFOVVisualEventArgs> OnFOVVisual;
    public event EventHandler<IHasFieldOfView.OnUpdateFOVSizeEventArgs> UpdateFOVSize;
    public event EventHandler ShowFOVVisual;
    public event EventHandler HideFOVVisual;

    [Header("Archer Data")]
    [SerializeField] private ArcherTowerSO archerTowerSO;

    [Header("UI")]
    [SerializeField] private ArcherTowerUI archerTowerUI;

    [Header("ArcherSpawner Data")]
    [SerializeField] private Transform archerSpawner;
    [SerializeField] private Transform upSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;
    [SerializeField] private Transform downSpawnPoint;
    [SerializeField] private Transform leftSpawnPoint;

    private ArcherTowerLifeControl archerTowerLifeControl;
    private List<Archer> archerList;

    private Dictionary<ITowerObject.LevelTower, ArcherTowerLevelData> archerTowerDataDict;
    private ITowerObject.LevelTower currentTowerLevel;
    private ArcherTowerLevelData currentTowerStatus;

    private float currentAttackZone;
    private bool isUpgrading;

    #region Enemy List Base on Direction
    private List<Collider2D> upEnemyList;
    private List<Collider2D> rightEnemyList;
    private List<Collider2D> downEnemyList;
    private List<Collider2D> leftEnemyList;
    #endregion

    #region Radar Scan Behavior
    private List<Collider2D> enemyDetectedList;
    private ContactFilter2D filter2D;
    private float scanTimer = 0f;
    #endregion

    #region UI
    private bool isOnArcherTowerUI = false;
    #endregion

    private void Awake() {

        archerTowerLifeControl = GetComponent<ArcherTowerLifeControl>();

        archerTowerDataDict = new Dictionary<ITowerObject.LevelTower, ArcherTowerLevelData>();

        archerList = new List<Archer>();

        // Setup Enemy Detected List Base On Direction
        upEnemyList = new List<Collider2D>();
        rightEnemyList = new List<Collider2D>();
        downEnemyList = new List<Collider2D>();
        leftEnemyList = new List<Collider2D>();

        // Setup Radar
        enemyDetectedList = new List<Collider2D>();
        filter2D.useTriggers = true;
        filter2D.useLayerMask = true;
        filter2D.SetLayerMask(archerTowerSO.canAttackLayer);

        // Load Level Data into dictionary
        foreach (ArcherTowerLevelData archerTowerLevelData in archerTowerSO.towerLevelDataList) {
            archerTowerDataDict[archerTowerLevelData.levelTower] = archerTowerLevelData;
        }

    }

    private void Start() {

        // After spawn
        ChangeLevelTo(ITowerObject.LevelTower.Spawn);

    }

    private void Update() {

        scanTimer += Time.deltaTime;

        if (scanTimer >= archerTowerSO.scanTimerMax) {
            scanTimer = 0f;

            upEnemyList.Clear();
            rightEnemyList.Clear();
            downEnemyList.Clear();
            leftEnemyList.Clear();

            Physics2D.OverlapCircle(this.transform.position, currentAttackZone, filter2D, enemyDetectedList);

            // --- COMMAND ARCHER ---

            // 0. Ensure that enemis have been detected before sort
            if (enemyDetectedList.Count <= 0) {
                return;
            }

            // 1. Sort enemy base on their Direction with Tower
            foreach (Collider2D enemyDetected in enemyDetectedList) {

                // 1.1 Skip enemies with magic resistance
                if (enemyDetected.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                    if (baseEnemy.IsResistPhysic()) {

                        continue;
                    }
                }
                else {
                    Debug.LogError("This enemy doesn't inherit from BaseEnemy class");
                    return;
                }

                Vector2 dirFromArcherToEnemy = enemyDetected.transform.position - this.transform.position;
                float xPos = dirFromArcherToEnemy.x;
                float yPos = dirFromArcherToEnemy.y;

                if (Mathf.Abs(xPos) <= yPos) {
                    // Enemy is UP with Tower

                    upEnemyList.Add(enemyDetected);

                } 
                else if (Mathf.Abs(xPos) <= -yPos) {
                    // Enemy is DOWN with Tower

                    downEnemyList.Add(enemyDetected);

                }
                else if (Mathf.Abs(yPos) <= xPos) {
                    // Enemy is Right with Tower

                    rightEnemyList.Add(enemyDetected);

                }
                else if (Mathf.Abs(yPos) <= -xPos) {
                    // Enemy is Left with Tower

                    leftEnemyList.Add(enemyDetected);

                }

            }

            // 2. Command Archer Base on Enemy List
            foreach (Archer archer in archerList) {

                List<Collider2D> enemyList = GetEnemyListByDirection(archer.GetArcherDirection());

                if (enemyList.Count <= 0) {
                    // If this direction dont have any enemy in list

                    continue;
                }

                archer.OnTowerScannedNewData(enemyList);
            }
        }

    }

    private List<Collider2D> GetEnemyListByDirection(SoldierSO.SoldierDirection soldierDirection) {

        switch (soldierDirection) {

            case SoldierSO.SoldierDirection.Up:
                return upEnemyList;
            case SoldierSO.SoldierDirection.Right:
                return rightEnemyList;
            case SoldierSO.SoldierDirection.Down:
                return downEnemyList;
            case SoldierSO.SoldierDirection.Left:
                return leftEnemyList;

            default:
                return null;
        }
    }

    private IEnumerator UpgradeCoroutine() {

        float upgradeTimer = 0f;
        float upgradeTimerMax = currentTowerStatus.upgradeTimer;

        // First pos of upgrade progress
        OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = upgradeTimer / upgradeTimerMax });

        OnUpgradeLevelProgress?.Invoke(this, EventArgs.Empty);

        archerSpawner.localPosition = currentTowerStatus.archerSpawnPos;

        yield return new WaitForSeconds(archerTowerSO.placedSFXTimer);
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
            OnArcherTowerUI?.Invoke(this, EventArgs.Empty);
        }


    }

    public override void HandleLeftClicked() {
        
        if (isUpgrading || isOnArcherTowerUI) {
            return;
        }

        // Attack zone
        OnAttackZone?.Invoke(this, EventArgs.Empty);
        UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone =  currentAttackZone});

        // FOV Visual
        ShowFOVVisual?.Invoke(this, EventArgs.Empty);

        // ArcherTowerUI control
        isOnArcherTowerUI = true;
        OnArcherTowerUI?.Invoke(this, EventArgs.Empty);

    }

    public override void HandleRightClicked() {

        if (archerTowerUI.HasBuyArcherUI()) {
            // If is on BuyArcherUI

            UnBuyArcherUI?.Invoke(this, EventArgs.Empty);
            OnArcherTowerUI?.Invoke(this, EventArgs.Empty);

            isOnArcherTowerUI = true;

            return;
        }

        if (isOnArcherTowerUI) {

            UnAttackZone?.Invoke(this, EventArgs.Empty);
            HideFOVVisual?.Invoke(this, EventArgs.Empty);
            UnArcherTowerUI?.Invoke(this, EventArgs.Empty);

            isOnArcherTowerUI = false;

        }

    }

    public override void HandleDeselectedAll() {

        DeselectedAllUI?.Invoke(this, EventArgs.Empty);
        isOnArcherTowerUI = false;

        if (!isUpgrading) {
            // If method was call when not upgrading

            UnAttackZone?.Invoke(this, EventArgs.Empty);
            HideFOVVisual?.Invoke(this, EventArgs.Empty);
        }
        else {
            // If method was call when upgrading

        }
    }

    public override void HitDamage(float damageGet) {
        archerTowerLifeControl.TakeDamage(damageGet);
    }

    public override bool IsDeselectLastUI() {
        // If last ui was deselect
        return isOnArcherTowerUI == false;
    }

    public override bool IsRecoveringHealth() {
        return archerTowerLifeControl.IsRecoveringHealth();
    }

    public void ChangeLevelTo(ITowerObject.LevelTower levelTower) {

        this.currentTowerLevel = levelTower;

        isUpgrading = true;
        HandleDeselectedAll();

        // Update Data
        switch (this.currentTowerLevel) {

            case ITowerObject.LevelTower.Spawn:

                OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = 0f });

                ChangeLevelTo(ITowerObject.LevelTower.Level1);

                break;

            case ITowerObject.LevelTower.Level1:

                currentTowerStatus = archerTowerDataDict[currentTowerLevel];
                currentAttackZone = archerTowerSO.baseRange; // Mặc định level 1 dùng baseRange

                OnUpgradeLevel1?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = this.currentAttackZone });
                UpdateFOVSize?.Invoke(this, new IHasFieldOfView.OnUpdateFOVSizeEventArgs { size = this.currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level2:

                currentTowerStatus = archerTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel2?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });
                UpdateFOVSize?.Invoke(this, new IHasFieldOfView.OnUpdateFOVSizeEventArgs { size = this.currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level3:

                currentTowerStatus = archerTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel3?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });
                UpdateFOVSize?.Invoke(this, new IHasFieldOfView.OnUpdateFOVSizeEventArgs { size = this.currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level4:

                currentTowerStatus = archerTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel4?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });
                UpdateFOVSize?.Invoke(this, new IHasFieldOfView.OnUpdateFOVSizeEventArgs { size = this.currentAttackZone });

                break;

        }

        // Start upgrade
        StartCoroutine(UpgradeCoroutine());


    }

    public ArcherTowerSO GetArcherTowerSO() {
        return this.archerTowerSO;
    }

    public List<Collider2D> GetEnemyDetectedList() {
        return this.enemyDetectedList;
    }

    public ArcherTowerLevelData GetCurrentTowerStatus() {
        return this.currentTowerStatus;
    }

    public float GetCurrentAttackRange() {
        return this.currentAttackZone;
    }

    public Transform GetSpawnPoint(SoldierSO.SoldierDirection archerDirection) {
        // Method này chỉ được call khi bought new archer ---> Unlock FOV của archer đó

        OnFOVVisual?.Invoke(this, new IHasFieldOfView.OnFOVVisualEventArgs { soldierDirection = archerDirection, size = this.currentAttackZone });

        switch (archerDirection) {
            case SoldierSO.SoldierDirection.Up:
                return upSpawnPoint;
            case SoldierSO.SoldierDirection.Right:
                return rightSpawnPoint;
            case SoldierSO.SoldierDirection.Down:
                return downSpawnPoint;
            case SoldierSO.SoldierDirection.Left:
                return leftSpawnPoint;
        }

        return null;    
    }

    public List<Archer> GetArcherList() {
        return this.archerList;
    }

    public ArcherTowerLifeControl GetArcherTowerLifeControl() {
        return this.archerTowerLifeControl;
    }

    public ArcherTowerUI GetArcherTowerUI() {
        return this.archerTowerUI;
    }

    public Dictionary<ITowerObject.LevelTower, ArcherTowerLevelData> GetArcherTowerDataDict() {
        return this.archerTowerDataDict;
    }

    public bool IsUpgrading() {
        return this.isUpgrading;
    }


    public void AddToArcherList(Archer archer) {
        archerList.Add(archer);
    }


    // ----- VISUALIZE -----

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentAttackZone); // viền
    }
}
