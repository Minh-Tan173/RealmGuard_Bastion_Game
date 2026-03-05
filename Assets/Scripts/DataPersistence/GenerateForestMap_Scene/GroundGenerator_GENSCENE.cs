using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GroundGenerator_GENSCENE : MonoBehaviour
{
    public static GroundGenerator_GENSCENE Instance { get; private set; }

    public enum Grid {
        Floor,
        Path,
        Fence,
        Empty
    }

    public event EventHandler MapCreated;

    [Header("TileMap")]
    [SerializeField] private Tilemap groundTileMap;

    [Header("Data")]
    [SerializeField] private GroundSO groundSO;

    private BackgroundGenerator_GENSCENE backgroundGenerator;
    private Grid[,] gridMap;

    private void Awake() {

        Instance = this;
        backgroundGenerator = GetComponentInParent<GridManager_GENSCENE>().GetBackgroundGenerator();
    }

    private void Start() {
        backgroundGenerator.BackgroundCreated += GroundGenerator_BackgroundCreated;
    }

    private void OnDestroy() {
        backgroundGenerator.BackgroundCreated -= GroundGenerator_BackgroundCreated;
    }

    private void GroundGenerator_BackgroundCreated(object sender, EventArgs e) {
        InitializeGrid();
    }

    private void InitializeGrid() {

        // 1. Dọn dẹp Tilemap cũ
        groundTileMap.ClearAllTiles();

        // 2. Dọn dẹp dữ liệu logic cũ
        GridManager_GENSCENE.Instance.ClearGridData();

        gridMap = new Grid[groundSO.mapWidth, groundSO.mapHeight];

        for (int x = 0; x < groundSO.mapWidth; x++) {

            for (int y = 0; y < groundSO.mapHeight; y++) {

                gridMap[x, y] = Grid.Empty;

                int currentIndex = GridManager_GENSCENE.Instance.GetGridMap().Count;

                GridManager_GENSCENE.Instance.GetGridMap().Add(new GridNode(new Vector2Int(x, y), IGridNode.Grid.Empty, currentIndex));
            }

        }

        //StartCoroutine(CreateFloor());
        CreateGround();
    }

    private void CreateGround() {

        for (int x = 1; x <= groundSO.mapWidth - 2; x++) {

            for (int y = 1; y <= groundSO.mapHeight - 2; y++) {

                if (gridMap[x, y] != Grid.Floor) {

                    SetGridTileTo(new Vector2Int(x, y), Grid.Floor);

                }

            }

        }

        StartCoroutine(CreateBorder());

    }

    private IEnumerator CreateBorder() {

        // Biên dưới (y = 0)
        for (int x = 0; x < groundSO.mapWidth; x++) {

            if (gridMap[x, 0] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(x, 0), Grid.Fence);

            }
        }

        // Biên trái (x = 0)
        for (int y = 0; y < groundSO.mapHeight; y++) {
            if (gridMap[0, y] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(0, y), Grid.Fence);

            }
        }

        // Biên trên (y = mapHeight - 1)
        for (int x = 0; x < groundSO.mapWidth; x++) {

            if (gridMap[x, groundSO.mapHeight - 1] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(x, groundSO.mapHeight - 1), Grid.Fence);

            }
        }

        // Biên phải (x = mapWidth - 1)
        for (int y = 0; y < groundSO.mapHeight; y++) {
            if (gridMap[groundSO.mapWidth - 1, y] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(groundSO.mapWidth - 1, y), Grid.Fence);
            }
        }

        yield return null;
        MapCreated?.Invoke(this, EventArgs.Empty);

    }

    private IEnumerator CreateFloor() {

        for (int x = 1; x <= groundSO.mapWidth - 2; x++) {

            for (int y = 1; y <= groundSO.mapHeight - 2; y++) {

                if (gridMap[x, y] != Grid.Floor) {

                    SetGridTileTo(new Vector2Int(x, y), Grid.Floor);

                    yield return new WaitForSeconds(groundSO.spawnWaitTimer);
                }

            }

        }

        StartCoroutine(CreateWall());

    }

    private IEnumerator CreateWall() {
        // Biên dưới (y = 0)
        for (int x = 0; x < groundSO.mapWidth; x++) {

            if (gridMap[x, 0] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(x, 0), Grid.Fence);

            }
            yield return new WaitForSeconds(groundSO.spawnWaitTimer);
        }

        // Biên trái (x = 0)
        for (int y = 0; y < groundSO.mapHeight; y++) {
            if (gridMap[0, y] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(0, y), Grid.Fence);

            }
            yield return new WaitForSeconds(groundSO.spawnWaitTimer);
        }

        // Biên trên (y = mapHeight - 1)
        for (int x = 0; x < groundSO.mapWidth; x++) {

            if (gridMap[x, groundSO.mapHeight - 1] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(x, groundSO.mapHeight - 1), Grid.Fence);

            }
            yield return new WaitForSeconds(groundSO.spawnWaitTimer);
        }

        // Biên phải (x = mapWidth - 1)
        for (int y = 0; y < groundSO.mapHeight; y++) {
            if (gridMap[groundSO.mapWidth - 1, y] != Grid.Fence) {

                SetGridTileTo(new Vector2Int(groundSO.mapWidth - 1, y), Grid.Fence);
            }
            yield return new WaitForSeconds(groundSO.spawnWaitTimer);
        }

        yield return null;

        MapCreated?.Invoke(this, EventArgs.Empty);
    }

    public Grid[,] GetGridMap() {
        return this.gridMap;
    }

    public void SetGridTileTo(Vector2Int tilePosition, Grid grid) {

        this.gridMap[tilePosition.x, tilePosition.y] = grid;

        switch (grid) {
            case Grid.Path:

                GridManager_GENSCENE.Instance.GetNode(tilePosition).SetGridID(IGridNode.Grid.Path);

                break;

            case Grid.Fence:

                groundTileMap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), groundSO.floorTile);

                GridManager_GENSCENE.Instance.GetNode(tilePosition).SetGridID(IGridNode.Grid.Fence);

                break;

            case Grid.Floor:

                groundTileMap.SetTile(new Vector3Int(tilePosition.x, tilePosition.y, 0), groundSO.floorTile);

                GridManager_GENSCENE.Instance.GetNode(tilePosition).SetGridID(IGridNode.Grid.Floor);

                break;

        }
    }



    public GroundSO GetGroundSO() {
        return this.groundSO;
    }
}
