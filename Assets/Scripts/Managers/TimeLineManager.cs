using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class TimeLineManager : BaseManager
{
    private const float defaultWidth = 0.03f;
    private const float selectedWidth = 0.04f;
    private readonly Color selectedColor = new Color(1.0f, 0.7f, 0.1f);
    private readonly List<Color> colorList = new List<Color> { new Color(0.8f, 0.5f, 0.3f, 0.5f), new Color(0.4f, 0.2f, 0.3f, 0.5f), new Color(0.3f, 0.7f, 0.4f, 0.5f), new Color(0.5f, 0.1f, 0.4f, 0.5f), new Color(0.1f, 0.8f, 0.4f, 0.5f), new Color(0.3f, 0.4f, 0.1f, 0.5f) };
    private const int defaultNum = 200;// num of time lines to render
    private const float defaultDensity = 0.25f; // ?
    private const string timeline_prefab_path = "Prefabs/timeLine";
    private float _density = 0;
    private int _partition;
    public int totalBeats;
    private int _selectedId;
    private float _curplaypos;
    public List<GameObject> timelines;
    private GameObject baseline;
    public GameObject lineSet;
    public GameObject ChartMaker;
    public int selectedId
    {
        get { return _selectedId; }
        set { _selectedId = value; resetLines(); }
    }
    public int partition
    {
        get { return _partition; }
        set { _partition = Mathf.Max(1, value); changePartition(); }
    }
    public float density
    {
        get { return _density; }
        set { changeDensity(_density, value); _density = value; }
    }
    public float curPlayPos // in play mod, music is on this pos
    {
        get { return _curplaypos; }
        private set { _curplaypos = value; lineSet.transform.localPosition = new Vector3(0, -value, 0); }
    }
    public void init(int totalBeats, int partition = 4, float density = defaultDensity, GameObject parent = null)
    {
        _curplaypos = 0;
        _density = density;
        timelines = new List<GameObject>();
        lineSet = new GameObject();
        lineSet.name = "lineSet";
        lineSet.transform.SetParent(parent.transform);
        lineSet.transform.localPosition = Vector3.zero;
        lineSet.transform.localScale = Vector3.one;

        initPool(timeline_prefab_path, defaultNum, lineSet);

        baseline = pool.get_item();
        baseline.transform.SetParent(parent.transform);
        baseline.GetComponent<TimeLineRenderer>().init(Color.white, new Vector3(0, 0, 0));
        baseline.GetComponent<TimeLineRenderer>().width = defaultWidth * 1.2f;
        baseline.tag = "Untagged";// not selectable
        baseline.GetComponent<LineRenderer>().sortingOrder = 3;//on the top of other timelines

        this.totalBeats = totalBeats;
        this.partition = partition;
    }
    public void update(float time)
    {
        float beats = getbeats(time);
        curPlayPos = beats / density;
        resetLines();
    }
    private void changeDensity(float oldDensity, float newDensity)
    {
        curPlayPos = curPlayPos * oldDensity / newDensity;
        for (int i = 0; i < timelines.Count; i++)
        {
            int id = timelines[i].GetComponent<TimeLineRenderer>().id;
            timelines[i].GetComponent<TimeLineRenderer>().position = new Vector3(0, id / newDensity / partition, 0);
        }
    }
    private void resetLines()
    {
        for (int i = 0; i < timelines.Count; i++)
            pool.release_item(timelines[i]);
        timelines.Clear();
        int lineNum = totalBeats * partition + 1;
        int id_below = (int)(curPlayPos * density * partition);// lineid below the baseline
        for (int i = Mathf.Max(0, id_below - defaultNum / 2); i < Mathf.Min(lineNum, id_below + defaultNum / 2); i++)
        {
            GameObject newline = pool.get_item();
            newline.GetComponent<TimeLineRenderer>().init(colorList[(i % partition) % colorList.Count], new Vector3(0, i / density / partition, 0), i, partition);
            newline.GetComponent<TimeLineRenderer>().width = defaultWidth;
            if (i == selectedId)
            {
                newline.GetComponent<TimeLineRenderer>().color = selectedColor;
                newline.GetComponent<TimeLineRenderer>().width = selectedWidth;
            }
            timelines.Add(newline);
        }
    }
    private void changePartition()
    {
        resetLines();
    }
    public void selectLine(GameObject obj)
    {
        if (timelines.Contains(obj))
        {
            selectedId = obj.GetComponent<TimeLineRenderer>().id;
        }
    }
    private float getbeats(float time)
    {
        return ChartMaker.transform.GetComponent<chartMaker>().getbeats(time);// not good. a better way to organize this function?
    }
}
