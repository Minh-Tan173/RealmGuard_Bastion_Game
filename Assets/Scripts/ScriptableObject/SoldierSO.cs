using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoldierSO : ScriptableObject
{
    public enum SoldierLevel {
        Level1,
        Level2,
        Level3
    }

    public enum SoldierDirection {
        Up,
        Right,
        Down,
        Left
    }

    public Transform soldierPrefab;
    public Transform bulletPrefab;

}
