using System;
using UnityEngine;

abstract public class GameModeBase
{
    public float ElapsedTime;

    public GameModeBase ()
    {
        ElapsedTime = 0.0f;
    }

    public virtual void Update(float seconds)
    {
        ElapsedTime += seconds;
    }

    abstract public void Draw(Color[] outbuf);

    abstract public bool IsComplete();

    abstract public GameModeBase GetNextMode();
}

