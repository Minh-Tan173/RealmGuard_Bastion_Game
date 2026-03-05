using DG.Tweening;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class AssignPositionZoneVisual : MonoBehaviour, IPointerDownHandler
{
    public event EventHandler OnMoveCommandSFX;
    public event EventHandler<OnMoveCommandEventArgs> OnMoveCommand;
    public class OnMoveCommandEventArgs : EventArgs {
        public Vector3 mouseClickedPos;
    }

    [SerializeField] private float duration;
    [SerializeField] private GuardianTower guardianTower;

    [Header("Clicked Visualize")]
    [SerializeField] private Transform flag;
    [SerializeField] private Transform warningNotify;

    private SpriteRenderer spriteRenderer;
    private Coroutine currentCoroutine;

    private void Awake() {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start() {

        guardianTower.OnAssignPositionZone += GuardianTower_OnAssignPositionZone;
        guardianTower.UnAssignPositionZone += GuardianTower_UnAssignPositionZone;

        // After Spawn
        this.transform.localScale = new Vector3(0f, 0f, 0f);

        warningNotify.gameObject.SetActive(false);
        HideFlag();
        Hide();
    }

    private void OnDestroy() {
        guardianTower.OnAssignPositionZone -= GuardianTower_OnAssignPositionZone;
        guardianTower.UnAssignPositionZone -= GuardianTower_UnAssignPositionZone;
    }

    private void GuardianTower_UnAssignPositionZone(object sender, System.EventArgs e) {

        if (currentCoroutine != null && flag.gameObject.activeSelf) {

            StopCoroutine(currentCoroutine);
            currentCoroutine = null;

            HideFlag();
        }

        // DOTween Scale
        this.transform.DOScale(0f, duration).SetEase(Ease.Linear).OnComplete(Hide);
    }

    private void GuardianTower_OnAssignPositionZone(object sender, System.EventArgs e) {

        // 1. 
        Show();

        this.transform.localScale = new Vector3(0f, 0f, 0f);

        // 2. Update size
        float attackRange = guardianTower.GetCurrentAttackZone() * 0.75f; // Kích thước vùng chọn chỉ = 3/4 vùng chọn thật

        // 3. DOTween Scale

        this.transform.DOScale(attackRange * 2f, duration).SetEase(Ease.Linear);
    }

    private IEnumerator FlagCoroutine(Vector3 mouseClickedPos) {

        float activeTimer = 1f;

        yield return new WaitForSeconds(activeTimer);

        HideFlag();
        flag.transform.localPosition = Vector3.zero;
    }

    private void Show() {
        this.gameObject.SetActive(true);
    }

    private void ShowFlag() {
        flag.gameObject.SetActive(true);
    }

    private void Hide() {
        this.gameObject.SetActive(false);
    }

    private void HideFlag() {
        flag.gameObject.SetActive(false);
    }

    private void ShowWarningNotify() {

        // 1. Setup position and visual
        warningNotify.transform.position = guardianTower.GetCurrentCenterGuardPos();
        warningNotify.gameObject.SetActive(true);
        warningNotify.DOKill();

        // 2. Setup sequence
        float duration = 0.2f;
        float showTimer = 0.3f;
        Sequence showNotiSequence = DOTween.Sequence();

        warningNotify.localScale = Vector3.zero;

        // 3. Sequence progress
        showNotiSequence.Append(warningNotify.DOScale(Vector3.one, duration).SetEase(Ease.OutBack));
        showNotiSequence.AppendInterval(showTimer);
        showNotiSequence.Append(warningNotify.DOScale(Vector3.zero, duration).SetEase(Ease.InQuad));
        showNotiSequence.OnComplete(() => {

            warningNotify.gameObject.SetActive(false);
        });

    }


    public void OnPointerDown(PointerEventData eventData) {
        
        if (eventData.button == PointerEventData.InputButton.Left) {
            // If left clicked on this zone

            GridNode gridNode = GridManager.Instance.GetNodeAtPoinClicked();

            if (gridNode.GetGridID() == IGridNode.Grid.Path) {
                // If clicked on PATH node

                Vector3 mouseClickedPos = GameInput.Instance.GetMouseWorldPos();
                OnMoveCommand?.Invoke(this, new OnMoveCommandEventArgs { mouseClickedPos = mouseClickedPos });

                OnMoveCommandSFX?.Invoke(this, EventArgs.Empty);

                // Show Flag
                if (currentCoroutine != null && flag.gameObject.activeSelf) {

                    StopCoroutine(currentCoroutine);
                    currentCoroutine = null;

                    HideFlag();
                }

                flag.transform.position = mouseClickedPos;
                ShowFlag();
                currentCoroutine = StartCoroutine(FlagCoroutine(mouseClickedPos));
            }
            else {
                // If not clicked on PATH node

                ShowWarningNotify();
            }
        }
        else {
            // If left clicked outside zone

            ShowWarningNotify();
        }

    }

    
}
