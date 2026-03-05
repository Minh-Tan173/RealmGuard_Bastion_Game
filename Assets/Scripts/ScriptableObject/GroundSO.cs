using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu()]
public class GroundSO : ScriptableObject
{
    [Header("Grid data")]
    public int mapWidth;
    public int mapHeight;
    public float spawnWaitTimer;

    [Header("Tile")]
    public Tile floorTile;
}
