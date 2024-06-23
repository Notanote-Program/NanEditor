using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public class Chart
{
    public const int LatestVersion = 2;
    public int formatVersion = 0;
    public string name;
    public string composer;
    public string charter;
    public string illustrator;
    public float difficulty;
    public float bpm;
    public List<ModifyBpm> bpmList;
    public float offset;
    public int noteNum;
    public List<JudgeLine> judgelineList;
    public List<PerformImg> performImgList;

    [UsedImplicitly]
    public string _startTipcolor
    {
        get => ColorUtility.ToHtmlStringRGBA(startTipcolor);
        set => startTipcolor =
            ColorUtility.TryParseHtmlString("#" + value, out Color outcolor) ? outcolor : Color.white;
    }

    [JsonIgnore] public Color startTipcolor = Color.white;

    public Chart()
    {
        formatVersion = LatestVersion;
        offset = 0;
        bpm = 120; // default;
        bpmList = new List<ModifyBpm>();
        judgelineList = new List<JudgeLine>();
        performImgList = new List<PerformImg>();
    }

    public static Chart LoadChart(string path, Config.LoadType type = Config.LoadType.External)
    {
        if (type == Config.LoadType.External && !File.Exists(path)) return null;
        return UpdateChartVersion(Utilities.LoadFromJson<Chart>(path, type));
    }

    private static Chart UpdateChartVersion(Chart oldChart)
    {
        while (oldChart.formatVersion < LatestVersion)
        {
            switch (oldChart.formatVersion)
            {
                case 0:
                    foreach (JudgeLine judgeLine in oldChart.judgelineList)
                    {
                        judgeLine.eventList.moveEvents.ForEach(ConvertType);
                        judgeLine.eventList.rotateEvents.ForEach(ConvertType);
                        judgeLine.eventList.scaleEvents.ForEach(ConvertType);
                        judgeLine.eventList.colorModifyEvents.ForEach(ConvertType);
                    }

                    foreach (PerformImg performImg in oldChart.performImgList)
                    {
                        performImg.eventList.moveEvents.ForEach(ConvertType);
                        performImg.eventList.rotateEvents.ForEach(ConvertType);
                        performImg.eventList.scaleEvents.ForEach(ConvertType);
                        performImg.eventList.colorModifyEvents.ForEach(ConvertType);
                    }

                    break;
                case 1:
                    foreach (PerformImg performImg in oldChart.performImgList)
                    {
                        performImg.eventList.scaleXEvents.AddRange(performImg.eventList.scaleEvents.Select(e => e.Clone()));
                        performImg.eventList.scaleYEvents.AddRange(performImg.eventList.scaleEvents.Select(e => e.Clone()));
                        performImg.eventList.scaleEvents.Clear();
                    }
                    break;
            }

            oldChart.formatVersion++;
        }

        return oldChart;

        void ConvertType(PerformEvent performEvent)
        {
            int type = (int)performEvent.type;
            if (type > 0)
            {
                if (type == 10) type = 3;
                else type += 3;
            }

            performEvent.type = (Config.EventType)type;
            switch (performEvent.type)
            {
                case Config.EventType.CubicIn:
                    performEvent.type = Config.EventType.QuadIn;
                    break;
                case Config.EventType.CubicOut:
                    performEvent.type = Config.EventType.QuadOut;
                    break;
                case Config.EventType.CubicInOut:
                    performEvent.type = Config.EventType.QuadInOut;
                    break;
                case Config.EventType.QuadIn:
                    performEvent.type = Config.EventType.CubicIn;
                    break;
                case Config.EventType.QuadOut:
                    performEvent.type = Config.EventType.CubicOut;
                    break;
                case Config.EventType.QuadInOut:
                    performEvent.type = Config.EventType.CubicInOut;
                    break;
            }
        }
    }

    // public static IEnumerator LoadChartAsync(AsyncRequest<Chart> ar, string path, Config.LoadType type = Config.LoadType.External)
    // {
    //     if (type == Config.LoadType.External)
    //     {
    //         if (File.Exists(path))
    //         {
    //             Utilities.LoadFromJsonAsync<Chart>(ar, path, type);
    //         }
    //         else
    //         {
    //             ar.asset = null;
    //             ar.isDone = true;
    //             ar.progress = 1;
    //         }
    //     }
    //     else
    //     {
    //         Utilities.LoadFromJsonAsync<Chart>(ar, path, type);
    //     }
    //     yield return null;
    // }
    public static void SaveChart(Chart chart, string path)
    {
        Utilities.SaveJson(chart, path);
    }

    public static void Show(Chart chart)
    {
        Debug.Log(JsonUtility.ToJson(chart));
    }

    public void addBpmlist(float time, float bpm)
    {
    }

    public void deleteBpmlist(float time, float bpm)
    {
    }

    public void addJudgeline()
    {
        JudgeLine judgeline = new JudgeLine(new Color(1, 1, 1, 0), Vector3.zero);
        judgelineList.Add(judgeline);
    }

    public void deleteJudgeline(int id)
    {
        if (judgelineList.Count > 1)
            judgelineList.RemoveAt(id);
    }

    public void addNote(Note note, int lineId)
    {
        if (lineId < 0 || lineId > judgelineList.Count)
            return;
        List<Note> notelist = judgelineList[lineId].noteList;
        if (notelist.Count == 0)
        {
            notelist.Add(note);
        }

        int id = notelist.BinarySearch(note, new noteComparer());
        if (id < 0)
            id = -id - 1;
        if (id == notelist.Count)
        {
            if (note.time >= notelist[id - 1].time + notelist[id - 1].duration && !(notelist[id - 1].duration == 0 &&
                    note.time == notelist[id - 1].time + notelist[id - 1].duration))
                notelist.Add(note);
        }
        else if (id == 0)
        {
            if (notelist[id].time >= note.time + note.duration &&
                !(note.duration == 0 && notelist[id].time == note.time + note.duration))
                notelist.Insert(id, note);
        }
        else
        {
            if (note.duration > 0) // duration > 0
            {
                if (notelist[id].time >= note.time + note.duration &&
                    notelist[id - 1].duration + notelist[id - 1].time <= note.time)
                    notelist.Insert(id, note);
            }
            else if (note.duration == 0) // duration = 0
            {
                if (notelist[id].time >= note.time + note.duration &&
                    notelist[id - 1].time + notelist[id - 1].duration <= note.time &&
                    !(notelist[id - 1].duration == 0 &&
                      notelist[id - 1].time + notelist[id - 1].duration == note.time) &&
                    !(notelist[id].time == note.time + note.duration))
                    notelist.Insert(id, note);
            }
        }
    }

    public void deleteNote(Note note, int lineId)
    {
        if (lineId < 0 || lineId > judgelineList.Count)
            return;
        List<Note> notelist = judgelineList[lineId].noteList;
        if (notelist.Contains(note))
            notelist.Remove(note);
    }

    public void addPerformImg()
    {
        PerformImg img = new PerformImg("", new Color(1, 1, 1, 0), Vector3.zero);
        performImgList.Insert(0, img);
    }

    public void deletePerformImg(int id)
    {
        if (performImgList.Count > 0)
            performImgList.RemoveAt(id);
    }

    public int resetPerformImg(int id)
    {
        if (id < 0 || id >= performImgList.Count)
            return -1;
        if (performImgList.Count == 1)
            return 0;
        PerformImg img = performImgList[id];
        performImgList.RemoveAt(id);
        int _id = performImgList.BinarySearch(img, new imgComparer());
        Debug.Log("binarysearch:" + _id);
        if (_id < 0)
            _id = -_id - 1;
        performImgList.Insert(_id, img);
        return _id;
    }

    public int addEvent_Judgeline(int id, PerformEvent _event, string type,
        Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        Debug.Log("Add event");
        if (id < 0 || id >= this.judgelineList.Count)
            return -1;
        switch (type)
        {
            case "moveEvent":
                return addmoveEvent(_event as MoveEvent, this.judgelineList[id].eventList.moveEvents, pasteTyte);
            case "rotateEvent":
                return AddRotateEvent(_event as RotateEvent, this.judgelineList[id].eventList.rotateEvents, pasteTyte);
            case "colorEvent":
                return AddColorEvent(_event as ColorModifyEvent, this.judgelineList[id].eventList.colorModifyEvents,
                    pasteTyte);
            case "scaleXEvent":
            case "scaleYEvent":
                return 1;
        }

        return -1;
    }

    public int addEvent_PerformImg(int id, PerformEvent _event, string type,
        Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        if (id < 0 || id >= this.performImgList.Count)
            return -1;
        switch (type)
        {
            case "moveEvent":
                return addmoveEvent(_event as MoveEvent, this.performImgList[id].eventList.moveEvents, pasteTyte);
            case "rotateEvent":
                return AddRotateEvent(_event as RotateEvent, this.performImgList[id].eventList.rotateEvents, pasteTyte);
            case "colorEvent":
                return AddColorEvent(_event as ColorModifyEvent, this.performImgList[id].eventList.colorModifyEvents, pasteTyte);
            case "scaleXEvent":
                return AddScaleEvent(_event as ScaleEvent, this.performImgList[id].eventList.scaleXEvents, pasteTyte);
            case "scaleYEvent":
                return AddScaleEvent(_event as ScaleEvent, this.performImgList[id].eventList.scaleYEvents, pasteTyte);
        }

        return -1;
    }

    public void deleteEvent_Judgeline(int id, PerformEvent _event, string type)
    {
        if (id < 0 || id >= this.judgelineList.Count)
            return;
        switch (type)
        {
            case "moveEvent":
                deleteEvent(_event as MoveEvent, this.judgelineList[id].eventList.moveEvents);
                break;
            case "rotateEvent":
                deleteEvent(_event as RotateEvent, this.judgelineList[id].eventList.rotateEvents);
                break;
            case "colorEvent":
                deleteEvent(_event as ColorModifyEvent, this.judgelineList[id].eventList.colorModifyEvents);
                break;
        }
    }

    public void deleteEvent_PerformImg(int id, PerformEvent _event, string type)
    {
        if (id < 0 || id >= this.performImgList.Count)
            return;
        switch (type)
        {
            case "moveEvent":
                deleteEvent(_event as MoveEvent, this.performImgList[id].eventList.moveEvents);
                break;
            case "rotateEvent":
                deleteEvent(_event as RotateEvent, this.performImgList[id].eventList.rotateEvents);
                break;
            case "colorEvent":
                deleteEvent(_event as ColorModifyEvent, this.performImgList[id].eventList.colorModifyEvents);
                break;
            case "scaleXEvent":
                deleteEvent(_event as ScaleEvent, this.performImgList[id].eventList.scaleXEvents);
                break;
            case "scaleYEvent":
                deleteEvent(_event as ScaleEvent, this.performImgList[id].eventList.scaleYEvents);
                break;
        }
    }

    private int addmoveEvent(MoveEvent _event, List<MoveEvent> events,
        Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        if (events.Count == 0)
        {
            events.Add(_event);
            return 0;
        }

        int id = events.BinarySearch(_event, new moveEventComparer());
        if (id < 0)
            id = -id - 1;

        if (id == events.Count)
        {
            if (_event.startTime >= events[id - 1].endTime && !(events[id - 1].startTime == events[id - 1].endTime &&
                                                                _event.startTime == events[id - 1].endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    MoveEvent prev_event = events[id - 1];
                    _event.positions[0] = prev_event.positions[prev_event.positions.Count - 1];
                    if (id < events.Count)
                    {
                        MoveEvent next_event = events[id];
                        _event.positions[_event.positions.Count - 1] = next_event.positions[0];
                    }
                }
                else if (pasteTyte == Config.PasteTyte.Smart)
                {
                    MoveEvent prev_event = events[id - 1];
                    Vector3 delta_pos = -_event.positions[0] + prev_event.positions[prev_event.positions.Count - 1];
                    for (int i = 0; i < _event.positions.Count; i++)
                    {
                        _event.positions[i] += delta_pos;
                    }
                }

                events.Add(_event);
                return id;
            }
        }
        else if (id == 0)
        {
            if (events[id].startTime >= _event.endTime &&
                !(_event.startTime == _event.endTime && events[id].startTime == _event.endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    _event.positions[_event.positions.Count - 1] = events[0].positions[0];
                }

                events.Insert(id, _event);
                return id;
            }
        }
        else
        {
            if (_event.endTime - _event.startTime > 0) // duration > 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime)
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        MoveEvent prev_event = events[id - 1];
                        _event.positions[0] = prev_event.positions[prev_event.positions.Count - 1];
                        if (id < events.Count)
                        {
                            MoveEvent next_event = events[id];
                            _event.positions[_event.positions.Count - 1] = next_event.positions[0];
                        }
                    }
                    else if (pasteTyte == Config.PasteTyte.Smart)
                    {
                        MoveEvent prev_event = events[id - 1];
                        Vector3 delta_pos = -_event.positions[0] + prev_event.positions[prev_event.positions.Count - 1];
                        for (int i = 0; i < _event.positions.Count; i++)
                        {
                            _event.positions[i] += delta_pos;
                        }
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
            else if (_event.endTime == _event.startTime) // duration = 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime &&
                    !(events[id - 1].endTime - events[id - 1].startTime == 0 &&
                      events[id - 1].endTime == _event.startTime) && !(events[id].startTime == _event.endTime))
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        MoveEvent prev_event = events[id - 1];
                        _event.positions[0] = prev_event.positions[prev_event.positions.Count - 1];
                        if (id < events.Count)
                        {
                            MoveEvent next_event = events[id];
                            _event.positions[_event.positions.Count - 1] = next_event.positions[0];
                        }
                    }
                    else if (pasteTyte == Config.PasteTyte.Smart)
                    {
                        MoveEvent prev_event = events[id - 1];
                        Vector3 delta_pos = -_event.positions[0] + prev_event.positions[prev_event.positions.Count - 1];
                        for (int i = 0; i < _event.positions.Count; i++)
                        {
                            _event.positions[i] += delta_pos;
                        }
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
        }

        return -1;
    }

    private int AddRotateEvent(RotateEvent _event, List<RotateEvent> events,
        Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        if (events.Count == 0)
        {
            events.Add(_event);
            return 0;
        }

        int id = events.BinarySearch(_event, new rotateEventComparer());
        if (id < 0)
            id = -id - 1;

        if (id == events.Count)
        {
            if (_event.startTime >= events[id - 1].endTime && !(events[id - 1].startTime == events[id - 1].endTime &&
                                                                _event.startTime == events[id - 1].endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    RotateEvent prev_event = events[id - 1];
                    _event.startAngle = prev_event.endAngle;
                    if (id < events.Count)
                    {
                        _event.endAngle = events[id].startAngle;
                    }
                }
                else if (pasteTyte == Config.PasteTyte.Smart)
                {
                    RotateEvent prev_event = events[id - 1];
                    float delta_angle = prev_event.endAngle - _event.startAngle;
                    _event.startAngle += delta_angle;
                    _event.endAngle += delta_angle;
                }

                events.Add(_event);
                return id;
            }
        }
        else if (id == 0)
        {
            if (events[id].startTime >= _event.endTime &&
                !(_event.startTime == _event.endTime && events[id].startTime == _event.endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    _event.endAngle = events[0].startAngle;
                }

                events.Insert(id, _event);
                return id;
            }
        }
        else
        {
            if (_event.endTime - _event.startTime > 0) // duration > 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime)
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        RotateEvent prev_event = events[id - 1];
                        _event.startAngle = prev_event.endAngle;
                        if (id < events.Count)
                        {
                            _event.endAngle = events[id].startAngle;
                        }
                    }
                    else if (pasteTyte == Config.PasteTyte.Smart)
                    {
                        RotateEvent prev_event = events[id - 1];
                        float delta_angle = prev_event.endAngle - _event.startAngle;
                        _event.startAngle += delta_angle;
                        _event.endAngle += delta_angle;
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
            else if (_event.endTime == _event.startTime) // duration = 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime &&
                    !(events[id - 1].endTime - events[id - 1].startTime == 0 &&
                      events[id - 1].endTime == _event.startTime) && !(events[id].startTime == _event.endTime))
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        RotateEvent prev_event = events[id - 1];
                        _event.startAngle = prev_event.endAngle;
                        if (id < events.Count)
                        {
                            _event.endAngle = events[id].startAngle;
                        }
                    }
                    else if (pasteTyte == Config.PasteTyte.Smart)
                    {
                        RotateEvent prev_event = events[id - 1];
                        float delta_angle = prev_event.endAngle - _event.startAngle;
                        _event.startAngle += delta_angle;
                        _event.endAngle += delta_angle;
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
        }

        return -1;
    }
    
    private int AddScaleEvent(ScaleEvent _event, List<ScaleEvent> events,
        Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        if (events.Count == 0)
        {
            events.Add(_event);
            return 0;
        }

        int id = events.BinarySearch(_event, new scaleEventComparer());
        if (id < 0)
            id = -id - 1;


        if (id == events.Count)
        {
            if (_event.startTime >= events[id - 1].endTime && !(events[id - 1].startTime == events[id - 1].endTime &&
                                                                _event.startTime == events[id - 1].endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    _event.startScale = events[id - 1].endScale;
                }
                else if (pasteTyte == Config.PasteTyte.Smart)
                {
                    float delta_scale = events[id - 1].endScale - _event.startScale;
                    _event.startScale += delta_scale;
                    _event.endScale += delta_scale;
                    // _event.startScale = Mathf.Max(0, _event.startScale);
                    // _event.endScale = Mathf.Max(0, _event.endScale);
                }

                events.Add(_event);
                return id;
            }
        }
        else if (id == 0)
        {
            if (events[id].startTime >= _event.endTime &&
                !(_event.startTime == _event.endTime && events[id].startTime == _event.endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    _event.endScale = events[0].startScale;
                }

                events.Insert(id, _event);
                return id;
            }
        }
        else
        {
            if (_event.endTime - _event.startTime > 0) // duration > 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime)
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        _event.startScale = events[id - 1].endScale;
                        if (id < events.Count)
                        {
                            _event.endScale = events[id].startScale;
                        }
                    }
                    else if (pasteTyte == Config.PasteTyte.Smart)
                    {
                        float delta_scale = events[id - 1].endScale - _event.startScale;
                        _event.startScale += delta_scale;
                        _event.endScale += delta_scale;
                        // _event.startScale = Mathf.Max(0, _event.startScale);
                        // _event.endScale = Mathf.Max(0, _event.endScale);
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
            else if (_event.endTime == _event.startTime) // duration = 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime &&
                    !(events[id - 1].endTime - events[id - 1].startTime == 0 &&
                      events[id - 1].endTime == _event.startTime) && !(events[id].startTime == _event.endTime))
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        _event.startScale = events[id - 1].endScale;
                        if (id < events.Count)
                        {
                            _event.endScale = events[id].startScale;
                        }
                    }
                    else if (pasteTyte == Config.PasteTyte.Smart)
                    {
                        float delta_scale = events[id - 1].endScale - _event.startScale;
                        _event.startScale += delta_scale;
                        _event.endScale += delta_scale;
                        _event.startScale = Mathf.Max(0, _event.startScale);
                        _event.endScale = Mathf.Max(0, _event.endScale);
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
        }

        return -1;
    }

    private int AddColorEvent(ColorModifyEvent _event, List<ColorModifyEvent> events,
        Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        if (events.Count == 0)
        {
            events.Add(_event);
            return 0;
        }

        int id = events.BinarySearch(_event, new colorEventComparer());
        if (id < 0)
            id = -id - 1;

        if (id == events.Count)
        {
            if (_event.startTime >= events[id - 1].endTime && !(events[id - 1].startTime == events[id - 1].endTime &&
                                                                _event.startTime == events[id - 1].endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    _event.startColor = events[id - 1].endColor;
                }

                events.Add(_event);
                return id;
            }
        }
        else if (id == 0)
        {
            if (events[id].startTime >= _event.endTime &&
                !(_event.startTime == _event.endTime && events[id].startTime == _event.endTime))
            {
                if (pasteTyte == Config.PasteTyte.Inherit)
                {
                    _event.endColor = events[0].startColor;
                }

                events.Insert(id, _event);
                return id;
            }
        }
        else
        {
            if (_event.endTime - _event.startTime > 0) // duration > 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime)
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        _event.startColor = events[id - 1].endColor;
                        if (id < events.Count)
                        {
                            _event.endColor = events[id].startColor;
                        }
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
            else if (_event.endTime == _event.startTime) // duration = 0
            {
                if (events[id].startTime >= _event.endTime && events[id - 1].endTime <= _event.startTime &&
                    !(events[id - 1].endTime - events[id - 1].startTime == 0 &&
                      events[id - 1].endTime == _event.startTime) && !(events[id].startTime == _event.endTime))
                {
                    if (pasteTyte == Config.PasteTyte.Inherit)
                    {
                        _event.startColor = events[id - 1].endColor;
                        if (id < events.Count)
                        {
                            _event.endColor = events[id].startColor;
                        }
                    }

                    events.Insert(id, _event);
                    return id;
                }
            }
        }

        return -1;
    }

    private void deleteEvent<T>(T _event, List<T> events) where T : PerformEvent
    {
        if (events.Contains(_event))
            events.Remove(_event);
    }

    public void addMovePosition(MoveEvent _event, int id, Vector3 pos)
    {
        if (id >= 0 && id < _event.positions.Count)
            _event.positions.Insert(id + 1, pos);
    }

    public void deleteMovePosition(MoveEvent _event, int id)
    {
        if (_event.positions.Count > 2)
            _event.positions.RemoveAt(id);
    }

    public void editMovePosition(MoveEvent _event, int id, Vector3 pos)
    {
        if (id >= 0 && id < _event.positions.Count)
            _event.positions[id] = pos;
    }

    public void movePositionAddDelta(MoveEvent _event, Vector3 delta_pos)
    {
        for (int i = 0; i < _event.positions.Count; i++)
            _event.positions[i] += delta_pos;
    }

    public void flipPositionX(MoveEvent _event)
    {
        for (int i = 0; i < _event.positions.Count; i++)
            _event.positions[i] = new Vector3(-_event.positions[i].x, _event.positions[i].y, _event.positions[i].z);
    }

    public void flipPositionY(MoveEvent _event)
    {
        for (int i = 0; i < _event.positions.Count; i++)
            _event.positions[i] = new Vector3(_event.positions[i].x, -_event.positions[i].y, _event.positions[i].z);
    }
}

