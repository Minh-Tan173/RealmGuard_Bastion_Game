using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHasFunctionButton
{
    public event EventHandler OnFunctionButton;
    public event EventHandler<OffFunctionButtonEventArgs> OffFunctionButton;
    public class OffFunctionButtonEventArgs : EventArgs {
        public Action callbackAction;
    }

}
