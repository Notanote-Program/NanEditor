using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class PerformImgManager : BaseManager
{
    private const string img_prefab_path = "Prefabs/PerformImg";
    private List<PerformImg> imgList; // all imgs
    private Dictionary<int, GameObject> imgObjectList; //selected imgs
    private int nextImgId;
    private Config.LoadType loadType;
    private string path; // path of img file
    // Start is called before the first frame update
    public void init(List<PerformImg> imgList, int num, GameObject parent = null, Config.LoadType loadType = Config.LoadType.Resource, string chartName = "test")
    {
        this.loadType = loadType;
        initPool(img_prefab_path, num, parent);
        this.imgList = new List<PerformImg>(imgList);
        imgObjectList = new Dictionary<int, GameObject>();
        EventList = new List<EventList>();

        presentMoveEventNum = new List<int>();
        presentRotateEventNum = new List<int>();
        presentColorEventNum = new List<int>();
        presentScaleEventNum = new List<int>();
        nextImgId = 0;

        if(loadType == Config.LoadType.Resource)
        {
            path = "Charts/" + chartName + "/imgs/";
        }
        else
        {
            path = System.Environment.CurrentDirectory + "/Charts/" + chartName + "/imgs/";
        }
        for (int i = 0; i < imgList.Count; i++)
        {
            EventList.Add(new EventList(imgList[i].eventList.moveEvents, imgList[i].eventList.rotateEvents, imgList[i].eventList.colorModifyEvents, imgList[i].eventList.scaleEvents));
            if (imgList[i].path == null)
                imgList[i].path = "";
            if (!Config.spriteList.ContainsKey(imgList[i].path))
                Config.spriteList[imgList[i].path] = Utilities.loadSprite(path + imgList[i].path,loadType);
            presentMoveEventNum.Add(0);
            presentRotateEventNum.Add(0);
            presentColorEventNum.Add(0);
            presentScaleEventNum.Add(0);
        }
    }
    
    public void reset()
    {
        foreach (GameObject obj in imgObjectList.Values)
        {
            pool.release_item(obj);
        }
        imgObjectList.Clear();
        nextImgId = 0;
        for (int i = 0; i < imgList.Count; i++)
        {
            presentMoveEventNum[i] = 0;
            presentRotateEventNum[i] = 0;
            presentColorEventNum[i] = 0;
            presentScaleEventNum[i] = 0;
        }
    }
    public void update(float time)
    {
        while (nextImgId < imgList.Count && imgList[nextImgId].startTime < time)
        {
            PerformImg img = imgList[nextImgId];
            GameObject newImg = pool.get_item();
            //Debug.Log(img.path);

            newImg.GetComponent<PerformImgRenderer>().init(Config.spriteList[img.path], img.color, img.position, img.scale, img.angle, img.sortingOrder,loadType);
            imgObjectList.Add(nextImgId, newImg);
            nextImgId++;
        }
        foreach (KeyValuePair<int, GameObject> imgs in imgObjectList )
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
            Vector3 pos = events[presentMoveEventNum[Id] - 1].positions[events[presentMoveEventNum[Id] - 1].positions.Count - 1];
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
        List<ScaleEvent> events = EventList[Id].scaleEvents;
        if (events.Count == 0)
            return;
        while (presentScaleEventNum[Id] < events.Count && events[presentScaleEventNum[Id]].endTime <= time)
        {
            presentScaleEventNum[Id]++;
        }
        if (presentScaleEventNum[Id] >= events.Count && events.Count > 0)
        {
            float scale = events[events.Count - 1].endScale;
            imgObjectList[Id].GetComponent<PerformImgRenderer>().scale = scale;
            return;
        }
        if (events[presentScaleEventNum[Id]].startTime <= time)
        {
            float scale = getScale(events[presentScaleEventNum[Id]], time);
            imgObjectList[Id].GetComponent<PerformImgRenderer>().scale = scale;
        }
        else if (presentScaleEventNum[Id] > 0)
        {
            float scale = events[presentScaleEventNum[Id] - 1].endScale;
            imgObjectList[Id].GetComponent<PerformImgRenderer>().scale = scale;
        }
    }
    public void reload(string imgpath)
    {
        if (imgpath == null)
            imgpath = "";
        Config.spriteList[imgpath] = Utilities.loadSprite(path + imgpath,loadType);
    }
}
