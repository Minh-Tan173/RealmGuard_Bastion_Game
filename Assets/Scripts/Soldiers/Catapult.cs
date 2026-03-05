using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Analytics;

public class Catapult : MonoBehaviour
{
    public enum CatapultBehavior {

        Idle,
        Attack,

    }

    public enum CatapultDirection {
        Up,
        UpRight,    // Chéo trên bên phải
        Right,
        DownRight,  // Chéo dưới bên phải
        Down,
        DownLeft,   // Chéo dưới bên trái
        Left,
        UpLeft      // Chéo trên bên trái
    }

    public static event EventHandler OnAttackSFX;

    public event EventHandler OnAttackAnim;
    public event EventHandler OnIdleAnim;

    public event EventHandler OnChangedCatapultLevel;

    [Header("Parent")]
    [SerializeField] private CatapultTower catapultTower;

    [Header("Data")]
    [SerializeField] private SoldierSO catapultSO;

    [Header("Attack Weight")]
    [SerializeField] private float weightHealth; // wHeight cao thì ưu tiên target có HP cao hơn
    [SerializeField] private float weightProgress; // ưProgress cao thì ưu tiên target gần baseHome

    private CatapultDirection currentDirection;
    private SoldierSO.SoldierLevel catapultLevel;
    private CatapultBehavior catapultBehavior;

    private float currentAttackRange;

    #region Aiming Target Behavior
    private Transform currentTarget;
    private bool isForceReAim;
    #endregion

    private float nextAllowedAttackTimer = 0f;

    private Coroutine currentCoroutine;

    private List<Pebble> pebbleList;
    private int currentPebbleIndex = 0;

    private Vector3 debugLastLeadPos;

    private void Awake() {

        pebbleList = new List<Pebble>();

        ResetSetup();

        catapultTower.OnUpgradeLevelProgress += CatapultTower_OnUpgradeLevelProgress;
        catapultTower.UnUpgradeLevelProgress += CatapultTower_UnUpgradeLevelProgress;

        // Level 1 is default
        ChangeCatapultLevelTo(SoldierSO.SoldierLevel.Level1);
    }

    private void Start() {

        // After spawn
        catapultLevel = SoldierSO.SoldierLevel.Level1;

        currentDirection = CatapultDirection.Down;

        SpawnPebbleAddList();

    }

    private void OnDestroy() {

        catapultTower.OnUpgradeLevelProgress -= CatapultTower_OnUpgradeLevelProgress;
        catapultTower.UnUpgradeLevelProgress -= CatapultTower_UnUpgradeLevelProgress;
    }

    private void CatapultTower_UnUpgradeLevelProgress(object sender, EventArgs e) {
        // When Upgrade Progress done

        // 1. Reset setup animator base on catapult level
        if (catapultTower.GetCurrentTowerStatus().levelTower == ITowerObject.LevelTower.Level3) {
            // When catapult tower reached level 3

            ChangeCatapultLevelTo(SoldierSO.SoldierLevel.Level2);
        }
        else if (catapultTower.GetCurrentTowerStatus().levelTower == ITowerObject.LevelTower.Level4) {
            // When catapult tower reached level 4

            ChangeCatapultLevelTo(SoldierSO.SoldierLevel.Level3);

        }

        // 2. Reset catapult setup
        this.transform.localPosition = catapultTower.GetCurrentTowerStatus().catapulSpawnPos;
        this.gameObject.SetActive(true);
        ResetSetup();

        
    }

    private void CatapultTower_OnUpgradeLevelProgress(object sender, EventArgs e) {
        // When Upgrade Progress start

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        foreach (Pebble pebble in pebbleList) {
            pebble.Hide();
        }

        SpawnPebbleAddList();

        this.gameObject.SetActive(false);

    }

    private void Update() {

        HandleAimTarget();

        if (catapultBehavior == CatapultBehavior.Idle) {
            // Đang Idle

            if (currentTarget != null) {
                // If has currentTarget

                ChangeCatapultBehaviorTo(CatapultBehavior.Attack);

            }
        }


    }

    public void OnTowerScannedNewData() {
        // Tower command catapult find new target base on new data tower scanned

        if (catapultBehavior == CatapultBehavior.Idle) {

            FindNewTarget();
        }
    }

    private void HandleAimTarget() {

        if (currentTarget != null) {
            // If has aim target


            ChangeDirectionByTarget(currentTarget);

            if (!IsTargetValid() && !isForceReAim) {

                isForceReAim = true;
                ChangeCatapultBehaviorTo(CatapultBehavior.Idle);
            }
        }

        if (isForceReAim) {
            // Re aim target when force to reAim

            FindNewTarget();

        }


    }

