using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SettingsManager))]
public class FrontUIManager : MonoBehaviour
{
    // main slider
    Slider slider;
    Text time;
    Text pitch; // music speed

    // saveconfirm ui
    public GameObject saveConfirmUI;

    // Views
    public GameObject baseView,
        infoView,
        noteView,
        eventView,
        playView,
        timelineView,
        judgelineView,
        imageView;

    //infoView
    InputField info_songname;
    InputField info_composer;
    InputField info_charter;
    InputField info_illustrator;
    InputField info_bpm;
    InputField info_offset;
    InputField info_difficulty;
    InputField info_notenum;
    ColorSelector color_start_tips;

    // noteView
    Dropdown note_type;
    InputField note_starttime;
    InputField note_duration;
    InputField note_livingtime;
    InputField note_speed;
    Dropdown note_lineside;
    ColorSelector note_color_selector;
    InputField template_note_livingtime;
    InputField template_note_speed;
    ColorSelector template_note_color_selector;

    // baseView
    Text base_Title;

    // judgelineView
    InputField judgeline_num;
    InputField judgeline_id;
    InputField judgeline_notenum;
    InputField judgeline_eventnum;

    // imageView
    InputField img_num;
    Dropdown img_list;
    InputField img_name;
    InputField img_path;
    Image img_sprite;
    Dropdown img_sortingLayer;
    InputField img_sortingOrder;
    InputField img_starttime;
    InputField img_endtime;
    InputField img_eventnum;

    // eventView
    InputField event_type;
    InputField event_starttime;
    InputField event_endtime;
    Dropdown event_movetype;

    // moveEvent
    GameObject moveEventView;
    PosList move_poslist;
    Dropdown move_pathtype;

    //rotateEvent
    GameObject rotateEventView;
    InputField rotate_startangle;
    InputField rotate_endangle;

    //colorEvent
    GameObject colorEventView;
    ColorSelector color_start;
    ColorSelector color_end;

    //scaleEvent
    GameObject scaleEventView;
    InputField scale_start;
    InputField scale_end;

    //recordView
    Dropdown record_list;

    // public values
    public Color noteColor
    {
        get { return note_color_selector.color; }
    }

    public Color templateColor
    {
        get { return template_note_color_selector.color; }
    }

    public int posId
    {
        get { return move_poslist.selectedId; }
    }

    public Vector3 pos
    {
        get { return move_poslist.selectedPos; }
    }

    public Vector3 startPos
    {
        get { return move_poslist.startPos; }
    }

    public Vector3 endPos
    {
        get { return move_poslist.endPos; }
    }

    public Color startTipsColor
    {
        get { return color_start_tips.color; }
    }

    public Color startColor
    {
        get { return color_start.color; }
    }

    public Color endColor
    {
        get { return color_end.color; }
    }

    public void init()
    {
        slider = transform.Find("Slider").gameObject.GetComponent<Slider>();
        time = transform.Find("Time").gameObject.GetComponent<Text>();
        pitch = transform.Find("Pitch").gameObject.GetComponent<Text>();
        initInfoView();
        initBaseView();
        initNoteView();
        initJudgelineView();
        initImageView();
        initEventView();
        initRecordView();
    }

    private void initInfoView()
    {
        info_songname = infoView.transform.Find("songName").transform.Find("InputField").GetComponent<InputField>();
        info_composer = infoView.transform.Find("composer").transform.Find("InputField").GetComponent<InputField>();
        info_charter = infoView.transform.Find("charter").transform.Find("InputField").GetComponent<InputField>();
        info_illustrator = infoView.transform.Find("illustrator").transform.Find("InputField")
            .GetComponent<InputField>();
        info_bpm = infoView.transform.Find("bpm").transform.Find("InputField").GetComponent<InputField>();
        info_offset = infoView.transform.Find("offset").transform.Find("InputField").GetComponent<InputField>();
        info_difficulty = infoView.transform.Find("difficulty").transform.Find("InputField").GetComponent<InputField>();
        info_notenum = infoView.transform.Find("noteNum").transform.Find("InputField").GetComponent<InputField>();
        color_start_tips = infoView.transform.Find("colorStartTips").GetComponent<ColorSelector>();
        color_start_tips.init();
    }

