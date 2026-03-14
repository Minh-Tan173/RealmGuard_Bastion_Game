using System;
using System.Collections;
using UnityEngine;

public class Guardian : MonoBehaviour {

    public enum GuadianState {
        Idle,
        Guarding,
        LockedIntentTarget,
        Attack
    }

    public event EventHandler ChangedLeftDir;
    public event EventHandler ChangedRightDir;
    public event EventHandler OnGuardianRequestTask;

    public event EventHandler OnResetAnimator;
    public event EventHandler OnAttackAnim;

    [Header("Guardian Data")]
    [SerializeField] private float moveSpeed;

    private GuardianTower guardianTower;
    private GuardianLifeControl guardianLifeControl;

    private Vector3 currentGuardPos;
    private BaseEnemy.EnemyDirection combatStanceDir; // Góc nhìn hiện tại của Guardian dựa trên guardPos
    private SoldierSO.SoldierDirection activeFacingDir; // Góc nhìn thực tế dựa trên vị trí hiện tại 
    private Coroutine currentCoroutine;

    private BaseEnemy intentTarget;
    private BaseEnemy currentTarget;

    private GuadianState currentGuadianState;

    private bool isMovingToNewGuardPos;

    private bool isWalking;

    private bool pendingResetAnimator = false;

    private void Awake() {

        guardianLifeControl = GetComponent<GuardianLifeControl>();

        currentGuardPos = Vector3.zero;

        ChangeGuardianState(GuadianState.Idle);

    }

    private void OnEnable() {
        
        if (pendingResetAnimator) {

            OnResetAnimator?.Invoke(this, EventArgs.Empty);
            pendingResetAnimator = false;

        }

    }

    private void Start() {

        guardianTower.OnUpgradeLevelProgress += GuardianTower_OnUpgradeLevelProgress;
        guardianTower.OnResetDataGuardian += GuardianTower_OnResetDataGuardian;

        guardianLifeControl.OnDeath += GuardianLifeControl_OnDeath;

    }

    private void OnDestroy() {

        guardianTower.OnUpgradeLevelProgress -= GuardianTower_OnUpgradeLevelProgress;
        guardianTower.OnResetDataGuardian -= GuardianTower_OnResetDataGuardian;

        guardianLifeControl.OnDeath -= GuardianLifeControl_OnDeath;

    }

    private void GuardianLifeControl_OnDeath(object sender, EventArgs e) {
        // When death

        Hide();
    }

    private void GuardianTower_OnResetDataGuardian(object sender, EventArgs e) {
        // When has reset command from Tower

        this.transform.position = guardianTower.GetSpawnPoint().position;

        guardianLifeControl.ChangeLifeStateTo(GuardianLifeControl.GuardianLifeState.Alive);
        ChangeGuardianState(GuadianState.Idle);
    }

    private void GuardianTower_OnUpgradeLevelProgress(object sender, EventArgs e) {
        // When upgrade progress start

        Hide();
    }

    private void ChangeGuardianState(GuadianState guadianState) {

        this.currentGuadianState = guadianState;

        switch (currentGuadianState) {

            case GuadianState.Idle:

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                // When back to Idle
                isWalking = false;
                intentTarget = null;
                currentTarget = null;
                
                OnGuardianRequestTask?.Invoke(this, EventArgs.Empty); // Guardian báo cho Tower là mình đang rảnh

                if (this.gameObject.activeSelf) {
                    // If this game object is activated
                    OnResetAnimator?.Invoke(this, EventArgs.Empty);

                }
                else {
                    // If this game object is not activated ---> wait to when activated

                    pendingResetAnimator = true;
                }

                break;

            case GuadianState.Guarding:

                break;

            case GuadianState.LockedIntentTarget:

                break;

            case GuadianState.Attack:

                isWalking = false;

                intentTarget = null;  // Vì lúc này đã có target thực, không còn target giả định nữa

                if (currentCoroutine != null) {
                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;
                }

                currentCoroutine = StartCoroutine(AttackCoroutine());

                break;
        }

    }

