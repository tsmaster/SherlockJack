using System.Collections.Generic;
using UnityEngine;

public class Portal
{
    public enum State {
        Hidden,
        Revealed, 
        Taken
    }

    Vector2[] centers;
    float[] radii;
    float[] angles;
    float[] orbitRates;

    int numRings;
    float ringWidth;
    float boxSize;

    public Vector2 Position;

    public float CollisionRadius = 4.0f;

    public State myState;
    City myCity;

    public Portal (Vector2 worldPos, City city)
    {
        myCity = city;
        centers = new Vector2[3];
        radii = new float[3];
        angles = new float[3];
        orbitRates = new float[3];

        for (int i = 0; i < 3; ++i) {
            angles[i] = Random.Range(0.0f, 2*Mathf.PI);
            orbitRates[i] = Random.Range(-2.0f, 2.0f);
        }
        radii[0] = 1.0f;
        radii[1] = 1.25f;
        radii[2] = 1.5f;

        numRings = 8;
        ringWidth = 2.0f;
        boxSize = (numRings * ringWidth + radii[2]) * 2.0f;

        this.Position = worldPos;

        myState = State.Hidden;
    }

    public void Update(float seconds)
    {
        switch (myState) {
        case State.Hidden:
            UpdateHidden(seconds);
            break;
        case State.Revealed:
            UpdateRevealed(seconds);
            break;
        case State.Taken:
            // do nothing
            break;
        }
    }

    void UpdateHidden(float elapsedSeconds)
    {
        // see if all clues are taken

        bool allTaken = true;

        foreach (Clue c in myCity.Clues) {
            if (c.myState != Clue.State.Taken) {
                allTaken = false;
            }
        }

        if (allTaken) {
            myState = State.Revealed;
            myCity.Jack.ShowText("QUICK WATSON, WE'VE GOT TO GET BACK!");
        }
    }

    void UpdateRevealed(float elapsedSeconds)
    {
        for (int i = 0; i < 3; ++i) {
            angles[i] += orbitRates[i] * elapsedSeconds;

            centers[i] = new Vector2(radii[i] * Mathf.Cos(angles[i]),
                radii[i] * Mathf.Sin(angles[i]));
        }

        float distToJack = (myCity.Jack.Position - Position).magnitude;
        if (distToJack < CollisionRadius) {
            Debug.Log("collided with jack, portal taken");
            myState = State.Taken;
        }
    }

    public void Draw(Color[] colors, Vector2 camPos, float cityScale)
    {
        if (myState != State.Revealed) {
            return;
        }

        float sx = (Position.x - camPos.x) * cityScale + 32;
        float sy = (Position.y - camPos.y) * cityScale + 32;

        int left = Mathf.FloorToInt(sx - boxSize);
        left = Mathf.Max(0, left);

        int right = Mathf.CeilToInt(sx + boxSize);
        right = Mathf.Min(right, 63);

        int top = Mathf.FloorToInt(sy - boxSize);
        top = Mathf.Max(0, top);

        int bottom = Mathf.CeilToInt(sy + boxSize);
        bottom = Mathf.Min(bottom, 63);

        for (int x = left; x <= right; ++x) {
            for (int y = top; y <= bottom; ++y) {
                Color c = Color.blue;
                if (GetPortalColor(x, y, sx, sy, ref c)) {
                    colors[64 * y + x] = c;
                }
            }
        }
    }

    bool GetPortalColor(int x, int y, float cx, float cy, ref Color outColor)
    {
        float[] dists = new float[3];
        for (int i = 0; i < 3; ++i) {
            float dx = x - (cx + centers[i].x);
            float dy = y - (cy + centers[i].y);
            dists[i] = Mathf.Sqrt(dx * dx + dy * dy);
        }

        for (int ring = 0; ring < numRings; ++ring) {
            int particle = ring % 3;
            Color c = ring % 2 == 0 ? Color.white : Color.black;

            if (dists[particle] < ringWidth * ring) {
                outColor = c;
                return true;
            }
        }
        outColor = Color.green;
        return false;
    }
}
