using System.Collections.Generic;
using UnityEngine;

public class Clue
{
    public enum State {
        Hidden,
        Revealed,
        Taken
    };

    City myCity;
    public Vector2 Position;

    public float DrawRadius = 1.5f;

    public State myState;

    public float spinAngle;
    public const float spinSpeed = 2.0f;

    public Clue (City c)
    {
        myCity = c;
        myState = State.Hidden;
        spinAngle = Random.Range(0, 2 * Mathf.PI);
    }

    public void SpawnAt(Vector2 pos)
    {
        this.Position = pos;
    }

    public void Update(float elapsedSeconds) {
        
        if (myState != State.Revealed) {
            return;
        }

        spinAngle += spinSpeed * elapsedSeconds;

        float distToJack = (myCity.Jack.Position - Position).magnitude;

        if (distToJack < DrawRadius) {
            myState = State.Taken;
            myCity.Jack.ShowText("A CLUE!");
        }
    }

    public void Draw(Color[] screenBuf, Vector2 camPos, float cityScale) {
        // todo draw a sprite, instead

        if (myState != State.Revealed) {
            return;
        }

        float screenRadius = DrawRadius * cityScale;

        float sx = (Position.x - camPos.x) * cityScale + 32;
        float sy = (Position.y - camPos.y) * cityScale + 32;

        int left = Mathf.FloorToInt(sx - screenRadius);
        left = Mathf.Max(0, left);

        int right = Mathf.CeilToInt(sx + screenRadius);
        right = Mathf.Min(right, 63);

        int top = Mathf.FloorToInt(sy - screenRadius);
        top = Mathf.Max(0, top);

        int bottom = Mathf.CeilToInt(sy + screenRadius);
        bottom = Mathf.Min(bottom, 63);

        Color[] coin1Colors = Globals.ClueCoin1Tex.GetPixels();
        Color[] coin2Colors = Globals.ClueCoin2Tex.GetPixels();

        float coinRot = Mathf.Sin(spinAngle);
        bool showOne = true;
        if (coinRot < 0) {
            showOne = false;
            coinRot = -coinRot;
        }
        if (coinRot == 0.0f) {
            return;
        }

        for (int x = left; x <= right; ++x) {
            for (int y = top; y <= bottom; ++y) {
                Vector2 deltaPos = new Vector2(x - sx, y - sy);
                if (deltaPos.magnitude < screenRadius) {
                    //screenBuf[64 * y + x] = Color.blue;
                    Vector2 normPos = deltaPos / screenRadius;
                    Color coinColor = Color.blue;

                    int cx = Mathf.RoundToInt(normPos.x * 16 + 16);
                    int cy = Mathf.RoundToInt(normPos.y * 16 + 16);

                    int fy = Mathf.RoundToInt((normPos.y * 16) / coinRot + 16);
                    int fx = Mathf.RoundToInt((normPos.x * 16) / coinRot + 16);

                    //if ((cx < 0) || (cx >= 32) || (fy < 0) || (fy >= 32)) {
                    //    continue;
                    //}

                    if ((fx < 0) || (fx >= 32) || (cy < 0) || (cy >= 32)) {
                        continue;
                    }

                    //coinColor = showOne ? coin1Colors[cx + 32 * fy] : coin2Colors[cx + 32 * fy];
                    coinColor = showOne ? coin1Colors[fx + 32 * cy] : coin2Colors[fx + 32 * cy];

                    screenBuf[64 * y + x] = coinColor;
                }
            }
        }
    }
}

