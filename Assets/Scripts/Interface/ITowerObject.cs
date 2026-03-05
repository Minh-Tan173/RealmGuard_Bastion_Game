using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITowerObject
{
    public enum TowerType {
        Default = 0,
        ArcherTower = 1,
        MageTower = 2,
        CatapultTower = 3,
        GuardianTower = 4
    };

    public enum LevelTower {
        Spawn = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
        Level4 = 4
    }

    public event EventHandler OnAttackZone;
    public event EventHandler UnAttackZone;

    public event EventHandler<UpgradeAttackZoneEventArgs> UpdateAttackZone;
    public class UpgradeAttackZoneEventArgs : EventArgs {
        public float attackZone;
    }

}
