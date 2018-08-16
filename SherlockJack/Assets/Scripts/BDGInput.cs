using System;
using UnityEngine;

public class BDGInput
{
    public BDGInput ()
    {
    }

    public static bool FirePressedThisTick()
    {
        return (Input.GetButtonDown("Fire1") ||
            Input.GetButtonDown("Fire2") ||
            Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.LeftControl) ||
            Input.GetKeyDown(KeyCode.RightControl));
    }
}

