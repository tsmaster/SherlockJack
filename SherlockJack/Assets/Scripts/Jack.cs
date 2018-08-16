using System.Collections.Generic;
using UnityEngine;


public class Jack
{
    public enum JackState
    {
        OK,
        REELING,
        DEAD,
    }

    public class SwordFrame
    {
        public Vector2 hiltPos;
        public Vector2 tipPos;

        public float frameTime;
    }

    List<List<SwordFrame>> Swings;

    public Vector2 Position;
    public float Angle;

    public const float MaxMoveSpeed = 10.0f;

    const float SwordFade = 0.5f;
    const float MaxSwordAngularSpeed = 3.0f;

    public const float DrawRadius = 0.9f;
    public const float CollisionRadius = 1.1f;

    string textDisplayString = "";
    float textDisplayElapsedSeconds = 0.0f;
    float textDisplayScrollSpeed = 18.0f;
    float textDisplayStartTime = 0.8f;
    int textDisplayScrollPosition = 0;

    bool SwordSwinging;
    float swingElapsedTime;
    int swingIndex;
    Vector2 hiltPos;
    Vector2 tipPos;
    List<SwordFrame> recordedFrames;

    City myCity;
    float reelingTimeRemaining;
    const float MAX_REEL_DURATION = 1.5f;
    const float REEL_BLINK_DURATION = 0.2f;
    public JackState myState;
    public int hitPoints;

    public Jack (Vector2 pos, City city)
    {
        SetPosition(pos);
        makeSwings();
        myCity = city;
        recordedFrames = new List<SwordFrame>();
        hitPoints = 3;


    }

    void makeSwings() {
        Swings = new List<List<SwordFrame>>();
        List<SwordFrame> leftSwing = new List<SwordFrame>();
        SwordFrame lsf0 = new SwordFrame();
        lsf0.hiltPos = new Vector2(1.0f, -1.0f);
        lsf0.tipPos = new Vector2(2.0f, -1.5f);
        lsf0.frameTime = 0.0f;
        leftSwing.Add(lsf0);
        SwordFrame lsf1 = new SwordFrame();
        lsf1.hiltPos = new Vector2(1.0f, 1.0f);
        lsf1.tipPos = new Vector2(2.0f, 1.5f);
        lsf1.frameTime = 0.2f;
        leftSwing.Add(lsf1);
        Swings.Add(leftSwing);

        List<SwordFrame> rightSwing = new List<SwordFrame>();
        SwordFrame rsf0 = new SwordFrame();
        rsf0.hiltPos = new Vector2(1.0f, 1.0f);
        rsf0.tipPos = new Vector2(2.0f, 1.5f);
        rsf0.frameTime = 0.0f;
        rightSwing.Add(rsf0);
        SwordFrame rsf1 = new SwordFrame();
        rsf1.hiltPos = new Vector2(1.0f, -1.0f);
        rsf1.tipPos = new Vector2(2.0f, -1.5f);
        rsf1.frameTime = 0.2f;
        rightSwing.Add(rsf1);
        Swings.Add(rightSwing);
    }

    public void Update(float deltaSeconds)
    {
        if (myState == JackState.DEAD) {
            return;
        }

        if (myState == JackState.REELING) {
            reelingTimeRemaining -= deltaSeconds;

            if (reelingTimeRemaining <= 0.0f) {
                myState = JackState.OK;
            }
        }

        textDisplayElapsedSeconds += deltaSeconds;

        if (textDisplayElapsedSeconds < textDisplayStartTime) {
            textDisplayScrollPosition = 0;
        }
        else {
            float scrollTime = textDisplayElapsedSeconds - textDisplayStartTime;
            float scrolledPixels = scrollTime * textDisplayScrollSpeed;
            textDisplayScrollPosition = Mathf.FloorToInt(scrolledPixels);
        }

        MoveJack(deltaSeconds);

        if (!SwordSwinging) {
            if (Input.GetButtonDown("Fire1")) {
                SwordSwinging = true;
                swingElapsedTime = 0.0f;
                swingIndex = Random.Range(0, Swings.Count);
            }
        }
        else {
            swingElapsedTime += deltaSeconds;
            MoveSword();
            CheckSwordHit();
            RecordSwordFrame();
            if (CheckSwingDone()) {
                SwordSwinging = false;
            }
        }
        PurgeSwordFrames();
    }

    void MoveJack(float deltaSeconds)
    {
        float dx = Input.GetAxis("Horizontal");
        float dy = Input.GetAxis("Vertical");

        Vector2 inputVector = new Vector2(dx, dy);
        if (inputVector.magnitude > 1.0f) {
            float scaleFactor = 1.0f / inputVector.magnitude;
            inputVector *= scaleFactor;
        }
        else if (inputVector.magnitude < 0.1f) {
            return;
        }

        Vector2 newPos = Position + new Vector2(inputVector.x * MaxMoveSpeed * deltaSeconds, inputVector.y * MaxMoveSpeed * deltaSeconds);
        // TODO better collision detection

        if (!myCity.DoesDiskCollide(newPos, Jack.CollisionRadius)) {
            Vector2 deltaPos = newPos - Position;
            Angle = Mathf.Atan2(deltaPos.y, deltaPos.x);
            Position = newPos;
        }
    }

