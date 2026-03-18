using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianTower : BaseTower, IHasClockTimer, ITowerObject
{
    public event EventHandler OnBuildingSFX;
    public event EventHandler UnBuildingSFX;
    public event EventHandler OnDeployCompleteSFX;

    public event EventHandler OnAttackZone;
    public event EventHandler UnAttackZone;
    public event EventHandler<ITowerObject.UpgradeAttackZoneEventArgs> UpdateAttackZone;

    public event EventHandler OnAssignPositionZone;
    public event EventHandler UnAssignPositionZone;

    public event EventHandler OnGuardianTowerUI;
    public event EventHandler UnGuardianTowerUI;

    public event EventHandler DeselectedAllUI;

    public event EventHandler OnSpawnGuardian;
    public event EventHandler OnResetDataGuardian;

    public event EventHandler<IHasClockTimer.OnChangeProgressEventArgs> OnChangeProgress;

    // Upgrade Progress Event
    public event EventHandler OnUpgradeLevelProgress;
    public event EventHandler UnUpgradeLevelProgress;

    // Deploy Guardian Progress Event
    public event EventHandler OnDeployPorgress;
    public event EventHandler UnDeployProgress;

    // Animator Event
    public event EventHandler OnUpgradeLevel1;
    public event EventHandler OnUpgradeLevel2;
    public event EventHandler OnUpgradeLevel3;
    public event EventHandler OnUpgradeLevel4;

    // Guardian Control Event
    public event EventHandler OnReleaseGuardianTarget;

    //public event EventHandler<OnResetViewDirectionEventArgs> OnResetViewDirection;
    //public class OnResetViewDirectionEventArgs : EventArgs {
    //    public Vector3 centerGuardPos;

    //}

    [Header("Guardian Data")]
    [SerializeField] private GuardianTowerSO guardianTowerSO;
    [SerializeField] private float aimTimerMax;

    [Header("Spawner")]
    [SerializeField] private Transform spawnerGuardian;
    [SerializeField] private Transform spawnPoint;

    [Header("Visual + UI")]
    [SerializeField] private GuardianTowerUI guardianTowerUI;
    [SerializeField] private AssignPositionZoneVisual assignPositionZoneVisual;

    private GuardianTowerLifeControl guardianTowerLifeControl;
    private Collider2D[] enemyDetectedArray;
    private List<Guardian> guardianList;

    private Dictionary<ITowerObject.LevelTower, GuardianTowerLevelData> guardianTowerDataDict;
    private ITowerObject.LevelTower currentTowerLevel;
    private GuardianTowerLevelData currentTowerStatus;

    private Coroutine currentDeployGuardianCoroutine;

    private Vector3 currentCenterGuardPos;
    private Vector3 defaultRallyPos;

    private float currentAttackZone;
    private bool isUpgrading;

    private int currentAlive;

    #region Aim Behavior
    private float aimTimer;
    private bool isForceReAim;
    #endregion

    #region UI
    private bool isOnGuardianTowerUI = false;
    private bool isOnAssignPositionVisual = false;
    #endregion

    private void Awake() {

        guardianTowerLifeControl = GetComponent<GuardianTowerLifeControl>();

        guardianTowerDataDict = new Dictionary<ITowerObject.LevelTower, GuardianTowerLevelData>();

        guardianList = new List<Guardian>();

        // Load Level Data into dictionary
        foreach (GuardianTowerLevelData guardianTowerLevelData in guardianTowerSO.towerLevelDataList) {
            guardianTowerDataDict[guardianTowerLevelData.levelTower] = guardianTowerLevelData;
        }

    }

    private void Start() {

        assignPositionZoneVisual.OnMoveCommand += AssignPositionZoneVisual_OnMoveCommand;

        // After spawn
        ChangeStateTo(ITowerObject.LevelTower.Spawn);

    }

    private void OnDestroy() {

        assignPositionZoneVisual.OnMoveCommand -= AssignPositionZoneVisual_OnMoveCommand;

        foreach (Guardian guardian in guardianList) {

            guardian.OnGuardianRequestTask -= Guardian_OnGuardianRequestTask;
            guardian.GetGuardianLifeControl().OnDeath -= Guardian_OnDeath;

        }


    }

    private void AssignPositionZoneVisual_OnMoveCommand(object sender, AssignPositionZoneVisual.OnMoveCommandEventArgs e) {

        currentCenterGuardPos = e.mouseClickedPos;

        OnReleaseGuardianTarget?.Invoke(this, EventArgs.Empty);

        ArrangeAliveSquadToSingleRing(e.mouseClickedPos);
    }

    private void Update() {

        aimTimer -= Time.deltaTime;

        if (IsForceReAim()) {
            FindNewTarget();
        }

    }


    private void FindNewTarget() {

        aimTimer = aimTimerMax;
        isForceReAim = false;

        enemyDetectedArray = Physics2D.OverlapCircleAll(this.transform.position, currentAttackZone, guardianTowerSO.canAttackLayer);

        if (enemyDetectedArray.Length > 0) {
            // Nếu phát hiện enemy thỏa mãn điều kiện thì phát lệnh điều phối Guardian


            List<Collider2D> enemyValidList = new List<Collider2D>();

            // 1. Lập List Enemy hợp lệ (Không locked target nào)
            foreach (Collider2D enemyDetected in enemyDetectedArray) {

                // Nếu enemy đó đang locked 1 target khác --> Bỏ qua
                if (enemyDetected.TryGetComponent<ICanAttackPhysic>(out ICanAttackPhysic enemy)) {

                    if (enemy.HasLockedTarget()) {

                        continue;
                    }

                }
                else {
                    // Enemy đó không inherit interface ICanAttackPhysic

                    continue;
                }

                // Nếu enemy đó đang là intent Target của guardian trong List
                if (enemyDetected.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                    if (IsEnemyBookedByAnyGuardian(baseEnemy)) {

                        continue;
                    }

                }

                enemyValidList.Add(enemyDetected);
            }

            // 2. Sort các element của List theo thứ tự gần tower nhất tới xa nhất
            enemyValidList.Sort((a, b) => {

                float sqrDistanceToA = (this.transform.position - a.transform.position).sqrMagnitude;
                float sqrDistanceToB = (this.transform.position - b.transform.position).sqrMagnitude;

                int compareDistance = sqrDistanceToA.CompareTo(sqrDistanceToB);

                if (compareDistance == 0) {
                    // Khoảng cách bằng nhau (rất khó vì a, b đi random)

                    // Todo: So sánh current health của a và b
                }

                return compareDistance;

            });

            // 3. Sau khi sort xong - Phân phối cho Guardian đang idle
            foreach (Collider2D enemyValid in enemyValidList) {

                if (enemyValid.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                    AssignTargetHandle(baseEnemy);

                }

            }

        }

        // Sau khi điều phối
        foreach (Guardian guardian in guardianList) {

            if (guardian.GetCurrentGuadianState() == Guardian.GuadianState.Idle && guardian.IsHasGuardPosBefore() && !guardian.GetGuardianLifeControl().IsDeath()) {
                // Nếu có guardian nào đang rảnh rỗi, chưa chết và chưa về vị trí guard pos (nếu có)

                guardian.CommandMoveToGuardPos();
            }
        }
    }

    private void AssignTargetHandle(BaseEnemy intentTarget) {

        foreach (Guardian guardian in guardianList) {

            if (!guardian.IsHasTarget() && !guardian.GetGuardianLifeControl().IsDeath()) {
                // Todo: Assign Target for guardian is free and not death 

                guardian.CommanMoveToIntentTarget(intentTarget);

                break;
            }
        }

    }

    private IEnumerator UpgradeCoroutine() {

        float upgradeTimer = 0f;
        float upgradeTimerMax = currentTowerStatus.upgradeTimer;

        yield return null;

        // First pos of upgrade progress
        OnChangeProgress?.Invoke(this, new IHasClockTimer.OnChangeProgressEventArgs { progressNormalized = upgradeTimer / upgradeTimerMax });
        OnUpgradeLevelProgress?.Invoke(this, EventArgs.Empty);

        if (currentDeployGuardianCoroutine != null) {
            // Đảm bảo ngừng coroutine deploy guardian đã trigger ở chỗ khác

            StopCoroutine(currentDeployGuardianCoroutine);
            currentDeployGuardianCoroutine = null;
        }

        yield return new WaitForSeconds(0.05f);
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

        // Comman Guardian move to nearest rally point
        yield return null;

        currentDeployGuardianCoroutine = StartCoroutine(DeploySquadCoroutine());
    }

    private void PreparedGuardian() {

        int maxGuardianCount = currentTowerStatus.countOfGuardian;
        int currentGuardianCount = guardianList.Count;

        while (currentGuardianCount < maxGuardianCount) {
            // If dont have enough guardian needed

            Guardian guardian= Guardian.SpawnGuardian(guardianTowerSO.guardianSO.soldierPrefab, this);
            guardianList.Add(guardian);

            // Guardian Event Listener
            guardian.OnGuardianRequestTask += Guardian_OnGuardianRequestTask;
            guardian.GetGuardianLifeControl().OnDeath += Guardian_OnDeath;

            currentGuardianCount += 1;
        }
    }

    private void Guardian_OnDeath(object sender, EventArgs e) {

        currentAlive -= 1;

        if (currentAlive == 0) {

            if (currentDeployGuardianCoroutine != null) {
                StopCoroutine(currentDeployGuardianCoroutine);
                currentDeployGuardianCoroutine = null;
            }

            currentDeployGuardianCoroutine = StartCoroutine(DeploySquadCoroutine());
        }
    }

    private void Guardian_OnGuardianRequestTask(object sender, EventArgs e) {
        isForceReAim = true;
    }

    private Vector3 FindNearestRallyPoint() {

        // 1. Tìm Vector hướng của 2 waypoint gần Tower nhất
        List<Transform> waypointList = new List<Transform>(PathGenerator.Instance.GetWaypointList());
        float closestDistance = Mathf.Infinity;
        Vector3 nearestPoint = Vector3.zero;

        for (int i = 0; i < waypointList.Count - 1; i++) {

            Vector3 A = waypointList[i].position;
            Vector3 B = waypointList[i + 1].position;

            Vector3 AB = B - A;
            Vector3 PA = this.transform.position - A;

            float t = Mathf.Clamp01(Vector3.Dot(PA, AB) / AB.sqrMagnitude);
            Vector3 projectedPoint = A + t * AB;

            float sqrDistanceToTPoint = (this.transform.position - projectedPoint).sqrMagnitude;

            if (sqrDistanceToTPoint <= closestDistance) {

                closestDistance = sqrDistanceToTPoint;
                nearestPoint =  projectedPoint;
            }
        }

        GridNode gridNode = GridManager.Instance.GetNodeAt(nearestPoint);
        Vector3 centerNode = GridManager.Instance.NodePosConvertToWordPos(gridNode.GetNodePosition());

        return new Vector3(centerNode.x - 0.5f, centerNode.y - 0.5f, centerNode.z);
    }

    private void ArrangeAliveSquadToSingleRing(Vector3 centerSquadPos) {

        List<Guardian> guardianAliveList = new List<Guardian>();

        foreach (Guardian guardian in guardianList) {

            if (guardian.GetGuardianLifeControl().GetCurrentLifeState() == GuardianLifeControl.GuardianLifeState.Alive) {
                guardianAliveList.Add(guardian);
            }

        }

        // 1. Trường hợp chỉ có 1 guardian
        if (guardianAliveList.Count == 1) {

            guardianAliveList[0].CommandMoveToGuardPos(centerSquadPos, centerSquadPos);

            return;
        }

        // 2. Trường hợp có trên 2 guardian
        float angleStep = 360f / guardianAliveList.Count;
        float radius = 0.5f;

        for (int i = 0; i < guardianAliveList.Count; i++) {

            // Góc Vector hướng của guardian thứ i
            float currentAngle = angleStep * i;

            // Vector hướng tính từ tâm (mouse clicked pos) của guardian thứ i
            Vector2 offset = Quaternion.Euler(0f, 0f, currentAngle) * Vector2.right * radius;

            Vector3 newGuardPos = centerSquadPos + new Vector3(offset.x, offset.y, 0f);

            guardianAliveList[i].CommandMoveToGuardPos(newGuardPos, centerSquadPos);
        }
    }

    private IEnumerator DeploySquadCoroutine() {

        OnDeployPorgress?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(guardianTowerSO.delayDeployGuardianTimer);

        // 0. Setup Data
        int currentActiveIndex = 0;
        this.defaultRallyPos = FindNearestRallyPoint();

        currentCenterGuardPos = this.defaultRallyPos; // Cập nhật center guard pos hiện tại 

        // 1. Reset Guardian Data
        OnResetDataGuardian?.Invoke(this, EventArgs.Empty);

        while (currentActiveIndex < guardianList.Count) {

            OnSpawnGuardian?.Invoke(this, EventArgs.Empty);

            ArrangeGuardianToSingleRing(this.defaultRallyPos, guardianList[currentActiveIndex], currentActiveIndex);

            currentActiveIndex += 1;

            if (currentActiveIndex >= guardianList.Count) {
                break;
            }

            yield return new WaitForSeconds(currentTowerStatus.respawnTimer);
        }

        // After deploy all guardian
        currentAlive = guardianList.Count;

        UnDeployProgress?.Invoke(this, EventArgs.Empty);

        OnDeployCompleteSFX?.Invoke(this, EventArgs.Empty); 
    }

    private void ArrangeGuardianToSingleRing(Vector3 centerSquadPos, Guardian guardian, int guardianIndex) {

        int countOfGuardian = guardianList.Count;

        // 1. Trường hợp chỉ có 1 Guardian 
        if (countOfGuardian == 1) {

            guardian.CommandMoveToGuardPos(centerSquadPos, centerSquadPos);

            return;
        }


        float angleStep = 360 / countOfGuardian;
        float radius = 0.5f;

        // Góc Vector hướng của guardian hiện tại dựa trên Index
        float currentAngle = angleStep * guardianIndex;

        // Vector hướng tính từ tâm (mouse clicked pos) của guardian thứ i
        Vector2 offset = Quaternion.Euler(0f, 0f, currentAngle) * Vector2.right * radius;

        Vector3 newGuardPos = centerSquadPos + new Vector3(offset.x, offset.y, 0f);

        guardian.CommandMoveToGuardPos(newGuardPos, centerSquadPos);

    }


    private bool IsForceReAim() {

        // 1. Nếu hết aimTimer
        if (aimTimer <= 0f) {
            return true;
        }

        // 2. Nếu 
        if (isForceReAim) {
            return true;
        }

        return false;
    }

    private bool IsEnemyBookedByAnyGuardian(BaseEnemy enemy) {

        foreach (Guardian guardian in guardianList) {

            if (enemy == guardian.GetCurrentIntentTarget()){

                return true; 
            }
        }

        return false;
    }

    public override void HandleLeftClicked() {

        if (isUpgrading || isOnGuardianTowerUI) {
            return;
        }

        // Assign Position Zone
        if (isOnAssignPositionVisual) {

            return;
        }

        // Attack zone
        OnAttackZone?.Invoke(this, EventArgs.Empty);
        UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });


        // GuardianTowerUI control
        isOnGuardianTowerUI = true;
        OnGuardianTowerUI?.Invoke(this, EventArgs.Empty);

    }

    public override void HandleRightClicked() {

        if (isOnAssignPositionVisual) {

            ToggleAssignPositionZone();

            OnGuardianTowerUI?.Invoke(this, EventArgs.Empty);
            isOnGuardianTowerUI = true;

            return;
        }

        if (isOnGuardianTowerUI) {

            UnAttackZone?.Invoke(this, EventArgs.Empty);
            UnGuardianTowerUI?.Invoke(this, EventArgs.Empty);

            isOnGuardianTowerUI = false;

        }

    }

    public override void HandleDeselectedAll() {

        if (isOnAssignPositionVisual) {

            ToggleAssignPositionZone();

        }

        DeselectedAllUI?.Invoke(this, EventArgs.Empty);
        isOnGuardianTowerUI = false;

        if (isUpgrading) { return; }

        UnAttackZone?.Invoke(this, EventArgs.Empty);
    }

    public override void HitDamage(float damageGet) {
        guardianTowerLifeControl.TakeDamage(damageGet);
    }

    public override bool IsDeselectLastUI() {
        // If last ui was deselect
        return isOnGuardianTowerUI == false;
    }

    public override bool IsRecoveringHealth() {
        return guardianTowerLifeControl.IsRecoveringHealth();
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

                // Đảm bảo đủ Guardian cho Level mới
                PreparedGuardian();

                return;


            case ITowerObject.LevelTower.Level1:

                currentTowerStatus = guardianTowerDataDict[currentTowerLevel];
                currentAttackZone = guardianTowerSO.baseRange; // Mặc định level 1 dùng baseRange

                OnUpgradeLevel1?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = this.currentAttackZone });

                break;

            case ITowerObject.LevelTower.Level2:

                currentTowerStatus = guardianTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel2?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                // Đảm bảo đủ Guardian cho Level mới
                PreparedGuardian();

                break;

            case ITowerObject.LevelTower.Level3:

                currentTowerStatus = guardianTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel3?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                // Đảm bảo đủ Guardian cho Level mới
                PreparedGuardian();

                break;
            case ITowerObject.LevelTower.Level4:

                currentTowerStatus = guardianTowerDataDict[currentTowerLevel];
                currentAttackZone = currentTowerStatus.attackRange;

                OnUpgradeLevel4?.Invoke(this, EventArgs.Empty);

                // Update attackZoneVisual
                UpdateAttackZone?.Invoke(this, new ITowerObject.UpgradeAttackZoneEventArgs { attackZone = currentAttackZone });

                // Đảm bảo đủ Guardian cho Level mới
                PreparedGuardian();

                break;

        }

        // Start upgrade
        StartCoroutine(UpgradeCoroutine());


    }

    public void ToggleAssignPositionZone() {

        isOnAssignPositionVisual = !isOnAssignPositionVisual;

        if (isOnAssignPositionVisual) {

            OnAssignPositionZone?.Invoke(this, EventArgs.Empty);

        }
        else {
            UnAssignPositionZone?.Invoke(this, EventArgs.Empty);
        }

    }

    public GuardianTowerSO GetGuardianTowerSO() {
        return this.guardianTowerSO;
    }

    public GuardianTowerLevelData GetCurrentTowerStatus() {
        return this.currentTowerStatus;
    }

    public float GetCurrentAttackZone() {
        return this.currentAttackZone;
    }

    public Transform GetSpawnPoint() {
        return this.spawnPoint;
    }

    public AssignPositionZoneVisual GetAssignPositionZoneVisual() {
        return this.assignPositionZoneVisual;
    }

    public GuardianTowerUI GetGuardianTowerUI() {
        return this.guardianTowerUI;
    }

    public Dictionary<ITowerObject.LevelTower, GuardianTowerLevelData> GetGuardianTowerDataDict() {
        return this.guardianTowerDataDict;
    }

    public Vector3 GetCurrentCenterGuardPos() {
        return this.currentCenterGuardPos;
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
