using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GridManager_GENSCENE : MonoBehaviour
{
    public static GridManager_GENSCENE Instance { get; private set; }

    [Header("Generator")]
    [SerializeField] private BackgroundGenerator_GENSCENE backgroundGenerator;
    [SerializeField] private GroundGenerator_GENSCENE groundGenerator;
    [SerializeField] private PathGenerator_GENSCENE pathGenerator;

    [Header("Tilemap")]
    [SerializeField] private Tilemap mainTileMap;

    [SerializeField] private Transform floorNode;
    [SerializeField] private Transform fenceNode;
    [SerializeField] private Transform pathNode;


    private List<GridNode> gridMap;
    private GridNode currentNode;

    private bool isChangedNode = false;
    public Vector3 mouseOnWorld;

    private void Awake() {

        Instance = this;

        // Khởi tạo gridMap có kích thước mapWidth * mapHeight
        gridMap = new List<GridNode>();

        currentNode = null;
    }

    private void Start() {

        floorNode.gameObject.SetActive(false);
        fenceNode.gameObject.SetActive(false);
        pathNode.gameObject.SetActive(false);
    }

    private void Update() {

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; // Camera ở z = -10

        mouseOnWorld = Camera.main.ScreenToWorldPoint(mousePos);
        mouseOnWorld.z = 0f;

        GridNode gridNode = GetNodeAt(mouseOnWorld);

        if (gridNode == null) {
            floorNode.gameObject.SetActive(false);
            fenceNode.gameObject.SetActive(false);
            pathNode.gameObject.SetActive(false);   
            return;
        }

        if (gridNode.GetGridID() == IGridNode.Grid.Floor) {
            ShowFloorNode(gridNode);
        }

        if (gridNode.GetGridID() == IGridNode.Grid.Path) {
            ShowPathNode(gridNode);
        }

        if (gridNode.GetGridID() == IGridNode.Grid.Fence) {
            ShowFenceNode(gridNode);
        }

    }

    private void ShowFloorNode(GridNode node) {
        floorNode.gameObject.SetActive(true);
        fenceNode.gameObject.SetActive(false);
        pathNode.gameObject.SetActive(false);

        floorNode.transform.position = NodePosConvertToWordPos(node.GetNodePosition());
    }

    private void ShowFenceNode(GridNode node) {
        floorNode.gameObject.SetActive(false);
        fenceNode.gameObject.SetActive(true);
        pathNode.gameObject.SetActive(false);

        fenceNode.transform.position = NodePosConvertToWordPos(node.GetNodePosition()); ;
    }

    private void ShowPathNode(GridNode node) {
        floorNode.gameObject.SetActive(false);
        fenceNode.gameObject.SetActive(false);
        pathNode.gameObject.SetActive(true);

        pathNode.transform.position = NodePosConvertToWordPos(node.GetNodePosition()); ;
    }


    public List<GridNode> GetGridMap() {
        return this.gridMap;
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

    public BackgroundGenerator_GENSCENE GetBackgroundGenerator() {
        return this.backgroundGenerator;
    }

    public GroundGenerator_GENSCENE GetGroundGenerator() {
        return this.groundGenerator;
    }

    public PathGenerator_GENSCENE GetPathGenerator() {
        return this.pathGenerator;
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

    public void ClearGridData() {
        if (gridMap != null) {
            gridMap.Clear(); // Xóa sạch toàn bộ dữ liệu Node cũ trong List
        }
    }
}