    private void FindNewTarget() {

        currentTarget = null;
        isForceReAim = false;

        List<Collider2D> enemyDetectedList = catapultTower.GetEnemyDetectedList();

        if (enemyDetectedList == null || enemyDetectedList.Count <= 0) return;

        List<BaseEnemy> validEnemiesList = new List<BaseEnemy>();

        // 0. Phase 0: Get Valid Enemy
        foreach (Collider2D enemyDetected in enemyDetectedList) {

            Vector2 dirFromArcherToEnemy = (enemyDetected.transform.position - this.transform.position).normalized;

            float sqrDistanceToEnemy = (this.transform.position - enemyDetected.transform.position).sqrMagnitude;
            float sqrBlindSpotRange = catapultTower.GetCatapultTowerSO().blindSpotRange * catapultTower.GetCatapultTowerSO().blindSpotRange;

            // 0. Loại bỏ enemy IsCantAttack

            // 1. Xác định xem enemy này có nằm trong điểm mù không
            if (sqrDistanceToEnemy <= sqrBlindSpotRange) {
                // If enemy position is in blind spot
                continue;
            }

            //// 2. Loại bỏ enemy nằm rìa Attack Zone
            //float canAttackZone = currentAttackRange / 100 * 97;
            //if (sqrDistanceToEnemy >= currentAttackRange * currentAttackRange) {
            //    continue;
            //}

            // 3. Add into valid list
            if (enemyDetected.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                if (baseEnemy.IsCantAttack()) {
                    // Nếu enemy này đã cant attack nhưng vẫn tồn tại vì lí do nào đó (chưa hết death animation)
                    continue;
                }

                validEnemiesList.Add(baseEnemy);
            }
            
        }

        // 1. PHASE 1: Gather & Find Max
        float maxHealth = -Mathf.Infinity;
        float maxProgress = -Mathf.Infinity;

        foreach (BaseEnemy validEnemy in validEnemiesList) {

            if (maxHealth <= validEnemy.GetEnemyHealth()) {

                maxHealth = validEnemy.GetEnemyHealth();
            }

            if (maxProgress <= validEnemy.GetEnemyProgress()) {

                maxProgress = validEnemy.GetEnemyProgress();
            }
        }

        // 2. PHASE 2: Calculate Individual Vi
        Dictionary<BaseEnemy, float> enemyIndividualScoreDict = new Dictionary<BaseEnemy, float>();

        foreach (BaseEnemy validEnemy in validEnemiesList) {

            float normH = validEnemy.GetEnemyHealth() / maxHealth; // normalized Health = currentHP / maxHP
            float normP = validEnemy.GetEnemyProgress() / maxProgress; // normalized Progress = currentProgress / maxProgress

            float individual = weightHealth * normH + weightProgress * normP;

            if (!enemyIndividualScoreDict.ContainsKey(validEnemy)) {

                enemyIndividualScoreDict.Add(validEnemy, individual);
            }
        }

        // 3. PHASE 3: Cluster Analysis - The "Smart" Part - Chọn target có tổng điểm vùng nổ (tổng Vi cao nhất)

        float simulateRadiusCheck = pebbleList[0].GetPebbleVisual().GetBaseBoomSize() * catapultTower.GetCurrentTowerStatus().pebbleSplashRadius;
        float sqrRadiusCheck = simulateRadiusCheck * simulateRadiusCheck;

        float highestClusterScore = -Mathf.Infinity;

        foreach (BaseEnemy validEnemy in validEnemiesList) {

            float totalIndividual = enemyIndividualScoreDict[validEnemy];

            
            foreach (BaseEnemy otherValidEnemy in validEnemiesList) {
 
                if (otherValidEnemy == validEnemy) { continue; }

                float distanceToOtherEnemy = (otherValidEnemy.transform.position - validEnemy.transform.position).sqrMagnitude;

                if (distanceToOtherEnemy > sqrRadiusCheck) { continue; }

                totalIndividual += enemyIndividualScoreDict[otherValidEnemy];

            }

            if (totalIndividual >= highestClusterScore) {

                highestClusterScore = totalIndividual;
                currentTarget = validEnemy.transform;
            }
        }




    }

    private void ChangeCatapultLevelTo(SoldierSO.SoldierLevel catapultLevel) {
        
        this.catapultLevel = catapultLevel;

        OnChangedCatapultLevel?.Invoke(this, EventArgs.Empty);
    }