public class JudgeLine
{
    public List<Note> noteList;
    public EventList eventList;

    public string _color
    {
        get { return ColorUtility.ToHtmlStringRGBA(color); }
        set
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color outcolor);
            color = outcolor;
        }
    }

    public string _pos
    {
        get { return "(" + position.x + "," + position.y + "," + position.z + ")"; }
        set
        {
            if (value.Length >= 4) position = Utilities.string2vector(value);
        }
    }

    [JsonIgnore] public Color color;
    [JsonIgnore] public Vector3 position;
    public float angle;
    public float scale;

    public JudgeLine(Color color, Vector3 position, float angle = 0, float scale = 1)
    {
        this.noteList = new List<Note>();
        this.eventList = new EventList();
        this.color = color;
        this.position = position;
        this.angle = angle;
        this.scale = scale;
    }
}

public class Note
{
    public Config.Type type;
    public float time;
    public float duration;
    public float speed;
    public float livingTime;
    public int lineId;
    public Config.LineType lineSide;
    public bool fake;

    public string _color
    {
        get { return ColorUtility.ToHtmlStringRGBA(color); }
        set
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color outcolor);
            color = outcolor;
        }
    }

    [JsonIgnore] public Color color;

    public Note(Config.Type type, Color color, float time, float duration = 0, float speed = 1, float livingTime = 5,
        Config.LineType lineSide = Config.LineType.Line1, bool fake = false)
    {
        this.type = type;
        this.time = time;
        this.speed = speed;
        this.livingTime = livingTime;
        this.lineId = 0;
        this.lineSide = lineSide;
        this.fake = fake;
        this.color = color;
        this.duration = duration;
    }
}