    private IEnumerator MoveToGuardPosCoroutine(Vector3 guardPos) {

        isMovingToNewGuardPos = true;

        isWalking = true;

        float smallestDistance = 0.1f;
        float sqrDistance = (this.transform.position - this.currentGuardPos).sqrMagnitude;

        while (sqrDistance >= smallestDistance * smallestDistance) {

            // Move and Changed Direction
            Vector3 moveDir = (this.currentGuardPos - this.transform.position).normalized;
            this.transform.position += moveDir * moveSpeed * Time.deltaTime;
            ChangedActiveFacingDirTo(currentGuardPos);

            sqrDistance = (this.transform.position - this.currentGuardPos).sqrMagnitude;

            yield return null;
        }

        // Last Pos
        this.transform.position = this.currentGuardPos;

        // Change active facing dir to opposite of combatStanceDir
        if (this.combatStanceDir == BaseEnemy.EnemyDirection.Up) {

            this.activeFacingDir = SoldierSO.SoldierDirection.Down;
        }
        else if (this.combatStanceDir == BaseEnemy.EnemyDirection.Down) {

            this.activeFacingDir = SoldierSO.SoldierDirection.Up;
        }
        else if (this.combatStanceDir == BaseEnemy.EnemyDirection.Right) {

            this.activeFacingDir = SoldierSO.SoldierDirection.Left;
            ChangedLeftDir?.Invoke(this, EventArgs.Empty);
        }
        else if (this.combatStanceDir == BaseEnemy.EnemyDirection.Left) {

            this.activeFacingDir = SoldierSO.SoldierDirection.Right;
            ChangedRightDir?.Invoke(this, EventArgs.Empty);
        }

        // After move to new Guard Pos
        yield return null;

        isWalking = false;
        isMovingToNewGuardPos = false;
    }

    private IEnumerator LockedIntentTargetCoroutine(BaseEnemy intentTarget) {

        isWalking = true;

        Vector2 checkBoxSize = new Vector2(0.41f, 0.41f);
        float minDistanctToEnemy = 0.2f;
        LayerMask canAttackLayer = guardianTower.GetGuardianTowerSO().canAttackLayer;

        float sqrDistance = (this.transform.position - intentTarget.transform.position).sqrMagnitude;

        while (sqrDistance > minDistanctToEnemy * minDistanctToEnemy) {


            float sqrDistanceFromTowerToEnemy = (intentTarget.transform.position - guardianTower.transform.position).sqrMagnitude;
            float attackRange = guardianTower.GetCurrentAttackZone();

            if (intentTarget.IsCantAttack() || intentTarget.GetComponent<ICanAttackPhysic>().HasLockedTarget() || sqrDistanceFromTowerToEnemy >= attackRange * attackRange) {
                // Nếu Intent Target không thể còn bị Attack hoặc bị Locked Target bởi Guardian khác hoặc rời khỏi attackRange

                ChangeGuardianState(GuadianState.Idle);

                yield break;

            }

            Collider2D[] enemyDetectedArray = Physics2D.OverlapBoxAll(this.transform.position, checkBoxSize, angle: 0f, canAttackLayer);

            if (enemyDetectedArray.Length > 0) {
                // Nếu phát hiện kẻ địch

                if (enemyDetectedArray[0].TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                    if (baseEnemy.TryGetComponent<ICanAttackPhysic>(out ICanAttackPhysic enemy)) {

                        if (!enemy.HasLockedTarget()) {
                            // Nếu enemy chưa bị Locked bởi Guardian nào

                            currentTarget = baseEnemy;
                            enemy.SetLockedTarget(isLockedTarget: true, this); // Thông báo cho Enemy biết bản thân đã bị khóa target bởi guardian này

                            ChangeGuardianState(GuadianState.Attack);

                            yield break;
                        }

                    }
                }

            }

            // Nếu không phát hiện kẻ địch
            Vector3 moveDir = (intentTarget.transform.position - this.transform.position).normalized;
            this.transform.position += moveDir * moveSpeed * Time.deltaTime;
            ChangedActiveFacingDirTo(intentTarget.transform.position);

            yield return null;  
        }

        // Nếu đi tới đây --> Lỗi tính toán hoặc lỗi dấu phẩy động
        ChangeGuardianState(GuadianState.Idle);
    }

