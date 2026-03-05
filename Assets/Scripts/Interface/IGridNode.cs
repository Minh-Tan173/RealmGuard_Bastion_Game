using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGridNode
{
    public enum Grid {
        Floor,
        Path,
        Fence,
        Empty
    }

}
