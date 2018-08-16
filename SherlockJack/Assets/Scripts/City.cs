using System.Collections.Generic;
using UnityEngine;

public class StreetSegment
{
    public Vector2[] Endpoints;
    public float Width;

    // padded to include width
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;

    public int[] EndpointIndices;

    public StreetSegment(Vector2 pt1, Vector2 pt2, float width)
    {
        Endpoints = new Vector2[]{ pt1, pt2 };
        Width = width;
        minX = Mathf.Min(pt1.x, pt2.x) - width;
        maxX = Mathf.Max(pt1.x, pt2.x) + width;
        minY = Mathf.Min(pt1.y, pt2.y) - width;
        maxY = Mathf.Max(pt1.y, pt2.y) + width;
    }

    public bool Contains(Vector2 pt)
    {
        //Debug.Log("checking containment for " + pt);

        if ((pt.x < minX) || (pt.x > maxX) || (pt.y < minY) || (pt.y > maxY)) {
            return false;
        }
        if (((pt - Endpoints[0]).magnitude < Width) ||
            ((pt - Endpoints[1]).magnitude < Width)) {
            return true;
        }

        float distToSeg = DistToSeg(pt);
        return (distToSeg < Width);
    }

    public float DistToSeg(Vector2 pt) 
    {
        float streetLenSqr = (Endpoints[1] - Endpoints[0]).sqrMagnitude;
        if (streetLenSqr == 0.0f) {
            return (pt - Endpoints[0]).magnitude;
        }

        float dot = DotProduct(pt - Endpoints[0], Endpoints[1] - Endpoints[0]);
        float t = Mathf.Max(0.0f, Mathf.Min(1.0f, dot / streetLenSqr));
        Vector2 proj = Endpoints[0] + t * (Endpoints[1] - Endpoints[0]);
        return (pt - proj).magnitude; 
    }

    float DotProduct(Vector2 v1, Vector2 v2)
    {
        return v1.x * v2.x + v1.y * v2.y;
    }

    public Vector2 InterpolateEndpoints(float t) {
        float dx = Endpoints[1].x - Endpoints[0].x;
        float dy = Endpoints[1].y - Endpoints[0].y;

        return new Vector2(Endpoints[0].x + dx * t, Endpoints[0].y + dy * t);
    }

    public void SetEndpointIndices(int i1, int i2)
    {
        this.EndpointIndices = new int[]{ i1, i2 };
    }
}

public class City
{
    const bool USE_PARTITION = true;

    // TODO for now, just store the streets as a list, eventually replace with a quadtree
    public List<StreetSegment> Streets;

    public GridCityPartition streetPartition;

    public Color[] Pixels;

    public float scale = 3.0f;

    public List<Vector2> intersections;

    public Dictionary<int, List<int>> intersectionToSegmentDict;

    const float MIN_STREET_WIDTH = 2.5f;
    const float MAX_STREET_WIDTH = 6.5f;

    const float MIN_INTERSECTION_ANGLE_DEGREES = 60.0f;

    public List<Beetle> Beetles;
    public List<Clue> Clues;

    public Jack Jack;

    float spawnTimer;
    const float MAX_SPAWN_TIME = 10.0f;

    public Portal portal;

    public City ()
    {
        Streets = new List<StreetSegment>();
        //makeStreets();
        streetPartition = new GridCityPartition();
        intersectionToSegmentDict = new Dictionary<int, List<int>>();
        makeRandomStreets();

        foreach (StreetSegment seg in Streets) {
            streetPartition.AddStreet(seg);
        }

        Pixels = new Color[64 * 64];
    }

    public void Start(int numClues) {
        portal = new Portal(GetRandomPointOnStreet(), this);

        Vector2 jackPos = Vector2.zero;
        SpawnJack(jackPos);
        Beetles = new List<Beetle>();
        SpawnBeetles(2, jackPos);
        SpawnClues(numClues, jackPos);

        spawnTimer = 2 * MAX_SPAWN_TIME;
    }

