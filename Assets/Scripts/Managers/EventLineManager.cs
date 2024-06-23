using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLineManager : BaseManager
{
    private const string eventline_prefab_path = "Prefabs/eventLine";
    private const int defaultNum = 100;
    private List<float> defaultPos;

    private GameObject lineSet;
    private Config.EventlineType type;
    private Chart chart;
    public float density;
    private float curPos;
    public int id;// id of selected judgeline or image 
    private List<GameObject> eventLines;// eventlines to render on screen
    public Dictionary<PerformEvent,string> selectedEvents;// selected events
    public List<Note> selectedNotes;// selected notes
    public Dictionary<PerformEvent, string> copiedEvents;// selected events
    public List<Note> copiedNotes;// selected notes 
    public float copyStartTime;
    public GameObject ChartMaker;
    private GameObject guideLine;
    public void init(GameObject lineSet, List<float> pos,float density, Chart chart = null, Config.EventlineType type = Config.EventlineType.Judgeline, int id = 0, GameObject parent = null)
    {
        defaultPos = new List<float>(pos);
        selectedEvents = new Dictionary<PerformEvent, string>();
        selectedNotes = new List<Note>();
        copiedEvents = new Dictionary<PerformEvent, string>();
        copiedNotes = new List<Note>();
        this.lineSet = lineSet;
        this.density = density;
        this.curPos = 0;
        this.type = type;
        this.id = id;
        eventLines = new List<GameObject>();
        initPool(eventline_prefab_path, defaultNum, lineSet);
        if (chart != null)
        {
            this.chart = chart;
            resetEventlines();
        }
        else
        {
            this.chart = new Chart();
        }
        initGuideLine();
    }
    private void initGuideLine()
    {
        guideLine = pool.get_item();
        guideLine.GetComponent<EventLineRenderer>().init(Vector3.zero, 0, -1, "guideLine", "Untagged");
        guideLine.GetComponent<LineRenderer>().material = Resources.Load<Material>("Materials/guideLine");
        guideLine.transform.SetParent(lineSet.transform);
        guideLine.GetComponent<LineRenderer>().startColor = new Color(0, 0, 0, 0);
        guideLine.GetComponent<LineRenderer>().endColor = new Color(0, 0, 0, 0);
    }
    public void drawGuidLine(float posX, float startBeats, float endBeats)
    {
        guideLine.GetComponent<LineRenderer>().startColor = Color.white;
        guideLine.GetComponent<LineRenderer>().endColor = Color.white;
        guideLine.GetComponent<EventLineRenderer>().position = new Vector3(posX,startBeats / density,0);
        guideLine.GetComponent<EventLineRenderer>().length = (endBeats - startBeats) / density;
    }
    public void hideGuideLine()
    {
        guideLine.GetComponent<LineRenderer>().startColor = new Color(0, 0, 0, 0);
        guideLine.GetComponent<LineRenderer>().endColor = new Color(0, 0, 0, 0);
    }
    private void resetEventlines()
    {
        foreach (GameObject obj in eventLines)
        {
            pool.release_item(obj);
        }
        eventLines.Clear();

        if (type == Config.EventlineType.Judgeline)
        {
            if (id < chart.judgelineList.Count)
            {
                JudgeLine line = chart.judgelineList[id];
                float curTime = gettime(curPos * density);

                int id_below = getNoteidBelow(line.noteList, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(line.noteList.Count, id_below + defaultNum / 2); i++)
                {
                    //Debug.Log(i);
                    Note note = line.noteList[i];
                    GameObject newObj = pool.get_item();
                    newObj.transform.parent = lineSet.transform;
                    float posy = getbeats(note.time) / density;
                    float length = getbeats(note.duration + note.time) / density - posy;
                    newObj.GetComponent<EventLineRenderer>().init(new Vector3(defaultPos[0], posy, 0), length, i, "note", "note");
                    if (selectedNotes.Contains(note))
                        newObj.GetComponent<EventLineRenderer>().select(true);
                    eventLines.Add(newObj);
                }

                id_below = getMoveEventidBelow(line.eventList.moveEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(line.eventList.moveEvents.Count, id_below + defaultNum / 2); i++)
                {
                    MoveEvent moveEvent = line.eventList.moveEvents[i];
                    addEventline(moveEvent, i, 1, "moveEvent");
                }

                id_below = getRotateEventidBelow(line.eventList.rotateEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(line.eventList.rotateEvents.Count, id_below + defaultNum / 2); i++)
                {
                    RotateEvent rotateEvent = line.eventList.rotateEvents[i];
                    addEventline(rotateEvent, i, 2, "rotateEvent");
                }

                id_below = getColorEventidBelow(line.eventList.colorModifyEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(line.eventList.colorModifyEvents.Count, id_below + defaultNum / 2); i++)
                {
                    ColorModifyEvent colorEvent = line.eventList.colorModifyEvents[i];
                    addEventline(colorEvent, i, 3, "colorEvent");
                }
            }
        }
        else
        {
            if (id < chart.performImgList.Count)
            {
                PerformImg img = chart.performImgList[id];
                float curTime = gettime(curPos * density);

                GameObject newObj = pool.get_item();
                newObj.transform.parent = lineSet.transform;
                float posy = getbeats(img.startTime) / density;
                float length = getbeats(img.endTime) / density - posy;
                newObj.GetComponent<EventLineRenderer>().init(new Vector3(defaultPos[0], posy, 0), length, id, "performImage", "Untagged");
                eventLines.Add(newObj);

                int id_below = getMoveEventidBelow(img.eventList.moveEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(img.eventList.moveEvents.Count, id_below + defaultNum / 2); i++)
                {
                    MoveEvent moveEvent = img.eventList.moveEvents[i];
                    addEventline(moveEvent, i, 1, "moveEvent");
                }

                id_below = getRotateEventidBelow(img.eventList.rotateEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(img.eventList.rotateEvents.Count, id_below + defaultNum / 2); i++)
                {
                    RotateEvent rotateEvent = img.eventList.rotateEvents[i];
                    addEventline(rotateEvent, i, 2, "rotateEvent");
                }

                id_below = getColorEventidBelow(img.eventList.colorModifyEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(img.eventList.colorModifyEvents.Count, id_below + defaultNum / 2); i++)
                {
                    ColorModifyEvent colorEvent = img.eventList.colorModifyEvents[i];
                    addEventline(colorEvent, i, 3, "colorEvent");
                }

                id_below = getScaleEventidBelow(img.eventList.scaleXEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(img.eventList.scaleXEvents.Count, id_below + defaultNum / 2); i++)
                {
                    ScaleEvent scaleEvent = img.eventList.scaleXEvents[i];
                    addEventline(scaleEvent, i, 4, "scaleXEvent");
                }
                
                id_below = getScaleEventidBelow(img.eventList.scaleYEvents, curTime);
                for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(img.eventList.scaleYEvents.Count, id_below + defaultNum / 2); i++)
                {
                    ScaleEvent scaleEvent = img.eventList.scaleYEvents[i];
                    addEventline(scaleEvent, i, 5, "scaleYEvent");
                }
            }
        }
    }
    private void addEventline(PerformEvent _event, int id, int posid, string type)
    {
        GameObject newObj = pool.get_item();
        float posy = getbeats(_event.startTime) / density;
        float length = getbeats(_event.endTime) / density - posy;
        newObj.transform.parent = lineSet.transform;
        newObj.GetComponent<EventLineRenderer>().init(new Vector3(defaultPos[posid], posy, 0), length, id, type);
        eventLines.Add(newObj);
        if (selectedEvents.ContainsKey(_event))
            newObj.GetComponent<EventLineRenderer>().select(true);
    }
    public void reset(Chart chart)
    {
        Debug.Log(chart.noteNum);
        this.chart = chart;
        Debug.Log("set chart");
        //selectedEvents.Clear();
        resetEventlines();
    }
    public void update(float curPos, float density)
    {
        this.density = density;
        this.curPos = curPos;
        resetEventlines();
    }
    public void changeEventlineType(Config.EventlineType type, int id)
    {
        this.type = type;
        this.id = id;
        selectedEvents.Clear();
        selectedNotes.Clear();
        resetEventlines();
    }
    private float getbeats(float time)
    {
        return ChartMaker.GetComponent<chartMaker>().getbeats(time);// not good. a better way to organize this function?
    }
    private float gettime(float beats)
    {
        return ChartMaker.GetComponent<chartMaker>().gettime(beats);// not good. a better way to organize this function?
    }
    public void copy()
    {
        copiedEvents.Clear();
        copiedNotes.Clear();
        if(selectedEvents.Count > 0)
        {
            copyStartTime = -1;
            foreach(KeyValuePair<PerformEvent,string> p in selectedEvents)
            {
                PerformEvent newEvent;
                switch(p.Value)
                {
                    case "moveEvent":
                        MoveEvent _event = p.Key as MoveEvent;
                        newEvent = new MoveEvent(_event.positions, _event.pathType, _event.startTime, _event.endTime, _event.type);
                        break;
                    case "rotateEvent":
                        RotateEvent rotate = p.Key as RotateEvent;
                        newEvent = new RotateEvent(rotate.startAngle, rotate.endAngle, rotate.startTime, rotate.endTime, rotate.type);
                        break;
                    case "colorEvent":
                        ColorModifyEvent color = p.Key as ColorModifyEvent;
                        newEvent = new ColorModifyEvent(color.startColor, color.endColor, color.startTime, color.endTime, color.type);
                        break;
                    case "scaleXEvent":
                    case "scaleYEvent":
                        ScaleEvent scale = p.Key as ScaleEvent;
                        newEvent = new ScaleEvent(scale.startScale, scale.endScale, scale.startTime, scale.endTime, scale.type);
                        break;
                    default:
                        return;
                }
                copiedEvents.Add(newEvent,p.Value);
                if(copyStartTime < 0 || copyStartTime>p.Key.startTime)
                {
                    copyStartTime = p.Key.startTime;
                }
            }
        }
        else if(selectedNotes.Count > 0)
        {
            copyStartTime = -1;
            foreach(Note note in selectedNotes)
            {
                Note newnote = new Note(note.type, note.color, note.time, note.duration, note.speed, note.livingTime, note.lineSide, note.fake);
                copiedNotes.Add(newnote);
                if (copyStartTime < 0 || copyStartTime > note.time)
                {
                    copyStartTime = note.time;
                }
            }
        }
    }
    public void selectEventline(GameObject obj)
    {
        if (!obj)
            return;
        Debug.Log(obj);
        selectedEvents.Clear();
        if (eventLines.Contains(obj))
        {
            selectedNotes.Clear();
            EventList eventList;
            if (type == Config.EventlineType.Judgeline)
                eventList = chart.judgelineList[id].eventList;
            else
                eventList = chart.performImgList[id].eventList;
            switch(obj.GetComponent<EventLineRenderer>().type)
            {
                case "moveEvent":
                    selectedEvents.Add(eventList.moveEvents[obj.GetComponent<EventLineRenderer>().id], "moveEvent");
                    break;
                case "rotateEvent":
                    selectedEvents.Add(eventList.rotateEvents[obj.GetComponent<EventLineRenderer>().id], "rotateEvent");
                    break;
                case "scaleXEvent":
                    selectedEvents.Add(eventList.scaleXEvents[obj.GetComponent<EventLineRenderer>().id], "scaleXEvent");
                    break;
                case "scaleYEvent":
                    selectedEvents.Add(eventList.scaleYEvents[obj.GetComponent<EventLineRenderer>().id], "scaleYEvent");
                    break;
                case "colorEvent":
                    selectedEvents.Add(eventList.colorModifyEvents[obj.GetComponent<EventLineRenderer>().id], "colorEvent");
                    break;
            }
        }
        resetEventlines();
    }
    public void addSelectEventline(GameObject obj)
    {
        if (!obj)
            return;
        if (eventLines.Contains(obj))
        {
            selectedNotes.Clear();
            EventList eventList;
            if (type == Config.EventlineType.Judgeline)
                eventList = chart.judgelineList[id].eventList;
            else
                eventList = chart.performImgList[id].eventList;
            PerformEvent _event = new PerformEvent();
            string _type = obj.GetComponent<EventLineRenderer>().type;
            switch (_type)
            {
                case "moveEvent":
                    _event = eventList.moveEvents[obj.GetComponent<EventLineRenderer>().id];
                    break;
                case "rotateEvent":
                    _event = eventList.rotateEvents[obj.GetComponent<EventLineRenderer>().id];
                    break;
                case "scaleXEvent":
                    _event = eventList.scaleXEvents[obj.GetComponent<EventLineRenderer>().id];
                    break;
                case "scaleYEvent":
                    _event = eventList.scaleYEvents[obj.GetComponent<EventLineRenderer>().id];
                    break;
                case "colorEvent":
                    _event = eventList.colorModifyEvents[obj.GetComponent<EventLineRenderer>().id];
                    break;
                default:
                    return;
            }
            if (!selectedEvents.ContainsKey(_event))
                selectedEvents.Add(_event,_type); 
        }
        resetEventlines();
    }
    public void selectAllEvents()
    {
        selectedNotes.Clear();
        selectedEvents.Clear();
        EventList eventList;
        if (type == Config.EventlineType.Judgeline)
            eventList = chart.judgelineList[id].eventList;
        else
            eventList = chart.performImgList[id].eventList;
        foreach (PerformEvent _event in eventList.moveEvents)
        {
            selectedEvents.Add(_event, "moveEvent");
        }
        foreach (PerformEvent _event in eventList.rotateEvents)
        {
            selectedEvents.Add(_event, "rotateEvent");
        }
        foreach (PerformEvent _event in eventList.colorModifyEvents)
        {
            selectedEvents.Add(_event, "colorEvent");
        }
        foreach (PerformEvent _event in eventList.scaleXEvents)
        {
            selectedEvents.Add(_event, "scaleXEvent");
        }
        foreach (PerformEvent _event in eventList.scaleYEvents)
        {
            selectedEvents.Add(_event, "scaleYEvent");
        }
        resetEventlines();
    }
    public void selectNote(GameObject obj)
    {
        if (!obj)
            return;
        if(eventLines.Contains(obj))
        {
            selectedEvents.Clear();
            selectedNotes.Clear();
            Note note = chart.judgelineList[id].noteList[obj.GetComponent<EventLineRenderer>().id];
            selectedNotes.Add(note);
        }
        resetEventlines();
    }
    public void addSelectNote(GameObject obj)
    {
        if (!obj)
            return;
        if (eventLines.Contains(obj))
        {
            selectedEvents.Clear();
            Note note = chart.judgelineList[id].noteList[obj.GetComponent<EventLineRenderer>().id];
            if(!selectedNotes.Contains(note))
                selectedNotes.Add(note);
        }
        resetEventlines();
    }
    public void cancelSelect()
    {
        selectedEvents.Clear();
        selectedNotes.Clear();
        resetEventlines();
    }
    public void setPos(List<float> pos)
    {
        this.defaultPos = new List<float>(pos);
        resetEventlines();
    }
    private int getNoteidBelow(List<Note> noteList, float curTime)
    {
        int id_below = noteList.BinarySearch(new Note(Config.Type.Tap, Color.white, curTime), new noteComparer());
        if (id_below < 0)
            id_below = -id_below - 1;
        return id_below;
    }
    private int getMoveEventidBelow(List<MoveEvent> eventList, float curTime)
    {
        int id_below = eventList.BinarySearch(new MoveEvent(null, Config.PathType.Bessel, curTime, curTime), new moveEventComparer());
        if (id_below < 0)
            id_below = -id_below - 1;
        return id_below;
    }
    private int getRotateEventidBelow(List<RotateEvent> eventList, float curTime)
    {
        int id_below = eventList.BinarySearch(new RotateEvent(0, 0, curTime, curTime), new rotateEventComparer());
        if (id_below < 0)
            id_below = -id_below - 1;
        return id_below;
    }
    private int getScaleEventidBelow(List<ScaleEvent> eventList, float curTime)
    {
        int id_below = eventList.BinarySearch(new ScaleEvent(0, 0, curTime, curTime), new scaleEventComparer());
        if (id_below < 0)
            id_below = -id_below - 1;
        return id_below;
    }
    private int getColorEventidBelow(List<ColorModifyEvent> eventList, float curTime)
    {
        int id_below = eventList.BinarySearch(new ColorModifyEvent(Color.white, Color.white, curTime, curTime), new colorEventComparer());
        if (id_below < 0)
            id_below = -id_below - 1;
        return id_below;
    }
}