public class PerformImg
{
    public string path;
    public string name;
    public EventList eventList;

    public string _color
    {
        get { return ColorUtility.ToHtmlStringRGBA(color); }
        set
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color outcolor);
            color = outcolor;
        }
    }

    public string _pos
    {
        get { return "(" + position.x + "," + position.y + "," + position.z + ")"; }
        set
        {
            if (value.Length >= 4) position = Utilities.string2vector(value);
        }
    }

    [JsonIgnore] public Color color;
    [JsonIgnore] public Vector3 position;
    public float angle;
    public float scale;
    public float scaleX;
    public float scaleY;
    public float startTime;
    public float endTime;
    public Config.PerformImgLayer layer = Config.PerformImgLayer.Background;
    public int sortingOrder;

    public PerformImg(string path, Color color, Vector3 position, float startTime = 0, float endTime = 1000,
        float angle = 0, float scale = 1, Config.PerformImgLayer layer = Config.PerformImgLayer.Background, int sortingOrder = 500)
    {
        this.name = "";
        this.path = path;
        this.color = color;
        this.eventList = new EventList();
        this.position = position;
        this.startTime = startTime;
        this.endTime = endTime;
        this.angle = angle;
        this.scale = scale;
        this.layer = layer;
        this.sortingOrder = sortingOrder;
    }
}