    void RecordSwordFrame()
    {
        SwordFrame thisFrame = new SwordFrame();
        thisFrame.hiltPos = this.hiltPos;
        thisFrame.tipPos = this.tipPos;
        thisFrame.frameTime = Time.realtimeSinceStartup;
        recordedFrames.Add(thisFrame);
    }

    void PurgeSwordFrames()
    {
        while(recordedFrames.Count > 0) {
            SwordFrame f = recordedFrames[0];
            if (Time.realtimeSinceStartup - f.frameTime > SwordFade) {
                recordedFrames.RemoveAt(0);
            }
            else {
                break;
            }
        }
    }

    void MoveSword() {
        List<SwordFrame> swingDesc = Swings[swingIndex];

        SwordFrame f;
        int prevIndex = -1;
        for (int i = 0; i < swingDesc.Count; ++i) {
            f = swingDesc[i];
            if (f.frameTime < swingElapsedTime) {
                prevIndex = i;
                break;
            }
        }
        if (prevIndex == -1) {
            // erm?
            prevIndex = 0;
        }
        int nextIndex = prevIndex + 1;

        if (nextIndex == swingDesc.Count) {
            InterpSwordFrames(prevIndex, prevIndex, 0.0f);
        }
        else {
            SwordFrame prevFrame = swingDesc[prevIndex];
            SwordFrame nextFrame = swingDesc[nextIndex];

            float deltaTimeBetweenFrames = nextFrame.frameTime - prevFrame.frameTime;
            float elapsedTimeSinceStartTime = swingElapsedTime - prevFrame.frameTime;
            InterpSwordFrames(prevIndex, nextIndex, elapsedTimeSinceStartTime / deltaTimeBetweenFrames);
        }
    }

    void InterpSwordFrames(int index0, int index1, float interpFactor) {
        List<SwordFrame> swingDesc = Swings[swingIndex];
        SwordFrame prevFrame = swingDesc[index0];
        SwordFrame nextFrame = swingDesc[index1];

        Vector2 hiltDelta = nextFrame.hiltPos - prevFrame.hiltPos;
        this.hiltPos = prevFrame.hiltPos + hiltDelta * interpFactor;

        Vector2 tipDelta = nextFrame.tipPos - prevFrame.tipPos;
        this.tipPos = prevFrame.tipPos + tipDelta * interpFactor;
    }

    bool CheckSwingDone() {
        List<SwordFrame> swingDesc = Swings[swingIndex];
        SwordFrame lastFrame = swingDesc[swingDesc.Count - 1];
        return (swingElapsedTime > lastFrame.frameTime);
    }

    Vector2 ConvertSwordCoordsToWorldCoords(Vector2 swordPos) {
        Quaternion rot = Quaternion.AngleAxis(this.Angle * 180.0f / Mathf.PI, new Vector3(0, 0, 1));
        Vector2 rotatedSwordPos = rot * swordPos;
        return rotatedSwordPos + Position;
    }

    bool CheckSwordHit() {
        Vector2 swordTipInWorld = ConvertSwordCoordsToWorldCoords(tipPos);
        Vector2 swordHiltInWorld = ConvertSwordCoordsToWorldCoords(hiltPos);
        Vector2 swordMidInWorld = (swordTipInWorld + swordHiltInWorld) / 2.0f;

        List<Vector2> points = new List<Vector2>{swordTipInWorld, swordHiltInWorld, swordMidInWorld};

        bool anyHit = false;
        foreach (Beetle b in myCity.Beetles) {
            if (b.myState != Beetle.State.Alive) {
                continue;
            }

            foreach (Vector2 w in points) {
                float tipToBeetleDist = (w - b.Position).magnitude;
                if (tipToBeetleDist < Beetle.CollisionRadius) {
                    b.Hit();
                    anyHit = true;
                    break;
                }
            }
        }
        return anyHit;
    }

    public void Draw(Color[] pixels, float cityScale)
    {
        DrawHitPoints(pixels);
        // always assumes Jack's in the center of the frame

        Color jackColor = Color.white;

        if (myState == JackState.REELING) {
            float blinkFrac = reelingTimeRemaining % REEL_BLINK_DURATION;
            if (blinkFrac < (REEL_BLINK_DURATION / 2.0f)) {
                jackColor = Color.red;
            }
            else {
                jackColor = Color.black;
            }
        }
         
        float sizeSqr = DrawRadius * DrawRadius * cityScale * cityScale;

        for (int x = 0; x < 64; ++x) {
            float dx = x - 32;
            float dx2 = dx * dx;
            for (int y = 0; y < 64; ++y) {
                float dy = y - 32;
                float dy2 = dy * dy;
                if (sizeSqr > (dx2 + dy2)) {
                    pixels[x + y * 64] = jackColor;
                }
            }
        }

        if (myState != JackState.DEAD) {
            foreach (SwordFrame f in recordedFrames) {
                float elapsedTime = Time.realtimeSinceStartup - f.frameTime;
                float alpha = 1.0f - elapsedTime / SwordFade;
                DrawSwordFrame(f, pixels, cityScale, alpha);
            }
        }

        TextRender.DrawString(pixels, Globals.FontTex, 1 - textDisplayScrollPosition, 1, Color.white, textDisplayString);
    }

