using System.Collections.Generic;
using UnityEngine;

public class Beetle
{
    public enum State {
        Alive,
        Dead,
    };

    City myCity;
    public Vector2 Position;
    float heading = 0.0f;
    float mySpeed = 8.0f;
    float intersectionTolerance = 2.0f;

    int nextIntersection;

    // todo probably make this around 60?
    public const float NearJackThreshold = 5.0f;

    public State myState;

    public const float CollisionRadius = 2.0f;

    public Beetle (City c)
    {
        myCity = c;
        myState = State.Alive;
        nextIntersection = -1;
    }

    public void SpawnAt(Vector2 pos)
    {
        this.Position = pos;
    }


    bool IsNearJack() {
        Vector2 relPos = myCity.Jack.Position - this.Position;
        return relPos.magnitude < NearJackThreshold;
    }

    public void Update(float elapsedSeconds) {
        if (myState == State.Dead) {
            return;
        }

        if (myCity.Jack.myState != Jack.JackState.OK) {
            UpdateWithdraw(elapsedSeconds);
        }
        else if (IsNearJack()) {
            UpdateNear(elapsedSeconds);
        }
        else {
            UpdateFar(elapsedSeconds);
        }

        if ((myCity.Jack.myState == Jack.JackState.OK) && (HitJack())) {
            myCity.Jack.Hit();
        }
    }

    bool HitJack() {
        Vector2 jackPos = myCity.Jack.Position;
        float dist = (jackPos - Position).magnitude;
        return dist < (CollisionRadius + Jack.CollisionRadius);
    }

    void UpdateFar(float elapsedSeconds) {
        if (Random.Range(0, 5) == 0) {
            UpdateNear(elapsedSeconds);
            return;
        }

        bool shouldFindPath = false;

        if (nextIntersection < 0) {
            //Debug.Log("no intersection, pathing");
            shouldFindPath = true;
        }
        else {
            Vector2 destPoint = myCity.intersections[nextIntersection];
            Vector2 vectorTowardsGoal = destPoint - Position;
            float distToDest = vectorTowardsGoal.magnitude;

            if (distToDest < elapsedSeconds * mySpeed) {
                //Debug.Log("now arriving");
                nextIntersection = -1;
                MoveTo(destPoint);
            }
            else {
                MoveTo(Position + vectorTowardsGoal.normalized * (mySpeed * elapsedSeconds));
            }
            return;
        }

        if (shouldFindPath) {
            //Debug.Log("pathfind flagged");
            List<int> intersections = this.FindPath(myCity.Jack.Position);
            //Debug.Log("path: " + intersections);

            if (intersections == null) {
                //Debug.Log("no path");
                nextIntersection = -1;
            }
            else {
                while (intersections.Count > 0) {
                    Vector2 nextInt = myCity.intersections[intersections[0]];
                    float dist = (nextInt - Position).magnitude;
                    if (dist < intersectionTolerance) {
                        intersections.RemoveAt(0);
                        continue;
                    }
                    else {
                        // found good dest
                        break;
                    }
                }
                if (intersections.Count > 0) {
                    nextIntersection = intersections[0];
                    //Debug.Log("next up:" + nextIntersection);
                }
                else {
                    //Debug.Log("I don't know - picking randomly");
                    float distToIntersection;
                    int closestPoint = myCity.FindClosestIntersection(Position, -1, out distToIntersection);
                    if (distToIntersection > intersectionTolerance) {
                        nextIntersection = closestPoint;
                    }
                    else {
                        List<int> neighbors = myCity.GetIntersectionNeighbors(closestPoint);
                        int randIndex = Random.Range(0, neighbors.Count);
                        nextIntersection = neighbors[randIndex];
                    }
                }
            }
        }
    }

    void UpdateNear(float elapsedSeconds) {
        /*
         * if jack and I am in the same segment, head straight toward jack, otherwise find a path to Jack and follow it
         * 
         */

        Vector2 deltaTowardJack = myCity.Jack.Position - Position;
        Vector2 normDir = deltaTowardJack.normalized;
        Vector2 newPos = Position + normDir * (mySpeed * elapsedSeconds);
        if (!myCity.DoesDiskCollide(newPos, CollisionRadius)) {
            MoveTo(newPos);
        }
    }

    void UpdateWithdraw(float elapsedSeconds) {
        /*
         * back away
         */

        Vector2 deltaTowardJack = myCity.Jack.Position - Position;
        Vector2 normDir = deltaTowardJack.normalized;
        Vector2 newPos = Position - (normDir * (mySpeed * elapsedSeconds));
        if (!myCity.DoesDiskCollide(newPos, CollisionRadius)) {
            MoveTo(newPos);
        }
    }

    void MoveTo(Vector2 pos) {
        Vector2 delta = pos - Position;
        Position = pos;
        heading = Mathf.Atan2(delta.y, delta.x);

        //Debug.Log("heading:" + heading);
    }