    void makeRandomStreets()
    {
        const int INTERSECTION_COUNT = 30;
        const int MAX_BRANCHES = 5;
        const float MIN_STREET_LENGTH = 25.0f;
        const float MAX_STREET_LENGTH = 50.0f;

        int MAX_ITERATIONS = 1000;
        int iterations = 0;

        intersections = new List<Vector2>();

        intersections.Add(Vector2.zero);

        int[] branchesPerIntersection = new int[INTERSECTION_COUNT];

        while((intersections.Count < INTERSECTION_COUNT) && (iterations < MAX_ITERATIONS)) {
            iterations++;

            int oldIndex = Random.Range(0, intersections.Count);
            //Debug.Log("picked index " + oldIndex);

            if (branchesPerIntersection[oldIndex] >= MAX_BRANCHES) {
                //Debug.Log("has too many branches already");
                continue;
            }

            Vector2 oldIntersection = intersections[oldIndex];
            //Debug.Log("old intersection pos: " + oldIntersection);

            float candidateAngle = Random.Range(0, Mathf.PI * 2);
            float candidateDistance = Random.Range(MIN_STREET_LENGTH, MAX_STREET_LENGTH);
            Vector2 candidateOffset = new Vector2(Mathf.Cos(candidateAngle) * candidateDistance, Mathf.Sin(candidateAngle) * candidateDistance);

            //Debug.Log("testing candidate angle on the old intersection");
            if (WithinExistingAngles(oldIndex, candidateAngle)) {
                //Debug.Log("too close to an existing angle on the old intersection");
                continue;
            }

            Vector2 candidatePos = oldIntersection + candidateOffset;

            int joinIndex = FindAnyIntersectionWithin(candidatePos, candidateOffset.magnitude * 0.7f);
            if (joinIndex >= 0) {
                //Debug.Log("considering joining with " + joinIndex);

                if (AreIntersectionsJoined(oldIndex, joinIndex)) {
                    //Debug.Log("already joined, trying again");
                    continue;
                }
                    
                candidatePos = intersections[joinIndex];
                Vector2 candidateStreetVector = candidatePos - oldIntersection;
                //Debug.Log("Considering existing location " + candidatePos);

                candidateAngle = Mathf.Atan2(candidateStreetVector.y, candidateStreetVector.x);

                //Debug.Log("testing candidate angle on the new candidate intersection");
                if (WithinExistingAngles(joinIndex, candidateAngle + Mathf.PI)) {
                    //Debug.Log("too close to an existing angle on the new intersection");
                    continue;
                }

                //if (FindAnyIntersectionWithin(candidatePos, MIN_STREET_LENGTH)) {
                //    continue;
                //}


                if (candidateStreetVector.magnitude > MAX_STREET_LENGTH) {
                    //Debug.Log("other location is too far away");
                    continue;
                }

                if (candidateStreetVector.magnitude < MIN_STREET_LENGTH) {
                    //Debug.Log("other location is too close");
                    continue;
                }

                if (branchesPerIntersection[joinIndex] > MAX_BRANCHES) {
                    //Debug.Log("candidate intersection is already full");
                    continue;
                }

                //Debug.Log(string.Format("joining {0} and {1}", oldIndex, joinIndex));
                JoinIntersections(oldIndex, joinIndex, branchesPerIntersection);
                continue;
            }

            //Debug.Log("adding new intersection");
            intersections.Add(candidatePos);
            int intersectionIndex = intersections.Count - 1;
            intersectionToSegmentDict[intersectionIndex] = new List<int>();
            JoinIntersections(oldIndex, intersections.Count - 1, branchesPerIntersection);
        }
    }

    float Wrap(float val, float wrapMag) {
        while(val >= wrapMag) {
            val -= wrapMag;
        }
        while(val < 0) {
            val += wrapMag;
        }
        return val;
    }

    bool WithinExistingAngles(int intersectionIndex, float candidateAngleRadians) {
        candidateAngleRadians = Wrap(candidateAngleRadians, 2 * Mathf.PI);

        //Debug.Log("Trying angle " + candidateAngleRadians);

        Vector2 myPos = intersections[intersectionIndex];

        float minStreetAngleRadians = MIN_INTERSECTION_ANGLE_DEGREES * Mathf.PI / 180.0f;

        if (!intersectionToSegmentDict.ContainsKey(intersectionIndex)) {
            //Debug.Log("no segments for this intersection, no overlap");
            return false;
        }

        foreach (int segIndex in intersectionToSegmentDict[intersectionIndex]) {
            int otherIntersectionIndex = -1;
            StreetSegment seg = Streets[segIndex];
            if (seg.EndpointIndices[0] == intersectionIndex) {
                otherIntersectionIndex = seg.EndpointIndices[1];
            } else {
                otherIntersectionIndex = seg.EndpointIndices[0];
            }

            Vector2 otherPos = intersections[otherIntersectionIndex];
            Vector2 deltaPos = otherPos - myPos;
            float angleToOther = Mathf.Atan2(deltaPos.y, deltaPos.x);
            //Debug.Log("existing angle:" + angleToOther);

            float deltaAngle = Wrap(angleToOther - candidateAngleRadians, 2*Mathf.PI);
            if (deltaAngle > Mathf.PI) { 
                deltaAngle -= 2 * Mathf.PI;
            }
            //Debug.Log("delta angle:" + deltaAngle);
            //Debug.Log("delta angle degrees: " + deltaAngle * 180 / Mathf.PI);
            if (Mathf.Abs(deltaAngle) < minStreetAngleRadians) {
                //Debug.Log("deltaAngle too close, reporting overlap");
                return true;
            }
        }
        //Debug.Log("no overlap");
        return false;
    }