public class ModifyBpm
{
    public float time;
    public float bpm;

    public ModifyBpm(float time, float bpm)
    {
        this.time = time;
        this.bpm = bpm;
    }
}

public class PerformEvent
{
    public float startTime;
    public float endTime;
    public Config.EventType type;

    public PerformEvent(float startTime = 0, float endTime = 0, Config.EventType type = Config.EventType.Linear)
    {
        this.startTime = startTime;
        this.endTime = endTime;
        this.type = type;
    }
}

public class MoveEvent : PerformEvent
{
    public string _pos
    {
        get
        {
            string s = "";
            foreach (Vector3 pos in positions)
            {
                s = s + "(" + pos.x + "," + pos.y + "," + pos.z + ") ";
            }

            return s;
        }
        set
        {
            positions.Clear();
            if (value.Length > 0)
            {
                string[] _s = value.Split(" ");
                foreach (string s in _s)
                {
                    if (s.Length >= 4)
                        positions.Add(Utilities.string2vector(s));
                }
            }
        }
    }

    [JsonIgnore] public List<Vector3> positions;
    public Config.PathType pathType;

    public MoveEvent(List<Vector3> positions = null, Config.PathType pathType = Config.PathType.Bessel,
        float startTime = 0, float endTime = 0, Config.EventType type = Config.EventType.Linear) : base(startTime,
        endTime, type)
    {
        if (positions == null)
        {
            this.positions = new List<Vector3>() { Vector3.zero, Vector3.zero };
        }
        else
            this.positions = new List<Vector3>(positions);

        this.pathType = pathType;
    }
}

