using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMini : BaseEnemy
{
    public event EventHandler ChangedLeftDir;
    public event EventHandler ChangedRightDir;

    [SerializeField] private EnemySO slimeSO;

    private List<Transform> waypointList;

    private BaseEnemy.EnemyDirection currentSlimeMiniDir;
    private SlimeMiniLifeControl slimeMiniLifeControl;

    private int targetIndex;
    private Vector3 targetPos;
    private float randomTargetTimer;

    private bool canMove = false;

    private void Awake() {
        slimeMiniLifeControl = GetComponent<SlimeMiniLifeControl>();
    }

    private void Start() {

        // After Spawn
        this.ActiveEvent();
    }

    private void Update() {

        if (!canMove) {
            return;
        }

        HandleMovement();

    }

    private void HandleMovement() {

        // 1. Control Movement
        Vector3 moveDir = (targetPos - this.transform.position).normalized;
        float sqrDistance = (this.transform.position - targetPos).sqrMagnitude;

        transform.position += moveDir * slimeSO.moveSpeed * Time.deltaTime;

        if (sqrDistance <= 0.1f * 0.1f) {
            // If reachingTargetPoint

            if (targetIndex == waypointList.Count - 1) {
                // If reach last index

                OnDestroySelf();

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

                    randomTargetTimer = slimeSO.randomTargetTimer;
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

                this.currentSlimeMiniDir = BaseEnemy.EnemyDirection.Left;

                ChangedLeftDir?.Invoke(this, EventArgs.Empty);

            }
            else if (moveDir.x > 0) {
                // Turn Right

                this.currentSlimeMiniDir = BaseEnemy.EnemyDirection.Right;

                ChangedRightDir?.Invoke(this, EventArgs.Empty);
            }


        }

        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x)) {
            // Đang đi dọc

            if (moveDir.y < 0) {
                // Turn Down

                this.currentSlimeMiniDir = BaseEnemy.EnemyDirection.Down;
            }
            else if (moveDir.y > 0) {
                // Turn Up

                this.currentSlimeMiniDir = BaseEnemy.EnemyDirection.Up;
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


    public override void HitDamage(float damageGet) {

        slimeMiniLifeControl.TakeDamage(damageGet);
    }

    public override bool IsCantAttack() {
        return slimeMiniLifeControl.GetCurrentSlimeLifeState() == BaseEnemy.EnemyLifeState.Death;
    }

    public override bool IsResistMagic() {
        return slimeSO.resistanceType == DamageResistance.MagicResistance;
    }

    public override bool IsResistPhysic() {
        return slimeSO.resistanceType == DamageResistance.PhysicResistance;
    }

    public override Vector3 GetEnemyVelocity() {

        if (!canMove || waypointList == null || waypointList.Count == 0) {
            return Vector3.zero;
        }

        Vector3 targetPos = waypointList[targetIndex].position;
        Vector3 moveDir = (targetPos - this.transform.position).normalized;

        return moveDir * slimeSO.moveSpeed;
    }

    public override float GetEnemyHealth() {
        return slimeMiniLifeControl.GetCurrentHealth();
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

    public void Initialize(Slime slimeParent) {

        this.waypointList = slimeParent.GetWaypointList();
        this.currentSlimeMiniDir = slimeParent.GetCurrentSlimeDirection();
        this.targetIndex = slimeParent.GetTargerIndex();
        this.targetPos = RandomWaypointPos(waypointList[targetIndex]);

        ChangeDirection();

        canMove = true;

    }

    public void OnDestroySelf() {
        Destroy(this.gameObject);   
    }

    public BaseEnemy.EnemyDirection GetCurrentSlimeMiniDirection() {
        return this.currentSlimeMiniDir;
    }

    public SlimeMiniLifeControl GetSlimeMiniLifeControl() {
        return this.slimeMiniLifeControl;
    }

    public EnemySO GetSlimeSO() {
        return this.slimeSO;
    }

    public bool CanMove() {
        return this.canMove;
    }

    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

    public static void SpawnSlimeMini(Transform slimeMiniPrefab, Transform parent, Slime slimeParent, Vector3 spawnPos) {

        Transform slimeTransform = Instantiate(slimeMiniPrefab, parent);
        slimeTransform.position = spawnPos;

        SlimeMini slimeMini = slimeTransform.GetComponent<SlimeMini>();

        slimeMini.Initialize(slimeParent);
    }
}
