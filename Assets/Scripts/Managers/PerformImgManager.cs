using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Windows;

public class PerformImgManager : BaseManager
{
    private const string img_prefab_path = "Prefabs/PerformImg";
    private List<PerformImg> imgList; // all imgs
    private Dictionary<int, GameObject> imgObjectList; //selected imgs
    private int nextImgId;
    private Config.LoadType loadType;

    private string path; // path of img file

    // Start is called before the first frame update
    public void init(List<PerformImg> imgList, int num, GameObject parent = null,
        Config.LoadType loadType = Config.LoadType.Resource, string chartName = "test")
    {
        this.loadType = loadType;
        initPool(img_prefab_path, num, parent);
        this.imgList = new List<PerformImg>(imgList);
        imgObjectList = new Dictionary<int, GameObject>();
        EventList = new List<EventList>();

        presentMoveEventNum = new List<int>();
        presentRotateEventNum = new List<int>();
        presentColorEventNum = new List<int>();
        presentScaleXEventNum = new List<int>();
        presentScaleYEventNum = new List<int>();
        nextImgId = 0;

        if (loadType == Config.LoadType.Resource)
        {
            path = "Charts/" + chartName + "/imgs/";
        }
        else
        {
            path = System.Environment.CurrentDirectory + "/Charts/" + chartName + "/imgs/";
        }

        foreach (var performImg in imgList)
        {
            EventList.Add(performImg.eventList.Clone());
            performImg.path ??= "";
            if (!Config.spriteList.ContainsKey(performImg.path))
            {
                Config.spriteList[performImg.path] = Config.GetImgSprite(path, performImg.path, loadType);
                Config.spriteList[performImg.path].name = performImg.path;
            }

            presentMoveEventNum.Add(0);
            presentRotateEventNum.Add(0);
            presentColorEventNum.Add(0);
            presentScaleXEventNum.Add(0);
            presentScaleYEventNum.Add(0);
        }
    }

    public void reset()
    {
        foreach (GameObject obj in imgObjectList.Values)
        {
            obj.GetComponent<PerformImgRenderer>().OnRelease();
            pool.release_item(obj);
        }

        imgObjectList.Clear();
        nextImgId = 0;
        for (int i = 0; i < imgList.Count; i++)
        {
            presentMoveEventNum[i] = 0;
            presentRotateEventNum[i] = 0;
            presentColorEventNum[i] = 0;
            presentScaleXEventNum[i] = 0;
            presentScaleYEventNum[i] = 0;
        }
    }

    public void update(float time)
    {
        while (nextImgId < imgList.Count && imgList[nextImgId].startTime < time)
        {
            PerformImg img = imgList[nextImgId];
            GameObject newImg = pool.get_item();
            //Debug.Log(img.path);

            newImg.GetComponent<PerformImgRenderer>().init(Config.spriteList[img.path], img.color, img.position,
                img.scaleX, img.scaleY, img.layer, img.angle, img.sortingOrder,
                img.path.StartsWith("$") ? img.path[1..] : null);
            imgObjectList.Add(nextImgId, newImg);
            nextImgId++;
        }

        foreach (KeyValuePair<int, GameObject> imgs in imgObjectList)
        {
            int id = imgs.Key;
            updateMoveEvents(id, time);
            updateRotateEvents(id, time);
            updateColorEvents(id, time);
            updateScaleEvents(id, time);
        }

        List<int> destroyId = new List<int>();
        foreach (KeyValuePair<int, GameObject> imgs in imgObjectList)
        {
            int id = imgs.Key;
            PerformImg img = imgList[id];
            if (time > img.endTime)
            {
                destroyId.Add(id);
            }
        }

        foreach (int id in destroyId)
        {
            imgObjectList[id].GetComponent<PerformImgRenderer>().OnRelease();
            pool.release_item(imgObjectList[id]);
            imgObjectList.Remove(id);
        }
    }

    private void updateMoveEvents(int Id, float time)
    {
        List<MoveEvent> events = EventList[Id].moveEvents;
        if (events.Count == 0)
            return;
        while (presentMoveEventNum[Id] < events.Count && events[presentMoveEventNum[Id]].endTime <= time)
        {
            presentMoveEventNum[Id]++;
        }

        if (presentMoveEventNum[Id] >= events.Count && events.Count > 0)
        {
            Vector3 pos = events[events.Count - 1].positions[events[events.Count - 1].positions.Count - 1];
            imgObjectList[Id].GetComponent<PerformImgRenderer>().position = pos;
            return;
        }

        if (events[presentMoveEventNum[Id]].positions.Count < 2)
        {
            Debug.LogError("error: not enough positions in Object " + Id + " , MoveEvent " + presentMoveEventNum[Id]);
            return;
        }

        if (events[presentMoveEventNum[Id]].startTime <= time)
        {
            Vector2 pos = getPosition(events[presentMoveEventNum[Id]], time);
            imgObjectList[Id].GetComponent<PerformImgRenderer>().position = pos;
        }
        else if (presentMoveEventNum[Id] > 0)
        {
            Vector3 pos = events[presentMoveEventNum[Id] - 1]
                .positions[events[presentMoveEventNum[Id] - 1].positions.Count - 1];
            imgObjectList[Id].GetComponent<PerformImgRenderer>().position = pos;
        }
    }