    private IEnumerator AttackCoroutine() {

        float attackSpeed = guardianTower.GetCurrentTowerStatus().attackSpeed;
        float attackTimer = attackSpeed;
        float attackRange = guardianTower.GetCurrentAttackZone();
        float delayAttackAnimTimer = 0.4f;
        
        float attackDamage = guardianTower.GetCurrentTowerStatus().attackDamage;
        float damageBonusRate = guardianTower.GetGuardianTowerSO().damageBonusRate;

        if (currentTarget.GetEnemyCurrentDirection() == this.combatStanceDir) {
            // Nếu chung 1 lane với Guardian -> thêm 25% damage bonus

            attackDamage += attackDamage * (damageBonusRate / 100);
        }


        while (!currentTarget.IsCantAttack()) {

            // 1. Kiểm tra xem guardian còn sống không
            if (guardianLifeControl.IsDeath()) {

                yield break;
            }

            // 2 . Kiểm tra khoảng cách với enemy
            float sqrDistanceToEnemy = (this.transform.position - currentTarget.transform.position).sqrMagnitude;

            if (sqrDistanceToEnemy > attackRange * attackRange) {
                // If target move left attack range

                if (currentTarget.TryGetComponent<ICanAttackPhysic>(out ICanAttackPhysic enemy)) {
                    enemy.SetLockedTarget(false, null);
                }

                ChangeGuardianState(GuadianState.Idle);

                yield break;
            }

            float maxDistanceToEnemy = 1f;
            if (sqrDistanceToEnemy > maxDistanceToEnemy * maxDistanceToEnemy) {
                // If target move left melee range

                CommanMoveToIntentTarget(currentTarget);

                yield break;
            }

            ChangedActiveFacingDirTo(currentTarget.transform.position);

            OnAttackAnim?.Invoke(this, EventArgs.Empty);

            yield return new WaitForSeconds(delayAttackAnimTimer); // Wait attack animation

            if (currentTarget.IsCantAttack()) {
                
                break;
            }


            if ((this.transform.position - currentTarget.transform.position).sqrMagnitude > attackRange * attackRange) {
                // If target move left attack range

                if (currentTarget.TryGetComponent<ICanAttackPhysic>(out ICanAttackPhysic enemy)) {
                    enemy.SetLockedTarget(false, null);
                }

                ChangeGuardianState(GuadianState.Idle);

                yield break;
            }

            if (sqrDistanceToEnemy > maxDistanceToEnemy * maxDistanceToEnemy) {
                // If target move left melee range

                CommanMoveToIntentTarget(currentTarget);
                yield break;
            }


            currentTarget.HitDamage(attackDamage);

            yield return new WaitForSeconds(attackSpeed - delayAttackAnimTimer);
        }

        if (currentTarget.IsCantAttack()) {
            ChangeGuardianState(GuadianState.Idle);
        }
    }

    private void ChangeCombatStandDir(Vector3 rallyPos) {

        GridNode gridNode = GridManager.Instance.GetNodeAt(rallyPos);

        if (GridManager.Instance.GetPathNodeDirDict()[gridNode] == PathGenerator.PathDirection.Left) {

            this.combatStanceDir = BaseEnemy.EnemyDirection.Left;
        }
        else if (GridManager.Instance.GetPathNodeDirDict()[gridNode] == PathGenerator.PathDirection.Right) {

            this.combatStanceDir = BaseEnemy.EnemyDirection.Right;
        }
        else if (GridManager.Instance.GetPathNodeDirDict()[gridNode] == PathGenerator.PathDirection.Down) {

            this.combatStanceDir = BaseEnemy.EnemyDirection.Down;
        }
        else {

            this.combatStanceDir = BaseEnemy.EnemyDirection.Up;
        }
    }