    private void initEventView()
    {
        event_type = eventView.transform.Find("eventType").transform.Find("InputField").GetComponent<InputField>();
        event_starttime = eventView.transform.Find("starttime").transform.Find("InputField").GetComponent<InputField>();
        event_endtime = eventView.transform.Find("endtime").transform.Find("InputField").GetComponent<InputField>();
        event_movetype = eventView.transform.Find("moveType").transform.Find("Dropdown").GetComponent<Dropdown>();
        initMoveEvent();
        initRotateEvent();
        initColorEvent();
        initScaleEvent();
    }

    private void initMoveEvent()
    {
        moveEventView = eventView.transform.Find("moveEvent").gameObject;
        move_poslist = moveEventView.transform.Find("posList").GetComponent<PosList>();
        move_poslist.init();
        move_pathtype = moveEventView.transform.Find("pathType").transform.Find("Dropdown").GetComponent<Dropdown>();
    }

    private void initRotateEvent()
    {
        rotateEventView = eventView.transform.Find("rotateEvent").gameObject;
        rotate_endangle = rotateEventView.transform.Find("endAngle").transform.Find("InputField")
            .GetComponent<InputField>();
        rotate_startangle = rotateEventView.transform.Find("startAngle").transform.Find("InputField")
            .GetComponent<InputField>();
    }

    private void initColorEvent()
    {
        colorEventView = eventView.transform.Find("colorEvent").gameObject;
        color_start = colorEventView.transform.Find("startColor").GetComponent<ColorSelector>();
        color_start.init();
        color_end = colorEventView.transform.Find("endColor").GetComponent<ColorSelector>();
        color_end.init();
    }

    private void initScaleEvent()
    {
        scaleEventView = eventView.transform.Find("scaleEvent").gameObject;
        scale_start = scaleEventView.transform.Find("startScale").transform.Find("InputField")
            .GetComponent<InputField>();
        scale_end = scaleEventView.transform.Find("endScale").transform.Find("InputField").GetComponent<InputField>();
    }

    private void initBaseView()
    {
        base_Title = baseView.transform.parent.parent.parent.Find("Title").GetComponent<Text>();
        base_Title.text = "编辑判定线";
    }

    private void initNoteView()
    {
        note_type = noteView.transform.Find("type").transform.Find("Dropdown").GetComponent<Dropdown>();
        note_starttime = noteView.transform.Find("starttime").transform.Find("InputField").GetComponent<InputField>();
        note_duration = noteView.transform.Find("duration").transform.Find("InputField").GetComponent<InputField>();
        note_livingtime = noteView.transform.Find("livingTime").transform.Find("InputField").GetComponent<InputField>();
        note_speed = noteView.transform.Find("speed").transform.Find("InputField").GetComponent<InputField>();
        note_lineside = noteView.transform.Find("lineSide").transform.Find("Dropdown").GetComponent<Dropdown>();
        note_color_selector = noteView.transform.Find("colorSelector").GetComponent<ColorSelector>();
        note_color_selector.init();

        template_note_speed = noteView.transform.Find("speed_template").transform.Find("InputField")
            .GetComponent<InputField>();
        template_note_livingtime = noteView.transform.Find("livingTime_template").transform.Find("InputField")
            .GetComponent<InputField>();
        template_note_color_selector = noteView.transform.Find("colorSelector_template").GetComponent<ColorSelector>();
        template_note_color_selector.init();
    }

    private void initJudgelineView()
    {
        judgeline_num = judgelineView.transform.Find("lineNum").transform.Find("InputField").GetComponent<InputField>();
        judgeline_id = judgelineView.transform.Find("lineId").transform.Find("InputField").GetComponent<InputField>();
        judgeline_eventnum = judgelineView.transform.Find("eventNum").transform.Find("InputField")
            .GetComponent<InputField>();
        judgeline_notenum = judgelineView.transform.Find("noteNum").transform.Find("InputField")
            .GetComponent<InputField>();
    }

    private void initImageView()
    {
        img_num = imageView.transform.Find("imgNum").transform.Find("InputField").GetComponent<InputField>();
        img_name = imageView.transform.Find("imgName").transform.Find("InputField").GetComponent<InputField>();
        img_path = imageView.transform.Find("imgPath").transform.Find("InputField").GetComponent<InputField>();
        img_list = imageView.transform.Find("imgId").transform.Find("Dropdown").GetComponent<Dropdown>();
        img_sprite = imageView.transform.Find("Image").GetComponent<Image>();
        img_sortingLayer = imageView.transform.Find("sortLayer").transform.Find("Dropdown").GetComponent<Dropdown>();
        img_sortingOrder = imageView.transform.Find("sortOrder").transform.Find("InputField").GetComponent<InputField>();
        img_starttime = imageView.transform.Find("startTime").transform.Find("InputField").GetComponent<InputField>();
        img_endtime = imageView.transform.Find("endTime").transform.Find("InputField").GetComponent<InputField>();
        img_eventnum = imageView.transform.Find("eventNum").transform.Find("InputField").GetComponent<InputField>();
    }

