using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class chartMaker : MonoBehaviour
{
    private readonly Vector2 defaultContentPos = new Vector2(0,-100);
    private readonly List<float> defaultPos = new List<float> { -3, 0, 2, 4, 6, 8 };// the positions of eventlines
    public int partition = 4;
    private float _density = 0.25f;
    public float density
    {
        get { return _density; }
        set 
        {
            _density = Mathf.Clamp(value,0.01f,1f);
            timeline_manager.GetComponent<TimeLineManager>().density = density;
            eventline_manager.GetComponent<EventLineManager>().update(timeline_manager.GetComponent<TimeLineManager>().curPlayPos, density);
        }
    }
    public Chart chart;
    public float curPos
    {
        get { return timeline_manager.GetComponent<TimeLineManager>().curPlayPos; }
    }
    private GameObject timeline_manager;
    private GameObject eventline_manager;
    private ScrollRect scroll;
    private GameObject UIbackground;
    private bool guideMode = false;
    public int guidePos = 0;
    public float guideBeats;
    // Start is called before the first frame update
    void Start()
    {
    
    }
    // Update is called once per frame
    void Update()
    {
        if(guideMode)
        {
            drawGuideLine();
        }            
    }
    public void init(Chart chart = null)
    {
        this.chart = chart;
        UIbackground = transform.Find("UIbackground").gameObject;
        scroll = transform.Find("scroll").GetComponent<ScrollRect>();
        initTimeline();
        initEventline();
        scroll.content.GetComponent<RectTransform>().anchoredPosition = defaultContentPos;
        setVisible(true);
    }
    private void initTimeline()
    {
        timeline_manager = new GameObject();
        timeline_manager.name = "timelineManager";
        timeline_manager.transform.parent = scroll.content;
        timeline_manager.AddComponent<TimeLineManager>();
        timeline_manager.GetComponent<TimeLineManager>().ChartMaker = this.gameObject;
        timeline_manager.GetComponent<TimeLineManager>().init(200, partition, density, timeline_manager);
    }
    private void initEventline()
    {
        eventline_manager = new GameObject();
        eventline_manager.name = "eventlineManager";
        eventline_manager.transform.parent = scroll.content;
        eventline_manager.AddComponent<EventLineManager>();
        eventline_manager.GetComponent<EventLineManager>().ChartMaker = this.gameObject;
        eventline_manager.GetComponent<EventLineManager>().init(timeline_manager.GetComponent<TimeLineManager>().lineSet, defaultPos, density, chart, Config.EventlineType.Judgeline, 0, eventline_manager);
    }
    public void setTime(float time)
    {
        scroll.content.GetComponent<RectTransform>().anchoredPosition = defaultContentPos;
        timeline_manager.GetComponent<TimeLineManager>().update(time);
        eventline_manager.GetComponent<EventLineManager>().update(timeline_manager.GetComponent<TimeLineManager>().curPlayPos,density);
    }
    public void setFullTime(float t)
    {
        timeline_manager.GetComponent<TimeLineManager>().totalBeats = (int)getbeats(t)+1;
    }
    public GameObject getTimeline()
    {
        return Utilities.getNearestObject("timeline");
    }   
    public GameObject getEventline()
    {
        return Utilities.getNearestObject("eventline");
    }
    public GameObject getNote()
    {
        return Utilities.getNearestObject("note");
    }
    public void setLineDensity(float density)
    {
        this.density = density;
    }
    public void setLinePartition(string _partition)
    {
        if (_partition != null && _partition != "")
        {
            int partition = int.Parse(_partition);
            this.partition = partition;
            timeline_manager.GetComponent<TimeLineManager>().partition = partition;
        }
    }
    public void setVisible(bool b = true)
    {
        scroll.content.gameObject.SetActive(b);
        UIbackground.SetActive(b);
    }
    public float getbeats(float time)
    {
        if(chart.bpmList.Count == 0)
            return chart.bpm / 4.0f * time / 1000.0f / 60.0f;// todo: beat list
        float beats = 0;
        float last_time = 0;
        float last_bpm = chart.bpm;
        for(int i=0;i<chart.bpmList.Count;i++)
        {
            ModifyBpm newbpm = chart.bpmList[i];
            if(newbpm.time < time)
            {
                float delta_time = newbpm.time - last_time;
                last_time = newbpm.time;
                beats += last_bpm / 4.0f * delta_time / 1000.0f / 60.0f;
                last_bpm = newbpm.bpm;
                if(i == chart.bpmList.Count - 1)
                {
                    delta_time = time - last_time;
                    beats += last_bpm / 4.0f * delta_time / 1000.0f / 60.0f;
                }
            }
            else
            {
                float delta_time = time - last_time;
                beats += last_bpm / 4.0f * delta_time / 1000.0f / 60.0f;
                break;
            }
        }
        return beats;
    }
    public float gettime(float beats)
    {
        if(chart.bpmList.Count == 0)
            return (int)(beats * 4.0f * 60.0f / chart.bpm * 1000.0f);
        float time = 0;
        float last_time = 0;
        float last_bpm = chart.bpm;
        for(int i=0;i<chart.bpmList.Count;i++)
        {
            ModifyBpm newbpm = chart.bpmList[i];
            float delta_time = newbpm.time - last_time;
            float delta_beats = last_bpm / 4.0f * delta_time / 1000.0f / 60.0f;
            last_time = newbpm.time;
            last_bpm = newbpm.bpm;
            if(beats > delta_beats)
            {
                beats -= delta_beats;
                time = newbpm.time;
                if(i == chart.bpmList.Count - 1)
                {
                    time += beats * 4.0f * 60.0f / last_bpm * 1000.0f;
                }
            }
            else
            {
                time += beats / delta_beats * delta_time; 
                break;
            }
        }
        return time;
    }
    public void setGuideMode(bool b, int guidePos = 0)
    {
        if(b)
        {
            this.guidePos = Mathf.Clamp(guidePos, 0 ,defaultPos.Count - 1);
            guideBeats = (float)timeline_manager.GetComponent<TimeLineManager>().selectedId / partition;
            guideMode = true;
        }
        else
        {
            eventline_manager.GetComponent<EventLineManager>().hideGuideLine();
            guideMode = false;
            this.guidePos = -1;
        }
    }
    private void drawGuideLine()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos = eventline_manager.transform.InverseTransformPoint(mousePos);
        float posX = defaultPos[guidePos];
        float startBeats = guideBeats;
        float endBeats = (mousePos.y + curPos) * density;
        eventline_manager.GetComponent<EventLineManager>().drawGuidLine(posX, startBeats, endBeats);
    }
    public float getSelectedTime()
    {
        return gettime((float)timeline_manager.GetComponent<TimeLineManager>().selectedId / partition);
    }
    public void reset(Chart chart)
    {
        Debug.Log(chart.noteNum);
        this.chart = chart;
        eventline_manager.GetComponent<EventLineManager>().reset(chart);
    }
    public void changeEventlineType(Config.EventlineType type, int id)
    {
        eventline_manager.GetComponent<EventLineManager>().changeEventlineType(type, id);
    }
    public void selectTimeline()
    {
        timeline_manager.GetComponent<TimeLineManager>().selectLine(getTimeline());
    }
    public void selectAll()
    {
        eventline_manager.GetComponent<EventLineManager>().selectAllEvents();
    }
    public void selectEventline(bool multi = false)
    {
        GameObject eventline = getEventline();
        if (!multi)
            eventline_manager.GetComponent<EventLineManager>().selectEventline(eventline);
        else
            eventline_manager.GetComponent<EventLineManager>().addSelectEventline(eventline);
    }
    public void selectNote(bool multi = false)
    {
        GameObject note = getNote();
        if (note)
            Debug.Log(note.GetComponent<EventLineRenderer>().type);
        if (!multi)
            eventline_manager.GetComponent<EventLineManager>().selectNote(note);
        else
            eventline_manager.GetComponent<EventLineManager>().addSelectNote(note);
    }
    public void cancelSelect()
    {
        eventline_manager.GetComponent<EventLineManager>().cancelSelect();
        
    }
    public void copy()
    {
        eventline_manager.GetComponent<EventLineManager>().copy();
    }
    public Dictionary<PerformEvent, string> getSelectedEvent()
    {
        return eventline_manager.GetComponent<EventLineManager>().selectedEvents;
    }
    public List<Note> getSelectedNote()
    {
        return eventline_manager.GetComponent<EventLineManager>().selectedNotes;
    }
    public Dictionary<PerformEvent, string> getCopiedEvent()
    {
        return eventline_manager.GetComponent<EventLineManager>().copiedEvents;
    }
    public List<Note> getCopiedNote()
    {
        return eventline_manager.GetComponent<EventLineManager>().copiedNotes;
    }
    public float getCopiedTime()
    {
        return eventline_manager.GetComponent<EventLineManager>().copyStartTime;
    }
}

