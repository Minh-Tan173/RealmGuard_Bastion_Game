using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MageTower : BaseTower, IHasClockTimer, ITowerObject
{

    public event EventHandler OnChangeMageTowerType;

    public event EventHandler OnBuildingSFX;
    public event EventHandler UnBuildingSFX;

    public event EventHandler OnAttackZone;
    public event EventHandler UnAttackZone;
    public event EventHandler<ITowerObject.UpgradeAttackZoneEventArgs> UpdateAttackZone;

    public event EventHandler OnMageTowerUI;
    public event EventHandler UnMageTowerUI;
    public event EventHandler UnBuyMageUI;
    public event EventHandler DeselectedAllUI;

    public event EventHandler<IHasClockTimer.OnChangeProgressEventArgs> OnChangeProgress;

    public event EventHandler OnUpgradeLevelProgress;
    public event EventHandler UnUpgradeLevelProgress;

    // Animator Event
    public event EventHandler OnUpgradeLevel1;
    public event EventHandler OnUpgradeLevel2;
    public event EventHandler OnUpgradeLevel3;
    public event EventHandler OnUpgradeLevel4;

    [Header("Mage Tower Data")]
    [SerializeField] private MageTowerSO mageTowerSO;

    [Header("UI")]
    [SerializeField] private MageTowerUI mageTowerUI;

    [Header("ArcherSpawner Data")]
    [SerializeField] private Transform mageSpawner;
    [SerializeField] private Transform upSpawnPoint;
    [SerializeField] private Transform rightSpawnPoint;
    [SerializeField] private Transform downSpawnPoint;
    [SerializeField] private Transform leftSpawnPoint;

    private MageTowerLifeControl mageTowerLifeControl;
    private BoxCollider2D boxCollider2D;
    private Collider2D[] enemyDetectedArray;
    private List<Mage> mageList;

    private MageTowerSO.MageType currentMageType;
    private LayerMask currentAttackLayer;

    private Dictionary<ITowerObject.LevelTower, MageTowerLevelData> mageTowerDataDict;
    private ITowerObject.LevelTower currentTowerLevel;
    private MageTowerLevelData currentTowerStatus;

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

        boxCollider2D = GetComponent<BoxCollider2D>();
        mageTowerLifeControl = GetComponent<MageTowerLifeControl>();
        SetBoxColliderEnable(true);

        mageTowerDataDict = new Dictionary<ITowerObject.LevelTower, MageTowerLevelData>();

        mageList = new List<Mage>();

        // Setup Enemy Detected List Base On Direction
        upEnemyList = new List<Collider2D>();
        rightEnemyList = new List<Collider2D>();
        downEnemyList = new List<Collider2D>();
        leftEnemyList = new List<Collider2D>();

        // Setup Radar
        enemyDetectedList = new List<Collider2D>();
        filter2D.useTriggers = true;
        filter2D.useLayerMask = true;

        // Load Level Data into dictionary
        foreach (MageTowerLevelData mageTowerLevelData in mageTowerSO.towerLevelDataList) {
            mageTowerDataDict[mageTowerLevelData.levelTower] = mageTowerLevelData;
        }

    }

    private void Start() {

        ChangeMageTowerTypeTo(MageTowerSO.MageType.Ground);

        // After spawn
        ChangeStateTo(ITowerObject.LevelTower.Spawn);

    }

    private void Update() {

        scanTimer += Time.deltaTime;

        if (scanTimer >= mageTowerSO.scanTimerMax) {

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

                Vector2 dirFromMageToEnemy = enemyDetected.transform.position - this.transform.position;
                float xPos = dirFromMageToEnemy.x;
                float yPos = dirFromMageToEnemy.y;

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

            // 2. Command Mage Base on Enemy List
            foreach (Mage mage in mageList) {

                List<Collider2D> enemyList = GetEnemyListByDirection(mage.GetMageDirection());

                if (enemyList.Count <= 0) {
                    // If this direction dont have any enemy in list

                    continue;
                }

                mage.OnTowerScannedNewData(enemyList);
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

        mageSpawner.localPosition = currentTowerStatus.mageSpawnPos;

        yield return new WaitForSeconds(mageTowerSO.placedSFXTimer);

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
            OnMageTowerUI?.Invoke(this, EventArgs.Empty);
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
        OnMageTowerUI?.Invoke(this, EventArgs.Empty);

    }

    public override void HandleRightClicked() {

        if (mageTowerUI.HasBuyMageUI()) {
            // If is on BuyArcherUI

            UnBuyMageUI?.Invoke(this, EventArgs.Empty);
            OnMageTowerUI?.Invoke(this, EventArgs.Empty);

            isOnArcherTowerUI = true;

            return;
        }

        if (isOnArcherTowerUI) {

            UnAttackZone?.Invoke(this, EventArgs.Empty);
            UnMageTowerUI?.Invoke(this, EventArgs.Empty);

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
        mageTowerLifeControl.TakeDamage(damageGet);
    }

    public override bool IsDeselectLastUI() {
        // If last ui was deselect
        return isOnArcherTowerUI == false;
    }

    public override bool IsRecoveringHealth() {
        return mageTowerLifeControl.IsRecoveringHealth();
    }

    public void ChangeMageTowerTypeTo(MageTowerSO.MageType mageType) {

        this.currentMageType = mageType;

        switch (this.currentMageType) {

            case MageTowerSO.MageType.Ground:

                currentAttackLayer = mageTowerSO.groundEnemyLayer;

                break;

            case MageTowerSO.MageType.Sky:

                currentAttackLayer = mageTowerSO.flyEnemyLayer;

                break;

            case MageTowerSO.MageType.River:

                // To do: river enemy

                break;
        }

        // Set LayerMask for Filter
        filter2D.SetLayerMask(currentAttackLayer);

        OnChangeMageTowerType?.Invoke(this, EventArgs.Empty);
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

                currentTowerStatus = mageTowerDataDict[currentTowerLevel];
                currentAttackZone = mageTowerSO.baseRange; // Mặc định level 1 dùng baseRange

                OnUpgradeLevel1?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = this.currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level2:

                currentTowerStatus = mageTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel2?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level3:

                currentTowerStatus = mageTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel3?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level4:

                currentTowerStatus = mageTowerDataDict[currentTowerLevel];
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
        return mageTowerSO.price; ;
    }

    public MageTowerSO GetMageTowerSO() {
        return this.mageTowerSO;
    }

    public Collider2D[] GetEnemyDetectedArray() {
        return this.enemyDetectedArray;
    }

    public MageTowerLevelData GetCurrentTowerStatus() {
        return this.currentTowerStatus;
    }

    public float GetCurrentAttackRange() {
        return this.currentAttackZone;
    }

    public MageTowerSO.MageType GetCurrentMageType() {

        Debug.Log(this.currentMageType.ToString());

        return this.currentMageType;
    }

    public Transform GetSpawnPoint(SoldierSO.SoldierDirection mageDirection) {

        switch (mageDirection) {
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

    public LayerMask GetCurrentAttackLayer() {
        return this.currentAttackLayer;
    }

    public List<Mage> GetMageList() {
        return this.mageList;
    }

    public MageTowerLifeControl GetMageTowerLifeControl() {
        return this.mageTowerLifeControl;
    }

    public MageTowerUI GetMageTowerUI() {
        return this.mageTowerUI;
    }

    public Dictionary<ITowerObject.LevelTower, MageTowerLevelData> GetMageTowerDataDict() {
        return this.mageTowerDataDict;
    }

    public bool IsUpgrading() {
        return this.isUpgrading;
    }


    public void AddToMageList(Mage mage) {
        mageList.Add(mage);
    }

    public void SetBoxColliderEnable(bool isEnable) {
        this.boxCollider2D.enabled = isEnable;
    }


    // ----- VISUALIZE -----

    private void OnDrawGizmos() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, currentAttackZone); // viền
    }
}
