using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeBase : MonoBehaviour
{

    [SerializeField] private LayerMask canTakeDamageByLayer;

    private BoxCollider2D boxCollider;
    private bool hasTakeDamage;

    private void Awake() {

        boxCollider = GetComponent<BoxCollider2D>();
        hasTakeDamage = false;

    }

    private void Start() {

        PathGenerator.Instance.PathVisualShowAll += PathGenerator_PathVisualShowAll;

    }

    private void OnDestroy() {
        PathGenerator.Instance.PathVisualShowAll -= PathGenerator_PathVisualShowAll;
    }

    private void Update() {

        Collider2D enemy = Physics2D.OverlapBox(this.transform.position, boxCollider.size, angle: 0f, canTakeDamageByLayer);

        if (enemy != null) {

            if (enemy.TryGetComponent<BaseEnemy>(out BaseEnemy baseEnemy)) {

                if (!hasTakeDamage) {

                    hasTakeDamage = true;

                    TakeDame();
                }

            }

        }
        else {
            hasTakeDamage = false;
        }

    }

    private void PathGenerator_PathVisualShowAll(object sender, System.EventArgs e) {
        // After path create done

        Transform lastWayPoint = GridManager.Instance.GetPathGenerator().GetWaypointList()[GridManager.Instance.GetPathGenerator().GetWaypointList().Count - 1];

        this.transform.position = lastWayPoint.position;

    }

    private void TakeDame() {

        LevelManager.Instance.ChangedHeartTo(ILevelManager.HeartChangedState.Decrase);

    }
}
