using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour
{

    [SerializeField] private float distanceCheck;
    [SerializeField] private Transform arrowVisual;

    private Archer archerShooting;
    private ArcherTower archerTowerParent;

    private Coroutine currentCoroutine;

    private Vector3 spawnPoint;

    private float moveSpeed;

    private void Awake() {

        // After Spawn
        spawnPoint = this.transform.position;

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

    private void SetArcherShooting(Archer archerShooting) {
        this.archerShooting = archerShooting;
    }

    private void SetArcherTower(ArcherTower archerTower) {
        this.archerTowerParent = archerTower;
    }
    
    private IEnumerator MovementCoroutine(Vector3 targetPosition) {

        LayerMask canAttackLayer = this.archerTowerParent.GetArcherTowerSO().canAttackLayer;
        float damage = this.archerTowerParent.GetCurrentTowerStatus().attackDamage;
        float fartheastDistance = this.archerTowerParent.GetCurrentAttackRange() / 100f * 110f;

        // 0. Before move 
        Vector3 startPoint = this.archerShooting.transform.position;
        this.transform.position = startPoint;

        // 1. Start point
        Vector2 dirToTarget = (targetPosition - this.transform.position).normalized;

        // Couting angle of arrow
        float zAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        arrowVisual.rotation = Quaternion.Euler(0f, 0f, zAngle);

        yield return null;

        // 2. Movement progress

        float sqrDistance = (this.transform.position - targetPosition).sqrMagnitude;
        Vector3 moveDir = (targetPosition - this.transform.position).normalized;

        while (sqrDistance >= 0.1f * 0.1f) {

            transform.position += moveDir * moveSpeed * Time.deltaTime;

            sqrDistance = (this.transform.position - targetPosition).sqrMagnitude;

            RaycastHit2D enemyCollide = Physics2D.Raycast(transform.position, dirToTarget, distanceCheck, canAttackLayer);

            if (enemyCollide) {
                // If interact with enemy

                if (enemyCollide.collider.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                    baseEnemy.HitDamage(damage);

                }

                break;
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

    public void StartMovement(Vector3 targetPosition, float arrowSpeed) {

        if (currentCoroutine != null) {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }

        Show();

        currentCoroutine = StartCoroutine(MovementCoroutine(targetPosition));

    }

    public static Arrow SpawnArrow(Transform arrowPrefab, ArcherTower archerTower, Archer archerShooting) {

        Transform arrowTransform = Instantiate(arrowPrefab, archerTower.transform);
        arrowTransform.localPosition = Vector3.zero;

        Arrow arrow = arrowTransform.GetComponent<Arrow>();

        arrow.archerShooting = archerShooting;
        arrow.archerTowerParent = archerTower;
        arrow.moveSpeed = archerTower.GetArcherTowerSO().arrowSpeed;

        return arrow;

    }

    // ----- VISUALIZE -----
    private void OnDrawGizmos()
    {
        if (!gameObject.activeSelf) return;

        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * distanceCheck);
    }
}
