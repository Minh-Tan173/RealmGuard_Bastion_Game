using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Archer : MonoBehaviour {
    public enum ArcherBehavior {

        Idle,
        PreAttack,
        Attack,

    }

    public event EventHandler OnAttackSFX;
    public event EventHandler OnSpawnSFX;

    public event EventHandler OnAttackAnim;


    [Header("Data")]
    [SerializeField] private SoldierSO archerSO;

    private ArcherTower archerTower;

    private SoldierSO.SoldierDirection archerDirection;
    private SoldierSO.SoldierLevel archerLevel; // Todo: Open when Tower reached Level 5,6,7
    private ArcherBehavior archerBehavior;

    #region Aim Behavior
    private List<Collider2D> enemyDetectedList;
    private float sqrClosestDistance;
    private Transform currentTarget;
    private bool isForcedReAim;
    #endregion

    private Coroutine currentCoroutine;

    private float nextAllowedAttackTimer;

    private List<Arrow> arrowList;
    private int currentArrowIndex = 0;

    private void Awake() {

        arrowList = new List<Arrow>();

        enemyDetectedList = new List<Collider2D>();

        ResetSetup();
    }

    private void Start() {

        archerTower.OnUpgradeLevelProgress += ArcherTower_OnUpgradeLevelProgress;
        archerTower.UnUpgradeLevelProgress += ArcherTower_UnUpgradeLevelProgress;

        // After spawn
        archerLevel = SoldierSO.SoldierLevel.Level1;

        SpawnArrowAddList();

        OnSpawnSFX?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy() {
        archerTower.OnUpgradeLevelProgress -= ArcherTower_OnUpgradeLevelProgress;
        archerTower.UnUpgradeLevelProgress -= ArcherTower_UnUpgradeLevelProgress;
    }

    private void ArcherTower_UnUpgradeLevelProgress(object sender, EventArgs e) {
        // When Upgrade Progress done

        this.gameObject.SetActive(true);

        ResetSetup();

    }

    private void ArcherTower_OnUpgradeLevelProgress(object sender, EventArgs e) {
        // When Upgrade Progress start

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        foreach (Arrow arrow in arrowList) {
            arrow.Hide();
        }

        SpawnArrowAddList();

        this.gameObject.SetActive(false);

    }

    private void Update() {

        HandleCurrentAimTarget();

        if (archerBehavior == ArcherBehavior.Idle) {
            // Đang Idle

            if (currentTarget != null) {
                // If has currentTarget

                ChangeArcherBehaviorTo(ArcherBehavior.PreAttack);

            }
        }

    }

    public void OnTowerScannedNewData(List<Collider2D> enemyDetectedList) {

        // 1. Reset list enemyDetected
        this.enemyDetectedList.Clear();
        this.enemyDetectedList.AddRange(enemyDetectedList);

        // 2.
        if (archerBehavior == ArcherBehavior.Idle) {
            // Whenever archer is in idle behavior

            FindNewTarget();
        }
        

    }

    private void HandleCurrentAimTarget() {

        if (currentTarget != null) {
            // When having target

            if (!IsTargetValid() && !isForcedReAim) {

                ChangeArcherBehaviorTo(ArcherBehavior.Idle);
                isForcedReAim = true;
            }

        }
        
        if (isForcedReAim) {
            // If have forced reAim command

            FindNewTarget();
        }

    }
    
    private void FindNewTarget() {

        // 0. Reset Data Before Get New Data
        currentTarget = null;
        isForcedReAim = false;
        sqrClosestDistance = Mathf.Infinity;

        // 1. Ensure that data is not null or empty
        if (this.enemyDetectedList == null || enemyDetectedList.Count <= 0) { return; }

        // 2. Sort Data nằm trong vùng nhìn thấy phía trước (70o)
        foreach (Collider2D enemyDetected in enemyDetectedList) {
            // Duyệt lần lượt enemy trong array để tìm khoảng cách gần nhất tới enemy nằm trong vùng nhìn thấy

            if (enemyDetected == null) {
                continue;
            }

            Vector2 dirFromArcherToEnemy = (enemyDetected.transform.position - this.transform.position).normalized;

            if (Vector2.Angle(GetViewDir(), dirFromArcherToEnemy) > archerTower.GetArcherTowerSO().viewAngle / 2f) {
                // If enemy outside view range

                continue;
            }

            float sqrDistaneToEnemy = (this.transform.position - enemyDetected.transform.position).sqrMagnitude;                

            if (sqrDistaneToEnemy <= sqrClosestDistance) {

                sqrClosestDistance = sqrDistaneToEnemy;
                currentTarget = enemyDetected.transform;

            }

        }
    }

    private Vector3 GetViewDir() {

        switch (archerDirection) {

            case SoldierSO.SoldierDirection.Up:
                return Vector3.up;

            case SoldierSO.SoldierDirection.Right:
                return Vector3.right;

            case SoldierSO.SoldierDirection.Down:
                return Vector3.down;

            case SoldierSO.SoldierDirection.Left:
                return Vector3.left;

            default:
                return transform.right;
        }
    }

    private void ChangeArcherBehaviorTo(ArcherBehavior archerState) {

        this.archerBehavior = archerState;

        // Setup after change state
        switch (archerState) {

            case ArcherBehavior.Idle:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                break;

            case ArcherBehavior.PreAttack:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                currentCoroutine = StartCoroutine(PreAttackCoroutine());

                break;

            case ArcherBehavior.Attack:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                currentCoroutine = StartCoroutine(AttackCoroutine());

                break;
        }

    }

    private IEnumerator PreAttackCoroutine() {

        while (Time.time <= nextAllowedAttackTimer) {
            // Thinking time...
            yield return null;
        }

        if (IsTargetValid()) {

            ChangeArcherBehaviorTo(ArcherBehavior.Attack);
        }
        else {

            ChangeArcherBehaviorTo(ArcherBehavior.Idle);
            isForcedReAim = true;
        }

    }



    private IEnumerator AttackCoroutine() {

        //float drawTimer = 0f;
        float drawTimerMax = 0.23f;

        yield return new WaitForSeconds(drawTimerMax);
        
        if (!IsTargetValid()) {

            ChangeArcherBehaviorTo(ArcherBehavior.Idle);
            isForcedReAim = true;

            nextAllowedAttackTimer = Time.time + archerTower.GetCurrentTowerStatus().cooldownTimer;

            yield break;
        }

        // 2. Attack progress

        OnAttackSFX?.Invoke(this, EventArgs.Empty);

        Vector3 enemyVelocity = Vector3.zero;

        Vector3 currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, 0f);

        float timeToHitEnemy = Vector3.Distance(currentTargetPos, this.transform.position) / archerTower.GetArcherTowerSO().arrowSpeed; // time = Distance(archer, enemy) / arrowSpeed

        if (currentTarget.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

            enemyVelocity = baseEnemy.GetEnemyVelocity();

        }

        Vector3 leadOffsetTargetPos = currentTargetPos + (enemyVelocity * timeToHitEnemy); // P_future = P_current + (V * t)

        arrowList[currentArrowIndex].StartMovement(leadOffsetTargetPos, archerTower.GetArcherTowerSO().arrowSpeed);
        OnAttackAnim?.Invoke(this, EventArgs.Empty);

        currentArrowIndex = (currentArrowIndex + 1) % arrowList.Count;

        // 3. After attack behavior
        nextAllowedAttackTimer = Time.time + archerTower.GetCurrentTowerStatus().cooldownTimer;
        ChangeArcherBehaviorTo(ArcherBehavior.PreAttack);

    }

    private void ResetSetup() {

        // Scan Data Behavior
        sqrClosestDistance = 0f;
        currentTarget = null;
        isForcedReAim = false;
        enemyDetectedList.Clear();

        // Arrow
        currentArrowIndex = 0;

        // State
        ChangeArcherBehaviorTo(ArcherBehavior.Idle);
    }

    private void SpawnArrowAddList() {

        float maxRangeMove = archerTower.GetCurrentAttackRange();
        float arrowSpeed = archerTower.GetArcherTowerSO().arrowSpeed;
        float attackTimer = archerTower.GetCurrentTowerStatus().cooldownTimer;

        int buffer = 3; // Phần arrow dôi ra thêm để đảm bảo số lượng

        int numberOfArrow = (int)Mathf.Ceil(maxRangeMove / (arrowSpeed * attackTimer)) + buffer;

        while (arrowList.Count < numberOfArrow) {
            // When number of arrow in arrowList not equal to numberOfArror

            arrowList.Add(Arrow.SpawnArrow(archerSO.bulletPrefab, archerTower, this));
        }

    }

    private bool IsTargetValid() {

        // 1. Check if has target 
        if (this.currentTarget == null) { return false; }

        // 2. Kiểm tra trạng thái hiện tại của Target
        if (this.currentTarget.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

            if (baseEnemy.IsCantAttack()) {
                return false;
            }

        }

        // 3. Kiểm tra khoảng cách với target hiện tại
        float distanceToTarget = (this.transform.position - this.currentTarget.transform.position).sqrMagnitude;
        float AttackRange = archerTower.GetCurrentAttackRange();

        if (distanceToTarget >= AttackRange * AttackRange) {
            return false;
        }

        // 4. Kiểm tra góc của target hiện tại còn nằm trong angleView không
        Vector2 dirFromArcherToEnemy = (currentTarget.transform.position - this.transform.position).normalized;

        if (Vector2.Angle(GetViewDir(), dirFromArcherToEnemy) > archerTower.GetArcherTowerSO().viewAngle / 2f) {
            return false;
        }

        return true;
    }


    public ArcherBehavior GetArcherState() {
        return this.archerBehavior;
    }

    public SoldierSO.SoldierDirection GetArcherDirection() {
        return this.archerDirection;
    }

    public void SetArcherTowerParent() {
        this.archerTower = GetComponentInParent<ArcherTower>();
    }

    public void SetArcherDirection(SoldierSO.SoldierDirection archerDir) {
        this.archerDirection = archerDir;
    }

    public static Archer SpawnArcher(SoldierManagerSO soldierSO, Transform spawnPoint, SoldierSO.SoldierDirection archerDir) {

        SoldierSO archerSO = null;

        if (archerDir == SoldierSO.SoldierDirection.Up) {
            archerSO = soldierSO.upArcherSO;
        }
        if (archerDir == SoldierSO.SoldierDirection.Right) {
            archerSO = soldierSO.rightArcherSO;
        }
        if (archerDir == SoldierSO.SoldierDirection.Down) {
            archerSO = soldierSO.downArcherSO;
        }
        if (archerDir == SoldierSO.SoldierDirection.Left) {
            archerSO = soldierSO.leftArcherSO;
        }

        Transform archerTransform = Instantiate(archerSO.soldierPrefab, spawnPoint.parent);
        archerTransform.localPosition = spawnPoint.localPosition;

        Archer archer = archerTransform.GetComponent<Archer>();

        archer.SetArcherTowerParent();
        archer.SetArcherDirection(archerDir);

        return archer;
        
    }


    #region Visualize
    private void OnDrawGizmos() {

        Gizmos.color = new Color(0, 1, 1, 0.2f);
        float range = 10f;
        Vector3 forward = GetViewDir();

        float viewAngle = archerTower.GetArcherTowerSO().viewAngle;
        
    #if UNITY_EDITOR
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, viewAngle / 2) * forward * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, -viewAngle / 2) * forward * range);
        Handles.DrawWireArc(transform.position, Vector3.forward, Quaternion.Euler(0, 0, viewAngle / 2) * forward, -viewAngle, range);
    #endif
    }
    #endregion
}
