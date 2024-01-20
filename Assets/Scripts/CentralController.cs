using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CentralController : MonoBehaviour
{
    private const float sync_time = 0.1f;
    GameObject display_manager;
    GameObject editor;
    GameObject chart_maker;
    GameObject front_ui;

    public string chart_name;
    public string song_name;

    public Chart chart;
    private Config.EventlineType type;// judgeline or performImage
    private int id;// index of current judgeline or performImage
    private int id_judgeline;
    private int id_performimg;

    List<editorRecord> recordList;
    const int MaxRecordNum = 100;// max num of record
    int recordIndex = 0;// index of record

    bool autosave_on = true;

    private float density
    {
        get { return chart_maker.GetComponent<chartMaker>().density; }
    }
    private float guideBeats
    {
        get { return chart_maker.GetComponent<chartMaker>().guideBeats; }
    }
    public float time
    {
        get { return _time; }
        set { _time = Mathf.Max(0, value); }
    }
    private float _time;
    private enum State
    {
        editor,
        display
    }
    State state;// edit mode or display mode

    bool showEditor = true;
    bool showChart = true;
    bool showLineId = false;
    bool showUI = true;

    Note template;// a template to generate notes

    // Start is called before the first frame update
    void Start()
    {
        init();
    }
    // Update is called once per frame
    void Update()
    {
        display();
        userInput();
    }
    public void loadChart()
    {
        // todo
        Config.spriteList.Clear();//reload textures at begining

        string path = System.Environment.CurrentDirectory + "/Charts/" + chart_name + "/" + chart_name + ".json";
        Chart newchart = Chart.LoadChart(path, Config.LoadType.External);
        if (newchart != null)
        {
            chart = updateChartVersion(newchart);
            record("load chart: " + chart_name);
            if(autosave_on)
                StartCoroutine(autoSave());
        }
        reset();
        synchronize();
        setInfo();
        if (type == Config.EventlineType.Judgeline)
        {
            id = Mathf.Min(id, chart.judgelineList.Count - 1);
            front_ui.GetComponent<FrontUIManager>().setJudgeline(chart.judgelineList[id], id, chart.judgelineList.Count);
        }
        else
        {
            id = Mathf.Min(id, chart.performImgList.Count - 1);
            if (id < 0)
                id = 0;
            front_ui.GetComponent<FrontUIManager>().setImageList(chart.performImgList);
            if (chart.performImgList.Count > 0)
            {
                front_ui.GetComponent<FrontUIManager>().setImage(chart.performImgList[id], id, chart.performImgList.Count);
            }
        }

    }

    private Chart updateChartVersion(Chart oldChart)
    {
        while (oldChart.formatVersion < Chart.LatestVersion)
        {
            if (oldChart.formatVersion == 0)
            {
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
        }
    }
    public void saveChart()
    {
        int num = 0;
        foreach (JudgeLine line in chart.judgelineList)
        {
            num += line.noteList.Count;
        }
        chart.noteNum = num;
        string path = System.Environment.CurrentDirectory + "/Charts/" + chart_name + "/" + chart_name + ".json";
        Chart.SaveChart(chart, path);
        setInfo();
    }
    public void trySaveChart()
    {
        front_ui.GetComponent<FrontUIManager>().setSaveUI(true);
    }
    private IEnumerator autoSave()
    {
        while (true)
        {
            yield return new WaitForSeconds(30);
            int num = 0;
            foreach (JudgeLine line in chart.judgelineList)
            {
                num += line.noteList.Count;
            }
            chart.noteNum = num;
            string path = System.Environment.CurrentDirectory + "/Charts/" + chart_name + "/" + chart_name + "_autosaved.json";
            Chart.SaveChart(chart, path);
        }
    }
    private void init()
    {
        chart = new Chart();
        chart.judgelineList.Add(new JudgeLine(Color.white, Vector3.zero));

        recordList = new List<editorRecord>();

        editor = this.gameObject;
        chart_maker = this.transform.Find("ChartMaker").gameObject;
        front_ui = this.transform.Find("FrontUI").gameObject;
        display_manager = GameObject.Find("manager").gameObject;
        display_manager.GetComponent<Test>().chart_name = chart_name;
        display_manager.GetComponent<Test>().song_name = song_name;

        chart_maker.GetComponent<chartMaker>().init(chart);
        front_ui.GetComponent<FrontUIManager>().init();
        display_manager.GetComponent<Test>().init(chart, Config.LoadType.External);

        state = State.editor;
        time = 0;
        template = new Note(Config.Type.Tap, Color.white, 0, 0, 10, 2000);

        id = 0;
        id_judgeline = 0;
        id_performimg = 0;

        front_ui.GetComponent<FrontUIManager>().setJudgeline(chart.judgelineList[0], 0, chart.judgelineList.Count);
        front_ui.GetComponent<FrontUIManager>().setImageList(chart.performImgList);
        if (chart.performImgList.Count > 0)
        {
            front_ui.GetComponent<FrontUIManager>().setImage(chart.performImgList[0], 0, chart.performImgList.Count);
        }
    }
    private void display()
    {
        if (state == State.display)
        {
            float t = display_manager.GetComponent<Test>().getChartTime();
            if (t > display_manager.GetComponent<AudioSource>().clip.length * 1000.0f)
            {
                t = display_manager.GetComponent<AudioSource>().clip.length * 1000.0f;
                Pause();
                return;
            }
            if (showEditor)
                chart_maker.GetComponent<chartMaker>().setTime(t);
            setTime(t);
        }
    }
    public void setTime(float t)
    {
        this.time = t;
        if (state == State.editor)
        {
            display_manager.GetComponent<Test>().setTime(time, showLineId);
            chart_maker.GetComponent<chartMaker>().setTime(time);
        }
        front_ui.GetComponent<FrontUIManager>().setTime(time);
    }
    public void Play()
    {
        state = State.display;
        display_manager.GetComponent<Test>().Play(showChart, showLineId);
        chart_maker.GetComponent<chartMaker>().setVisible(showEditor);
    }
    public void Pause()
    {
        state = State.editor;
        display_manager.GetComponent<Test>().Pause();
        chart_maker.GetComponent<chartMaker>().setVisible(true);
        float t = display_manager.GetComponent<Test>().getChartTime();
        setTime(t);
    }
    public void reset()
    {
        if (state == State.editor)
        {
            Debug.Log(chart.noteNum);
            //synchronize();
            display_manager.GetComponent<Test>().reset(chart);
            chart_maker.GetComponent<chartMaker>().reset(chart);
            float t = display_manager.GetComponent<Test>().getChartTime();
            chart_maker.GetComponent<chartMaker>().setTime(t);
            setUI();
        }
    }
    public void record(string description = "???")
    {
        while (recordList.Count > recordIndex + 1)
            recordList.RemoveAt(recordList.Count - 1);
        string new_record = Utilities.toString(chart);
        recordList.Add(new editorRecord(new_record,description));
        while (recordList.Count > MaxRecordNum)
            recordList.RemoveAt(0);
        recordIndex = recordList.Count - 1;
        front_ui.GetComponent<FrontUIManager>().setRecordList(recordList);
        front_ui.GetComponent<FrontUIManager>().setRecord(recordIndex);
        Debug.Log("recorded num:" + recordList.Count);
    }
    public void Redo()
    {
        if (recordList.Count - 1 < recordIndex + 1)
            return;
        recordIndex++;
        front_ui.GetComponent<FrontUIManager>().setRecord(recordIndex);
        chart = Utilities.fromString<Chart>(recordList[recordIndex].value);
        reset();
        chart_maker.GetComponent<chartMaker>().cancelSelect();
        setEvent();
        garbageCollect();
    }
    public void Undo()
    {
        if (recordList.Count <= 1 || recordIndex == 0)
            return;
        recordIndex--;
        front_ui.GetComponent<FrontUIManager>().setRecord(recordIndex);
        chart = Utilities.fromString<Chart>(recordList[recordIndex].value);        
        reset();
        chart_maker.GetComponent<chartMaker>().cancelSelect();
        setEvent();
        garbageCollect();
    }
    public void backtraceRecord(int id)
    {
        if (id >= recordList.Count || id < 0)
            return;
        recordIndex = id;
        chart = Utilities.fromString<Chart>(recordList[recordIndex].value);
        reset();
        chart_maker.GetComponent<chartMaker>().cancelSelect();
        setEvent();
        garbageCollect();
    }
    public void ShowEditor(bool b)
    {
        showEditor = b;
    }
    public void ShowChart(bool b)
    {
        showChart = b;
    }
    public void ShowLineId(bool b)
    {
        showLineId = b;
    }
    private Chart getTestChart()
    {
        Chart chart = new Chart();
        chart.bpm = 120;
        chart.offset = 2;
        JudgeLine judgeLine = new JudgeLine(Color.white, new Vector3(0.2f, 0f, 0));
        JudgeLine judgeLine2 = new JudgeLine(Color.white, new Vector3(-0.2f, -0.5f, 0));
        List<PerformImg> img = new List<PerformImg>();
        PerformImg img2 = new PerformImg("img1", Color.white, new Vector3(0.3f, 0, 0), 0, 100);
        for (int i = 0; i < 10000; i++)
        {
            judgeLine.noteList.Add(new Note(Config.Type.Hold, Color.white, 1f + 2f * i, 1f, 15, 2f));
            judgeLine2.noteList.Add(new Note(Config.Type.Tap, Color.white, 0.5f * i, 0, 15, 2f));
        }
        List<Vector3> positions1 = new List<Vector3>();
        List<Vector3> positions2 = new List<Vector3>();
        positions1.Add(new Vector3(-0.5f, -0.5f, 0));
        positions1.Add(new Vector3(0.5f, -0.5f, 0));
        positions2.Add(new Vector3(0.5f, -0.5f, 0));
        positions2.Add(new Vector3(-0.5f, -0.5f, 0));
        for (int i = 0; i < 20000; i++)
        {
            judgeLine2.eventList.moveEvents.Add(new MoveEvent(positions1, Config.PathType.Bessel, 5f * i, 5f * i + 2.5f, Config.EventType.CubicInOut));
            judgeLine2.eventList.moveEvents.Add(new MoveEvent(positions2, Config.PathType.Bessel, 5f * i + 2.5f, 5f * i + 5f, Config.EventType.CubicInOut));
            judgeLine.eventList.rotateEvents.Add(new RotateEvent(30 * i, 30 * i + 33, 1f * i, 1f * i + 0.3f, Config.EventType.CubicOut));
            judgeLine.eventList.rotateEvents.Add(new RotateEvent(30 * i + 33, 30 * i + 30, 1f * i + 0.3f, 1f * i + 0.35f, Config.EventType.CubicInOut));
            judgeLine.eventList.colorModifyEvents.Add(new ColorModifyEvent(Color.white, new Color(1, 1, 1, 0), 1f * i, 1f * i + 0.3f, Config.EventType.CubicIn));
            judgeLine.eventList.colorModifyEvents.Add(new ColorModifyEvent(new Color(1, 1, 1, 0), Color.white, 1f * i + 0.5f, 1f * i + 0.5f, Config.EventType.CubicIn));
            //img.eventList.moveEvents.Add(new MoveEvent(positions1, Config.PathType.Bessel, 5f * i, 5f * i + 2.5f, Config.EventType.Uniform));
            //img.eventList.moveEvents.Add(new MoveEvent(positions2, Config.PathType.Bessel, 5f * i + 2.5f, 5f * i + 5f, Config.EventType.Uniform));
            //img2.eventList.rotateEvents.Add(new RotateEvent(30 * i, 30 * i + 30, 1f * i, 1f * i + 0.3f, Config.EventType.Deceletate));
            img2.eventList.moveEvents.Add(new MoveEvent(positions2, Config.PathType.Bessel, 5f * i, 5f * i + 2.5f, Config.EventType.Linear));
            img2.eventList.moveEvents.Add(new MoveEvent(positions1, Config.PathType.Bessel, 5f * i + 2.5f, 5f * i + 5f, Config.EventType.Linear));
            img2.eventList.rotateEvents.Add(new RotateEvent(30 * i, 30 * i + 30, 1f * i, 1f * i + 0.3f, Config.EventType.CubicOut));
            img2.eventList.scaleEvents.Add(new ScaleEvent(1.2f, 1f, 1f * i, 1f * i + 0.2f, Config.EventType.CubicIn));
            //img.Add(new PerformImg("Textures/ring2", Color.white, new Vector3(0.5f * Mathf.Sin(i), 0.5f * Mathf.Cos(i), 0), 2 * i, 2 * i + 20, 0, 0.3f, 50));
        }
        //Debug.Log(judgeLine.eventList.colorModifyEvents.Count);
        chart.noteNum = 20000;
        chart.judgelineList.Add(judgeLine);
        chart.judgelineList.Add(judgeLine2);
        chart.performImgList.Add(img2);
        foreach (PerformImg singleimg in img)
            chart.performImgList.Add(singleimg);

        Chart.Show(chart);
        return chart;
    }
    private void testGuideLine()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 3);
        }
        else if (Input.GetKeyUp(KeyCode.T))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
    }
    private void setSongLength(float t)
    {
        front_ui.GetComponent<FrontUIManager>().setMaxTime(t);
        chart_maker.GetComponent<chartMaker>().setFullTime(t);
    }
    private void synchronize()// to sync audio length
    {
        Debug.Log("load audio");
        StartCoroutine(_synchronize());
    }
    private IEnumerator _synchronize()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(sync_time);
            if (display_manager.GetComponent<AudioSource>().clip)
                setSongLength(display_manager.GetComponent<AudioSource>().clip.length * 1000.0f);
        }
    }
    public void setToSelectedTime()
    {
        if (state == State.editor)
        {
            setTime(chart_maker.GetComponent<chartMaker>().getSelectedTime());
        }
    }
    private void setInfo()
    {
        front_ui.GetComponent<FrontUIManager>().setInfo(chart);
    }
    public void setUI()
    {
        setNote();
        setjudgeline();
        setImage();
    }
    private void setNote()
    {
        List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
        if (notes.Count == 1)
        {
            front_ui.GetComponent<FrontUIManager>().setNote(notes[0]);
        }
        front_ui.GetComponent<FrontUIManager>().setTemplateNote(template);
    }
    private void setjudgeline()
    {
        if (type == Config.EventlineType.Judgeline && id < chart.judgelineList.Count)
            front_ui.GetComponent<FrontUIManager>().setJudgeline(chart.judgelineList[id], id, chart.judgelineList.Count);
    }
    private void setImage()
    {
        if (type == Config.EventlineType.PerformImg)
        {
            if (chart.performImgList.Count == 0)
            {
                front_ui.GetComponent<FrontUIManager>().setEmptyImage();
                return;
            }
            // Debug.Log(id);
            //Debug.Log(chart.performImgList.Count);
            // string path = System.Environment.CurrentDirectory + "/Charts/" + chart_name + "/imgs/" + chart.performImgList[id].path + ".png";
            //Texture2D texture = Utilities.LoadTexture2D(path);
            front_ui.GetComponent<FrontUIManager>().setImage(chart.performImgList[id], id, chart.performImgList.Count);
        }
    }
    private void reloadSprites()
    {
        List<string> paths = new List<string>();
        foreach (string imgpath in Config.spriteList.Keys)
        {
            paths.Add(imgpath);
        }
        foreach (string imgpath in paths)
        {
            string path = System.Environment.CurrentDirectory + "/Charts/" + chart_name + "/imgs/" + imgpath + ".png";
            Texture2D texture = Utilities.LoadTexture2D(path);
            if (texture != null)
                Config.spriteList[imgpath] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            else
                Config.spriteList[imgpath] = Resources.Load<Sprite>("Textures/defaultimg");
        }
        setImage();
    }
    private void reloadSpite(string imgpath)
    {
        string path = System.Environment.CurrentDirectory + "/Charts/" + chart_name + "/imgs/" + imgpath + ".png";
        Texture2D texture = Utilities.LoadTexture2D(path);
        if (texture != null)
            Config.spriteList[imgpath] = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        else
            Config.spriteList[imgpath] = Resources.Load<Sprite>("Textures/defaultimg");
    }
    private void setImageList()
    {
        if (type == Config.EventlineType.PerformImg)
            front_ui.GetComponent<FrontUIManager>().setImageList(chart.performImgList);
    }
    private void setEvent()
    {
        Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
        if (_event.Count == 0)
        {
            front_ui.GetComponent<FrontUIManager>().setEmptyEvent();
        }
        else if (_event.Count == 1)
        {
            foreach (KeyValuePair<PerformEvent, string> e in _event)// actually only one event
                front_ui.GetComponent<FrontUIManager>().setEvent(e.Value, e.Key);
        }
        else
        {
            //todo front_ui.GetComponent<FrontUIManager>().setEvent();
        }
    }
    public void setName(string s)
    {
        chart.name = s;
        setInfo();
    }
    public void setComposer(string s)
    {
        chart.composer = s;
        setInfo();
    }
    public void setCharter(string s)
    {
        chart.charter = s;
        setInfo();
    }
    public void setIllustrator(string s)
    {
        chart.illustrator = s;
        setInfo();
    }
    public void setBpm(string s)
    {
        if (s == "")
            return;
        chart.bpm = float.Parse(s);
        setInfo();
        reset();
    }
    public void setOffset(string s)
    {
        if (s == "")
            return;
        chart.offset = float.Parse(s);
        setInfo();
        reset();
    }
    public void setDifficulty(string s)
    {
        if (s == "")
            return;
        chart.difficulty = float.Parse(s);
        setInfo();
        reset();
    }
    public void setChartName(string s)
    {
        chart_name = s;
        display_manager.GetComponent<Test>().chart_name = chart_name;
    }
    public void setSongName(string s)
    {
        song_name = s;
        display_manager.GetComponent<Test>().song_name = song_name;
    }
    public void editJudgeline(string _id)
    {
        if (_id == "")
            return;
        int id = int.Parse(_id);
        if (id >= 0 && id < chart.judgelineList.Count)
        {
            this.id = id;
            this.id_judgeline = id;
            this.type = Config.EventlineType.Judgeline;
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.Judgeline, id);
            setjudgeline();
        }
    }
    public void editImage(int id)
    {
        if (id >= 0 && id < chart.performImgList.Count)
        {
            this.id = id;
            this.id_performimg = id;
            this.type = Config.EventlineType.PerformImg;
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.PerformImg, id);
            setImage();
        }
    }
    public void deleteJudgeLine()
    {
        if (state == State.editor && type == Config.EventlineType.Judgeline)
        {
            chart.deleteJudgeline(id);
            int delete_id = id;
            chart_maker.GetComponent<chartMaker>().reset(chart);
            id = Mathf.Min(id, chart.judgelineList.Count - 1);
            id = Mathf.Max(id, 0);
            id_judgeline = id;
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.Judgeline, id);
            setjudgeline();
            record("delete judgeline: " + delete_id);
        }
    }
    public void addJudgeLine()
    {
        if (state == State.editor && type == Config.EventlineType.Judgeline)
        {
            chart.addJudgeline();
            setjudgeline();
            record("add new judgeline");
        }
    }
    public void deleteImage()
    {
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            chart.deletePerformImg(id);
            chart_maker.GetComponent<chartMaker>().reset(chart);
            int old_id = id;
            id = Mathf.Min(id, chart.performImgList.Count - 1);
            id = Mathf.Max(id, 0);
            id_performimg = id;
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.PerformImg, id);
            setImageList();
            setImage();
            record("delete image, id: " + old_id);
        }
    }
    public void addImage()
    {
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            chart.addPerformImg();
            setImageList();
            setImage();
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.PerformImg, id);
            record("add new image");
        }
    }
    public void editImageName(string s)
    {
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            if (id >= 0 && id < chart.performImgList.Count)
            {
                chart.performImgList[id].name = s;
                setImageList();
                setImage();
                record("edit image name: " + s);
            }
        }
    }
    public void editImagePath(string s)
    {
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            if (id >= 0 && id < chart.performImgList.Count)
            {
                chart.performImgList[id].path = s;
                reset();
                setImage();
                record("edit image path: " + s);
            }
        }
    }
    public void setImageSortingLayer(string s)
    {
        if (s == "")
            return;
        int order = int.Parse(s);
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            if (id >= 0 && id < chart.performImgList.Count)
            {
                chart.performImgList[id].sortingOrder = order;
                setImage();
                record("edit sorting layer");
            }
        }
    }
    public void setImageStarttime(string s)
    {
        if (s == "")
            return;
        float t = float.Parse(s);
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            if (id >= 0 && id < chart.performImgList.Count)
            {
                chart.performImgList[id].startTime = t;
                chart.performImgList[id].endTime = Mathf.Max(t, chart.performImgList[id].endTime);
                id = chart.resetPerformImg(id);
                Debug.Log("perform image: " + id);
                id_performimg = id;
                setImageList();
                setImage();
                chart_maker.GetComponent<chartMaker>().reset(chart);
                record("edit image starttime");
            }
        }
    }
    public void setImageEndtime(string s)
    {
        if (s == "")
            return;
        float t = float.Parse(s);
        if (state == State.editor && type == Config.EventlineType.PerformImg)
        {
            if (id >= 0 && id < chart.performImgList.Count)
            {
                chart.performImgList[id].endTime = Mathf.Max(t, chart.performImgList[id].startTime);
                setImage();
                chart_maker.GetComponent<chartMaker>().reset(chart);
                record("edit image endtime");
            }
        }
    }
    public void setEventStarttime(string s)
    {
        if (s == "")
            return;
        float t = float.Parse(s);
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    p.Key.startTime = t;
                    p.Key.endTime = Mathf.Max(t, p.Key.endTime);
                    front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    if (type == Config.EventlineType.Judgeline)
                    {
                        chart.deleteEvent_Judgeline(id, p.Key, p.Value);
                        chart.addEvent_Judgeline(id, p.Key, p.Value);
                    }
                    else
                    {
                        chart.deleteEvent_PerformImg(id, p.Key, p.Value);
                        chart.addEvent_PerformImg(id, p.Key, p.Value);
                    }
                }
                chart_maker.GetComponent<chartMaker>().cancelSelect();
                chart_maker.GetComponent<chartMaker>().reset(chart);
                record("edit event start time");
            }
        }
    }
    public void setEventEndtime(string s)
    {
        if (s == "")
            return;
        float t = float.Parse(s);
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    p.Key.endTime = t;
                    p.Key.startTime = Mathf.Min(t, p.Key.startTime);
                    if (type == Config.EventlineType.Judgeline)
                    {
                        chart.deleteEvent_Judgeline(id, p.Key, p.Value);
                        chart.addEvent_Judgeline(id, p.Key, p.Value);
                    }
                    else
                    {
                        chart.deleteEvent_PerformImg(id, p.Key, p.Value);
                        chart.addEvent_PerformImg(id, p.Key, p.Value);
                    }
                    front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                }
                chart_maker.GetComponent<chartMaker>().cancelSelect();
                chart_maker.GetComponent<chartMaker>().reset(chart);
                record("edit event end time");
            }
        }
    }
    public void setEventMoveType(int t)
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count < 1)
                return;
            foreach (KeyValuePair<PerformEvent, string> p in _event)
            {
                p.Key.type = (Config.EventType)t;
                front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
            }
            record("edit event move type");
        }
    }
    public void addMovePosition()
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        int id = front_ui.GetComponent<FrontUIManager>().posId;
                        Vector3 pos = front_ui.GetComponent<FrontUIManager>().pos;
                        chart.addMovePosition(p.Key as MoveEvent, id, pos);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("add move position");
            }
        }
    }
    public void deleteMovePosition()
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        int id = front_ui.GetComponent<FrontUIManager>().posId;
                        chart.deleteMovePosition(p.Key as MoveEvent, id);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("delete move position");
            }
        }
    }
    public void editMovePosition()
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        int id = front_ui.GetComponent<FrontUIManager>().posId;
                        Vector3 pos = front_ui.GetComponent<FrontUIManager>().pos;
                        chart.editMovePosition(p.Key as MoveEvent, id, pos);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("edit move position");
            }
        }
    }
    public void setStartPosition() // set the start position of a bunch of move events
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count >= 1)
            {
                PerformEvent firstMoveEvent = null;
                //get the first move event. not a good way, sad
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        if (firstMoveEvent == null || firstMoveEvent.startTime > p.Key.startTime)
                        {
                            firstMoveEvent = p.Key;
                        }
                    }
                }
                if (firstMoveEvent == null)
                    return;//not valid                
                Vector3 old_pos = (firstMoveEvent as MoveEvent).positions[0];
                Vector3 delta_pos = front_ui.GetComponent<FrontUIManager>().startPos - old_pos;
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        chart.movePositionAddDelta(p.Key as MoveEvent, delta_pos);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("set start position");
            }
        }
    }
    public void setEndPosition() // set the end position of a bunch of move events
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count >= 1)
            {
                PerformEvent firstMoveEvent = null;
                //get the last move event. not a good way, sad
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        if (firstMoveEvent == null || firstMoveEvent.startTime < p.Key.startTime)
                        {
                            firstMoveEvent = p.Key;
                        }
                    }
                }
                if (firstMoveEvent == null)
                    return;//not valid                
                Vector3 old_pos = (firstMoveEvent as MoveEvent).positions[(firstMoveEvent as MoveEvent).positions.Count - 1];
                Vector3 delta_pos = front_ui.GetComponent<FrontUIManager>().endPos - old_pos;
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        chart.movePositionAddDelta(p.Key as MoveEvent, delta_pos);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("set end position");
            }
        }
    }
    public void flipPosition_X() // flip the position by X
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count >= 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        chart.flipPositionX(p.Key as MoveEvent);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("flip position_x");
            }
        }
    }
    public void flipPosition_Y() // flip the position by Y
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count >= 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        chart.flipPositionY(p.Key as MoveEvent);
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("flip position_y");
            }
        }
    }
    public void shiftPosition_X(int side)
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count >= 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        chart.movePositionAddDelta(p.Key as MoveEvent, new Vector3(side * 0.01f, 0, 0));
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                Debug.Log(chart_maker.GetComponent<chartMaker>().getSelectedEvent().Count);
                reset();
                Debug.Log(chart_maker.GetComponent<chartMaker>().getSelectedEvent().Count);
                garbageCollect();
                record("shift position_x");
            }
        }
    }
    public void shiftPosition_Y(int side)
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count >= 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        chart.movePositionAddDelta(p.Key as MoveEvent, new Vector3(0, side * 0.01f, 0));
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                reset();
                garbageCollect();
                record("shift position_y");
            }
        }
    }
    public void editMovePathType(int type)
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "moveEvent")
                    {
                        MoveEvent e = p.Key as MoveEvent;
                        e.pathType = (Config.PathType)type;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }

                }
                record("edit move type");
            }
        }
    }
    public void setStartAngle(string s)
    {
        if (s == "")
            return;
        float angle = float.Parse(s);
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "rotateEvent")
                    {
                        RotateEvent e = p.Key as RotateEvent;
                        e.startAngle = angle;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }

                }
                record("edit start angle");
            }
        }
    }
    public void setEndAngle(string s)
    {
        if (s == "")
            return;
        float angle = float.Parse(s);
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "rotateEvent")
                    {
                        RotateEvent e = p.Key as RotateEvent;
                        e.endAngle = angle;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("edit end angle");
            }
        }
    }
    public void setStartColor()
    {
        Color color = front_ui.GetComponent<FrontUIManager>().startColor;
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "colorEvent")
                    {
                        ColorModifyEvent e = p.Key as ColorModifyEvent;
                        e.startColor = color;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("edit start color");
            }
        }
    }
    public void setEndColor()
    {
        Color color = front_ui.GetComponent<FrontUIManager>().endColor;
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "colorEvent")
                    {
                        ColorModifyEvent e = p.Key as ColorModifyEvent;
                        e.endColor = color;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("edit end color");
            }
        }
    }
    public void setStartScale(string s)
    {
        if (s == "")
            return;
        float scale = float.Parse(s);
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "scaleEvent")
                    {
                        ScaleEvent e = p.Key as ScaleEvent;
                        e.startScale = scale;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("edit start scale");
            }
        }
    }
    public void setEndScale(string s)
    {
        if (s == "")
            return;
        float scale = float.Parse(s);
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count == 1)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (p.Value == "scaleEvent")
                    {
                        ScaleEvent e = p.Key as ScaleEvent;
                        e.endScale = scale;
                        front_ui.GetComponent<FrontUIManager>().setEvent(p.Value, p.Key);
                    }
                }
                record("edit end scale");
            }
        }
    }
    public void addScale(int side)
    {
        float scale = side > 0 ? 1.1f : 0.9f;
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            foreach (KeyValuePair<PerformEvent, string> p in _event)
            {
                if (p.Value == "scaleEvent")
                {
                    ScaleEvent e = p.Key as ScaleEvent;
                    e.startScale *= scale;
                    e.endScale *= scale;
                }
            }
            reset();
            setEvent();
            garbageCollect();
            record("multi scale by:" + scale);
        }
    }
    public void switchType()
    {
        if (type == Config.EventlineType.PerformImg)
        {
            type = Config.EventlineType.Judgeline;
            id = id_judgeline;
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.Judgeline, id);
            setjudgeline();
            front_ui.GetComponent<FrontUIManager>().switchType(Config.EventlineType.Judgeline);
        }
        else
        {
            type = Config.EventlineType.PerformImg;
            id = id_performimg;
            chart_maker.GetComponent<chartMaker>().changeEventlineType(Config.EventlineType.PerformImg, id);
            setImageList();
            setImage();
            front_ui.GetComponent<FrontUIManager>().switchType(Config.EventlineType.PerformImg);
        }
    }
    public void deleteEvent()
    {
        if (state == State.editor)
        {
            Dictionary<PerformEvent, string> _event = chart_maker.GetComponent<chartMaker>().getSelectedEvent();
            if (_event.Count > 0)
            {
                foreach (KeyValuePair<PerformEvent, string> p in _event)
                {
                    if (type == Config.EventlineType.Judgeline)
                        chart.deleteEvent_Judgeline(id, p.Key, p.Value);
                    else
                        chart.deleteEvent_PerformImg(id, p.Key, p.Value);
                }
                chart_maker.GetComponent<chartMaker>().cancelSelect();
                chart_maker.GetComponent<chartMaker>().reset(chart);
                record("delete events ,num: " + _event.Count);
            }
            setUI();
        }
    }
    public void addEvent(string type)
    {
        if (type == "")
            return;
        if (state == State.editor)
        {
            PerformEvent _event;
            switch (type)
            {
                case "moveEvent":
                    _event = new MoveEvent();
                    break;
                case "rotateEvent":
                    _event = new RotateEvent();
                    break;
                case "colorEvent":
                    _event = new ColorModifyEvent(Color.white, Color.white);
                    break;
                case "scaleEvent":
                    _event = new ScaleEvent(1, 1);
                    break;
                default:
                    return;
            }
            _event.startTime = chart_maker.GetComponent<chartMaker>().gettime(guideBeats);
            _event.endTime = Mathf.Max(chart_maker.GetComponent<chartMaker>().getSelectedTime(), _event.startTime);
            if (this.type == Config.EventlineType.Judgeline)
            {
                chart.addEvent_Judgeline(id, _event, type, Config.PasteTyte.Inherit);
            }
            else
            {
                chart.addEvent_PerformImg(id, _event, type, Config.PasteTyte.Inherit);
            }
            setUI();
            setEvent();
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().reset(chart);
            record("add event: " + type);
        }
    }
    public void deleteNote()
    {
        if (state == State.editor && type == Config.EventlineType.Judgeline)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            if (notes.Count > 0)
            {
                foreach (Note note in notes)
                    chart.deleteNote(note, id);
                chart_maker.GetComponent<chartMaker>().cancelSelect();
                chart_maker.GetComponent<chartMaker>().reset(chart);
                record("delete notes on judgeline " + id + ", num: " + notes.Count);
            }
            setUI();
        }
    }
    public void addNote(Config.Type noteType = Config.Type.Tap, Config.LineType lineSide = Config.LineType.Line1)
    {
        if (state == State.editor && type == Config.EventlineType.Judgeline)
        {
            float time = chart_maker.GetComponent<chartMaker>().gettime(guideBeats);
            float duration = 0;
            if (noteType == Config.Type.Hold)
            {
                duration = Mathf.Max(chart_maker.GetComponent<chartMaker>().getSelectedTime(), time) - time;
            }
            Note note = new Note(noteType, template.color, time, duration, template.speed, template.livingTime, lineSide);
            chart.addNote(note, id);
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setUI();
            record("add note on judgeline " + id);
        }
    }
    public void editNoteColor()
    {
        List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
        foreach (Note note in notes)
            note.color = front_ui.GetComponent<FrontUIManager>().noteColor;

        chart_maker.GetComponent<chartMaker>().reset(chart);
        setNote();
        record("edit note color");
    }
    public void editNoteType(int t)
    {
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            foreach (Note note in notes)
            {
                switch (t)
                {
                    case 0:
                        note.type = Config.Type.Tap;
                        note.duration = 0;
                        break;
                    case 1:
                        note.type = Config.Type.Drag;
                        note.duration = 0;
                        break;
                    case 2:
                        note.type = Config.Type.Hold;
                        break;
                }
            }
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setNote();
            record("change note type");
        }
    }
    public void editNoteTime(string t)
    {
        if (t == "")
            return;
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            if (notes.Count != 1)
                return;
            Note note = notes[0];
            float time = float.Parse(t);
            if (time >= 0 && time < display_manager.GetComponent<AudioSource>().clip.length * 1000.0f)
            {
                Note newnote = new Note(note.type, note.color, time, note.duration, note.speed, note.livingTime, note.lineSide, note.fake);
                chart.deleteNote(note, id);
                chart.addNote(newnote, id);
            }
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setUI();
            record("edit note starttime");
        }
    }
    public void editNoteDuration(string t)
    {
        if (t == "")
            return;
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            if (notes.Count != 1)
                return;
            Note note = notes[0];
            float time = float.Parse(t);
            if (note.type == Config.Type.Hold)
            {
                Note newnote = new Note(note.type, note.color, note.time, time, note.speed, note.livingTime, note.lineSide, note.fake);
                chart.deleteNote(note, id);
                chart.addNote(newnote, id);
            }
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setUI();
            record("edit note duration");
        }
    }
    public void editNoteSpeed(string t)
    {
        if (t == "")
            return;
        float speed = float.Parse(t);
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            foreach (Note note in notes)
            {
                note.speed = speed;
            }
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setNote();
            record("edit note speed");
        }
    }
    public void editNoteLineside(int t)
    {
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            foreach (Note note in notes)
            {
                switch (t)
                {
                    case 0:
                        note.lineSide = Config.LineType.Line1;
                        break;
                    case 1:
                        note.lineSide = Config.LineType.Line2;
                        break;
                }
            }
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setNote();
            record("edit note lineside");
        }
    }
    public void editNoteLivingtime(string t)
    {
        if (t == "")
            return;
        float time = float.Parse(t);
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            foreach (Note note in notes)
            {
                note.livingTime = time;
            }
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setNote();
            record("edit note livingtime");
        }
    }
    public void setTemplate()
    {
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getSelectedNote();
            if (notes.Count != 1)
            {
                return;
            }
            Note note = notes[0];
            template.livingTime = note.livingTime;
            template.speed = note.speed;
            template.color = note.color;
            setNote();
        }
    }
    public void editTemplateColor()
    {
        if (state == State.editor)
        {
            template.color = front_ui.GetComponent<FrontUIManager>().templateColor;
        }
        setNote();
    }
    public void editTemplateSpeed(string t)
    {
        if (t == "")
            return;
        if (state == State.editor)
        {
            template.speed = float.Parse(t);
        }
        setNote();
    }
    public void editTemplateLivingtime(string t)
    {
        if (t == "")
            return;
        if (state == State.editor)
        {
            template.livingTime = float.Parse(t);
        }
        setNote();
    }
    public void Copy()
    {
        if (state == State.editor)
        {
            chart_maker.GetComponent<chartMaker>().copy();
        }
    }
    public void Paste(Config.PasteTyte pasteTyte = Config.PasteTyte.Normal)
    {
        if (state == State.editor)
        {
            List<Note> notes = chart_maker.GetComponent<chartMaker>().getCopiedNote();
            Dictionary<PerformEvent, string> events = chart_maker.GetComponent<chartMaker>().getCopiedEvent();
            float time = chart_maker.GetComponent<chartMaker>().getSelectedTime() - chart_maker.GetComponent<chartMaker>().getCopiedTime();
            foreach (Note note in notes)
            {
                Note newnote = new Note(note.type, note.color, note.time, note.duration, note.speed, note.livingTime, note.lineSide, note.fake);
                newnote.time += time;
                chart.addNote(newnote, id);
            }
            foreach (KeyValuePair<PerformEvent, string> p in events)
            {
                PerformEvent _event;
                switch (p.Value)
                {
                    case "moveEvent":
                        MoveEvent move = p.Key as MoveEvent;
                        _event = new MoveEvent(move.positions, move.pathType, move.startTime, move.endTime, move.type);
                        break;
                    case "rotateEvent":
                        RotateEvent rotate = p.Key as RotateEvent;
                        _event = new RotateEvent(rotate.startAngle, rotate.endAngle, rotate.startTime, rotate.endTime, rotate.type);
                        break;
                    case "colorEvent":
                        ColorModifyEvent color = p.Key as ColorModifyEvent;
                        _event = new ColorModifyEvent(color.startColor, color.endColor, color.startTime, color.endTime, color.type);
                        break;
                    case "scaleEvent":
                        ScaleEvent scale = p.Key as ScaleEvent;
                        _event = new ScaleEvent(scale.startScale, scale.endScale, scale.startTime, scale.endTime, scale.type);
                        break;
                    default:
                        return;
                }
                _event.startTime += time;
                _event.endTime += time;

                if (type == Config.EventlineType.Judgeline)
                {
                    chart.addEvent_Judgeline(id, _event, p.Value, pasteTyte);
                }
                else
                {
                    chart.addEvent_PerformImg(id, _event, p.Value, pasteTyte);
                }
            }
            //chart_maker.GetComponent<chartMaker>().cancelSelect();
            chart_maker.GetComponent<chartMaker>().reset(chart);
            setUI();
            record("paste");
        }
    }
    private void addPitch(float d)
    {
    display_manager.GetComponent<Test>().pitch += d;
        front_ui.GetComponent<FrontUIManager>().setPitch(display_manager.GetComponent<Test>().pitch);
    }

    private void fastShift(int d)
    {
        if (type == Config.EventlineType.Judgeline)
        {
            id = (id + d + chart.judgelineList.Count) % chart.judgelineList.Count;
            editJudgeline(id.ToString());
        }
        else
        {
            if (chart.performImgList.Count > 0)
            {
                id = (id + d + chart.performImgList.Count) % chart.performImgList.Count;
                editImage(id);
            }
        }
    }
    private void HideUI()
    {
        showUI = !showUI;
        chart_maker.SetActive(showUI);
        front_ui.SetActive(showUI);
    }
    private void garbageCollect()
    {
        System.GC.Collect();
        Resources.UnloadUnusedAssets();
    }
    public void userInput()
    {

        //ui control
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(EventSystem.current.currentSelectedGameObject);
        }
        //global control
        if (EventSystem.current.currentSelectedGameObject)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            switchType();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            setToSelectedTime();
        }
        if (Input.GetMouseButtonDown(0))
        {
            chart_maker.GetComponent<chartMaker>().selectTimeline();// select a timeline;
        }
        if (Input.GetMouseButton(1))
        {
            if (Input.GetKey(KeyCode.LeftShift))// multi select
            {
                chart_maker.GetComponent<chartMaker>().selectEventline(true);
                chart_maker.GetComponent<chartMaker>().selectNote(true);
            }
            else// single select
            {
                chart_maker.GetComponent<chartMaker>().selectEventline(false);
                chart_maker.GetComponent<chartMaker>().selectNote(false);
            }
        }
        if (Input.GetMouseButtonUp(1))// syncronize info
        {
            setUI();
            setEvent();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.D))
        {
            chart_maker.GetComponent<chartMaker>().cancelSelect();
            setEvent();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.A))
        {
            chart_maker.GetComponent<chartMaker>().selectAll();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y))
        {
            Redo();
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            deleteEvent();
            deleteNote();
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha1) && chart_maker.GetComponent<chartMaker>().guidePos == 0)// add a Tap
        {
            addNote(Config.Type.Tap);
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha2) && chart_maker.GetComponent<chartMaker>().guidePos == 0)// add a Drag
        {
            addNote(Config.Type.Drag);
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 0);
        }
        if (Input.GetKeyUp(KeyCode.Alpha3) && chart_maker.GetComponent<chartMaker>().guidePos == 0)// add a Hold
        {
            addNote(Config.Type.Hold);
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 1);
        }
        if (Input.GetKeyUp(KeyCode.Q) && chart_maker.GetComponent<chartMaker>().guidePos == 1)// add a moveEvent
        {
            addEvent("moveEvent");
            Debug.Log("add a move event");
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 2);
        }
        if (Input.GetKeyUp(KeyCode.W) && chart_maker.GetComponent<chartMaker>().guidePos == 2)// add a moveEvent
        {
            addEvent("rotateEvent");
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 3);
        }
        if (Input.GetKeyUp(KeyCode.E) && chart_maker.GetComponent<chartMaker>().guidePos == 3)// add a moveEvent
        {
            addEvent("colorEvent");
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            chart_maker.GetComponent<chartMaker>().setGuideMode(true, 4);
        }
        if (Input.GetKeyUp(KeyCode.R) && chart_maker.GetComponent<chartMaker>().guidePos == 4)// add a moveEvent
        {
            addEvent("scaleEvent");
            chart_maker.GetComponent<chartMaker>().setGuideMode(false);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f)// set density
        {
            chart_maker.GetComponent<chartMaker>().density = density / (Input.GetAxis("Mouse ScrollWheel") * density + 1);
        }
        if (!Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f)// move the chart view
        {
            time += Input.GetAxis("Mouse ScrollWheel") * density * 10f * 1000.0f;
            setTime(time);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Play();
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Pause();
        }
        if (Input.GetKeyUp(KeyCode.F))
        {
            reset();
            garbageCollect();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C))
        {
            Copy();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.X))
        {
            Copy();
            deleteEvent();
            deleteNote();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V))
        {
            if (Input.GetKey(KeyCode.LeftShift)) //smart paste
                Paste(Config.PasteTyte.Smart);
            else
                Paste();
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.RightArrow))
        {
            fastShift(1);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            fastShift(-1);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.UpArrow))
        {
            addPitch(0.1f);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.DownArrow))
        {
            addPitch(-0.1f);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.RightArrow))
        {
            shiftPosition_X(1);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftArrow))
        {
            shiftPosition_X(-1);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.UpArrow))
        {
            shiftPosition_Y(1);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.DownArrow))
        {
            shiftPosition_Y(-1);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Equals))
        {
            addScale(1);
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Minus))
        {
            addScale(-1);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            HideUI();
        }
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
        {
            trySaveChart();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            garbageCollect();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            reset();
            reloadSprites();
        }
    }
}
