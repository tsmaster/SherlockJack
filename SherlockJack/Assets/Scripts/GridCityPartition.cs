using System.Collections.Generic;
using UnityEngine;

class GridAddress {
    public const float Resolution = 15.0f;

    public int x;
    public int y;

    public GridAddress (float x, float y)
    {
        this.x = Mathf.FloorToInt(x / Resolution);
        this.y = Mathf.FloorToInt(y / Resolution);
    }

    public GridAddress (Vector2 pos) : this(pos.x, pos.y)
    {
    }

    public override bool Equals(object otherObj)
    {
        GridAddress otherAddress = otherObj as GridAddress;
        if (otherAddress == null) {
            return false;
        }
        return ((otherAddress.x == this.x) && (otherAddress.y == this.y));
    }

    public override int GetHashCode()
    {
        const int mult = 32767;
        return this.x + this.y * mult;
    }

    public override string ToString()
    {
        return string.Format("[GridAddress {0} {1}]", this.x, this.y);
    }
}


public class GridCityPartition
{
    Dictionary<GridAddress, List<int>> addressToStreetSegmentIdListDict;
    Dictionary<int, StreetSegment> StreetSegmentIdToStreetSegmentDict;

    int nextStreetSegmentId;

    // the number of world units in a single tile
    public GridCityPartition ()
    {
        addressToStreetSegmentIdListDict = new Dictionary<GridAddress, List<int>>();
        StreetSegmentIdToStreetSegmentDict = new Dictionary<int, StreetSegment>();

        nextStreetSegmentId = 100;
    }

    public void AddStreet(StreetSegment seg)
    {
        AddStreetRange(seg, 0.0f, 1.0f);
    }

    void AddStreetRange(StreetSegment seg, float f0, float f1)
    {
        //Debug.Log(string.Format("Adding segment with values {0} {1}", f0, f1));

        Vector2 v0 = seg.InterpolateEndpoints(f0);
        Vector2 v1 = seg.InterpolateEndpoints(f1);

        GridAddress a0 = new GridAddress(v0);
        GridAddress a1 = new GridAddress(v1);

        if (a0 == a1) {
            //Debug.Log(string.Format("Adding segment at location {0}", a0));
            AddStreetAtPoint(seg, v0);
        }
        else
        if ((Mathf.Abs(a0.x - a1.x) <= 1) &&
                 (Mathf.Abs(a0.y - a1.y) <= 1)) {
            //Debug.Log(string.Format("Adding segment at locations {0} and {1}", a0, a1));
            AddStreetAtPoint(seg, v0);
            AddStreetAtPoint(seg, v1);
        }
        else {
            float fh = (f0 + f1) / 2.0f;
            AddStreetRange(seg, f0, fh);
            AddStreetRange(seg, fh, f1);
        }
    }

    public void AddStreetAtPoint(StreetSegment seg, Vector2 pt)
    {
        GridAddress addr = new GridAddress(pt);

        if (!addressToStreetSegmentIdListDict.ContainsKey(addr)) {
            addressToStreetSegmentIdListDict[addr] = new List<int>();
        }

        int streetSegmentId = -1;

        if (!StreetSegmentIdToStreetSegmentDict.ContainsValue(seg)) {
            streetSegmentId = nextStreetSegmentId++;
            StreetSegmentIdToStreetSegmentDict[streetSegmentId] = seg;
        }
        else {
            foreach (int segId in StreetSegmentIdToStreetSegmentDict.Keys) {
                if (seg == StreetSegmentIdToStreetSegmentDict[segId]) {
                    streetSegmentId = segId;
                    break;
                }
            }
        }

        if (!addressToStreetSegmentIdListDict[addr].Contains(streetSegmentId)) {
            addressToStreetSegmentIdListDict[addr].Add(streetSegmentId);
        }
    }

    public List<StreetSegment> GetSegmentsNear(Vector2 point, float radius) {
        List<int> segmentIds = new List<int>();

        for (float fx = point.x - radius; fx < point.x + radius + GridAddress.Resolution; fx += GridAddress.Resolution) {
            for (float fy = point.y - radius; fy < point.y + radius + GridAddress.Resolution; fy += GridAddress.Resolution) {
                GridAddress addr = new GridAddress(fx, fy);
                if (addressToStreetSegmentIdListDict.ContainsKey(addr)) {
                    foreach (int segId in addressToStreetSegmentIdListDict[addr]) {
                        if (!segmentIds.Contains(segId)) {
                            segmentIds.Add(segId);
                        }
                    }
                }
            }
        }

        List<StreetSegment> segments = new List<StreetSegment>();
        foreach (int segId in segmentIds) {
            segments.Add(StreetSegmentIdToStreetSegmentDict[segId]);
        }
        return segments;
    }
}