    private void ChangeDirectionByTarget(Transform target) {

        // Counting Angle of target with catapult
        Vector3 dirToTarget = (target.transform.position - this.transform.position).normalized;

        float catapultAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

        if (catapultAngle > -22.5f && catapultAngle <= 22.5f) {
            currentDirection = CatapultDirection.Right;
        }
        else if (catapultAngle > 22.5f && catapultAngle <= 67.5f) {
            currentDirection = CatapultDirection.UpRight;
        }
        else if (catapultAngle > 67.5f && catapultAngle <= 112.5f) {
            currentDirection = CatapultDirection.Up;
        }
        else if (catapultAngle > 112.5f && catapultAngle <= 157.5f) {
            currentDirection = CatapultDirection.UpLeft;
        }
        else if (catapultAngle > 157.5f || catapultAngle <= -157.5f) {
            // Hướng Left bao phủ khu vực quanh 180 và -180
            currentDirection = CatapultDirection.Left;
        }
        else if (catapultAngle > -157.5f && catapultAngle <= -112.5f) {
            currentDirection = CatapultDirection.DownLeft;
        }
        else if (catapultAngle > -112.5f && catapultAngle <= -67.5f) {
            currentDirection = CatapultDirection.Down;
        }
        else if (catapultAngle > -67.5f && catapultAngle <= -22.5f) {
            currentDirection = CatapultDirection.DownRight;
        }

    }