public class RotateEvent : PerformEvent
{
    public float startAngle;
    public float endAngle;

    public RotateEvent(float startAngle = 0, float endAngle = 0, float startTime = 0, float endTime = 0,
        Config.EventType type = Config.EventType.Linear) : base(startTime, endTime, type)
    {
        this.startAngle = startAngle;
        this.endAngle = endAngle;
    }
}

public class ScaleEvent : PerformEvent
{
    public float startScale;
    public float endScale;

    public ScaleEvent(float startScale = 0, float endScale = 0, float startTime = 0, float endTime = 0,
        Config.EventType type = Config.EventType.Linear) : base(startTime, endTime, type)
    {
        this.startScale = startScale;
        this.endScale = endScale;
    }

    public ScaleEvent Clone()
    {
        return new ScaleEvent(startScale, endScale, startTime, endTime, type);
    }
}

public class ColorModifyEvent : PerformEvent
{
    public string _startcolor
    {
        get { return ColorUtility.ToHtmlStringRGBA(startColor); }
        set
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color outcolor);
            startColor = outcolor;
        }
    }

    public string _endcolor
    {
        get { return ColorUtility.ToHtmlStringRGBA(endColor); }
        set
        {
            ColorUtility.TryParseHtmlString("#" + value, out Color outcolor);
            endColor = outcolor;
        }
    }

    [JsonIgnore] public Color startColor;
    [JsonIgnore] public Color endColor;

    public ColorModifyEvent(Color startColor, Color endColor, float startTime = 0, float endTime = 0,
        Config.EventType type = Config.EventType.Linear) : base(startTime, endTime, type)
    {
        this.startColor = startColor;
        this.endColor = endColor;
    }
}