    private void updateRotateEvents(int Id, float time)
    {
        List<RotateEvent> events = EventList[Id].rotateEvents;
        if (events.Count == 0)
            return;
        while (presentRotateEventNum[Id] < events.Count && events[presentRotateEventNum[Id]].endTime <= time)
        {
            presentRotateEventNum[Id]++;
        }

        if (presentRotateEventNum[Id] >= events.Count && events.Count > 0)
        {
            float angle = events[events.Count - 1].endAngle;
            imgObjectList[Id].GetComponent<PerformImgRenderer>().angle = angle;
            return;
        }

        if (events[presentRotateEventNum[Id]].startTime <= time)
        {
            float angle = getAngle(events[presentRotateEventNum[Id]], time);
            //Debug.Log(time + ":" + angle);
            imgObjectList[Id].GetComponent<PerformImgRenderer>().angle = angle;
        }
        else if (presentRotateEventNum[Id] > 0)
        {
            float angle = events[presentRotateEventNum[Id] - 1].endAngle;
            imgObjectList[Id].GetComponent<PerformImgRenderer>().angle = angle;
        }
    }

    private void updateColorEvents(int Id, float time)
    {
        List<ColorModifyEvent> events = EventList[Id].colorModifyEvents;
        if (events.Count == 0)
            return;
        while (presentColorEventNum[Id] < events.Count && events[presentColorEventNum[Id]].endTime <= time)
        {
            presentColorEventNum[Id]++;
        }

        if (presentColorEventNum[Id] >= events.Count && events.Count > 0)
        {
            Color color = events[events.Count - 1].endColor;
            imgObjectList[Id].GetComponent<PerformImgRenderer>().color = color;
            return;
        }

        if (events[presentColorEventNum[Id]].startTime <= time)
        {
            Color color = getColor(events[presentColorEventNum[Id]], time);
            //Debug.Log(time + ":" + color);
            imgObjectList[Id].GetComponent<PerformImgRenderer>().color = color;
        }
        else if (presentColorEventNum[Id] > 0)
        {
            Color color = events[presentColorEventNum[Id] - 1].endColor;
            //Debug.Log(time + ":" + color);
            imgObjectList[Id].GetComponent<PerformImgRenderer>().color = color;
        }
    }

    private void updateScaleEvents(int Id, float time)
    {
        List<ScaleEvent> xEvents = EventList[Id].scaleXEvents;
        List<ScaleEvent> yEvents = EventList[Id].scaleYEvents;
        if (xEvents.Count + yEvents.Count == 0)
            return;
        float x = 1f;
        float y = 1f;
        bool xHasEvent = xEvents.Count != 0;
        bool yHasEvent = yEvents.Count != 0;
        if (xHasEvent)
        {
            while (presentScaleXEventNum[Id] < xEvents.Count && xEvents[presentScaleXEventNum[Id]].endTime <= time)
            {
                presentScaleXEventNum[Id]++;
            }

            if (presentScaleXEventNum[Id] >= xEvents.Count && xEvents.Count > 0)
            {
                x = xEvents[^1].endScale;
            }
            else
            {
                if (xEvents[presentScaleXEventNum[Id]].startTime <= time)
                {
                    x = getScale(xEvents[presentScaleXEventNum[Id]], time);
                }
                else if (presentScaleXEventNum[Id] > 0)
                {
                    x = xEvents[presentScaleXEventNum[Id] - 1].endScale;
                }
            }
        }

        if (yHasEvent)
        {
            while (presentScaleYEventNum[Id] < yEvents.Count && yEvents[presentScaleYEventNum[Id]].endTime <= time)
            {
                presentScaleYEventNum[Id]++;
            }

            if (presentScaleYEventNum[Id] >= yEvents.Count && yEvents.Count > 0)
            {
                y = yEvents[^1].endScale;
            }
            else
            {
                if (yEvents[presentScaleYEventNum[Id]].startTime <= time)
                {
                    y = getScale(yEvents[presentScaleYEventNum[Id]], time);
                }
                else if (presentScaleYEventNum[Id] > 0)
                {
                    y = yEvents[presentScaleYEventNum[Id] - 1].endScale;
                }
            }
        }

        if (xHasEvent && yHasEvent)
        {
            imgObjectList[Id].GetComponent<PerformImgRenderer>().SetScaleRespectively(x, y);
        }
        else if (xHasEvent)
        {
            imgObjectList[Id].GetComponent<PerformImgRenderer>().scaleX = x;
        }
        else if (yHasEvent)
        {
            imgObjectList[Id].GetComponent<PerformImgRenderer>().scaleY = y;
        }
    }

    public void reload(string imgpath)
    {
        if (imgpath == null)
            imgpath = "";
        Config.spriteList[imgpath] = Config.GetImgSprite(path, imgpath, loadType);
    }
}