using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter_Meta 
{
    public int chartNum
    {
        get { return chartMetas.Count; }
    }
    public List<Chart_Meta> chartMetas;
    public Chapter_Meta(string chapterName = "chapter")
    {
        chartMetas = new List<Chart_Meta>();
        this.chapterName = chapterName;
    }
    public string chapterName;
}
public class Chart_Meta
{
    public string chartName;
    public bool SP = false;//if the chart has sp difficulty
    public string songName;
    public string Composer;
    public bool seal = false;
//    public ScoreManager score;
}
