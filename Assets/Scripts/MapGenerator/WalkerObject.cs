using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkerObject
{
    public Vector2 Position;
    public Vector2 Direction;
    public float ChanceToChange;

    public WalkerObject(Vector2 pos, Vector2 dir, float chanceToChange) {
        
        Position = pos;
        Direction = dir;

        this.ChanceToChange = chanceToChange;
    }

    //public Vector2 GetPosition() { return this.position; }
    //public Vector2 GetDirection() { return this.direction; }
    //public float GetChanceToChange() { return this.chanceToChange; }
}
