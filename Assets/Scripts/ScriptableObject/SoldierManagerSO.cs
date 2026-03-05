using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class SoldierManagerSO : ScriptableObject
{
    [Header("Archer")]
    public SoldierSO upArcherSO;
    public SoldierSO rightArcherSO;
    public SoldierSO downArcherSO;
    public SoldierSO leftArcherSO;

    [Header("Mage")]
    public SoldierSO upMageSO;
    public SoldierSO rightMageSO;
    public SoldierSO downMageSO;
    public SoldierSO leftMageSO;

    [Header("Catapul")]
    public SoldierSO catapulSO;

    [Header("Guardian")]
    public SoldierSO guardianSO;

}
