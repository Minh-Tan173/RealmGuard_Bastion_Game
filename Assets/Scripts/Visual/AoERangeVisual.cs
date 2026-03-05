using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AoERangeVisual : MonoBehaviour
{
    public enum IconType {
        Tower,
        Ability
    }

    [SerializeField] private Transform validVisual;
    [SerializeField] private Transform unvalidVisual;
    [SerializeField] private IconType iconType;

    private GridNode currentNode;

    private void Awake() {
        currentNode = null;
    }

    private void Start() {
        validVisual.gameObject.SetActive(false);
        unvalidVisual.gameObject.SetActive(false);
    }

    private void Update() {

        GridNode gridNode = GridManager.Instance.GetNodeAt(this.transform.position);

        if (gridNode == null) {
            // If outsize gridmap

            validVisual.gameObject.SetActive(false);
            unvalidVisual.gameObject.SetActive(false);

            if (currentNode != null) {
                currentNode = null;
            }

            return;
        }

        if (currentNode != gridNode) {

            currentNode = gridNode;

            // Update Visual
            if (IsValid(gridNode)) {

                ShowValidVisual();
            }
            else {
                ShowUnvalidVisual();
            }

        }

    }

    private bool IsValid(GridNode gridNode) {

        switch (iconType) {

            case IconType.Tower:
                return !gridNode.HasItemOn() && gridNode.GetGridID() == IGridNode.Grid.Floor;

            case IconType.Ability:

                return !gridNode.HasItemOn() && gridNode.GetGridID() == IGridNode.Grid.Path;

            default:
                break;

        }

        // If dont match any case
        return false;
    }

    private void ShowValidVisual() {
        validVisual.gameObject.SetActive(true);
        unvalidVisual.gameObject.SetActive(false);
    }
     
    private void ShowUnvalidVisual() {
        validVisual.gameObject.SetActive(false);
        unvalidVisual.gameObject.SetActive(true);
    }

    public void SetAoERange(float aoeRange) {
        this.transform.localScale = new Vector3(aoeRange * 2f, aoeRange * 2f, 1f);
    }
}
