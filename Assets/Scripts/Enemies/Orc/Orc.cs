using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orc : BaseEnemy, ICanAttackPhysic
{
    public enum OrcBehavior {
        Walk,
        Attack
    }

    public event EventHandler ChangedLeftDir;
    public event EventHandler ChangedRightDir;
    public event EventHandler OnAttackAnim;
    public event EventHandler OnResetAnim;
    public event EventHandler ActiveAnomaly;

    [Header("Orc Data")]
    [SerializeField] private EnemySO orcSO;

    [Header("Thunder Strike Anomaly")]
    [SerializeField] private Transform thunderLightningAnomaly;
    [SerializeField] private float anomalyDmgMultiply;

    private OrcLifeControl orcLifeControl;
    private EnemySpawner parentSpawner;

    private List<Transform> waypointList;

    private BaseEnemy.EnemyDirection currentOrcDirection;
    private OrcBehavior currentBehavior;

    private int targetIndex;
    private Vector3 targetPos;
    private float randomTargetTimer;

    private bool canMove = false;

    private bool isLockedByTarget;
    private Guardian currentTarget;
    private Coroutine currentCoroutine;

    private void Awake() {

        waypointList = new List<Transform>();

        orcLifeControl = GetComponent<OrcLifeControl>();

        parentSpawner = GetComponentInParent<EnemySpawner>();

    }

    private void Start() {

        parentSpawner.ActiveEnemy += ParentSpawner_ActiveEnemy;
        
        orcLifeControl.OnSpawn += OrcLifeControl_OnSpawn;
        orcLifeControl.OnDeath += OrcLifeControl_OnDeath;

        this.gameObject.SetActive(false);
    }

    private void OnDestroy() {

        parentSpawner.ActiveEnemy -= ParentSpawner_ActiveEnemy;

        orcLifeControl.OnSpawn -= OrcLifeControl_OnSpawn;
        orcLifeControl.OnDeath -= OrcLifeControl_OnDeath;
    }


    private void OrcLifeControl_OnDeath(object sender, EventArgs e) {

        if (SaveData.IsUnlockAnomaly(orcSO)) {
            // If anomaly is unlocked

            int randomValue = Mathf.FloorToInt(UnityEngine.Random.value * 9.99f);

            if (randomValue == 0f) {
                // 10% chance to active anomaly

                StartCoroutine(ActivatedAnomalyCoroutine());
            }
            else {
                Hide();
            }
        }
        else {
            // If anomaly isn't unlocked 

            Hide();
        }
    }


    private void OrcLifeControl_OnSpawn(object sender, EventArgs e) {

        // 1. Reset Position and Next Target
        this.transform.position = waypointList[0].position;
        targetIndex = 1;

        // 2. Reset movement behavior and direction
        ChangeOrcBehaviorTo(OrcBehavior.Walk);
        ChangeDirection(waypointList[targetIndex].position);
        canMove = true;
        targetPos = RandomWaypointPos(waypointList[targetIndex]);

        // 3. Reset target
        isLockedByTarget = false;
        currentTarget = null;

    }


    private void ParentSpawner_ActiveEnemy(object sender, EnemySpawner.OnActiveEnemyEventArgs e) {

        if (this == e.baseEnemy) {

            waypointList = PathGenerator.Instance.GetWaypointList();

            Vector3 randomPos = RandomWaypointPos(waypointList[0]);
            this.transform.position = randomPos;

            Show();

            this.ActiveEvent();

            StartCoroutine(orcLifeControl.RespawnCoroutine());

        }

    }

    private void Update() {

        if (isLockedByTarget && canMove) {
            // Nếu bị block bởi lính khi đang di Walk

            canMove = false; // Dừng lại

            ChangeOrcBehaviorTo(OrcBehavior.Attack);
        }

        HandleMovement();

    }

    private void HandleMovement() {

        if (!canMove) {
            return;
        }

        Vector3 moveDir = (targetPos - this.transform.position).normalized;
        float sqrDistance = (this.transform.position - targetPos).sqrMagnitude;

        transform.position += moveDir * orcSO.moveSpeed * Time.deltaTime;

        if (sqrDistance <= 0.1f * 0.1f) {
            // If reachingTargetPoint

            if (targetIndex == waypointList.Count - 1) {
                // If reach last index

                orcLifeControl.ChangeLifeStateTo(EnemyLifeState.Despawn);

                return;

            }
            else {
                // If not reach last index 
                targetIndex += 1;

                targetPos = RandomWaypointPos(waypointList[targetIndex]);

                ChangeDirection(waypointList[targetIndex].position);

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

    private void ChangeOrcBehaviorTo(OrcBehavior orcBehavior) {

        this.currentBehavior = orcBehavior;

        switch (currentBehavior) {

            case OrcBehavior.Walk:

                canMove = true;
                isLockedByTarget = false;

                ChangeDirection(waypointList[targetIndex].position); // Counting direction again

                OnResetAnim?.Invoke(this, EventArgs.Empty);
                
                break;

            case OrcBehavior.Attack:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                currentCoroutine = StartCoroutine(AttackCoroutine());

                break;
        }

    }

    private IEnumerator AttackCoroutine() {

        float attackSpeed = orcSO.attackTimer; // Mặc định là 0.5s (Dựa trên animation)
        float attackDamage = orcSO.attackDamage;
        float delayAttackAnimTimer = 0.4f;

        while (!this.currentTarget.GetGuardianLifeControl().IsDeath()) {

            if (IsCantAttack()) {
                // If orc is death while attack

                yield break;
            }

            ChangeDirection(currentTarget.transform.position);

            OnAttackAnim?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(delayAttackAnimTimer);

            if (this.currentTarget.GetGuardianLifeControl().IsDeath()) {
                // If current target is death

                ChangeOrcBehaviorTo(OrcBehavior.Walk);

                yield break;
            }
            else {
                // If current target is not death

                this.currentTarget.GetGuardianLifeControl().TakeDamage(attackDamage);
            }

            yield return new WaitForSeconds(attackSpeed - delayAttackAnimTimer);
        }

        if (this.currentTarget.GetGuardianLifeControl().IsDeath()) {
            // If current target is death

            ChangeOrcBehaviorTo(OrcBehavior.Walk);
        }
    }

    private IEnumerator ActivatedAnomalyCoroutine() {

        float anomalyDamage = orcSO.attackDamage * anomalyDmgMultiply; // Sát thương lớn hơn thông thường

        // Anomaly Setup
        Transform thunderLightningAnomalyTransform = Instantiate(thunderLightningAnomaly);

        if (!currentTarget.GetGuardianLifeControl().IsDeath()) {
            // If current guardian is not death

            thunderLightningAnomalyTransform.transform.position = currentTarget.transform.position;

        }
        else {
            // If current guardian is death

            Collider2D[] attackObjectDetectedArray = Physics2D.OverlapCircleAll(this.transform.position, orcSO.detectedZone, orcSO.canAttackLayer);

            if (attackObjectDetectedArray.Length > 0) {
                // If detected any canAttack object

                Transform currentStrikeTarget = null;
                float closestDistance = Mathf.Infinity;

                foreach (Collider2D attackObject in attackObjectDetectedArray) {

                    float distanceToObject = (this.transform.position - attackObject.transform.position).sqrMagnitude;

                    if (distanceToObject <= closestDistance) {

                        closestDistance = distanceToObject;

                        currentStrikeTarget = attackObject.transform;
                    }

                }

                thunderLightningAnomalyTransform.position = currentStrikeTarget.position;

            }
            else {
                // If cant detected any canAttack object

                Hide();
                yield break;
            }
        }

        ActiveAnomaly?.Invoke(this, EventArgs.Empty);

        yield return new WaitForSeconds(0.05f);

        if (!currentTarget.GetGuardianLifeControl().IsDeath()) {
            // If current guardian is not death

            currentTarget.GetGuardianLifeControl().TakeDamage(anomalyDamage);

        }
        else {
            // If current guardian is death
        }

        yield return new WaitForSeconds(0.45f); // Wait for end of Strike Anim

        yield return new WaitForSeconds(0.6f); // Wait for end of Strike sfx

        Destroy(thunderLightningAnomalyTransform.gameObject);

        Hide();

    }

    private void ChangeDirection(Vector3 targetPosition) {

        //Vector3 moveDir = waypointList[targetIndex].position - this.transform.position;
        Vector3 moveDir = targetPosition - this.transform.position;

        if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) {
            // Đang đi ngang

            if (moveDir.x < 0) {
                // Turn Left

                this.currentOrcDirection = BaseEnemy.EnemyDirection.Left;

                ChangedLeftDir?.Invoke(this, EventArgs.Empty);
            }
            else if (moveDir.x > 0) {
                // Turn Right

                this.currentOrcDirection = BaseEnemy.EnemyDirection.Right;

                ChangedRightDir?.Invoke(this, EventArgs.Empty);
            }


        }

        if (Mathf.Abs(moveDir.y) > Mathf.Abs(moveDir.x)) {
            // Đang đi dọc

            if (moveDir.y < 0) {
                // Turn Down

                this.currentOrcDirection = BaseEnemy.EnemyDirection.Down;
            }
            else if (moveDir.y > 0) {
                // Turn Up

                this.currentOrcDirection = BaseEnemy.EnemyDirection.Up;
            }

        }

    }

    private Vector3 RandomWaypointPos(Transform targetWaypoint) {

        // Mặc định đặt lại randomTargetTimer mỗi lần randomWaypoint
        randomTargetTimer = orcSO.randomTargetTimer;

        // 1 node có kích thước 2 * 2 với center là waypoint ---> waypointRandom nằm trong khoảng (2,2)
        float offsetX = UnityEngine.Random.Range(-orcSO.radiusOffset, orcSO.radiusOffset);
        float offsetY = UnityEngine.Random.Range(-orcSO.radiusOffset, orcSO.radiusOffset);

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
        orcLifeControl.TakeDamage(damageGet);
    }

    public override bool IsCantAttack() {
        return orcLifeControl.GetCurrentOrcLifeState() == BaseEnemy.EnemyLifeState.Death || orcLifeControl.GetCurrentOrcLifeState() == BaseEnemy.EnemyLifeState.Despawn;
    }

    public override bool IsResistMagic() {
        return orcSO.resistanceType == DamageResistance.MagicResistance;
    }

    public override bool IsResistPhysic() {
        return orcSO.resistanceType == DamageResistance.PhysicResistance;
    }

    public override EnemyDirection GetEnemyCurrentDirection() {
        return this.currentOrcDirection;
    }

    public override float GetEnemyHealth() {
        return orcLifeControl.GetCurrentHealth();
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

    public override Vector3 GetEnemyVelocity() {

        if (!canMove || waypointList == null || waypointList.Count == 0) {
            return Vector3.zero;
        }

        Vector3 targetPos = waypointList[targetIndex].position;
        Vector3 moveDir = (targetPos - this.transform.position).normalized;

        return moveDir * orcSO.moveSpeed;
    }

    public bool HasLockedTarget() {
        return isLockedByTarget;
    }

    public void SetLockedTarget(bool isLockedTarget, Guardian guardian) {
        this.isLockedByTarget = isLockedTarget;
        this.currentTarget = guardian;
    }

    public EnemySO GetOrcSO() {
        return this.orcSO;
    }

    public OrcBehavior GetCurrentOrcBehavior() {
        return this.currentBehavior;
    }

    public OrcLifeControl GetOrcLifeControl() {
        return this.orcLifeControl;
    }

    public void SetCanMove(bool canMove) {
        this.canMove = canMove;
    }

}
