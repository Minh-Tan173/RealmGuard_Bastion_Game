using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBolt : MonoBehaviour
{

    [SerializeField] private MagicBoltVisual magicBoltVisual;

    [Header("Scale Rate")]
    [SerializeField] private float minScaleRate;
    [SerializeField] private float minDamageRate;

    private Mage mageShooting;
    private MageTower mageTowerParent;

    private Coroutine currentCoroutine;

    #region Movement Behavior
    private float moveSpeed;
    #endregion

    #region Attack Strategic;
    private List<RaycastHit2D> enemyDetectedList;
    private HashSet<Transform> enemyAttackedHast;
    private ContactFilter2D filter2D;
    #endregion 

    private void Awake() {

        enemyDetectedList = new List<RaycastHit2D>();
        enemyAttackedHast = new HashSet<Transform>();

        filter2D.useTriggers = true;
        filter2D.useLayerMask = true;

    }

    private void Start() {

        // After spawn
        Hide();
    }

    private void Show() {

        this.gameObject.SetActive(true);

    }

    public void Hide() {

        this.transform.localPosition = Vector3.zero;

        this.gameObject.SetActive(false);
    }


    private IEnumerator MovementCoroutine(Vector3 targetPosition, float timeToHitEnemy) {

        LayerMask currentAttackLayer = this.mageTowerParent.GetCurrentAttackLayer();
        float damage = this.mageTowerParent.GetCurrentTowerStatus().attackDamage;
        float startDamage = damage * (minDamageRate / 100f);
        float fartheastDistance = this.mageTowerParent.GetCurrentAttackRange() / 100f * 110f;

        float maxSize = this.transform.localScale.x;
        float minSize = this.transform.localScale.x * (minScaleRate / 100f);
       

        // 0. Before move 
        Vector3 startPoint = this.mageShooting.transform.position;
        this.transform.position = startPoint;
        float timeMoveElapsed = 0f;

        this.filter2D.SetLayerMask(currentAttackLayer);

        enemyDetectedList.Clear();
        enemyAttackedHast.Clear();

        // 1. Couting angle of magicBolt
        Vector3 dirToTarget = (targetPosition - this.transform.position).normalized;
        float zAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        magicBoltVisual.transform.rotation = Quaternion.Euler(0f, 0f, zAngle);

        yield return null;

        // 2. Movement progress

        float sqrDistance = (this.transform.position - targetPosition).sqrMagnitude;
        Vector3 moveDir = (targetPosition - this.transform.position).normalized;

        // ----- CASE 1: Single Strategic ----- 
        //while (sqrDistance >= 0.1f * 0.1f) {

        //    transform.position += moveDir * projectileSpeed * Time.deltaTime;

        //    sqrDistance = (this.transform.position - targetPosition).sqrMagnitude;

        //    Collider2D[] enemyDetectedArray = Physics2D.OverlapBoxAll(transform.position, new Vector2(currentSizeCheck, currentSizeCheck), angle: 0f, canAttackLayer);

        //    Collider2D enemyDetected;
        //    if (enemyDetectedArray.Length > 0) {
        //        // Luôn luôn ưu tiên chọn enemy đầu tiên detect được
        //        enemyDetected = enemyDetectedArray[0];

        //        if (enemyDetected.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {
        //            baseEnemy.HitDamage(damage);
        //        }

        //        break;
        //    }


        //    float movedDistance = (this.transform.position - startPoint).sqrMagnitude;
        //    if (movedDistance > fartheastDistance * fartheastDistance) {
        //        // If moved out side attackRange

        //        break;

        //    }

        //    yield return null;

        //}


        // ---- CASE 2: Growing Pierce Strategy -----
        while (sqrDistance >= 0.1f * 0.1f) {

            timeMoveElapsed += Time.deltaTime;

            // 1. Tính quãng đường đi được trong Frame hiện tại
            float stepDistance = moveSpeed * Time.deltaTime;

            // 2. Di chuyển magicBolt và setup Visual
            this.transform.position += moveDir * moveSpeed * Time.deltaTime;
            sqrDistance = (this.transform.position - targetPosition).sqrMagnitude;

            float progress = Mathf.Clamp01(timeMoveElapsed / timeToHitEnemy);
            float smoothT = Mathf.SmoothStep(minSize, maxSize, progress);
            this.transform.localScale = new Vector3(smoothT, smoothT, smoothT);

            enemyDetectedList.Clear();

            // 3. Quét ngược lại để truy xem trong 1 frame di chuyển trước đó có tìm được enemy nào không - "quét dựa trên quãng đường thực tế"
            Physics2D.Raycast(this.transform.position, -moveDir, filter2D, enemyDetectedList, stepDistance);

            if (enemyDetectedList.Count > 0) {

                List<BaseEnemy> enemyNotAttacked = new List<BaseEnemy>();

                foreach (RaycastHit2D enemyDetected in enemyDetectedList) {

                    if (enemyAttackedHast.Contains(enemyDetected.collider.transform)) {
                        // if this enemy was attacked before
                        continue;
                    }

                    enemyNotAttacked.Add(enemyDetected.collider.GetComponent<BaseEnemy>());
                }

                if (enemyNotAttacked.Count > 0) {

                    foreach (BaseEnemy baseEnemy in enemyNotAttacked) {

                        float currentDamage = Mathf.Lerp(startDamage, damage, progress);
                        baseEnemy.HitDamage(currentDamage);

                        enemyAttackedHast.Add(baseEnemy.transform);
                    }
                }
            }

            float movedDistance = (this.transform.position - startPoint).sqrMagnitude;
            if (movedDistance > fartheastDistance * fartheastDistance) {
                // If moved out side attackRange

                break;

            }

            yield return null;
        }

        // 3. End point
        Hide();

    }

    public void StartMovement(Vector3 targetPosition, float timeToHitEnemy) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        Show();

        // Setup sprite base on MageTower current type
        MageTowerSO.MageType mageType = mageTowerParent.GetCurrentMageType();

        currentCoroutine = StartCoroutine(MovementCoroutine(targetPosition, timeToHitEnemy));

    }

    public MageTower GetMageTower() {
        return this.mageTowerParent;
    }

    public Mage GetMageShooting() {
        return this.mageShooting;
    }

    public static MagicBolt SpawnSpellProjectile(Transform arrowPrefab, MageTower mageTower, Mage mageShooting) {

        Transform spellProjectileTransform = Instantiate(arrowPrefab, mageTower.transform);
        spellProjectileTransform.localPosition = Vector3.zero;

        MagicBolt magicBolt = spellProjectileTransform.GetComponent<MagicBolt>();

        magicBolt.mageTowerParent = mageTower;
        magicBolt.mageShooting = mageShooting;
        magicBolt.moveSpeed = mageTower.GetMageTowerSO().magicBoltSpeed;

        return magicBolt;

    }

    // ----- VISUALIZE -----
    private void OnDrawGizmos() {
        if (!gameObject.activeSelf) return;

        Gizmos.color = Color.yellow;
        Vector3 backDir = -magicBoltVisual.transform.right;
        float visualStep = 12f * Time.fixedDeltaTime;

        Gizmos.DrawRay(transform.position, backDir * visualStep);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.05f);

    }
}

