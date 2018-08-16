using System.Collections.Generic;
using UnityEngine;

public enum Speaker {
    Maku,
    Jack,
    Beetle,
    Portal
};

public class LineDesc {
    public Speaker speaker;
    public bool isLeft;
    public string text;

    public LineDesc (Speaker speaker, bool isLeft, string text) {
        this.speaker = speaker;
        this.isLeft = isLeft;
        this.text = text;
    }
}

public class CharacterTextMode : GameModeBase
{
    Color32[] pixels;
    int charWidth;
    int charHeight;
    Texture2D font;

    int charX;

    int lineIndex;
    int stringScrollPosition;
    float lineElapsedTime;
    float lineSpeed; // pixels per second
    float lineStartTime; // seconds before scrolling starts

    List<LineDesc> lines;

    public CharacterTextMode (Texture2D font, List<LineDesc> lines)
    {
        this.font = font;
        lineSpeed = 18.0f;
        lineStartTime = 0.8f;
        this.lines = lines;
        lineIndex = 0;
        ResetLineSettings();
    }

    override public void Update(float seconds)
    {
        base.Update(seconds);

        lineElapsedTime += seconds;

        if (lineElapsedTime < lineStartTime) {
            stringScrollPosition = 0;
        }
        else {
            float scrollTime = lineElapsedTime - lineStartTime;
            float scrolledPixels = scrollTime * lineSpeed;
            stringScrollPosition = Mathf.FloorToInt(scrolledPixels);
        }

        if (BDGInput.FirePressedThisTick()) {
            IncrementLine();
        }
    }

    void IncrementLine() {
        this.lineIndex++;
        this.ResetLineSettings();
    }

    void ResetLineSettings()
    {
        stringScrollPosition = 0;
        lineElapsedTime = 0.0f;

        if (lineIndex >= lines.Count) {
            return;
        }

        switch (lines[lineIndex].speaker) {
        case Speaker.Jack:
            SetSpeakerTexture(Globals.JackTex);
            break;
        case Speaker.Maku:
            SetSpeakerTexture(Globals.MakuTex);
            break;
        case Speaker.Beetle:
            SetSpeakerTexture(Globals.BeetleTex);
            break;
        case Speaker.Portal:
            SetSpeakerTexture(Globals.PortalTex);
            break;
        }

        if (lines[lineIndex].isLeft) {
            charX = 0;
        }
        else {
            charX = 63 - charWidth;
        }
    }

    void SetSpeakerTexture(Texture2D texture)
    {
        pixels = texture.GetPixels32();
        charWidth = texture.width;
        charHeight = texture.height;
    }

    bool IsLineDone() {
        return stringScrollPosition >= 6 * lines[lineIndex].text.Length;
    }

    void Clear(Color[] outbuf, Color clearColor)
    {
        for (int x = 0; x < 64; ++x) {
            for (int y = 0; y < 64; ++y) {
                outbuf[64 * y + x] = clearColor;
            }
        }
    }

    override public void Draw(Color[] outbuf)
    {
        Color pageColor = new Color(192, 192, 192);

        Clear(outbuf, pageColor);       
        for (int x = 0; x < charWidth; ++x) {
            for (int y = 0; y < charHeight; ++y) {
                int sx = charX + x;
                int sy = y;
                Color32 px = pixels[charWidth * y + x];
                if (px.a > 128) {
                    Color noAlpha = px;
                    outbuf[64 * sy + sx] = noAlpha;
                }
            }
        }

        TextRender.DrawString(outbuf, font, 1 - stringScrollPosition, 56, Color.black, lines[lineIndex].text);

        if (IsLineDone()) {
            TextRender.DrawString(outbuf, font, 20, 48, Color.black, "[OK]");
        }
    }

    override public GameModeBase GetNextMode()
    {
        return null;
    }

    override public bool IsComplete()
    {
        return lineIndex >= lines.Count;
    }

    public static List<LineDesc> MakeDialog() {
        List<LineDesc> lines = new List<LineDesc>();

        lines.Add(new LineDesc(Speaker.Jack, true, "MAKURITY!"));
        lines.Add(new LineDesc(Speaker.Maku, false, "YES, SHERLOCK JACK, IT IS I, MAKURITY."));
        lines.Add(new LineDesc(Speaker.Maku, false, "THE SHAPE-SHIFTING MASTER OF EVIL."));
        lines.Add(new LineDesc(Speaker.Maku, false, "YOU HAVE CHASED ME THROUGH TIME AND SPACE."));
        lines.Add(new LineDesc(Speaker.Maku, false, "AND NOW, WE BATTLE ON THE STREETS OF LONDON."));
        lines.Add(new LineDesc(Speaker.Beetle, false, "DEFEAT THE MEMBERS OF THE BEETLE GANG"));
        lines.Add(new LineDesc(Speaker.Portal, false, "THAT GUARD THE TIME PORTAL THAT YOU SEEK!"));
        lines.Add(new LineDesc(Speaker.Maku, false, "AH HAH HAH HAH HAH!!!"));

        return lines;
    }
}