    public int FindClosestIntersection(Vector2 newPt, int baseIndex, out float bestDist) {
        bestDist = float.MaxValue;
        int bestIndex = -1;

        for (int i = 0; i < intersections.Count; ++i) {
            if (i == baseIndex) {
                continue;
            }
            float dist = (newPt - intersections[i]).magnitude;
            if (dist < bestDist) {
                bestIndex = i;
                bestDist = dist;
            }
        }
        return bestIndex;
    }

    int FindAnyIntersectionWithin(Vector2 newPt, float minDist) {
        for (int i = 0; i < intersections.Count; ++i) {
            float dist = (newPt - intersections[i]).magnitude;
            if (dist < minDist) {
                return i;
            }
        }
        return -1;
    }

    void JoinIntersections(int index1, int index2, int[] branchesPerIntersection) {
        StreetSegment seg = new StreetSegment(intersections[index1], intersections[index2], Random.Range(MIN_STREET_WIDTH, MAX_STREET_WIDTH));
        seg.SetEndpointIndices(index1, index2);
        Streets.Add(seg);
        branchesPerIntersection[index1]++;
        branchesPerIntersection[index2]++;
        int streetIndex = Streets.Count - 1;
        if (!intersectionToSegmentDict.ContainsKey(index1)) {
            intersectionToSegmentDict[index1] = new List<int>();
        }
        if (!intersectionToSegmentDict.ContainsKey(index2)) {
            intersectionToSegmentDict[index2] = new List<int>();
        }
        intersectionToSegmentDict[index1].Add(streetIndex);
        intersectionToSegmentDict[index2].Add(streetIndex);
    }

    bool AreIntersectionsJoined(int i0, int i1) {
        if (!intersectionToSegmentDict.ContainsKey(i0)) {
            return false;
        }
        foreach (int segIndex in intersectionToSegmentDict[i0]) {
            StreetSegment seg = Streets[segIndex];
            if (((seg.EndpointIndices[0] == i0) &&
                (seg.EndpointIndices[1] == i1)) ||
                ((seg.EndpointIndices[0] == i1) &&
                (seg.EndpointIndices[1] == i0))) {
                return true;
            }
        }
        return false;
    }

    void makeStreets()
    {
        Streets.Add(new StreetSegment(new Vector2(0.0f, 0.0f),
            new Vector2(30.0f, 25.0f),
            5.0f));

        Streets.Add(new StreetSegment(new Vector2(0.0f, 0.0f),
            new Vector2(-30.0f, -20.0f),
            5.0f));

/*        Streets.Add(new StreetSegment(new Vector2(0.0f, 0.0f),
            new Vector2(30.0f, -30.0f),
            3.0f));*/

        Streets.Add(new StreetSegment(new Vector2(0.0f, 0.0f),
            new Vector2(-30.0f, 27.0f),
            3.0f));
    }

    public void Update(float elapsedSeconds) {
        spawnTimer -= elapsedSeconds;
        //Debug.Log("spawn timer:" + spawnTimer);
        if (spawnTimer <= 0.0f) {
            spawnTimer = MAX_SPAWN_TIME;
            SpawnBeetles(1, Jack.Position);
        }
    }

    void SpawnJack(Vector2 JackPos)
    {
        this.Jack = new Jack(JackPos, this);
    }

    void SpawnBeetles(int count, Vector2 JackPos)
    {
        int spawnCount = 0;

        while (spawnCount < count) {
            Vector2 randPos = GetRandomPointOnStreet();
            if ((randPos - JackPos).magnitude < Beetle.NearJackThreshold) {
                continue;
            }
            Beetle b = new Beetle(this);
            b.SpawnAt(randPos);
            //Debug.Log("spawning beetle at " + randPos);
            Beetles.Add(b);
            spawnCount += 1;
        }
    }

