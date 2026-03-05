using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ObjectPlaced : MonoBehaviour
{
    public static ObjectPlaced Instance { get; private set; }

    public event EventHandler ObjectPlacedDone;

    [Header("ObjectSO")]
    [SerializeField] private ObjectSO grassSO;
    [SerializeField] private ObjectSO flowerSO;
    [SerializeField] private ObjectSO treeSO;
    [SerializeField] private ObjectSO decorObjectSO;
    [SerializeField] private ObjectSO campfireSO;
    [SerializeField] private ObjectSO flagSO;

    private List<GridNode> gridMap;
    private List<GridNode> validGridMap;
    private HashSet<GridNode> nodesNearPath;

    private void Awake() {

        Instance = this;

        gridMap = new List<GridNode>();
        validGridMap = new List<GridNode>();
        nodesNearPath = new HashSet<GridNode>();

    }

    private void Start() {
        PathGenerator.Instance.PathCreateDone += PathGenerator_PathCreateDone;
        PathGenerator.Instance.SpawnFlagAtLastPathNode += PathGenerator_SpawnFlagAtLastPathNode;
    }

    private void OnDestroy() {

        PathGenerator.Instance.PathCreateDone -= PathGenerator_PathCreateDone;
        PathGenerator.Instance.SpawnFlagAtLastPathNode -= PathGenerator_SpawnFlagAtLastPathNode;

    }

    private void PathGenerator_SpawnFlagAtLastPathNode(object sender, EventArgs e) {

        Vector2Int lastWaypointPos = PathGenerator.Instance.GetWaypointPosList()[PathGenerator.Instance.GetWaypointPosList().Count - 1];
        Vector2Int leftLastWaypointNode = new Vector2Int(lastWaypointPos.x - 2, lastWaypointPos.y);
        Vector2Int rightLastWaypointNode = new Vector2Int(lastWaypointPos.x + 1, lastWaypointPos.y);

        // Left Flag
        Transform leftFlagTransform = Instantiate(flagSO.prefab, this.transform);
        Vector3 centerLeftNode = GridManager.Instance.NodePosConvertToWordPos(leftLastWaypointNode);
        leftFlagTransform.position = new Vector3(centerLeftNode.x, centerLeftNode.y - 0.5f, centerLeftNode.z);

        // Right Flag
        Transform rightFlagTransform = Instantiate(flagSO.prefab, this.transform);
        Vector3 centerRightNode = GridManager.Instance.NodePosConvertToWordPos(rightLastWaypointNode);
        rightFlagTransform.position = new Vector3(centerRightNode.x, centerRightNode.y - 0.5f, centerRightNode.z);


    }

    private void PathGenerator_PathCreateDone(object sender, System.EventArgs e) {

        this.gridMap = GridManager.Instance.GetGridMap();
        
        foreach(GridNode gridNode in this.gridMap) {

            if (gridNode.GetGridID() != IGridNode.Grid.Floor) {
                continue;
            }
            this.validGridMap.Add(gridNode);    

        }

        // 1. Trồng cỏ và hoa
        PlacedGrassOrFlower();

        // 2. Tìm các node là hàng xóm của Path
        foreach(GridNode gridNode in this.gridMap) {

            if (gridNode.GetGridID() == IGridNode.Grid.Fence || gridNode.GetGridID() == IGridNode.Grid.Path) {
                continue;
            }

            // Kiểm tra 
            Vector2Int nodePos = gridNode.GetNodePosition();
            int xPos = nodePos.x;
            int yPos = nodePos.y;
            int bufferDistance = 2;

            bool isNearPath = false;

            for (int x = xPos - bufferDistance; x <= xPos + bufferDistance; x++) {

                for (int y = yPos - bufferDistance; y <= yPos + bufferDistance; y++) {

                    GridNode neighborNode = GridManager.Instance.GetNode(new Vector2Int(x, y));

                    if (neighborNode == null) {
                        continue;
                    }

                    if (neighborNode.GetGridID() == IGridNode.Grid.Path) {
                        // Nếu bất kì 1 node hàng xóm nào thỏa mãn điều kiện == Path

                        nodesNearPath.Add(gridNode);

                        validGridMap.Remove(gridNode);

                        isNearPath = true;

                        break;

                    }

                }
                if (isNearPath) { break; }
            }

        }

        // 3. Đặt CampFire
        PlacedCampfire();

        // 4. Đặt cây
        PlacedTree();

        // 5. Đặt DecorObject
        PlacedDecorObject();
    }

    private void PlacedGrassOrFlower() {

        foreach (GridNode gridNode in gridMap) {

            if (gridNode.GetGridID() == IGridNode.Grid.Fence || gridNode.GetGridID() == IGridNode.Grid.Path) {
                continue;
            }

            int randomIndex = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

            if (randomIndex == 0) {
                // ~25% chance for Grass grow

                Transform grassTransform = Instantiate(grassSO.prefab, this.transform);
                grassTransform.position = RandomPositionIn1x1Area(gridNode);

            }
            else if (randomIndex == 1) {
                // ~25% chance for Flower grow

                Transform flowerTranform = Instantiate(flowerSO.prefab, this.transform);
                flowerTranform.position = RandomPositionIn1x1Area(gridNode);

            }


        }

    }

    private void PlacedTree() {
        // Tree đặt tại tâm vùng 2 * 2

        // Khởi tạo list area2x2List
        List<List<GridNode>> area2x2List = FindValid2x2Area();

        // Trộn List
        int currentIndex = area2x2List.Count;

        while(currentIndex > 1) {

            currentIndex -= 1;

            int randomIndex = UnityEngine.Random.Range(0, currentIndex + 1);

            List<GridNode> tempArea2x2 = area2x2List[randomIndex];
            area2x2List[randomIndex] = area2x2List[currentIndex];
            area2x2List[currentIndex] = tempArea2x2;
        }

        // Trồng cây
        foreach (List<GridNode> area2x2 in area2x2List) {

            bool isSkipThisLoop = false;
            // 1.  ReCheck area2x2 valid
            foreach(GridNode gridNode in area2x2) {
                if (gridNode.HasItemOn()) {
                    // If any node in area has item on

                    isSkipThisLoop = true;
                    break;
                }
            }
            if (isSkipThisLoop) { continue; }

            // 2. Random Tree grow
            int randomValue = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

            if (randomValue == 0) {
                // 25% chance for Tree grow

                Transform treeTranform = Instantiate(treeSO.prefab, this.transform);

                GridNode topRight = area2x2[area2x2.Count - 1];
                Vector3 topRightCenterPos = GridManager.Instance.NodePosConvertToWordPos(topRight.GetNodePosition());

                treeTranform.position = new Vector3(topRightCenterPos.x - 0.5f, topRightCenterPos.y - 0.5f, 0f);
                
                // After Tree grow

                foreach(GridNode gridNode in area2x2) {
                    gridNode.SetHasItemOn(true);

                    validGridMap.Remove(gridNode);
                }
            }

        }

    }

    private void PlacedDecorObject() {

        List<GridNode> tempNodeList = new List<GridNode>(validGridMap);

        foreach (GridNode gridNode in tempNodeList) {

            int randomValue = Mathf.FloorToInt(UnityEngine.Random.value * 9.99f);

            // 10% to DecorObject spawn
            if (randomValue == 0) {

                Transform objectTransform = Instantiate(decorObjectSO.prefab, this.transform);
                Vector3 nodePos = GridManager.Instance.NodePosConvertToWordPos(gridNode.GetNodePosition());
                float xRandom = nodePos.x + UnityEngine.Random.Range(-0.5f, 0.5f);
                float yRandom = nodePos.y + UnityEngine.Random.Range(-0.5f, 0.5f);
                objectTransform.position = new Vector3(xRandom, yRandom, 0f);

                gridNode.SetHasItemOn(true);

                validGridMap.Remove(gridNode);
            }

        }

        // 6. Thông báo event đã đặt Object xong
        ObjectPlacedDone?.Invoke(this, EventArgs.Empty);

    }

    private void PlacedCampfire() {

        // Khởi tạo list area3x3List
        List<List<GridNode>> area3x3List = FindValid3x3Area();

        if (area3x3List.Count == 0) {
            // Kết thúc ngay lập tức
            return;
        }

        // Trộn list
        int currentIndex = area3x3List.Count;

        while (currentIndex > 1) {

            currentIndex -= 1;

            int randomIndex = UnityEngine.Random.Range(0, currentIndex + 1);

            List<GridNode> gridNodeListTemp = area3x3List[randomIndex];
            area3x3List[randomIndex] = area3x3List[currentIndex];
            area3x3List[currentIndex] = gridNodeListTemp;
        }

        // Placed Campfire
        foreach(List<GridNode> area3x3 in area3x3List) {

            int randomValue = Mathf.FloorToInt(UnityEngine.Random.value * 9.99f);

            // ~10% chance to placed Campfire at this Node
            if (randomValue == 0) {

                Transform campfireTranform = Instantiate(campfireSO.prefab, this.transform);

                // Mặc định area3x3[4] là center của vùng 3x3
                GridNode centerNode = area3x3[4];
                Vector3 centerPos = GridManager.Instance.NodePosConvertToWordPos(centerNode.GetNodePosition());

                campfireTranform.position = centerPos;

                // After placed 1 campfire

                foreach(GridNode gridNode in area3x3) {
                    gridNode.SetHasItemOn(true);

                    validGridMap.Remove(gridNode);
                }

                return;
            }


        }

    }
    
    private Vector3 RandomPositionIn1x1Area(GridNode gridNode) {

        float xRandom = UnityEngine.Random.Range(-0.5f, 0.5f);
        float yRandom = UnityEngine.Random.Range(-0.5f, 0.5f);

        Vector3 nodePos = GridManager.Instance.NodePosConvertToWordPos(gridNode.GetNodePosition());

        return new Vector3(nodePos.x + xRandom, nodePos.y + yRandom, nodePos.z);

    }

    private List<List<GridNode>> FindValid2x2Area() {
        // "Optimized Brute Force" algorithm

        List<List<GridNode>> area2x2List = new List<List<GridNode>>();

        for (int i = 0; i < validGridMap.Count; i++) {

            // Duyệt vùng 2 * 2 với tempMap[i] là node dưới cùng bên trái

            Vector2Int nodePos = validGridMap[i].GetNodePosition();

            List<GridNode> are2x2 = new List<GridNode>();
            bool isValidArea = true;

            for (int x = nodePos.x; x <= nodePos.x + 1; x++) {

                for (int y = nodePos.y; y <= nodePos.y + 1; y++) {

                    GridNode currentNode = GridManager.Instance.GetNode(new Vector2Int(x, y));

                    // 0. Check current node is exists
                    if (currentNode == null) {
                        isValidArea = false;
                        break;
                    }

                    // 1. Check Node Id
                    if (currentNode.GetGridID() != IGridNode.Grid.Floor) {
                        isValidArea = false;
                        break;
                    }

                    // 2 Check Node State
                    if (currentNode.HasItemOn() || nodesNearPath.Contains(currentNode)) {
                        isValidArea = false;
                        break;
                    }

                    if (isValidArea) {
                        // Đến đây mà isValidArea true thì currentNode Valid
                        are2x2.Add(currentNode);
                    }

                }
                if (!isValidArea) { break; }
            }

            if (isValidArea) {
                // Nếu area 2x2 hợp lệ

                area2x2List.Add(are2x2);

            }
            else {
                // Nếu area 2x2 không hợp lệ
            }
        }

        return area2x2List;
    }

    private List<List<GridNode>> FindValid3x3Area() {
        // "Optimized Brute Force" algorithm

        List<List<GridNode>> area3x3List = new List<List<GridNode>>();

        for (int i = 0; i < validGridMap.Count; i++) {

            // Duyệt vùng 3 * 3 với tempMap[i] là node dưới cùng bên trái

            Vector2Int nodePos = validGridMap[i].GetNodePosition();

            List<GridNode> are3x3 = new List<GridNode>();
            bool isValidArea = true;

            for (int x = nodePos.x; x <= nodePos.x + 2; x++) {

                for (int y = nodePos.y; y <= nodePos.y + 2; y++) {

                    GridNode currentNode = GridManager.Instance.GetNode(new Vector2Int(x, y));

                    // 0. Check current node is exists
                    if (currentNode == null) {
                        isValidArea = false;
                        break;
                    }

                    // 1. Check Node Id
                    if (currentNode.GetGridID() != IGridNode.Grid.Floor) {
                        isValidArea = false;
                        break;
                    }

                    // 2 Check Node State
                    if (currentNode.HasItemOn() || nodesNearPath.Contains(currentNode)) {
                        isValidArea = false;
                        break;
                    }

                    if (isValidArea) {
                        // Đến đây mà isValidArea true thì currentNode Valid
                        are3x3.Add(currentNode);
                    }

                }
                if (!isValidArea) { break; }
            }

            if (isValidArea) {
                // Nếu area 3x3 hợp lệ

                area3x3List.Add(are3x3);

            }
            else {
                // Nếu area 3x3 không hợp lệ
            }
        }

        return area3x3List;
    }

}