    private void ChangeCatapultBehaviorTo(CatapultBehavior catapultState) {

        this.catapultBehavior = catapultState;

        if (currentCoroutine != null) {

            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        // Setup after change state
        switch (catapultState) {

            case CatapultBehavior.Idle:

                currentTarget = null;
                isForceReAim = true;

                OnIdleAnim?.Invoke(this, EventArgs.Empty);

                break;

            case CatapultBehavior.Attack:
                // Mặc định vào Attack thì mọi coroutine đã stop

                currentCoroutine = StartCoroutine(AttackCoroutine());

                break;
        }

    }

    private IEnumerator AttackCoroutine() {

        CatapultTowerLevelData currentStatus = catapultTower.GetCurrentTowerStatus();

        // 1. Waiting CD Attack
        if (!IsTargetValid()) {

            ChangeCatapultBehaviorTo(CatapultBehavior.Idle);
            yield break;

        }

        float attackRange = catapultTower.GetCurrentAttackRange();
        while (Time.time < nextAllowedAttackTimer) {

            if (!IsTargetValid()) {

                ChangeCatapultBehaviorTo(CatapultBehavior.Idle);
                yield break;

            }
            
            yield return null;
        }

        // 2. Start Attack Progress

        if (!IsTargetValid()) {

            ChangeCatapultBehaviorTo(CatapultBehavior.Idle);
            yield break;

        }


        BaseEnemy baseEnemy = currentTarget.GetComponent<BaseEnemy>();

        Vector3 currentTargetPos = new Vector3(currentTarget.position.x, currentTarget.position.y, 0f);

        float timeToHitEnemy = Vector3.Distance(currentTargetPos, this.transform.position) / currentStatus.pebbleSpeed; // time = Distance(catapult, enemy) / arrowSpeed
        Vector3 enemyVelocity = baseEnemy.GetEnemyVelocity();

        Vector3 leadOffsetTargetPos = currentTargetPos + (enemyVelocity * timeToHitEnemy); // P_future = P_current + (V * t)

        // Giới hạn LeadPos chỉ có thể nằm trong ngoài vùng attack Range 10%
        Vector3 dirFromTowerToLeadPos = leadOffsetTargetPos - this.transform.position;
        float distance = dirFromTowerToLeadPos.magnitude;

        if (distance > catapultTower.GetCurrentAttackRange() / 100 * 110) {
            // If leadPos is outside attack range 10%

            leadOffsetTargetPos = this.transform.position + (dirFromTowerToLeadPos.normalized * catapultTower.GetCurrentAttackRange());

        }

        // Get center node where target are on
        GridNode nodeOfLeadTargetPos = GridManager.Instance.GetNodeAt(leadOffsetTargetPos);
        Vector3 centerNodeOnWorld;

        if (nodeOfLeadTargetPos != null) {
            Vector2Int centerNodeOnGridMap = nodeOfLeadTargetPos.GetNodePosition();
            centerNodeOnWorld = GridManager.Instance.NodePosConvertToWordPos(centerNodeOnGridMap);
        }
        else {
            centerNodeOnWorld = leadOffsetTargetPos;
        }

        debugLastLeadPos = centerNodeOnWorld;

        OnAttackAnim?.Invoke(this, EventArgs.Empty);
        OnAttackSFX?.Invoke(this, EventArgs.Empty);

        float randomStartAngle = UnityEngine.Random.Range(0f, 360f); // Góc quay khởi đầu ngẫu nhiên

        float countOfPebbleAgain = 0f;
        float angleStep = 0f;
        float radius = 0f;

        if (currentStatus.pebblePerVolley > 1) {

            countOfPebbleAgain = currentStatus.pebblePerVolley - 1; // count of pebble except first one
            angleStep = 360 / countOfPebbleAgain;
            radius = pebbleList[0].GetPebbleVisual().GetBaseBoomSize() * catapultTower.GetCurrentTowerStatus().pebbleSplashRadius; // Khoảng cách để tận dụng tối đa kích thước vụ nổ của pebble

        }

        for (int i = 0; i < currentStatus.pebblePerVolley; i++) {
            
            if (i == 0) {
                // If is first pebble

                pebbleList[currentPebbleIndex].StartMovement(leadOffsetTargetPos);

            }
            else {
                // If is other pebble

                float currentAngle = randomStartAngle + angleStep * (i - 1);
                Vector2 offset = Quaternion.Euler(0f, 0f, currentAngle) * Vector2.right * radius;
                Vector3 pebbleExploPos = leadOffsetTargetPos + new Vector3(offset.x, offset.y, 0f);

                pebbleList[currentPebbleIndex].StartMovement(pebbleExploPos);
            }

            currentPebbleIndex = (currentPebbleIndex + 1) % pebbleList.Count;
        }

        // 3. After attack behavior
        nextAllowedAttackTimer = Time.time + currentStatus.recoilTimer;

        yield return new WaitForSeconds(0.5f); // Wait for end of Attack Anim

        ChangeCatapultBehaviorTo(CatapultBehavior.Idle);

    }

    private void ResetSetup() {
        // Attack Behavior
        currentTarget = null;
        isForceReAim = false;

        currentAttackRange = catapultTower.GetCurrentAttackRange();

        // Arrow
        currentPebbleIndex = 0;

        // State
        ChangeCatapultBehaviorTo(CatapultBehavior.Idle);
    }

    private void SpawnPebbleAddList() {

        float maxRangeMove = catapultTower.GetCurrentAttackRange();
        float pebbleSpeed = catapultTower.GetCurrentTowerStatus().pebbleSpeed;
        float attackTimer = catapultTower.GetCurrentTowerStatus().recoilTimer;
        int pebblePerVolley = catapultTower.GetCurrentTowerStatus().pebblePerVolley; // số lượng pebble dùng mỗi lần attack

        int buffer = 3; // Phần pebble dôi ra thêm để đảm bảo số lượng

        int numberOfPebbel = ((int)Mathf.Ceil(maxRangeMove / (pebbleSpeed * attackTimer))) * pebblePerVolley + buffer;

        while (pebbleList.Count < numberOfPebbel) {
            // When number of pebble in pebbleList not equal to numberOfPebble

            pebbleList.Add(Pebble.SpawnPebble(catapultSO.bulletPrefab, catapultTower, this));
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
        float sqrDistanceToTartget = (this.transform.position - currentTarget.transform.position).sqrMagnitude;

        if (sqrDistanceToTartget > this.currentAttackRange * this.currentAttackRange) {

            return false;

        }

        return true;
    }

    public SoldierSO.SoldierLevel GetCatapultLevel() {
        return this.catapultLevel;
    }

    public CatapultBehavior GetCatapultState() {
        return this.catapultBehavior;
    }

    public void SetCatapultTowerParent() {
        this.catapultTower = GetComponentInParent<CatapultTower>();
    }

    public CatapultDirection GetCurrentCatapultDirection() {
        return this.currentDirection;
    }

    private void OnDrawGizmos() {
        
        // Visualize Current Target in debug mode
        if (currentTarget != null) {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, currentTarget.position);
            Gizmos.DrawWireSphere(currentTarget.position, 0.3f);
        }

        // Visualize Last Lead Pos
        if (debugLastLeadPos != Vector3.zero) {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(debugLastLeadPos, 0.2f);
            Gizmos.DrawLine(transform.position, debugLastLeadPos);
        }

    }
}