public class EventList
{
    public List<MoveEvent> moveEvents;
    public List<RotateEvent> rotateEvents;
    public List<ColorModifyEvent> colorModifyEvents;
    public List<ScaleEvent> scaleEvents;
    public List<ScaleEvent> scaleXEvents;
    public List<ScaleEvent> scaleYEvents;
    
    public EventList(List<MoveEvent> moveEvents = null, List<RotateEvent> rotateEvents = null,
        List<ColorModifyEvent> colorModifyEvents = null, List<ScaleEvent> scaleXEvents = null, List<ScaleEvent> scaleYEvents = null)
    {
        this.moveEvents = moveEvents == null ? new List<MoveEvent>() : new List<MoveEvent>(moveEvents);
        this.rotateEvents = rotateEvents == null ? new List<RotateEvent>() : new List<RotateEvent>(rotateEvents);
        this.colorModifyEvents = colorModifyEvents == null ? new List<ColorModifyEvent>() : new List<ColorModifyEvent>(colorModifyEvents);
        this.scaleXEvents = scaleXEvents == null ? new List<ScaleEvent>() : new List<ScaleEvent>(scaleXEvents);
        this.scaleYEvents = scaleYEvents == null ? new List<ScaleEvent>() : new List<ScaleEvent>(scaleYEvents);
    }

