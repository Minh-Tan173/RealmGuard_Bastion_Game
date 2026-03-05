using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TowerManagerSO : ScriptableObject
{
    public ArcherTowerSO archerTowerSO;
    public GuardianTowerSO guardianTowerSO;
    public MageTowerSO mageTowerSO;
    public CatapultTowerSO catapultTowerSO;

}