    public void Draw(Color[] screenBuf, Vector2 camPos, float cityScale) {

        // todo draw a sprite, instead

        if (myState == State.Dead) {
            return;
        }

        float screenRadius = CollisionRadius * cityScale;
        float boundRadius = screenRadius * 1.5f;

        float sx = (Position.x - camPos.x) * cityScale + 32;
        float sy = (Position.y - camPos.y) * cityScale + 32;

        int left = Mathf.FloorToInt(sx - boundRadius);
        left = Mathf.Max(0, left);

        int right = Mathf.CeilToInt(sx + boundRadius);
        right = Mathf.Min(right, 63);

        int top = Mathf.FloorToInt(sy - boundRadius);
        top = Mathf.Max(0, top);

        int bottom = Mathf.CeilToInt(sy + boundRadius);
        bottom = Mathf.Min(bottom, 63);

        Vector3 z = new Vector3(0, 0, 1);
        float headingDegrees = -heading * 180.0f / Mathf.PI;
        headingDegrees += 90.0f;

        Quaternion hQuat = Quaternion.AngleAxis(headingDegrees, z);
        //Debug.Log("heading: " + heading);
        //Debug.Log("hQuat: " + hQuat);

        Color32[] spriteColors = Globals.BeetleSpriteTex.GetPixels32();

        for (int x = left; x <= right; ++x) {
            for (int y = top; y <= bottom; ++y) {
                Vector2 deltaPos = new Vector2(x - sx, y - sy);
                //if (deltaPos.magnitude < screenRadius) {
                Vector2 normPos = deltaPos / screenRadius;
                Vector2 rotPos = hQuat * normPos;

                int bx = Mathf.RoundToInt(rotPos.x * 16 + 16);
                int by = Mathf.RoundToInt(rotPos.y * 16 + 16);

                if ((bx < 0) || (bx >= 32) || (by < 0) || (by >= 32)) {
                    continue;
                }

                Color32 c = spriteColors[bx + by * 32];
                if (c.a < 128) {
                    continue;
                }

                screenBuf[64 * y + x] = c;
                //}
            }
        }
    }

    List<int> FindPath(Vector2 endPoint) {
        float dummy;
        int myIntersectionPoint = myCity.FindClosestIntersection(Position, -1, out dummy);
        int destIntersectionPoint = myCity.FindClosestIntersection(endPoint, -1, out dummy);

        //Debug.Log(string.Format("I want to find a path between {0} and {1}", myIntersectionPoint, destIntersectionPoint));
        return FindPathBetweenIntersectionIndices(myIntersectionPoint, destIntersectionPoint);
    }

    class PathRecord : System.IComparable<PathRecord> {
        public List<int> pathSoFar;
        City city;
        int destIndex;
        float cachedDist = -1;
        float cachedHeuristic = -1;

        public PathRecord(City city, int destIndex, List<int> prevPath)
        {
            //Debug.Assert(city != null);
            this.city = city;
            pathSoFar = new List<int>();
            pathSoFar.AddRange(prevPath);
            this.destIndex = destIndex;
        }

        public float calcDistSoFar() {
            if (cachedDist > 0) {
                return cachedDist;
            }
            //Debug.Assert(city != null);
            float dist = 0.0f;
            for (int i = 1; i < pathSoFar.Count; ++i) {
                Vector2 p0 = city.intersections[pathSoFar[i - 1]];
                Vector2 p1 = city.intersections[pathSoFar[i]];
                dist += (p1 - p0).magnitude;
            }
            cachedDist = dist;
            return dist;
        }

        public float heuristicDistToGo() {
            if (cachedHeuristic > 0) {
                return cachedHeuristic;
            }
            if (pathSoFar.Count == 0) {
                return float.MaxValue;
            }
            int index = pathSoFar.Count - 1;
            Vector2 lastPoint = city.intersections[pathSoFar[index]];
            Vector2 destPoint = city.intersections[destIndex];
            cachedHeuristic =  (lastPoint - destPoint).magnitude;
            return cachedHeuristic;
        }

        public int CompareTo(PathRecord other) {
            float thisVal = this.calcDistSoFar() + this.heuristicDistToGo();
            float otherVal = other.calcDistSoFar() + other.heuristicDistToGo();
            return thisVal.CompareTo(otherVal);
        }

        public override string ToString()
        {
            string s = "[PathRecord ";
            foreach (int i in pathSoFar) {
                s += i.ToString();
                s += " ";
            }
            s += "]";
            return s;
        }
    }

    List<int> FindPathBetweenIntersectionIndices(int i0, int i1) {
        int ITERCOUNT = 30;

        List<PathRecord> openPaths = new List<PathRecord>();
        List<int> startPath = new List<int>();
        startPath.Add(i0);
        PathRecord start = new PathRecord(myCity, i1, startPath);
        openPaths.Add(start);

        int iterationsSoFar = 0;

        while(openPaths.Count > 0) {
            if (iterationsSoFar > ITERCOUNT) {
                //Debug.Log("ITER MAX");
                return openPaths[0].pathSoFar;
            }
            iterationsSoFar ++;

            PathRecord test = openPaths[0];
            openPaths.RemoveAt(0);

            //Debug.Log("have a candidate path " + test);

            int pathLen = test.pathSoFar.Count;
            int lastIndex = test.pathSoFar[pathLen - 1];

            //Debug.Log("path steps : " + pathLen);
            //Debug.Log("last step : " + lastIndex);

            List<int> neighbors = myCity.GetIntersectionNeighbors(lastIndex);

            //Debug.Log(string.Format("found {0} neighbors", neighbors.Count));

            foreach (int n in neighbors) {
                if (!test.pathSoFar.Contains(n)) {
                    PathRecord next = new PathRecord(myCity, i1, test.pathSoFar);
                    next.pathSoFar.Add(n);
                    if (n == i1) {
                        return next.pathSoFar;
                    }
                    //Debug.Log("Adding new path with step " + n);
                    openPaths.Add(next);
                }
            }
            openPaths.Sort();
            //Debug.LogFormat("have {0} paths", openPaths.Count);
            //Debug.Log("breaking out for testing");
            //break;
        }
        //Debug.Log("returning null because ran out of paths");
        return null;
    }

    public void Hit() {
        // one hit?
        myState = State.Dead;
        myCity.RevealClue();
    }
}