    private void initRecordView()
    {
        record_list = GetComponent<SettingsManager>().settingsPanel.transform.Find("Scroll View").GetComponent<ScrollRect>().content.Find("record").GetComponent<Dropdown>();
    }

    public void setMaxTime(float t)
    {
        slider.maxValue = Mathf.Max(0.01f, t);
        time.text = Utilities.round(slider.value / 1000.0f, 2) + "/" + Utilities.round(slider.maxValue / 1000.0f, 2);
    }

    public void setTime(float t)
    {
        slider.SetValueWithoutNotify(Mathf.Clamp(t, slider.minValue, slider.maxValue));
        time.text = Utilities.round(slider.value / 1000.0f, 2) + "/" + Utilities.round(slider.maxValue / 1000.0f, 2);
    }

    public void setPitch(float t)
    {
        pitch.text = "播放倍速：" + Utilities.round(t, 1).ToString();
    }

    public void setInfo(Chart chart)
    {
        info_songname.text = chart.name;
        info_composer.text = chart.composer;
        info_charter.text = chart.charter;
        info_illustrator.text = chart.illustrator;
        info_bpm.text = chart.bpm.ToString();
        info_offset.text = chart.offset.ToString();
        info_difficulty.text = chart.difficulty.ToString();
        info_notenum.text = chart.noteNum.ToString();
        color_start_tips.setColor(chart.startTipcolor);
    }

    public void setNote(Note note)
    {
        activateView(noteView, true);
        activateView(eventView, false);
        switch (note.type)
        {
            case Config.Type.Tap:
                note_type.SetValueWithoutNotify(0);
                break;
            case Config.Type.Drag:
                note_type.SetValueWithoutNotify(1);
                break;
            case Config.Type.Hold:
                note_type.SetValueWithoutNotify(2);
                break;
        }

        note_starttime.text = note.time.ToString();
        note_duration.text = note.duration.ToString();
        note_livingtime.text = note.livingTime.ToString();
        note_speed.text = note.speed.ToString();
        switch (note.lineSide)
        {
            case Config.LineType.Line1:
                note_lineside.SetValueWithoutNotify(0);
                break;
            case Config.LineType.Line2:
                note_lineside.SetValueWithoutNotify(1);
                break;
        }

        note_color_selector.setColor(note.color);
    }

    public void setTemplateNote(Note note)
    {
        template_note_livingtime.text = note.livingTime.ToString();
        template_note_speed.text = note.speed.ToString();
        template_note_color_selector.setColor(note.color);
    }

    public void setJudgeline(JudgeLine judgeline, int id, int num)
    {
        judgeline_id.text = id.ToString();
        judgeline_num.text = num.ToString();
        int eventnum = judgeline.eventList.moveEvents.Count + judgeline.eventList.rotateEvents.Count +
                       judgeline.eventList.colorModifyEvents.Count;
        judgeline_eventnum.text = eventnum.ToString();
        judgeline_notenum.text = judgeline.noteList.Count.ToString();
    }

    public void setImage(PerformImg img, int id, int num, Texture2D texture = null)
    {
        img_num.text = num.ToString();
        img_list.SetValueWithoutNotify(id);
        img_list.options[id].text = img.name;
        img_name.text = img.name;
        img_path.text = img.path;
        img_sortingLayer.value = (int) img.layer;
        img_sortingOrder.text = img.sortingOrder.ToString();
        img_starttime.text = img.startTime.ToString();
        img_endtime.text = img.endTime.ToString();
        int eventnum = img.eventList.moveEvents.Count + img.eventList.rotateEvents.Count +
                       img.eventList.scaleXEvents.Count + img.eventList.scaleYEvents.Count +
                       img.eventList.colorModifyEvents.Count;
        img_eventnum.text = eventnum.ToString();
        if (img.path == null)
            img.path = "";
        if (Config.spriteList.ContainsKey(img.path))
        {
            img_sprite.sprite = Config.spriteList[img.path];
        }
        else
        {
            img_sprite.sprite = Resources.Load<Sprite>("Textures/defaultimg");
        }
        /*
        if (texture != null)
        {
            img_sprite.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
            img_sprite.sprite = Resources.Load<Sprite>("Textures/defaultimg");
        */
    }