    void DrawHitPoints(Color[] pixels) {
        for (int i = 0; i < hitPoints; ++i) {
            int right = 62 - 3 * i;
            int left = right - 1;
            int top = 62;
            int bottom = 61;

            DrawBox(pixels, left, top, right, bottom, Color.white);
        }
    }

    void DrawBox(Color[] pixels, int left, int top, int right, int bottom, Color drawColor)
    {
        for (int x = left; x <= right; ++x) {
            for (int y = bottom; y <= top; ++y) {
                pixels[x + 64 * y] = drawColor;
            }
        }
    }

    void DrawSwordFrame(SwordFrame f, Color[] pixels, float cityScale, float alpha)
    {
        Vector2 worldTipPos = ConvertSwordCoordsToWorldCoords(f.tipPos);
        Vector2 worldHiltPos = ConvertSwordCoordsToWorldCoords(f.hiltPos);
        Vector2Int screenTipPos = myCity.WorldToScreen(worldTipPos);
        Vector2Int screenHiltPos = myCity.WorldToScreen(worldHiltPos);
        FakeBresenham(screenTipPos, screenHiltPos, pixels, Color.white, alpha);
        //Bresenham(screenTipPos, screenHiltPos, pixels, Color.white, alpha);
    }

    void FakeBresenham(Vector2Int p0, Vector2Int p1, Color[] pixels, Color drawColor, float alpha)
    {
        int dx = p1.x - p0.x;
        int dy = p1.y - p0.y;

        if ((dx == 0) && (dy == 0))
        {
            DrawPoint(p0, pixels, drawColor, alpha);
        }

        int numSteps = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
        for (int i = 0; i<=numSteps; ++i)
        {
            float frac = ((float)i) / numSteps;

            int px = Mathf.RoundToInt(dx * frac);
            int py = Mathf.RoundToInt(dy * frac);

            Vector2Int point = new Vector2Int(p0.x + px, p0.y + py);
            DrawPoint(point, pixels, drawColor, alpha);
        }
    }

    void DrawPoint(Vector2Int point, Color[] pixels, Color drawColor, float alpha)
    {
        Color oldColor = pixels[point.x + 64 * point.y];
        pixels[point.x + 64 * point.y] = (1 - alpha) * oldColor + alpha * drawColor;
    }

    void Bresenham(Vector2Int p0, Vector2Int p1, Color[] pixels, Color drawColor, float alpha)  {
        if (Mathf.Abs(p0.x - p1.x) > Mathf.Abs(p0.y - p1.y)) {
            BresX(p0, p1, pixels, drawColor, alpha);
        }
        else {
            BresY(p0, p1, pixels, drawColor, alpha);
        }
    }

    void BresX(Vector2Int p0, Vector2Int p1, Color[] pixels, Color drawColor, float alpha)  {
        int dx = p1.x - p0.x;
        if (dx < 0) {
            BresX(p1, p0, pixels, drawColor, alpha);
        }
        int dy = p1.y - p0.y;
        float yOverX = ((float)dy / dx);

        for (int x = p0.x; x <= p1.x; x++) {
            int elapsedX = x - p0.x;
            float fracX = ((float)elapsedX) / dx;
            float fracY = fracX * yOverX;
            int elapsedY = Mathf.RoundToInt(fracY * dy);
            int y = elapsedY + p0.y;

            Color oldPx = pixels[x + 64 * y];
            pixels[x + 64 * y] = oldPx * (1.0f - alpha) + drawColor * alpha;
        }
    }

    void BresY(Vector2Int p0, Vector2Int p1, Color[] pixels, Color drawColor, float alpha)  {
        int dy = p1.y - p0.y;
        if (dy < 0) {
            BresY(p1, p0, pixels, drawColor, alpha);
        }
        int dx = p1.x - p0.x;
        float xOverY = ((float)dx / dy);

        for (int y = p0.y; y <= p1.y; y++) {
            int elapsedY = y - p0.y;
            float fracY = ((float)elapsedY) / dy;
            float fracX = fracY * xOverY;
            int elapsedX = Mathf.RoundToInt(fracX * dx);
            int x = elapsedX + p0.x;

            Color oldPx = pixels[x + 64 * y];
            pixels[x + 64 * y] = oldPx * (1.0f - alpha) + drawColor * alpha;
        }
    }

    public void SetPosition(Vector2 pos) {
        this.Position = pos;
    }

    public void Hit()
    {
        hitPoints--;
        if (hitPoints == 0) {
            myState = JackState.DEAD;
            return;
        }
        else {
            myState = JackState.REELING;
            reelingTimeRemaining = MAX_REEL_DURATION;
        }
    }

    public void ShowText(string s) 
    {
        Debug.Log("Show Text: " + s);
        textDisplayString = s;
        textDisplayElapsedSeconds = 0.0f;
    }
}

