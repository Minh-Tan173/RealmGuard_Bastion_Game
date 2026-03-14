using System;
using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

public class Pebble : MonoBehaviour
{
    public event EventHandler OnExplosionAnim;
    public event EventHandler OnResetAnim;

    [SerializeField] private Transform shadowVisual;

    [SerializeField] private float arcHeightFactor;
    [SerializeField] private PebbleVisual pebbleVisual;

    [SerializeField] private float testSizeChecked;

    private Catapult catapultShooting;
    private CatapultTower catapultTowerParent;

    private float currentRadiusCheck = 0f;
    private Vector3 spawnPoint;

    private void Awake() {

        // After Spawn
        spawnPoint = this.transform.position;

        Hide();
    }

    private void Start() {

        // After spawn
        //Hide();
    }

    private void Show() {

        this.gameObject.SetActive(true);

    }

    public void Hide() {

        this.transform.localPosition = Vector3.zero;

        OnResetAnim?.Invoke(this, EventArgs.Empty);

        this.gameObject.SetActive(false);
    }

    private void SetPebbleShooting(Catapult catapultShooting) {
        this.catapultShooting = catapultShooting;
    }

    private void SetPebbleTower(CatapultTower catapultTower) {
        this.catapultTowerParent = catapultTower;
    }

    private IEnumerator MovementCoroutine(Vector3 targetPosition) {

        float pebbleSpeed = this.catapultTowerParent.GetCurrentTowerStatus().pebbleSpeed;
        LayerMask canAttackLayer = this.catapultTowerParent.GetCatapultTowerSO().canAttackLayer;
        float damage = this.catapultTowerParent.GetCurrentTowerStatus().attackDamage;
        float fartheastDistance = this.catapultTowerParent.GetCurrentAttackRange() / 100f * 110f;

        Vector3 targetPos = new Vector3(targetPosition.x, targetPosition.y, 0f);

        // 0. Before move 
        Vector3 startPoint = this.catapultShooting.transform.position;
        this.transform.position = startPoint;

        yield return null;

        // 2. Movement progress

        float elapsedTime = 0f;
        float timeToTarget = Vector3.Distance(startPoint, targetPos) / pebbleSpeed; // t = S / v: công thức tính thời gian dựa vào quãng đường, vận tốc

        float heightMax = Vector3.Distance(startPoint, targetPos) * arcHeightFactor;

        while (elapsedTime <= timeToTarget) {

            elapsedTime += Time.deltaTime;

            float t = elapsedTime / timeToTarget;

            // Counting true position
            Vector3 groundPos = Vector3.Lerp(startPoint, targetPos, t);

            // Counting fake height position with heightMax
            float h = 4 * heightMax * t * (1 - t);

            // Fake position of pebble
            this.transform.position = new Vector3(groundPos.x, groundPos.y + h, 0f);

            yield return null;
        }

        // 3. When end of timeToTarget -> Explosion

        OnExplosionAnim?.Invoke(this, EventArgs.Empty);

        Collider2D[] enemyDetectedArray = Physics2D.OverlapCircleAll(targetPos, currentRadiusCheck, canAttackLayer);

        if (enemyDetectedArray.Length > 0) {

            foreach (Collider2D enemyDetected in enemyDetectedArray) {

                if (enemyDetected.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                    baseEnemy.HitDamage(damage);

                }

            }

        }

        // 4. After explosion
        yield return new WaitForSeconds(0.3f); // Wait for end of explosion animation
        Hide();

    }

    public void StartMovement(Vector3 targetPosition) {

        Show();

        // Setup Size base on Level
        currentRadiusCheck = GetSplashSize();

        StartCoroutine(MovementCoroutine(targetPosition));

    }

    public float GetSplashSize() {
        return catapultTowerParent.GetCurrentTowerStatus().pebbleSplashRadius;
    }

    public PebbleVisual GetPebbleVisual() {
        return this.pebbleVisual;
    }

    public static Pebble SpawnPebble(Transform pebblePrefab, CatapultTower catapultTower, Catapult catapultShooting) {

        Transform pebbleTransform = Instantiate(pebblePrefab, catapultTower.transform);
        pebbleTransform.localPosition = Vector3.zero;

        Pebble projectile = pebbleTransform.GetComponent<Pebble>();


        projectile.SetPebbleTower(catapultTower);
        projectile.SetPebbleShooting(catapultShooting);

        return projectile;

    }
    
    // ----- VISUALIZE -----
    private void OnDrawGizmos() {
        
        if (catapultTowerParent != null) {

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(this.transform.position, catapultTowerParent.GetCurrentTowerStatus().pebbleSplashRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.transform.position, testSizeChecked);
    }
}