    void SpawnClues(int count, Vector2 JackPos)
    {
        Clues = new List<Clue>();

        while (Clues.Count < count) {
            Vector2 randPos = GetRandomPointOnStreet();
            if ((randPos - JackPos).magnitude < Beetle.NearJackThreshold) {
                continue;
            }
            Clue c = new Clue(this);
            c.SpawnAt(randPos);
            //Debug.Log("spawning clue at " + randPos);
            Clues.Add(c);
        }
    }

    public Vector2 GetRandomPointOnStreet()
    {
        int streetIndex = Random.Range(0, Streets.Count);
        StreetSegment seg = Streets[streetIndex];
        Vector2 randPos = seg.InterpolateEndpoints(Random.Range(0.0f, 1.0f));
        return randPos;
    }

    public void Render(Vector2 camPos)
    {
        Color brickRed = new Color(0.5f, 0.0f, 0.0f);
        Color darkerRed = new Color(0.25f, 0.0f, 0.0f);

        List<StreetSegment> segsNearCam;

        if (USE_PARTITION) {
            segsNearCam = streetPartition.GetSegmentsNear(camPos, 20.0f);
            //Debug.Log("seg count" + segsNearCam.Count);
        }

        for (int x = 0; x < 64; ++x) {
            for (int y = 0; y < 64; ++y) {
                int nsx = x - 32;
                int nsy = y - 32;

                float wx = camPos.x + nsx / scale;
                float wy = camPos.y + nsy / scale;

                Color pCol = darkerRed;

                if (!USE_PARTITION) {
                    foreach (StreetSegment seg in Streets) {
                        Vector2 worldPos = new Vector2(wx, wy);
                        if (seg.Contains(worldPos)) {
                            pCol = GetStreetColor(worldPos);
                            break;
                        }
                    }
                }
                else {
                    foreach (StreetSegment seg in segsNearCam) {
                        Vector2 worldPos = new Vector2(wx, wy);
                        if (seg.Contains(worldPos)) {
                            pCol = GetStreetColor(worldPos);
                            break;
                        }
                    }
                }
                Pixels[y * 64 + x] = pCol;
            }
        }
    }

    float MapValue(float inVal, float inMin, float inMax, float outMin, float outMax)
    {
        float n = (inVal - inMin) / (inMax - inMin);
        return (n * (outMax - outMin) + outMin);
    }

    Color GetStreetColor(Vector2 worldPos)
    {
        float offsetX = 1022f;
        float offsetY = 22332f;

        float val = Mathf.PerlinNoise(worldPos.x + offsetX, worldPos.y + offsetY);

        float bright = MapValue(val, 0, 1, 0.3f, 0.6f);
        return new Color(bright, bright, bright);
    }

    public bool DoesDiskCollide(Vector2 center, float radius)
    {
        // if the disk is contained in any street, return false

        foreach (StreetSegment seg in Streets) {
            if (seg.DistToSeg(center) + radius < seg.Width) {
                return false;
            }
        }

        return true;
    }

    public void SetJackPos(Vector2 pos) {
        this.Jack.SetPosition(pos);
    }

    public List<int> GetIntersectionNeighbors(int intersectionIndex) {
        List<int> outList = new List<int>();
        foreach (int segmentIndex in this.intersectionToSegmentDict[intersectionIndex]) {
            StreetSegment seg = this.Streets[segmentIndex];
            int i0 = seg.EndpointIndices[0];
            int i1 = seg.EndpointIndices[1];

            int unselected = -1;
            if (i0 == intersectionIndex) {
                unselected = i1;
            }
            else {
                unselected = i0;
            }
            if (!outList.Contains(unselected)) {
                outList.Add(unselected);
            }
        }
        return outList;
    }

    public Vector2Int WorldToScreen(Vector2 worldPos) {
        float sx = (worldPos.x - Jack.Position.x) * scale + 32;
        float sy = (worldPos.y - Jack.Position.y) * scale + 32;

        return new Vector2Int(Mathf.RoundToInt(sx), Mathf.RoundToInt(sy));
    }

    public void RevealClue()
    {
        foreach (Clue c in Clues) {
            if (c.myState == Clue.State.Hidden) {
                c.myState = Clue.State.Revealed;
                return;
            }
        }
    }
}

