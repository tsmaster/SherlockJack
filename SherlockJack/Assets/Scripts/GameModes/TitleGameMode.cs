using System.Collections.Generic;
using UnityEngine;

public class TitleGameMode : GameModeBase
{
    Color[] pixels;

    public TitleGameMode (Texture2D titleTexture)
    {
        pixels = titleTexture.GetPixels();
    }

    override public void Update(float seconds)
    {
        base.Update(seconds);

        if (ElapsedTime > 2.0f) {
            // advance mode

        }
    }

    override public void Draw(Color[] outbuf)
    {
        for (int i = 0; i < 64 * 64; ++i) {
            outbuf[i] = pixels[i];
        }
    }

    override public bool IsComplete()
    {
        return (ElapsedTime > 2.0f);
    }

    override public GameModeBase GetNextMode()
    {
        if (ElapsedTime > 2.0f) {
            List<LineDesc> lines = CharacterTextMode.MakeDialog();
            return new CharacterTextMode(Globals.FontTex, lines);
        }
        else {
            return null;
        }
    }
}
