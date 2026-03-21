using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;


public class Wolf : BaseEnemy, ICanAttackPhysic
{
    public enum WolfBehavior {
        Walk,
        PreAttack,
        Attack
    }

    public event EventHandler OnAttackAnim;
    public event EventHandler OnPreAttackAnim;
    public event EventHandler ResetAnimator; // Xảy ra bất cứ khi nào thực hiện việc di chuyển
    public event EventHandler ChangedLeftDir;
    public event EventHandler ChangedRightDir;

    [Header("Wolf Data")]
    [SerializeField] private EnemySO wolfSO;

    [Header("View Angle")]
    [SerializeField] private float viewAngle;
    [SerializeField] private float aimTimerMax;

    private WolfBehavior currentWolfBehavior;
    private BaseEnemy.EnemyDirection currentWolfDirection;

    private EnemySpawner parentSpawner;
    private WolfLifeControl wolfLifeControl;

    private bool isLockedByTarget;

    #region Movement Behavior
    private List<Transform> waypointList;
    private int targetWaypointIndex;
    private bool isWalking;
    private Vector3 previousPos = Vector3.zero;
    private Vector3 targetPos;
    private float randomTargetTimer;
    #endregion

    #region Anomaly Behavior
    private bool isUnlockAnomaly;
    private float aimTimer;
    private Collider2D currentTarget;
    private Collider2D[] towerDetectedArray;
    private float closestDistance;
    #endregion

    private Coroutine currentCoroutine = null;

    private void Awake() {

        waypointList = new List<Transform>();

        aimTimer = aimTimerMax;
        currentTarget = null;
        isWalking = false;
        targetPos = Vector3.zero;

        parentSpawner = GetComponentInParent<EnemySpawner>();
        wolfLifeControl = GetComponent<WolfLifeControl>();

    }

    private void Start() {

        parentSpawner.ActiveEnemy += ParentSpawner_ActiveEnemy;

        wolfLifeControl.OnSpawn += WolfLifeControl_OnSpawn;
        // After Spawn
        ChangeWolfBehaviorTo(WolfBehavior.Walk);

        this.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        parentSpawner.ActiveEnemy -= ParentSpawner_ActiveEnemy;

        wolfLifeControl.OnSpawn -= WolfLifeControl_OnSpawn;
    }

    private void WolfLifeControl_OnSpawn(object sender, EventArgs e) {
        // 1. Reset Next Target
        targetWaypointIndex = 1;

        // 2. Reset movement and direction
        ChangeDirection(waypointList[targetWaypointIndex].position);
        targetPos = RandomWaypointPos(waypointList[targetWaypointIndex]);

        // 3. Reset Behavior
        ChangeWolfBehaviorTo(WolfBehavior.Walk);
    }

    private void ParentSpawner_ActiveEnemy(object sender, EnemySpawner.OnActiveEnemyEventArgs e) {

        if (this == e.baseEnemy) {

            waypointList = PathGenerator.Instance.GetWaypointList();

            this.transform.position = waypointList[0].position;

            Show();

            this.ActiveEvent();

            isUnlockAnomaly = SaveData.IsUnlockAnomaly(wolfSO);

            StartCoroutine(wolfLifeControl.RespawnCoroutine());
        }

    }

    private void Update() {

        if (IsCantAttack()) {
            // Vào trạng thái không thể bị attack thì loại bỏ mọi tương tác

            return;
        }

        float threesHold = 0.0001f;

        isWalking = Vector3.Distance(this.transform.position, previousPos) >= threesHold;

        previousPos = this.transform.position;

        HandleMovement();
        HandleAttackTarget();
    }

