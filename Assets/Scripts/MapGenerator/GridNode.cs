using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GridNode : IGridNode
{
    #region Node data
    [SerializeField] private Vector2Int nodePosition;
    [SerializeField] private IGridNode.Grid gridID;
    [SerializeField] private int index;
    #endregion

    #region Node state
    [SerializeField] private bool hasItemOn;
    #endregion

    public GridNode (Vector2Int nodePosition, IGridNode.Grid gridID, int index) {

        this.nodePosition = nodePosition;
        this.gridID = gridID;
        this.index = index;

        this.hasItemOn = false;
    }

    public void SetGridID(IGridNode.Grid gridID) {
        this.gridID = gridID;
    }

    public void SetHasItemOn(bool state) {
        hasItemOn = state;
    }

    public int GetIndex() {
        return this.index;
    }

    public IGridNode.Grid GetGridID() {
        return this.gridID;
    }

    public Vector2Int GetNodePosition() {
        return this.nodePosition;
    }

    public bool HasItemOn() {
        return hasItemOn;
    }


}
