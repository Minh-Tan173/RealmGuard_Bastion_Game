using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BackgroundGenerator_GENSCENE : MonoBehaviour
{
    public event EventHandler BackgroundCreated;

    [Header("Tile map data")]
    [SerializeField] private Tilemap backgroundTileMap;
    [SerializeField] private AnimatedTile waterAnimatedTile;
    [SerializeField] private Vector2Int extraTiles;

    private CinemachineVirtualCamera virtualCamera;


    private void Start() {

        StartCoroutine(GenerateBackground());
    }


    private IEnumerator GenerateBackground() {

        // Clear all old data (If have)
        backgroundTileMap.ClearAllTiles();

        yield return null;

        // Get Cam data
        virtualCamera = CameraControl_GENSCENE.Instance.GetCinemachineVirtualCamera();

        float heightCam = virtualCamera.m_Lens.OrthographicSize * 2f;
        float widthCam = heightCam * virtualCamera.m_Lens.Aspect;

        yield return null;

        Vector2 centerCam = virtualCamera.transform.position;

        // Tính góc trái dưới - phải trên của Camera
        Vector2 bottomLeft = centerCam - new Vector2(widthCam / 2, heightCam / 2);
        Vector2 topRight = centerCam + new Vector2(widthCam / 2, heightCam / 2);

        // Convert to tilemap size
        int xMin = Mathf.FloorToInt(bottomLeft.x - extraTiles.x);
        int yMin = Mathf.FloorToInt(bottomLeft.y - extraTiles.y);
        int xMax = Mathf.CeilToInt(topRight.x + extraTiles.x);
        int yMax = Mathf.CeilToInt(topRight.y + extraTiles.y);

        // Create background
        for (int x = xMin; x < xMax; x++) {
            for (int y = yMin; y < yMax; y++) {
                backgroundTileMap.SetTile(new Vector3Int(x, y, 0), waterAnimatedTile);
            }
        }

        yield return null;

        BackgroundCreated?.Invoke(this, EventArgs.Empty);

    }
}
