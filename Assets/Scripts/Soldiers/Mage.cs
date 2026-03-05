using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Mage : MonoBehaviour
{
    public enum MageBehavior {

        Idle,
        PreAttack,
        Attack,

    }

    public event EventHandler OnAttackSFX;
    public event EventHandler OnSpawnSFX;

    public event EventHandler OnAttackAnim;

    [Header("Data")]
    [SerializeField] private SoldierSO mageSO;

    private MageTower mageTower;

    private SoldierSO.SoldierDirection mageDirection;
    private SoldierSO.SoldierLevel mageLevel;
    private MageBehavior mageBehavior;

    #region Aim Behavior
    private List<Collider2D> enemyDetectedList;
    private float sqrFarestDistance;
    private Transform currentTarget;
    private bool isForcedReAim;
    #endregion

    private Coroutine currentCoroutine;

    private float nextAllowedAttackTimer;

    private List<MagicBolt> magicBoltList;
    private int currentMagicBoltIndex = 0;

    private void Awake() {

        magicBoltList = new List<MagicBolt>();

        enemyDetectedList = new List<Collider2D>();

        ResetSetup();
    }

    private void Start() {

        mageTower.OnUpgradeLevelProgress += MageTower_OnUpgradeLevelProgress;
        mageTower.UnUpgradeLevelProgress += MageTower_UnUpgradeLevelProgress;

        // After spawn
        SpawnProjectileAddList();
        OnSpawnSFX?.Invoke(this, EventArgs.Empty);
    }

    private void OnDestroy() {
        mageTower.OnUpgradeLevelProgress -= MageTower_OnUpgradeLevelProgress;
        mageTower.UnUpgradeLevelProgress -= MageTower_UnUpgradeLevelProgress;
    }

    private void MageTower_UnUpgradeLevelProgress(object sender, EventArgs e) {
        // When Upgrade Progress done

        this.gameObject.SetActive(true);

        ResetSetup();

    }

    private void MageTower_OnUpgradeLevelProgress(object sender, EventArgs e) {
        // When Upgrade Progress start

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        foreach (MagicBolt spellProjectile in magicBoltList) {
            spellProjectile.Hide();
        }

        SpawnProjectileAddList();

        this.gameObject.SetActive(false);

    }

    private void Update() {

        HandleCurrentAimTarget();

        if (mageBehavior == MageBehavior.Idle) {
            // Đang Idle

            if (currentTarget != null) {
                // If has currentTarget

                ChangeMageBehaviorTo(MageBehavior.PreAttack);

            }
        }

    }


    public void OnTowerScannedNewData(List<Collider2D> enemyDetectedList) {

        // 1. Reset list enemyDetected
        this.enemyDetectedList.Clear();
        this.enemyDetectedList.AddRange(enemyDetectedList);

        // 2.
        if (mageBehavior == MageBehavior.Idle) {
            // Whenever mage is in idle behavior

            FindNewTarget();
        }


    }

    private void HandleCurrentAimTarget() {

        if (this.currentTarget != null) {
            // When having target

            if (!IsTargetValid() && !isForcedReAim) {
                // If current target is not valid anymore --> Stop all behavior and back to Idle

                ChangeMageBehaviorTo(MageBehavior.Idle);
                isForcedReAim = true;
            }
        }

        if (isForcedReAim) {
            // If have forced reAim command

            FindNewTarget();
        }

    }

    private void FindNewTarget() {

        currentTarget = null;
        isForcedReAim = false;
        sqrFarestDistance = -Mathf.Infinity;

        // Thực hiện Aim Target

        if (enemyDetectedList == null || enemyDetectedList.Count == 0) { return; }

        foreach (Collider2D enemyDetected in enemyDetectedList) {
            // Duyệt lần lượt enemy trong array để tìm khoảng cách xa nhất tới enemy nằm trong vùng nhìn thấy

            Vector2 dirFromArcherToEnemy = (enemyDetected.transform.position - this.transform.position).normalized;

            if (Vector2.Angle(GetViewDir(), dirFromArcherToEnemy) <= mageTower.GetMageTowerSO().viewAngle / 2f) {
                // Nếu enemy đó nằm trong vùng nhìn thấy trước mặt (90o)

                float sqrDistanceToEnemy = (this.transform.position - enemyDetected.transform.position).sqrMagnitude;

                if (sqrFarestDistance <= sqrDistanceToEnemy) {

                    sqrFarestDistance = sqrDistanceToEnemy;
                    currentTarget = enemyDetected.transform;

                }


            }

        }
    }

    private Vector3 GetViewDir() {

        switch (mageDirection) {

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

    private void ChangeMageBehaviorTo(MageBehavior mageState) {

        this.mageBehavior = mageState;

        // Setup after change state
        switch (mageState) {

            case MageBehavior.Idle:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                break;

            case MageBehavior.PreAttack:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                currentCoroutine = StartCoroutine(PreAttackCoroutine());

                break;

            case MageBehavior.Attack:

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
            // Target still valid
            ChangeMageBehaviorTo(MageBehavior.Attack);

        }
        else {
            // If target not valid anymore
            ChangeMageBehaviorTo(MageBehavior.Idle);
            isForcedReAim = true;
        }

    }



    private IEnumerator AttackCoroutine() {

        //float castSpellTimer = 0f;
        float castSpellTimerMax = 0.32f;

        // 1. Before attack
        yield return new WaitForSeconds(castSpellTimerMax);

        if (!IsTargetValid()) {
            // If current target is not valid anymore --> Stop all behavior and back to Idle

            ChangeMageBehaviorTo(MageBehavior.Idle);
            isForcedReAim = true;

            nextAllowedAttackTimer = Time.time + mageTower.GetCurrentTowerStatus().recoilTimer;


            yield break;
        }

        OnAttackSFX?.Invoke(this, EventArgs.Empty);

        // 2. Attack progress

        Vector3 enemyVelocity = Vector3.zero;

        Vector3 currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, 0f);

        float timeToHitEnemy = Vector3.Distance(currentTargetPos, this.transform.position) / mageTower.GetMageTowerSO().magicBoltSpeed; // time = Distance(mage, enemy) / projectileSpeed

        if (currentTarget.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

            enemyVelocity = baseEnemy.GetEnemyVelocity();

        }

        Vector3 leadOffsetTargetPos = currentTargetPos + (enemyVelocity * timeToHitEnemy); // P_future = P_current + (V * t)

        magicBoltList[currentMagicBoltIndex].StartMovement(leadOffsetTargetPos, timeToHitEnemy);
        OnAttackAnim?.Invoke(this, EventArgs.Empty);

        currentMagicBoltIndex = (currentMagicBoltIndex + 1) % magicBoltList.Count;

        // 3. After attack behavior
        nextAllowedAttackTimer = Time.time + mageTower.GetCurrentTowerStatus().recoilTimer;
        ChangeMageBehaviorTo(MageBehavior.PreAttack);

    }

    private void ResetSetup() {
        // Attack Behavior
        sqrFarestDistance = -Mathf.Infinity;
        currentTarget = null;
        isForcedReAim = false;

        // Arrow
        currentMagicBoltIndex = 0;

        // State
        ChangeMageBehaviorTo(MageBehavior.Idle);
    }

    private void SpawnProjectileAddList() {

        float maxRangeMove = mageTower.GetCurrentAttackRange();
        float projectileSpeed = mageTower.GetMageTowerSO().magicBoltSpeed;
        float attackTimer = mageTower.GetCurrentTowerStatus().recoilTimer;

        int buffer = 3; // Phần projectile dôi ra thêm để đảm bảo số lượng

        int numberOfProjectile = (int)Mathf.Ceil(maxRangeMove / (projectileSpeed * attackTimer)) + buffer;

        while (magicBoltList.Count < numberOfProjectile) {
            // When number of arrow in arrowList not equal to numberOfProjectile

            magicBoltList.Add(MagicBolt.SpawnSpellProjectile(mageSO.bulletPrefab, mageTower, this));
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
        float AttackRange = mageTower.GetCurrentAttackRange();

        if (distanceToTarget >= AttackRange * AttackRange) {
            return false;
        }

        // 4. Kiểm tra góc của target hiện tại còn nằm trong angleView không
        Vector2 dirFromMageToEnemy = (currentTarget.transform.position - this.transform.position).normalized;

        if (Vector2.Angle(GetViewDir(), dirFromMageToEnemy) > mageTower.GetMageTowerSO().viewAngle / 2f) {
            return false;
        }

        return true;
    }

    public MageBehavior GetMageState() {
        return this.mageBehavior;
    }

    public SoldierSO.SoldierDirection GetMageDirection() {
        return this.mageDirection;
    }

    public void SetMageTowerParent() {
        this.mageTower = GetComponentInParent<MageTower>();
    }

    public void SetMageDirection(SoldierSO.SoldierDirection mageDir) {
        this.mageDirection = mageDir;
    }

    public static Mage SpawnMage(SoldierManagerSO soldierSO, Transform spawnPoint, SoldierSO.SoldierDirection mageDir) {

        SoldierSO mageSO = null;

        if (mageDir == SoldierSO.SoldierDirection.Up) {
            mageSO = soldierSO.upMageSO;
        }
        if (mageDir == SoldierSO.SoldierDirection.Right) {
            mageSO = soldierSO.rightMageSO;
        }
        if (mageDir == SoldierSO.SoldierDirection.Down) {
            mageSO = soldierSO.downMageSO;
        }
        if (mageDir == SoldierSO.SoldierDirection.Left) {
            mageSO = soldierSO.leftMageSO;
        }

        Transform mageTransform = Instantiate(mageSO.soldierPrefab, spawnPoint.parent);
        mageTransform.localPosition = spawnPoint.localPosition;

        Mage mage = mageTransform.GetComponent<Mage>();

        mage.SetMageTowerParent();
        mage.SetMageDirection(mageDir);

        return mage;

    }


    #region Visualize
    private void OnDrawGizmos() {

        Gizmos.color = new Color(0, 1, 1, 0.2f);
        float range = 10f;
        Vector3 forward = GetViewDir();

        float viewAngle;

        if (mageTower != null) {
            viewAngle = mageTower.GetMageTowerSO().viewAngle;
        }
        else {
            viewAngle = 90f;
        }

        #if UNITY_EDITOR
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, viewAngle / 2) * forward * range);
        Gizmos.DrawRay(transform.position, Quaternion.Euler(0, 0, -viewAngle / 2) * forward * range);
        Handles.DrawWireArc(transform.position, Vector3.forward, Quaternion.Euler(0, 0, viewAngle / 2) * forward, -viewAngle, range);
        #endif
    }
    #endregion
}