    private void ChangedActiveFacingDirTo(Vector3 targetPos) {

        Vector3 moveDir = targetPos - this.transform.position;

        if (Mathf.Abs(moveDir.x) >= Mathf.Abs(moveDir.y)) {
            // Đang đi ngang

            if (moveDir.x < 0) {
                // Turn Left

                if (this.activeFacingDir != SoldierSO.SoldierDirection.Left) {

                    this.activeFacingDir = SoldierSO.SoldierDirection.Left;

                    ChangedLeftDir?.Invoke(this, EventArgs.Empty);

                }

            }
            else if (moveDir.x > 0) {
                // Turn Right

                if (this.activeFacingDir != SoldierSO.SoldierDirection.Right) {

                    this.activeFacingDir = SoldierSO.SoldierDirection.Right;

                    ChangedRightDir?.Invoke(this, EventArgs.Empty);

                }
            }


        }
        else {
            // Đang đi dọc

            if (moveDir.y < 0) {
                // Turn Down

                if (this.activeFacingDir != SoldierSO.SoldierDirection.Down) {

                    this.activeFacingDir = SoldierSO.SoldierDirection.Down;
                }

            }
            else if (moveDir.y > 0) {
                // Turn Up

                if (this.activeFacingDir != SoldierSO.SoldierDirection.Up) {

                    this.activeFacingDir = SoldierSO.SoldierDirection.Up;

                }
            }
        }
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }


    public void CommandMoveToGuardPos(Vector3? guardPos = null, Vector3? rallyPos = null) {

        if (guardianLifeControl.IsDeath()) {
            return;
        }

        Show();

        if (guardPos.HasValue && rallyPos.HasValue) {
            // If move to new guard pos

            this.currentGuardPos = guardPos.Value;
            ChangeCombatStandDir(rallyPos.Value);
        }

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        ChangeGuardianState(GuadianState.Guarding);
        currentCoroutine = StartCoroutine(MoveToGuardPosCoroutine(this.currentGuardPos));
    }

    public void CommanMoveToIntentTarget(BaseEnemy intentTarget) {

        Debug.Log("Move to intent target");

        if (guardianLifeControl.IsDeath()) {
            return;
        }

        if (isMovingToNewGuardPos) { return; }

        this.intentTarget = intentTarget;

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        ChangeGuardianState(GuadianState.LockedIntentTarget);
        currentCoroutine = StartCoroutine(LockedIntentTargetCoroutine(intentTarget));
    }

    public bool IsHasTarget() {
        return currentTarget != null || intentTarget != null;
    }

    public bool IsHasGuardPosBefore() {
        return this.currentGuardPos != Vector3.zero;
    }

    public bool IsWalking() {
        return this.isWalking;
    }

    public BaseEnemy GetCurrentIntentTarget() {
        return this.intentTarget;
    }

    public GuardianTower GetGuardianTower() {
        return this.guardianTower;
    }

    public BaseEnemy.EnemyDirection GetCombatStandDir() {
        return this.combatStanceDir;
    }

    public Vector3 GetCurrentGuardPos() {
        return this.currentGuardPos;
    }

    public SoldierSO.SoldierDirection GetActiveFacingDir() {
        return this.activeFacingDir;
    }

    public GuardianLifeControl GetGuardianLifeControl() {
        return this.guardianLifeControl;
    }

    public GuadianState GetCurrentGuadianState() {
        return this.currentGuadianState;
    }

    public static Guardian SpawnGuardian(Transform guardianPrefab, GuardianTower parent) {

        Transform guardianTransform = Instantiate(guardianPrefab, parent.transform);
        guardianTransform.localPosition = Vector3.zero;

        Guardian guardian = guardianTransform.GetComponent<Guardian>();

        guardian.guardianTower = parent;

        return guardian;
    }
}
