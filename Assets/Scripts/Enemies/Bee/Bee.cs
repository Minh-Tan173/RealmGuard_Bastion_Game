using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Bee : BaseEnemy
{

    public event EventHandler ChangedLeftDir;
    public event EventHandler ChangedRightDir;

    [Header("Bee Data")]
    [SerializeField] private EnemySO beeSO;
    
    private BeeLifeControl beeLifeControl;
    private EnemySpawner parentSpawner;

    private List<Transform> waypointList;

    private BaseEnemy.EnemyDirection currentBeeDirection;

    private int targetIndex;
    private Vector3 targetPos;
    private float randomTargetTimer;

    private bool canMove = false;

    private void Awake() {

        waypointList = new List<Transform>();

        beeLifeControl = GetComponent<BeeLifeControl>();

        parentSpawner = GetComponentInParent<EnemySpawner>();

    }

    private void Start() {

        parentSpawner.ActiveEnemy += ParentSpawner_ActiveEnemy;
        beeLifeControl.OnSpawn += BeeLifeControl_OnSpawn;

        this.gameObject.SetActive(false);
    }

    private void OnDestroy() {
        parentSpawner.ActiveEnemy -= ParentSpawner_ActiveEnemy;
        beeLifeControl.OnSpawn -= BeeLifeControl_OnSpawn;
    }

    private void BeeLifeControl_OnSpawn(object sender, EventArgs e) {

        // 1. Reset Position and Next Target
        this.transform.position = waypointList[0].position;
        targetIndex = 1;

        // 2. Reset movement and direction
        ChangeDirection();
        canMove = true;
        targetPos = RandomWaypointPos(waypointList[targetIndex]);
    }


    private void ParentSpawner_ActiveEnemy(object sender, EnemySpawner.OnActiveEnemyEventArgs e) {

        if (this == e.baseEnemy) {

            waypointList = PathGenerator.Instance.GetWaypointList();

            this.transform.position = waypointList[0].position;

            Show();

            this.ActiveEvent();

            StartCoroutine(beeLifeControl.RespawnCoroutine());

        }

    }

    private void Update() {

        HandleMovement();

    }

    private void HandleMovement() {

        if (!canMove) {
            return;
        }

        Vector3 moveDir = (targetPos - this.transform.position).normalized;
        float sqrDistance = (this.transform.position - targetPos).sqrMagnitude;

        transform.position += moveDir * beeSO.moveSpeed * Time.deltaTime;

        if (sqrDistance <= 0.1f * 0.1f) {
            // If reachingTargetPoint

            if (targetIndex == waypointList.Count - 1) {
                // If reach last index

                beeLifeControl.ChangeLifeStateTo(EnemyLifeState.Despawn);

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

                float distanceToTarget = (targetPos - this.transform.position).sqrMagnitude;
                float distanceCantChangedTarget = 1f;

                if (distanceToTarget <= distanceCantChangedTarget * distanceCantChangedTarget) {
                    // If distance to target <= 1f ---> Near target point

                    randomTargetTimer = beeSO.randomTargetTimer;
                }
                else {
                    // Far target point

                    targetPos = RandomWaypointPos(waypointList[targetIndex]);
                }
            }
        }

    }

    private void ChangeDirection() {

        Vector3 moveDir = waypointList[targetIndex].position - this.transform.position;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
            // Đang đi ngang

            if (moveDir.x < 0) {
                // Turn Left

                this.currentBeeDirection = BaseEnemy.EnemyDirection.Left;

                ChangedLeftDir?.Invoke(this, EventArgs.Empty);
            }
            else if (moveDir.x > 0) {
                // Turn Right

                this.currentBeeDirection = BaseEnemy.EnemyDirection.Right;

                ChangedRightDir?.Invoke(this, EventArgs.Empty);
            }


        }

        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x)) {
            // Đang đi dọc

            if (moveDir.y < 0) {
                // Turn Down

                this.currentBeeDirection = BaseEnemy.EnemyDirection.Down;
            }
            else if (moveDir.y > 0) {
                // Turn Up

                this.currentBeeDirection = BaseEnemy.EnemyDirection.Up;
            }

        }

    }

    private Vector3 RandomWaypointPos(Transform targetWaypoint) {

        // Mặc định đặt lại randomTargetTimer mỗi lần randomWaypoint
        randomTargetTimer = beeSO.randomTargetTimer;

        // 1 node có kích thước 2 * 2 với center là waypoint ---> waypointRandom nằm trong khoảng (2,2)
        float offsetX = UnityEngine.Random.Range(-beeSO.radiusOffset, beeSO.radiusOffset);
        float offsetY = UnityEngine.Random.Range(-beeSO.radiusOffset, beeSO.radiusOffset);

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
        beeLifeControl.TakeDamage(damageGet);
    }

    public override bool IsCantAttack() {
        return beeLifeControl.GetCurrentBeeLifeState() == BaseEnemy.EnemyLifeState.Death || beeLifeControl.GetCurrentBeeLifeState() == BaseEnemy.EnemyLifeState.Despawn;
    }

    public override bool IsResistMagic() {
        return beeSO.resistanceType == DamageResistance.MagicResistance;
    }

    public override bool IsResistPhysic() {
        return beeSO.resistanceType == DamageResistance.PhysicResistance;
    }

    public override Vector3 GetEnemyVelocity() {
        
        if (!canMove || waypointList == null || waypointList.Count == 0) {
            return Vector3.zero;
        }

        Vector3 targetPos = waypointList[targetIndex].position;
        Vector3 moveDir = (targetPos - this.transform.position).normalized;

        return moveDir * beeSO.moveSpeed;
    }

    public override float GetEnemyProgress() {

        if (targetIndex <= 0) {
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
        return beeLifeControl.GetCurrentHealth();
    }

    public BaseEnemy.EnemyDirection GetCurrentBeeDirection() {
        return this.currentBeeDirection;
    }

    public EnemySO GetBeeSO() {
        return this.beeSO;
    }

    public BeeLifeControl GetBeeLifeControl() {
        return this.beeLifeControl;
    }

    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }
}

