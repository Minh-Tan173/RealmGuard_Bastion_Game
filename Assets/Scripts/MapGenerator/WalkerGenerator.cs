using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WalkerGenerator : MonoBehaviour {
    public static WalkerGenerator Instance { get; private set; }

    public enum Grid {
        Floor,
        Wall,
        Empty
    }

    public event EventHandler MapGenerateDone;

    [Header("Grid data")]
    [SerializeField] private int mapWidth;
    [SerializeField] private int mapHeight;

    [Header("Walker generator Data")]
    [SerializeField] private Tilemap tileMap;
    [SerializeField] private Tile floor;
    [SerializeField] private Tile wall;
    [SerializeField] private int maximumWalkers = 10;
    [SerializeField] private float fillPercentage = 0.4f;
    [SerializeField] private float waitTime = 0.05f;

    private int floorTileCount = 0;

    private Grid[,] gridMap;
    private List<WalkerObject> walkerList;

    private void Awake() {

        Instance = this;

    }

    private void Start() {
        InitializeGrid();
    }

    private void InitializeGrid() {

        gridMap = new Grid[mapWidth, mapHeight];

        for (int x = 0; x < gridMap.GetLength(0); x++) {

            for (int y = 0; y < gridMap.GetLength(1); y++) {
                // Duyệt qua từng element trong grid, khởi tạo toàn bộ bằng EMPTY

                gridMap[x, y] = Grid.Empty;
            }
        }

        walkerList = new List<WalkerObject>();

        Vector3Int TileCenter = new Vector3Int(gridMap.GetLength(0) / 2, gridMap.GetLength(1) / 2, 0);

        WalkerObject currentWalker = new WalkerObject(new Vector2(TileCenter.x, TileCenter.y), GetDirection(), 0.5f); // Khởi tạo walker với hướng ngẫu nhiên từ tâm

        gridMap[TileCenter.x, TileCenter.y] = Grid.Floor;

        tileMap.SetTile(TileCenter, floor);

        walkerList.Add(currentWalker);

        floorTileCount += 1;

        StartCoroutine(CreateFloors());
    }

    private Vector2 GetDirection() {

        int choice = Mathf.FloorToInt(UnityEngine.Random.value * 3.99f);

        switch (choice) {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            case 3:
                return Vector2.right;
            default:
                return Vector2.zero;
        }
    }

    private IEnumerator CreateFloors() {

        while ((float)floorTileCount / gridMap.Length < fillPercentage) {

            bool hasCreatedFloor = false;

            foreach (WalkerObject currentWalker in walkerList) {
                Vector3Int currentPos = new Vector3Int((int)currentWalker.Position.x, (int)currentWalker.Position.y, 0);

                if (gridMap[currentPos.x, currentPos.y] != Grid.Floor) {
                    // Nếu element ở currentPos chưa phải Floor

                    tileMap.SetTile(currentPos, floor);

                    floorTileCount += 1;

                    gridMap[currentPos.x, currentPos.y] = Grid.Floor;
                    hasCreatedFloor = true;
                }
            }

            //Walker Methods
            ChanceToRemove();
            ChanceToRedirect();
            ChanceToCreate();
            UpdatePosition();

            if (hasCreatedFloor) {
                yield return new WaitForSeconds(waitTime);
            }
        }

        StartCoroutine(CreateWalls());
    }

    private void ChanceToRemove() {
        // Duyệt qua toàn bộ các walker đang có, mỗi walker sẽ có 50% cơ hội bị xóa, nhưng luôn đảm bảo còn 1 walker tồn tại (không xóa hết)

        int updatedCount = walkerList.Count;

        for (int i = 0; i < updatedCount; i++) {
            if (UnityEngine.Random.value < walkerList[i].ChanceToChange && walkerList.Count > 1) {
                walkerList.RemoveAt(i);
                break;
            }
        }
    }

    private void ChanceToRedirect() {
        // Duyệt qua toàn bộ các walker đang có, mỗi walker sẽ có 50% cơ hội đổi hướng

        for (int i = 0; i < walkerList.Count; i++) {

            if (UnityEngine.Random.value < walkerList[i].ChanceToChange) {

                WalkerObject currentWalker = walkerList[i];
                currentWalker.Direction = GetDirection();
                walkerList[i] = currentWalker;
            }
        }
    }

    private void ChanceToCreate() {
        // Duyệt qua toàn bộ các walker đang có, mỗi walker sẽ có 50% cơ hội tự tạo thêm 1 walker nữa từ bản thân, chỉ xảy ra nếu số lượng walker hiện tại chưa max


        int updatedCount = walkerList.Count;

        for (int i = 0; i < updatedCount; i++) {
            if (UnityEngine.Random.value < walkerList[i].ChanceToChange && walkerList.Count < maximumWalkers) {

                Vector2 newDirection = GetDirection();
                Vector2 newPosition = walkerList[i].Position;

                WalkerObject newWalker = new WalkerObject(newPosition, newDirection, 0.5f);
                walkerList.Add(newWalker);
            }
        }
    }

    private void UpdatePosition() {

        for (int i = 0; i < walkerList.Count; i++) {

            WalkerObject FoundWalker = walkerList[i];
            FoundWalker.Position += FoundWalker.Direction;
            FoundWalker.Position.x = Mathf.Clamp(FoundWalker.Position.x, 1, mapWidth - 2);
            FoundWalker.Position.y = Mathf.Clamp(FoundWalker.Position.y, 1, mapHeight - 2);
            walkerList[i] = FoundWalker;
        }
    }

    private IEnumerator CreateWalls() {

        for (int x = 0; x < mapWidth - 1; x++) {

            for (int y = 0; y < mapHeight - 1; y++) {

                if (gridMap[x, y] == Grid.Floor) {

                    bool hasCreatedWall = false;

                    if (gridMap[x + 1, y] == Grid.Empty) {
                        tileMap.SetTile(new Vector3Int(x + 1, y, 0), wall);
                        gridMap[x + 1, y] = Grid.Wall;
                        hasCreatedWall = true;
                    }
                    if (gridMap[x - 1, y] == Grid.Empty) {
                        tileMap.SetTile(new Vector3Int(x - 1, y, 0), wall);
                        gridMap[x - 1, y] = Grid.Wall;
                        hasCreatedWall = true;
                    }
                    if (gridMap[x, y + 1] == Grid.Empty) {
                        tileMap.SetTile(new Vector3Int(x, y + 1, 0), wall);
                        gridMap[x, y + 1] = Grid.Wall;
                        hasCreatedWall = true;
                    }
                    if (gridMap[x, y - 1] == Grid.Empty) {
                        tileMap.SetTile(new Vector3Int(x, y - 1, 0), wall);
                        gridMap[x, y - 1] = Grid.Wall;
                        hasCreatedWall = true;
                    }

                    if (hasCreatedWall) {
                        yield return new WaitForSeconds(waitTime);
                    }
                }
            }
        }

        // After generate map done
        MapGenerateDone?.Invoke(this, EventArgs.Empty);

    }

    public float[] GetGridSize() {
        return new float[] { this.mapWidth, this.mapHeight };
    }

    public Grid[,] GetGridMap() {
        return this.gridMap;
    }
}
