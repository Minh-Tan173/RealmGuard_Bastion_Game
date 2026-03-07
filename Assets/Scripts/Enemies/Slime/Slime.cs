using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : BaseEnemy 
{

    public event EventHandler OnAnomalyAnim;
    public event EventHandler ChangedLeftDir;
    public event EventHandler ChangedRightDir;

    [Header("Slime Data")]
    [SerializeField] private EnemySO slimeSO;

    [Header("Anomaly Data")]
    [SerializeField] private float anomalyTimerMax;
    [SerializeField] private Transform splitPos1;
    [SerializeField] private Transform splitPos2;
    [SerializeField] private Transform slimeMiniPrefab;

    private EnemySpawner parentSpawner;

    private List<Transform> waypointList;

    private BaseEnemy.EnemyDirection currentSlimeDirection;

    private SlimeLifeControl slimeLifeControl;

    private int targetIndex;
    private Vector3 targetPos;
    private float randomTargetTimer;

    private bool canMove = false;

    private float anomalyTimer;
    private bool isAnomalying;
    private bool isUnlockAnomaly;

    private void Awake() {
        waypointList = new List<Transform>();

        slimeLifeControl = GetComponent<SlimeLifeControl>();

        parentSpawner = GetComponentInParent<EnemySpawner>();

        parentSpawner.ActiveEnemy += ParentSpawner_ActiveEnemy;
    }

    private void Start() {

        slimeLifeControl.OnSpawn += SlimeLifeControl_OnSpawn;

        // After Spawn
        this.gameObject.SetActive(false);

    }

    private void OnDestroy() {

        parentSpawner.ActiveEnemy -= ParentSpawner_ActiveEnemy;

        slimeLifeControl.OnSpawn -= SlimeLifeControl_OnSpawn;
    }

    private void SlimeLifeControl_OnSpawn(object sender, EventArgs e) {

        // 1. Reset Next Target
        targetIndex = 1;

        // 2. Reset movement and direction
        ChangeDirection();
        canMove = true;
        targetPos = RandomWaypointPos(waypointList[targetIndex]);

        // 3. Reset Anomaly time
        anomalyTimer = anomalyTimerMax + UnityEngine.Random.Range(-1f, 1f);
    }

    private void ParentSpawner_ActiveEnemy(object sender, EnemySpawner.OnActiveEnemyEventArgs e) {

        if (e.baseEnemy == this) {

            waypointList = PathGenerator.Instance.GetWaypointList();

            this.transform.position = waypointList[0].position;

            Show();
             
            this.ActiveEvent();

            isUnlockAnomaly = SaveData.IsUnlockAnomaly(slimeSO);

            StartCoroutine(slimeLifeControl.RespawnCoroutine());
        }

    }

    private void Update() {

        HandleMovement();
        HandleAnomaly();
    }

    private void HandleMovement() {

        if (!canMove) {
            return;
        }

        // 1. Control Movement
        Vector3 moveDir = (targetPos - this.transform.position).normalized;
        float sqrDistance = (this.transform.position - targetPos).sqrMagnitude;

        transform.position += moveDir * slimeSO.moveSpeed * Time.deltaTime;

        if (sqrDistance <= 0.1f * 0.1f) {
            // If reachingTargetPoint

            if (targetIndex == waypointList.Count - 1) {
                // If reach last index

                slimeLifeControl.ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Despawn);

                return;

            }
            else {
                // If not reach last index 
                targetIndex += 1;

                targetPos = RandomWaypointPos(waypointList[targetIndex]);

                ChangeDirection();

            }
        }
        else {
            // If not reaching targetPoint

            randomTargetTimer -= Time.deltaTime;

            if (randomTargetTimer <= 0f) {
                targetPos = RandomWaypointPos(waypointList[targetIndex]);

            }
        }

    }

    private void HandleAnomaly() {

        if (!isUnlockAnomaly) {
            return;
        }

        anomalyTimer -= Time.deltaTime;

        if (anomalyTimer <= 0f) {
            // After anomaly time

            anomalyTimer = anomalyTimerMax;

            int randomIndex = UnityEngine.Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

            if (randomIndex == 0f && canMove) {
                // 25% chance to active anomaly while moving

                canMove = false;
                StartCoroutine(AnomalyCoroutine());
            }
        }

    }

    private IEnumerator AnomalyCoroutine() {

        isAnomalying = true;

        // 1. Anomaly Anim Active
        OnAnomalyAnim?.Invoke(this, EventArgs.Empty);

        // 2. Wait anomaly anim end
        yield return new WaitForSeconds(1f);

        // 3. Spawn slimeChild
        yield return null; // Wait for 1 frame to make sure split point pos is moved to last frame Anim
        SlimeMini.SpawnSlimeMini(slimeMiniPrefab, parentSpawner.transform, this, splitPos1.position);
        SlimeMini.SpawnSlimeMini(slimeMiniPrefab, parentSpawner.transform, this, splitPos2.position);

        // 4. After spawn child
        yield return null;

        isAnomalying = false;

        slimeLifeControl.ChangeLifeStateTo(BaseEnemy.EnemyLifeState.Despawn);
    }


    private void ChangeDirection() {

        Vector3 moveDir = waypointList[targetIndex].position - this.transform.position;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
            // Đang đi ngang

            if (moveDir.x < 0) {
                // Turn Left

                this.currentSlimeDirection = BaseEnemy.EnemyDirection.Left;

                ChangedLeftDir?.Invoke(this, EventArgs.Empty);
            }
            else if (moveDir.x > 0) {
                // Turn Right

                this.currentSlimeDirection = BaseEnemy.EnemyDirection.Right;

                ChangedRightDir?.Invoke(this, EventArgs.Empty);
            }


        }

        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x)) {
            // Đang đi dọc

            if (moveDir.y < 0) {
                // Turn Down

                this.currentSlimeDirection = BaseEnemy.EnemyDirection.Down;
            }
            else if (moveDir.y > 0) {
                // Turn Up

                this.currentSlimeDirection = BaseEnemy.EnemyDirection.Up;
            }

        }

    }

    private Vector3 RandomWaypointPos(Transform targetWaypoint) {

        // Mặc định đặt lại randomTargetTimer mỗi lần randomWaypoint
        randomTargetTimer = slimeSO.randomTargetTimer;

        // 1 node có kích thước 2 * 2 với center là waypoint ---> waypointRandom nằm trong khoảng (2,2)
        float offsetX = UnityEngine.Random.Range(-slimeSO.radiusOffset, slimeSO.radiusOffset);
        float offsetY = UnityEngine.Random.Range(-slimeSO.radiusOffset, slimeSO.radiusOffset);

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

        if (isAnomalying) {
            return;
        }

        slimeLifeControl.TakeDamage(damageGet);

    }

    public override bool IsCantAttack() {

        bool cantAttack = slimeLifeControl.GetCurrentSlimeLifeState() == BaseEnemy.EnemyLifeState.Death || slimeLifeControl.GetCurrentSlimeLifeState() == BaseEnemy.EnemyLifeState.Despawn;

        return cantAttack;
    }

    public override Vector3 GetEnemyVelocity() {

        if (!canMove || waypointList == null || waypointList.Count == 0) {
            return Vector3.zero;
        }

        Vector3 targetPos = waypointList[targetIndex].position;
        Vector3 moveDir = (targetPos - this.transform.position).normalized;

        return moveDir * slimeSO.moveSpeed;
    }

    public override float GetEnemyProgress() {

        if (targetIndex <= 0f) {
            return 0f;
        }

        // 1 Prepared Data
        List<Transform> waypointList = PathGenerator.Instance.GetWaypointList();
        Dictionary<Transform, float> waypointCumulativeDistDict = GridManager.Instance.GetWaypointCumulativeDistDict();

        // 2 Counting
        Transform waypointBefore = waypointList[targetIndex - 1];
        int waypointLastIndex = waypointList.Count - 1;
        Transform waypointLast = waypointList[waypointLastIndex];

        float totalEnemyMoved = waypointCumulativeDistDict[waypointBefore] + Vector3.Distance(waypointBefore.position, this.transform.position); // Quãng đường Enemy đã đi được

        return totalEnemyMoved;
    }

    public override float GetEnemyHealth() {
        return slimeLifeControl.GetCurrentHealth();
    }

    public BaseEnemy.EnemyDirection GetCurrentSlimeDirection() {
        return this.currentSlimeDirection;
    }

    public EnemySO GetSlimeSO() {
        return this.slimeSO;
    }

    public SlimeLifeControl GetSlimeLifeControl() {
        return this.slimeLifeControl;
    }

    public List<Transform> GetWaypointList() {
        return this.waypointList;
    }

    public int GetTargerIndex() {
        return this.targetIndex;
    }


    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

    public bool CanMove() {
        return this.canMove;
    }

    private void OnDrawGizmos() {

        BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(this.transform.position + new Vector3(boxCollider2D.offset.x, boxCollider2D.offset.y, 0f), new Vector3(boxCollider2D.size.x, boxCollider2D.size.y, 0f));
    }
}
