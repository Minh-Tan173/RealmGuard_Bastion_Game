using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    //public event EventHandler LoadSavedFile;

    [Header("Generator")]
    [SerializeField] private BackgroundGenerator backgroundGenerator;
    [SerializeField] private GroundGenerator groundGenerator;
    [SerializeField] private PathGenerator pathGenerator;

    [Header("Tilemap")]
    [SerializeField] private Tilemap mainTileMap;    

    [Header("Visual Node 1 * 1")]
    [SerializeField] private Transform validNode;
    [SerializeField] private Transform unValidNode;

    [Header("Visual Node 2 * 2")]
    [SerializeField] private Transform validBigNode;
    [SerializeField] private Transform undValidBigNode;

    private SavedGridMap savedGridMap;
    private List<GridNode> gridMap;
    private GridNode currentNode;

    private Dictionary<GridNode, PathGenerator.PathDirection> pathNodeDirDict;
    private Dictionary<Transform, float> waypointCumulativeDistDict;

    private bool isValidNode;

    private bool hasLoadSavedFile;

    private void Awake() {

        Instance = this;

        pathNodeDirDict = new Dictionary<GridNode, PathGenerator.PathDirection>();
        waypointCumulativeDistDict = new Dictionary<Transform, float>();

        // Khởi tạo gridMap có kích thước mapWidth * mapHeight
        gridMap = new List<GridNode>();

        currentNode = null;

        hasLoadSavedFile = false;

        // Load saved if have
        if (LevelManager.Instance.GetLevelManagerSO().gameMode == ILevelManager.GameMode.Campain) {
            
            this.savedGridMap = LoadSavedMapByBiome(ILevelManager.BiomeType.Forest);

            this.gridMap = this.savedGridMap.gridMap;
            hasLoadSavedFile = true;
        }
    }

    private void Start() {

        validNode.gameObject.SetActive(false);
        unValidNode.gameObject.SetActive(false);
        validBigNode.gameObject.SetActive(false);
        undValidBigNode.gameObject.SetActive(false);

        PathGenerator.Instance.PathCreateDone += PathGenerator_PathCreateDone;
        PathGenerator.Instance.PathVisualShowAll += Instance_PathVisualShowAll;

    }


    private void OnDestroy() {
        PathGenerator.Instance.PathCreateDone -= PathGenerator_PathCreateDone;
        PathGenerator.Instance.PathVisualShowAll -= Instance_PathVisualShowAll;
    }

    private void Instance_PathVisualShowAll(object sender, EventArgs e) {
        //  Create waypointCumulativeDistDict
        List<Transform> wayPointList = pathGenerator.GetWaypointList();

        waypointCumulativeDistDict.Add(wayPointList[0], 0f); // Mặc định distance to first waypoint == 0f

        for (int i = 1; i < wayPointList.Count; i++) {

            if (!waypointCumulativeDistDict.ContainsKey(wayPointList[i])) {

                float distanceToWaypoint;

                if (i == 1) {
                    // Is Second Waypoint

                    distanceToWaypoint = Vector3.Distance(wayPointList[0].position, wayPointList[i].position);
                }
                else {
                    // Is other Waypoin

                    distanceToWaypoint = waypointCumulativeDistDict[wayPointList[i - 1]] + Vector3.Distance(wayPointList[i - 1].position, wayPointList[i].position);
                }


                waypointCumulativeDistDict.Add(wayPointList[i], distanceToWaypoint);
            }
        }
    }

    private void PathGenerator_PathCreateDone(object sender, EventArgs e) {
        // After path generated done

        // Create pathNodeDirDict
        List<NodePath2x2> nodePath2X2List = PathGenerator.Instance.GetNodePath2X2List();

        foreach (NodePath2x2 nodePath2X2 in nodePath2X2List) {

            Vector2Int centerPos = nodePath2X2.centerPos;
            Vector2Int downRightPos = new Vector2Int(centerPos.x, centerPos.y - 1);
            Vector2Int upLeftPos = new Vector2Int(centerPos.x - 1, centerPos.y);
            Vector2Int downLeftPos = new Vector2Int(centerPos.x - 1, centerPos.y - 1);

            GridNode upRightNode = GetNode(centerPos);
            GridNode downRightNode = GetNode(downRightPos);
            GridNode upLeftNode = GetNode(upLeftPos);
            GridNode downLeftNode = GetNode(downLeftPos);

            pathNodeDirDict.Add(upRightNode, nodePath2X2.currentDirection);
            pathNodeDirDict.Add(downRightNode, nodePath2X2.currentDirection);
            pathNodeDirDict.Add(upLeftNode, nodePath2X2.currentDirection);
            pathNodeDirDict.Add(downLeftNode, nodePath2X2.currentDirection);
        }

    }

    private void Update() {

        if (LevelManager.Instance.GetCurrentLevelState() == LevelManager.LevelState.GameRunning || LevelManager.Instance.GetCurrentLevelState() == LevelManager.LevelState.EndTurn) {


            if (BuildingManager.Instance.IsPlacingTower()) {

                UpdateHightLightVisualWhenPlacingTower();

            }
            else if (BuildingManager.Instance.IsPlacingAbility()) {

                UpdateHightLightVisualWhenPlacingAbility();

            }
            else {
                if (validNode.gameObject.activeSelf || unValidNode.gameObject.activeSelf || validBigNode.gameObject.activeSelf || undValidBigNode.gameObject.activeSelf) {

                    validNode.gameObject.SetActive(false);
                    unValidNode.gameObject.SetActive(false);
                    validBigNode.gameObject.SetActive(false);
                    undValidBigNode.gameObject.SetActive(false);

                    currentNode = null;
                }
            }

        }

        
    }

    private void UpdateHightLightVisualWhenPlacingTower() {
        // Only Happen When Placing Tower

        Vector3 mouseClickedPosition = GameInput.Instance.GetMouseWorldPos();
        float offset = 0.5f;

        Vector3 nodeCheckPos = mouseClickedPosition - new Vector3(offset, offset, 0f);

        GridNode gridNode = GetNodeAt(nodeCheckPos);

        if (gridNode == null) {
            // Outside Gridmap

            currentNode = null;

            validNode.gameObject.SetActive(false);
            unValidNode.gameObject.SetActive(false);
            validBigNode.gameObject.SetActive(false);
            undValidBigNode.gameObject.SetActive(false);

            return;
        }

        if (currentNode == gridNode) {
            //Inside Gridmap and not move Mouse to new gridNode
            return;
        }

        // Inside Gridmap and move Mouse to new gridNode
        if (currentNode != gridNode) {

            currentNode = gridNode;

            if (IsAreaValid2x2(gridNode)) {

                ShowValidBigNode();
                validBigNode.transform.position = NodePosConvertToWordPos(gridNode.GetNodePosition()) + new Vector3(0.5f, 0.5f, 0f);

            }
            else {

                ShowUnValidBigNode();
                undValidBigNode.transform.position = NodePosConvertToWordPos(gridNode.GetNodePosition()) + new Vector3(0.5f, 0.5f, 0f);

            }

        }


    }

    private void UpdateHightLightVisualWhenPlacingAbility() {
        // Only Happen When Placing Ability

        // Lấy vị trí Mouse hiện tại kiểm tra với currentNode
        Vector3 mousePosition = GameInput.Instance.GetMouseWorldPos();
        Vector2Int mousePositionInTileMap = WorldPosConvertToCellPos(mousePosition);

        GridNode gridNode = GetNode(mousePositionInTileMap);

        if (gridNode == null) {
            // Outside Gridmap

            currentNode = null;

            validNode.gameObject.SetActive(false);
            unValidNode.gameObject.SetActive(false);
            validBigNode.gameObject.SetActive(false);
            undValidBigNode.gameObject.SetActive(false);
        }

        if (currentNode == gridNode) {
            //Inside Gridmap and not move Mouse to new gridNode
            return;
        }

        // Inside Gridmap and move Mouse to new gridNode
        if (currentNode != gridNode) {

            currentNode = gridNode;

            if (gridNode.GetGridID() == IGridNode.Grid.Path) {

                ShowValidNode();
                validNode.transform.position = NodePosConvertToWordPos(mousePositionInTileMap);
            }
            else {

                ShowUnValidNode();
                unValidNode.transform.position = NodePosConvertToWordPos(mousePositionInTileMap);

            }

        }
    }

    public bool IsAreaValid2x2(GridNode leftDownNode) {
        if (leftDownNode == null) return false;

        Vector2Int anchorPos = leftDownNode.GetNodePosition();

        for (int x = 0; x <= 1; x++) {
            for (int y = 0; y <= 1; y++) {
                Vector2Int checkPos = new Vector2Int(anchorPos.x + x, anchorPos.y + y);
                GridNode nodeToCheck = GetNode(checkPos);

                if (nodeToCheck == null || nodeToCheck.GetGridID() != IGridNode.Grid.Floor || nodeToCheck.HasItemOn()) {
                    // If any node in area not valid

                    return false;
                }
            }
        }
        return true;
    }

    private void ShowValidNode() {
        // Node 1 * 1
        validNode.gameObject.SetActive(true);
        unValidNode.gameObject.SetActive(false);

        // Node 2 * 2
        validBigNode.gameObject.SetActive(false);
        undValidBigNode.gameObject.SetActive(false);
    }

    private void ShowUnValidNode() {
        // Node 1 * 1
        validNode.gameObject.SetActive(false);
        unValidNode.gameObject.SetActive(true);

        // Node 2 * 2
        validBigNode.gameObject.SetActive(false);
        undValidBigNode.gameObject.SetActive(false);
    }

    private void ShowValidBigNode() {
        // Node 1 * 1
        validNode.gameObject.SetActive(false);
        unValidNode.gameObject.SetActive(false);

        // Node 2 * 2
        validBigNode.gameObject.SetActive(true);
        undValidBigNode.gameObject.SetActive(false);
    }

    private void ShowUnValidBigNode() {
        // Node 1 * 1
        validNode.gameObject.SetActive(false);
        unValidNode.gameObject.SetActive(false);

        // Node 2 * 2
        validBigNode.gameObject.SetActive(false);
        undValidBigNode.gameObject.SetActive(true);
    }

    private SavedGridMap LoadSavedMapByBiome(ILevelManager.BiomeType biome) {
        
        if (biome == ILevelManager.BiomeType.Forest) {

            ILevelManager.GameLevel gameLevel = LevelManager.Instance.GetLevelManagerSO().gameLevel;
            
            List<SavedGridMap> savedGridMapList = new List<SavedGridMap>();

            savedGridMapList = GeneratedGridMapSaveData.GetForestBiomeMapCollectionByLevel(gameLevel);

            return savedGridMapList[UnityEngine.Random.Range(0, savedGridMapList.Count)];
        }

        if (biome == ILevelManager.BiomeType.River) {
        }

        if (biome == ILevelManager.BiomeType.Graveyard) {
        }

        if (biome == ILevelManager.BiomeType.Swamp) {

        }

        return null;
    }

    public List<GridNode> GetGridMap() {
        return this.gridMap;
    }

    public SavedGridMap GetSavedGridMap() {
        return this.savedGridMap;
    }

    public GridNode GetNodeAtPoinClicked() {

        Vector2Int cellClickedPosition = WorldPosConvertToCellPos(GameInput.Instance.GetMouseWorldPos());
        return GetNode(cellClickedPosition);

    }

    public GridNode GetNodeAt(Vector3 itemPosition) {

        Vector2Int cellAtItemPosition = WorldPosConvertToCellPos(itemPosition);
        return GetNode(cellAtItemPosition);
    }

    public GridNode GetNode(Vector2Int nodePosition) {

        if (nodePosition.x < 0 || nodePosition.x >= GetMapWidth() || nodePosition.y < 0 || nodePosition.y >= GetMapHeight()) { return null; }

        return gridMap[nodePosition.y + nodePosition.x * GetMapHeight()];
    }

    public int GetMapHeight() {
        return groundGenerator.GetGroundSO().mapHeight;
    }

    public int GetMapWidth() {
        return groundGenerator.GetGroundSO().mapWidth;
    }

    public BackgroundGenerator GetBackgroundGenerator() {
        return this.backgroundGenerator;
    }

    public GroundGenerator GetGroundGenerator() {
        return this.groundGenerator;
    }

    public PathGenerator GetPathGenerator() {
        return this.pathGenerator;
    }

    public bool HasLoadSavedFile() {
        return this.hasLoadSavedFile;
    }
    
    public Dictionary<GridNode, PathGenerator.PathDirection> GetPathNodeDirDict() {
        return this.pathNodeDirDict;
    }

    public Dictionary<Transform, float> GetWaypointCumulativeDistDict() {
        return waypointCumulativeDistDict;
    }

    public Vector2Int WorldPosConvertToCellPos(Vector3 wordPosition) {

        Vector3Int cellPosition = mainTileMap.WorldToCell(wordPosition);

        return new Vector2Int(cellPosition.x, cellPosition.y);

    }

    public Vector3 NodePosConvertToWordPos(Vector2Int cellPosition) {
        // Trả về tọa độ center của GridNode trong không gian thế giới

        Vector3 wordPosition = mainTileMap.GetCellCenterWorld(new Vector3Int(cellPosition.x, cellPosition.y, 0));

        return wordPosition;

    }

    public void SetHasItemArea2x2(Vector3 centerPosition, bool isSetHasItemON) {

        GridNode nodeTopRight = GetNodeAt(centerPosition); // Mặc định center là position của node Phải Trên

        Vector2Int nodeTopLeftPos = new Vector2Int(nodeTopRight.GetNodePosition().x - 1, nodeTopRight.GetNodePosition().y);
        Vector2Int nodeDownLeftPos = new Vector2Int(nodeTopRight.GetNodePosition().x - 1, nodeTopRight.GetNodePosition().y - 1);
        Vector2Int nodeDownRightPos = new Vector2Int(nodeTopRight.GetNodePosition().x, nodeTopRight.GetNodePosition().y - 1);

        nodeTopRight.SetHasItemOn(isSetHasItemON);
        GetNode(nodeTopLeftPos).SetHasItemOn(isSetHasItemON);
        GetNode(nodeDownLeftPos).SetHasItemOn(isSetHasItemON);
        GetNode(nodeDownRightPos).SetHasItemOn(isSetHasItemON);
    }

}
