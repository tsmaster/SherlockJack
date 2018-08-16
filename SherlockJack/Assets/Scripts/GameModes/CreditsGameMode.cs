using System.Collections.Generic;
using UnityEngine;

public class CreditsGameMode : GameModeBase
{
    Color[] pixels;

    public CreditsGameMode ()
    {
        pixels = Globals.CreditsTex.GetPixels();
    }

    override public void Update(float seconds)
    {
        base.Update(seconds);
    }

    override public void Draw(Color[] outbuf)
    {
        for (int i = 0; i < 64 * 64; ++i) {
            outbuf[i] = pixels[i];
        }
    }

    override public bool IsComplete()
    {
        return (ElapsedTime > 12.0f);
    }

    override public GameModeBase GetNextMode()
    {
        if (ElapsedTime > 12.0f) {
            return new TitleGameMode(Globals.TitleTex);
        }
        else {
            return null;
        }
    }
}
