using System.Collections.Generic;
using UnityEngine;

public class JudgelineManager : BaseManager
{
    private const string judgeline_prefab_path = "Prefabs/JudgeLine";
    public List<GameObject> judgelineObjectList;
    private List<JudgeLine> judgeLineList;
    // Start is called before the first frame update
    public void init(List<JudgeLine> judgelinelist, int num, GameObject parent = null)
    {
        this.judgeLineList = new List<JudgeLine>(judgelinelist);
        initPool(judgeline_prefab_path, num, parent);
        judgelineObjectList = new List<GameObject>();
        EventList = new List<EventList>();
        presentMoveEventNum = new List<int>();
        presentRotateEventNum = new List<int>();
        presentColorEventNum = new List<int>();
        presentRotateEventNum = new List<int>();

        for (int i = 0; i < judgelinelist.Count; i++)
        {
            EventList.Add(new EventList(judgelinelist[i].eventList.moveEvents, judgelinelist[i].eventList.rotateEvents, judgelinelist[i].eventList.colorModifyEvents));
            judgelineObjectList.Add(pool.get_item());
            JudgeLine line = judgelinelist[i];
            judgelineObjectList[i].GetComponent<JudgeLineRenderer>().init(line.color, line.position, line.scale, line.angle);
            presentMoveEventNum.Add(0);
            presentRotateEventNum.Add(0);
            presentColorEventNum.Add(0);
        }
    }
    public void reset()
    {
        for (int i = 0; i < judgelineObjectList.Count; i++)
        {
            presentMoveEventNum[i] = 0;
            presentRotateEventNum[i] = 0;
            presentColorEventNum[i] = 0;
            JudgeLine line = judgeLineList[i];
            judgelineObjectList[i].GetComponent<JudgeLineRenderer>().init(line.color, line.position, line.scale, line.angle);
        }
    }
    public void update(float time,bool showId = false)
    {
        for (int i = 0; i < judgelineObjectList.Count; i++)
        {
            judgelineObjectList[i].GetComponent<JudgeLineRenderer>().showId(showId,i);
            updateMoveEvents(i, time);
            updateRotateEvents(i, time);
            updateColorEvents(i, time);
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
        if (presentMoveEventNum[Id] >= events.Count && events.Count>0)
        {
            Vector3 lastpos = events[events.Count - 1].positions[events[events.Count - 1].positions.Count - 1];
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().setposition(lastpos);
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
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().setposition(pos);
        }
        else if (presentMoveEventNum[Id] > 0)
        {
            Vector3 lastpos = events[presentMoveEventNum[Id] - 1].positions[events[presentMoveEventNum[Id] - 1].positions.Count - 1];
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().setposition(lastpos);
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
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().setangle(angle);
            return;
        }
        if (events[presentRotateEventNum[Id]].startTime <= time)
        {

            float angle = getAngle(events[presentRotateEventNum[Id]], time);
            //Debug.Log(time + ":" + angle);
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().setangle(angle);
        }
        else if (presentRotateEventNum[Id] > 0)
        {
            float angle = events[presentRotateEventNum[Id] - 1].endAngle;
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().setangle(angle);
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
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().color = color;
            return;
        }
        if (events[presentColorEventNum[Id]].startTime <= time)
        {
            Color color = getColor(events[presentColorEventNum[Id]], time);
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().color = color;
        }
        else if (presentColorEventNum[Id] > 0)
        {
            Color color = events[presentColorEventNum[Id] - 1].endColor;
            judgelineObjectList[Id].GetComponent<JudgeLineRenderer>().color = color;
        }
    }
}
