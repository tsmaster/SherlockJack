using System.Collections.Generic;
using UnityEngine;

public class SloppyQuadtree
{
    /*
    const int SEGMENT_MAX = 4;
    const int CHILDREN_MAX = 4;

    List<StreetSegment> segments;
    List<SloppyQuadtree> children;

    Vector2 lowerLeft;
    Vector2 upperRight;

    public SloppyQuadtree ()
    {
    }

    public void AddSegment(StreetSegment seg) {
        if ((segments == null) || (segments.Count == 0)) {
            segments = new List<StreetSegment>();
            segments.Add(seg);
            CalcBounds();
            return null;
        }
        else
        if (segments.Count < SEGMENT_MAX) {
            segments.Add(seg);
            CalcBounds();
            return null;
        }
        else
        if ((children == null) || (children.Count == 0)) {
            children = new List<SloppyQuadtree>();
            segments.Add(seg);
            List<List<StreetSegment>> partitions = MakePartitions(segments);
            for (int i = 0; i < partitions.Count; ++i) {
                children.Add(new SloppyQuadtree());
                foreach (StreetSegment s in partitions[i]) {
                    children[i].AddSegment(s);
                }
            }
        }
    }

    void CalcBounds() {
    }

    List<List<StreetSegment>> MakePartitions(List<StreetSegment> segList) {
        Vector2[] box = FindBoundingBox(segList);
        float xDist = box[1].x - box[0].x;
        float yDist = box[1].y - box[0].y;

        if (xDist >= yDist) {
            return PartitionOnX(segList);
        }
        else {
            return PartitionOnY(segList);
        }
    }

*/
}

