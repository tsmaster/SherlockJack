using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixelRenderer : MonoBehaviour {

    public GameObject Target;

    City city;

    Texture2D ScreenBuf;

    Portal portal;

    public Texture2D FontTexture;

    public Texture2D JackTexture;
    public Texture2D MakuTexture;
    public Texture2D TitleTexture;
    public Texture2D BeetleTexture;
    public Texture2D PortalTexture;

    public Texture2D BeetleSpriteTexture;
    public Texture2D ClueCoin1Texture;
    public Texture2D ClueCoin2Texture;

    GameModeBase currentMode;

	// Use this for initialization
	void Start () {
        ScreenBuf = new Texture2D(64, 64);
        ScreenBuf.filterMode = FilterMode.Point;

        ScreenBuf.wrapMode = TextureWrapMode.Clamp;

        Renderer rend = Target.GetComponent<Renderer>();
        rend.material.mainTexture = ScreenBuf;

        for (int y = 0; y < ScreenBuf.height; y++)
        {
            for (int x = 0; x < ScreenBuf.width; x++)
            {
                float dx = x - 32;
                float dy = y - 32;
                float dz = Mathf.Sqrt(dx * dx + dy * dy);

                Color color = ((dz > 32) ? Color.white : Color.gray);
                ScreenBuf.SetPixel(x, y, color);
            }
        }

        ScreenBuf.Apply();

        city = new City();

        city.SetJackPos(Vector2.zero);

        portal = new Portal(city.GetRandomPointOnStreet(), city);

        currentMode = new TitleGameMode(TitleTexture);

        Globals.FontTex = FontTexture;
        Globals.JackTex = JackTexture;
        Globals.MakuTex = MakuTexture;
        Globals.BeetleTex = BeetleTexture;
        Globals.PortalTex = PortalTexture;

        Globals.BeetleSpriteTex = BeetleSpriteTexture;
        Globals.ClueCoin1Tex = ClueCoin1Texture;
        Globals.ClueCoin2Tex = ClueCoin2Texture;
	}
	
	// Update is called once per frame
	void Update () {
        Color[] screenBuf = new Color[64 * 64];
        if (currentMode != null) {
            currentMode.Update(Time.deltaTime);

            if (currentMode.IsComplete()) {
                currentMode = currentMode.GetNextMode();
            }
            else {
                currentMode.Draw(screenBuf);
            }

            ScreenBuf.SetPixels(screenBuf);
            ScreenBuf.Apply();

        }
        else {
            // walk around city "mode"
            float dt = Time.deltaTime;

            city.Update(dt);
            portal.Update(dt);
            city.Jack.Update(dt);
            foreach (Beetle b in city.Beetles) {
                b.Update(dt);
            }
            foreach (Clue c in city.Clues) {
                c.Update(dt);
            }

            Vector2 camPos = city.Jack.Position;
            city.Render(camPos);

            portal.Draw(city.Pixels, camPos, city.scale);
            foreach (Beetle b in city.Beetles) {
                b.Draw(city.Pixels, camPos, city.scale);
            }
            foreach (Clue c in city.Clues) {
                c.Draw(city.Pixels, camPos, city.scale);
            }

            DrawPadlock(city.Pixels);
            city.Jack.Draw(city.Pixels, city.scale);

            ScreenBuf.SetPixels(city.Pixels);
            ScreenBuf.Apply();
        }
	}

    void DrawPadlock(Color[] pixels) 
    {
        foreach (Beetle b in city.Beetles) {
            if (b.myState == Beetle.State.Alive) {
                DrawPadlockBlip(b.Position, Color.black, pixels);
            }
        }

        foreach (Clue c in city.Clues) {
            if (c.myState == Clue.State.Revealed) {
                DrawPadlockBlip(c.Position, Color.blue, pixels);
            }
        }

        if (portal.myState == Portal.State.Revealed) {
            DrawPadlockBlip(portal.Position, Color.white, pixels);
        }
    }

    void DrawPadlockBlip(Vector2 worldPos, Color blipColor, Color[] pixels) {
        Vector2 worldRelPos = worldPos - city.Jack.Position;
        Vector2 normScreenRelPos = worldRelPos * city.scale;

        if ((Mathf.Abs(normScreenRelPos.x) < 32.0f) &&
            (Mathf.Abs(normScreenRelPos.y) < 32.0f)) {
            return;
        }

        normScreenRelPos = ClipVec(normScreenRelPos, 32.0f);

        int cx = Mathf.RoundToInt(32.0f + normScreenRelPos.x);
        int cy = Mathf.RoundToInt(32.0f + normScreenRelPos.y);

        if (cx > 63) {
            cx = 63;
        }
        if (cx < 0) {
            cx = 0;
        }
        if (cy > 63) {
            cy = 63;
        }
        if (cy < 0) {
            cy = 0;
        }

        pixels[cx + 64 * cy] = blipColor;
    }

    Vector2 ClipVec(Vector2 baseVec, float maxComponent) {
        float scale = 1.0f;

        if (Mathf.Abs(baseVec.x) > maxComponent) {
            scale = maxComponent / Mathf.Abs(baseVec.x);
        }
        if (Mathf.Abs(baseVec.y) > maxComponent) {
            scale = Mathf.Min(scale, maxComponent / Mathf.Abs(baseVec.y));
        }
        return new Vector2(baseVec.x * scale, baseVec.y * scale);
    }
}