    private void HandleMovement() {
        // ---- LOCAL BEHAVIOR ----

        if (currentWolfBehavior != WolfBehavior.Walk) {
            // Only Happen if behavior is Walk
            return;
        }

        randomTargetTimer -= Time.deltaTime;

        if (randomTargetTimer <= 0f) {
            // ReRandom target
            targetPos = RandomWaypointPos(waypointList[targetWaypointIndex]);
        }

        Vector3 moveDir = (targetPos - this.transform.position).normalized;
        float sqrDistance = (this.transform.position - targetPos).sqrMagnitude; // Bình phương khoảng cách 2 Vector (Nhanh hơn Vector3.distance vì nó ko tính căn bậc 2)

        transform.position += moveDir * wolfSO.moveSpeed * Time.deltaTime;


        // Check can change to 
        if (sqrDistance <= 0.1f * 0.1f) {
            // Reach targetPoint

            if (targetWaypointIndex == waypointList.Count - 1) {
                // If reach last index

                wolfLifeControl.ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Despawn);

                HomeBase.Instance.ApplyDamage();

                return; 

            }
            else {
                // If not reach last index 
                targetWaypointIndex += 1;

                targetPos = RandomWaypointPos(waypointList[targetWaypointIndex]);

                ChangeDirection(waypointList[targetWaypointIndex].position);

            }
        }
        else {

            if (randomTargetTimer <= 0f) {
                // Random target

                float distanceToTarget = (targetPos - this.transform.position).sqrMagnitude;
                float distanceCantChangedTarget = 1f;

                if (distanceToTarget <= distanceCantChangedTarget * distanceCantChangedTarget) {
                    // If distance to target <= 1f ---> Near target point

                    randomTargetTimer = wolfSO.randomTargetTimer;
                }
                else {
                    // Far target point

                    targetPos = RandomWaypointPos(waypointList[targetWaypointIndex]);
                }
            }
        }

    }

    private void HandleAttackTarget() {

        if (!isUnlockAnomaly) { return; }

        if (currentWolfBehavior != WolfBehavior.Walk) {
            // Only Aim Target if behavior is Walk
            return;
        }

        // Detect Tower (Wolf can only attack tower with 25% chance Attack)

        aimTimer -= Time.deltaTime;

        if (aimTimer <= 0f) {
            // Thực hiện detect target sau mỗi aimTimerMax

            aimTimer = aimTimerMax;

            currentTarget = null;
            closestDistance = Mathf.Infinity;


            towerDetectedArray = Physics2D.OverlapCircleAll(this.transform.position, wolfSO.detectedZone, wolfSO.canAttackLayer);

            if (towerDetectedArray == null || towerDetectedArray.Length == 0) { return; }

            if (towerDetectedArray != null && towerDetectedArray.Length > 0) {

                foreach (Collider2D tower in towerDetectedArray) {

                    Vector2 dirFromWolfToTarget = (tower.transform.position - this.transform.position).normalized;

                    if (Vector2.Angle(GetViewDir(), dirFromWolfToTarget) <= viewAngle / 2f) {
                        // If tower is in viewAngle

                        float distanceToTower = Vector3.Distance(this.transform.position, tower.transform.position);

                        if (distanceToTower < closestDistance) {

                            closestDistance = distanceToTower;

                            currentTarget = tower;

                        }

                    }
                }

                // Sau khi duyệt xong array
                if (currentTarget != null) {

                    int chanceToAttack = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

                    if (chanceToAttack == 0) {
                        // 25% change Behavior

                        ChangeWolfBehaviorTo(WolfBehavior.PreAttack);

                        return;
                    }

                }

            }

        }
        else {
            return;
        }

    }

    private void ChangeWolfBehaviorTo(WolfBehavior wolfState) {

        this.currentWolfBehavior = wolfState;

        switch (currentWolfBehavior) {

            case WolfBehavior.Walk:

                aimTimer = aimTimerMax;

                ResetAnimator?.Invoke(this, EventArgs.Empty); // Trước khi về Walk, luôn đảm bảo reset animator về lại nhóm animator di chuyển

                break;

            case WolfBehavior.PreAttack:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;    
                }

                currentCoroutine = StartCoroutine(PreAttackCoroutine());

                break;

            case WolfBehavior.Attack:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                currentCoroutine = StartCoroutine(AttackCoroutine());

                break;

        }

    }

    private void ChangeDirection(Vector3 targetPosition) {

        Vector3 moveDir = targetPosition - this.transform.position;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
            // Đang đi ngang

            if (moveDir.x < 0) {
                // Turn Left

                this.currentWolfDirection = BaseEnemy.EnemyDirection.Left;

                ChangedLeftDir?.Invoke(this, EventArgs.Empty);

            }
            else if (moveDir.x > 0) {
                // Turn Right

                this.currentWolfDirection = BaseEnemy.EnemyDirection.Right;

                ChangedRightDir?.Invoke(this, EventArgs.Empty); 
            }


        }

        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x)) {
            // Đang đi dọc

            if (moveDir.y < 0) {
                // Turn Down

                this.currentWolfDirection = BaseEnemy.EnemyDirection.Down;
            }
            else if (moveDir.y > 0) {
                // Turn Up

                this.currentWolfDirection = BaseEnemy.EnemyDirection.Up;
            }

        }

    }

    private IEnumerator PreAttackCoroutine() {

        OnPreAttackAnim?.Invoke(this, EventArgs.Empty);

        // ---- Find new Target ----

        yield return new WaitForSeconds(wolfSO.thinkingTimer); // Đứng im 1 khoảng thời gian (Thời gian animator xử lí) ---> Thingking Timer

        // ----- Quyết định hành vi kế tiếp -----

        // Sau khi duyệt toàn bộ list towerDetected
        if (currentTarget != null) {
            // If has target --> Attack

            ChangeWolfBehaviorTo(WolfBehavior.Attack);
        }
        else {
            // If hasn't target --> Continue Walk

            // Về lại walk thì tính toán lại Direction
            ChangeDirection(waypointList[targetWaypointIndex].position);

            ChangeWolfBehaviorTo(WolfBehavior.Walk);
        }

    }

    private IEnumerator AttackCoroutine() {

        // ---- 0. Before attack, check again if tower is still exists ----
        if (currentTarget == null) {
            // if target was destroy

            // Về lại walk thì tính toán lại Direction
            ChangeDirection(waypointList[targetWaypointIndex].position);

            ChangeWolfBehaviorTo(WolfBehavior.Walk);

            yield break;
        }

        // ---- 1. Move Toward to tower - If target was destroy when movement behavior back to Walk ----
        ChangeDirection(currentTarget.transform.position);

        ResetAnimator?.Invoke(this, EventArgs.Empty); // Trước khi di chuyển, đảm bảo reset animator về lại nhóm animator di chuyển

        float sqrDistance = (this.transform.position - currentTarget.transform.position).sqrMagnitude;

        while (currentTarget != null && sqrDistance >= 1.2f * 1.2f) {

            Vector3 moveDir = (currentTarget.transform.position - this.transform.position).normalized;

            sqrDistance = (this.transform.position - currentTarget.transform.position).sqrMagnitude;

            transform.position += moveDir * wolfSO.moveSpeed * Time.deltaTime;

            yield return null;
        }

        // ---- 2. Attack Behavior - If target was destroy in Attack progress so back to Walk ----
        while (currentTarget != null && currentWolfBehavior == WolfBehavior.Attack) {

            if (currentTarget.TryGetComponent<BaseTower>(out BaseTower baseTower)) {

                OnAttackAnim?.Invoke(this, EventArgs.Empty);

                if (baseTower.IsRecoveringHealth()) {


                }
                else {
                    baseTower.HitDamage(wolfSO.attackDamage);
                }

            }

            yield return new WaitForSeconds(0.4f); // Đợi Attack anim kết thúc

            // After attack
            OnPreAttackAnim?.Invoke(this, EventArgs.Empty);

            if (currentTarget == null) {
                // If target is destroy

                break;

            }
            else {
                yield return new WaitForSeconds(wolfSO.attackTimer);
            }
        }

        yield return null;

        currentTarget = null;

        // Về lại walk thì tính toán lại Direction
        ChangeDirection(waypointList[targetWaypointIndex].position);

        ChangeWolfBehaviorTo(WolfBehavior.Walk);

    }

    private Vector3 GetViewDir() {

        switch (currentWolfDirection) {

            case BaseEnemy.EnemyDirection.Up:
                return Vector3.up;

            case BaseEnemy.EnemyDirection.Right:
                return Vector3.right;

            case BaseEnemy.EnemyDirection.Down:
                return Vector3.down;

            case BaseEnemy.EnemyDirection.Left:
                return Vector3.left;

            default:
                return transform.right;
        }


    }

    private Vector3 RandomWaypointPos(Transform targetWaypoint) {

        randomTargetTimer = wolfSO.randomTargetTimer;

        // 1 node có kích thước 2 * 2 với center là waypoint ---> waypointRandom nằm trong khoảng (2,2)
        float offsetX = UnityEngine.Random.Range(-wolfSO.radiusOffset, wolfSO.radiusOffset);
        float offsetY = UnityEngine.Random.Range(-wolfSO.radiusOffset, wolfSO.radiusOffset);

        Vector3 randomPos = new Vector3(targetWaypoint.position.x + offsetX, targetWaypoint.position.y + offsetY, 0f);

        return randomPos;
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    public void Hide() {

        this.gameObject.SetActive(false);
    }

    public override void HitDamage(float damageGet) {

        wolfLifeControl.TakeDamage(damageGet);

    }

    public override bool IsCantAttack() {
        return wolfLifeControl.GetCurrentWolfLifeState() == BaseEnemy.EnemyLifeState.Death || wolfLifeControl.GetCurrentWolfLifeState() == BaseEnemy.EnemyLifeState.Despawn;
    }

    public override bool IsResistMagic() {
        return wolfSO.resistanceType == DamageResistance.MagicResistance;
    }

    public override bool IsResistPhysic() {
        return wolfSO.resistanceType == DamageResistance.PhysicResistance;
    }

    public override Vector3 GetEnemyVelocity() {

        if (!isWalking) {
            return Vector3.zero;
        }

        Vector3 targetPos;

        if (currentWolfBehavior == WolfBehavior.Walk) {

            if (waypointList == null || waypointList.Count == 0) {
                return Vector3.zero;
            }

            targetPos = waypointList[targetWaypointIndex].position;

        }
        else {
            if (currentTarget == null) {
                return Vector3.zero;    
            }
            targetPos = currentTarget.transform.position;

        }

        Vector3 moveDir = (targetPos - this.transform.position).normalized;

        return moveDir * wolfSO.moveSpeed;
    }

    public override EnemyDirection GetEnemyCurrentDirection() {
        return this.currentWolfDirection;
    }

    public override float GetEnemyHealth() {
        return wolfLifeControl.GetCurrentHealth();
    }

    public override float GetEnemyProgress() {

        if (targetWaypointIndex <= 0) {
            return 0f;
        }

        // 1 Prepared Data
        List<Transform> waypointList = PathGenerator.Instance.GetWaypointList();
        Dictionary<Transform, float> waypointCumulativeDistDict = GridManager.Instance.GetWaypointCumulativeDistDict();

        // 2 Counting
        Transform waypointBefore = waypointList[targetWaypointIndex - 1];
        int waypointLastIndex = waypointList.Count - 1;
        Transform waypointLast = waypointList[waypointLastIndex];

        float totalEnemyMoved = waypointCumulativeDistDict[waypointBefore] + Vector3.Distance(waypointBefore.position, this.transform.position); // Quãng đường Enemy đã đi được

        return totalEnemyMoved;
    }

    public void SetLockedTarget(bool isLockedTarget, Guardian guardian) {
        this.isLockedByTarget = isLockedTarget;
    }

    public bool HasLockedTarget() {
        return this.isLockedByTarget;
    }

    public bool IsWalking() {
        return this.isWalking;
    }

    public EnemySO GetWolfSO() {
        return this.wolfSO;
    }

    public BaseEnemy.EnemyDirection GetCurrentWolfDirection() {
        return this.currentWolfDirection;
    }

    public WolfBehavior GetCurrentWolfBehavior() {
        return this.currentWolfBehavior;
    }

    public WolfLifeControl GetWolfLifeControl() {
        return this.wolfLifeControl;
    }

    #region Visualize
    private void OnDrawGizmos() {

        Gizmos.color = new Color(0, 1, 1, 0.2f);
        Vector3 forward = GetViewDir();

    #if UNITY_EDITOR
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, viewAngle / 2) * forward * wolfSO.detectedZone);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, -viewAngle / 2) * forward * wolfSO.detectedZone);
        Handles.DrawWireArc(transform.position, Vector3.forward, Quaternion.Euler(0, 0, viewAngle / 2) * forward, -viewAngle, wolfSO.detectedZone);
    #endif
    }
    #endregion
}
