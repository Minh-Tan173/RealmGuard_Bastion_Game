using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public interface ISceneControl
{
    public event EventHandler StartScene;
    public event EventHandler EndScene;
}