    public EventList Clone()
    {
        return new EventList(moveEvents, rotateEvents, colorModifyEvents, scaleXEvents, scaleYEvents);
    }
}

public class noteComparer : IComparer<Note>
{
    public int Compare(Note x, Note y)
    {
        return x.time.CompareTo(y.time);
    }
}

public class imgComparer : IComparer<PerformImg>
{
    public int Compare(PerformImg x, PerformImg y)
    {
        return x.startTime.CompareTo(y.startTime);
    }
}

public class moveEventComparer : IComparer<MoveEvent>
{
    public int Compare(MoveEvent x, MoveEvent y)
    {
        return x.startTime.CompareTo(y.startTime);
    }
}

public class rotateEventComparer : IComparer<RotateEvent>
{
    public int Compare(RotateEvent x, RotateEvent y)
    {
        return x.startTime.CompareTo(y.startTime);
    }
}

public class scaleEventComparer : IComparer<ScaleEvent>
{
    public int Compare(ScaleEvent x, ScaleEvent y)
    {
        return x.startTime.CompareTo(y.startTime);
    }
}

public class colorEventComparer : IComparer<ColorModifyEvent>
{
    public int Compare(ColorModifyEvent x, ColorModifyEvent y)
    {
        return x.startTime.CompareTo(y.startTime);
    }
}