using System.Collections.Generic;
using UnityEngine;

public class BaseManager : MonoBehaviour
{
    protected itempool pool;
    protected List<EventList> EventList;
    protected List<int> presentMoveEventNum;
    protected List<int> presentRotateEventNum;
    protected List<int> presentColorEventNum;
    protected List<int> presentScaleXEventNum;
    protected List<int> presentScaleYEventNum;
    protected void initPool(string path, int num, GameObject parent = null, bool needRelease = false)
    {
        pool = new itempool();
        pool.set_item(path, needRelease, parent);
        pool.init(num);
    }
    protected Vector3 getPosition(MoveEvent moveEvent,float time)
    {
        List<Vector3> positions = moveEvent.positions;
        if (positions.Count < 2)
        {
            Debug.LogError("not enough positions in moveevent");
            return Vector3.zero;
        }
        if(time > moveEvent.endTime)
            return positions[positions.Count - 1];
        float t = Utilities.getPartition((time-moveEvent.startTime) / (moveEvent.endTime - moveEvent.startTime),moveEvent.type);
        switch (moveEvent.pathType)
        {
            case Config.PathType.Bessel:
                {
                    Vector3 pos = Utilities.getBesselPosition(t, positions);
                    //Debug.Log(t + " " + pos);
                    return pos;
                }
            case Config.PathType.Straight:
                {
                    Vector3 pos = Utilities.getStraightPosition(t, positions);
                    return pos;
                }
        }
        return Vector3.zero;
    }
    protected float getAngle(RotateEvent rotateEvent, float time)
    {
        float t = Utilities.getPartition((time - rotateEvent.startTime) / (rotateEvent.endTime - rotateEvent.startTime), rotateEvent.type);
        return Mathf.Lerp(rotateEvent.startAngle,rotateEvent.endAngle,t);
    }
    protected Color getColor(ColorModifyEvent colorEvent, float time)
    {
        float t = Utilities.getPartition((time - colorEvent.startTime) / (colorEvent.endTime - colorEvent.startTime), colorEvent.type);
        return colorEvent.startColor * (1-t) + colorEvent.endColor * t;
    }
    protected float getScale(ScaleEvent scaleEvent, float time)
    {
        float t = Utilities.getPartition((time - scaleEvent.startTime) / (scaleEvent.endTime - scaleEvent.startTime), scaleEvent.type);
        return scaleEvent.startScale * (1 - t) + scaleEvent.endScale * t;
    }
}