    public void setEmptyImage()
    {
        img_num.text = "0";
        img_list.SetValueWithoutNotify(0);
        img_name.text = "";
        img_path.text = "";
        img_sortingLayer.value = 0;
        img_sortingOrder.text = "";
        img_starttime.text = "";
        img_endtime.text = "";
        img_eventnum.text = "";
        img_sprite.sprite = null;
    }

    public void setImageList(List<PerformImg> imgList)
    {
        img_list.ClearOptions();
        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();
        foreach (PerformImg img in imgList)
        {
            Debug.Log(img.name);
            newOptions.Add(new Dropdown.OptionData(img.name));
        }

        img_list.AddOptions(newOptions);
    }

    public void setEvent(string type, PerformEvent _event)
    {
        activateView(noteView, false);
        activateView(eventView, true);
        setEmptyEvent();
        event_starttime.text = _event.startTime.ToString();
        event_endtime.text = _event.endTime.ToString();
        event_movetype.SetValueWithoutNotify((int)_event.type);
        switch (type)
        {
            case "moveEvent":
                moveEventView.SetActive(true);
                setMoveEvent(_event as MoveEvent);
                break;
            case "rotateEvent":
                rotateEventView.SetActive(true);
                setRotateEvent(_event as RotateEvent);
                break;
            case "scaleXEvent":
                scaleEventView.SetActive(true);
                setScaleEvent(_event as ScaleEvent, false);
                break;
            case "scaleYEvent":
                scaleEventView.SetActive(true);
                setScaleEvent(_event as ScaleEvent, true);
                break;
            case "colorEvent":
                colorEventView.SetActive(true);
                setColorEvent(_event as ColorModifyEvent);
                break;
        }
    }

    private void setMoveEvent(MoveEvent _event)
    {
        event_type.text = "移动事件";
        move_pathtype.SetValueWithoutNotify((int)_event.pathType);
        move_poslist.setPosList(_event.positions, _event.pathType);
    }

    private void setRotateEvent(RotateEvent _event)
    {
        event_type.text = "旋转事件";
        rotate_startangle.text = _event.startAngle.ToString();
        rotate_endangle.text = _event.endAngle.ToString();
    }

    private void setColorEvent(ColorModifyEvent _event)
    {
        event_type.text = "颜色事件";
        color_start.setColor(_event.startColor);
        color_end.setColor(_event.endColor);
    }

    private void setScaleEvent(ScaleEvent _event, bool isY)
    {
        event_type.text = (isY ? "Y" : "X") + "大小事件";
        scale_start.text = _event.startScale.ToString();
        scale_end.text = _event.endScale.ToString();
    }

    public void setEmptyEvent()
    {
        moveEventView.SetActive(false);
        rotateEventView.SetActive(false);
        colorEventView.SetActive(false);
        scaleEventView.SetActive(false);
        event_type.text = "";
    }

    public void switchType(Config.EventlineType type)
    {
        if (type == Config.EventlineType.Judgeline)
        {
            judgelineView.SetActive(true);
            imageView.SetActive(false);
            base_Title.text = "编辑判定线";
        }
        else
        {
            judgelineView.SetActive(false);
            imageView.SetActive(true);
            base_Title.text = "编辑图像";
        }
    }

    public void setRecordList(List<editorRecord> records)
    {
        record_list.ClearOptions();
        List<Dropdown.OptionData> newOptions = new List<Dropdown.OptionData>();
        foreach (editorRecord record in records)
        {
            newOptions.Add(new Dropdown.OptionData(record.description));
        }

        record_list.AddOptions(newOptions);
    }

    public void setRecord(int id)
    {
        record_list.SetValueWithoutNotify(id);
    }

    public void hideUI(bool b)
    {
        baseView.SetActive(b);
        infoView.SetActive(b);
        noteView.SetActive(b);
        eventView.SetActive(b);
        playView.SetActive(b);
        timelineView.SetActive(b);
        judgelineView.SetActive(b);
        imageView.SetActive(b);
    }

    public void setSaveUI(bool b)
    {
        saveConfirmUI.SetActive(b);
    }

    private void activateView(GameObject view, bool b)
    {
        view.transform.parent.transform.parent.transform.parent.gameObject.SetActive(b);
    }
}