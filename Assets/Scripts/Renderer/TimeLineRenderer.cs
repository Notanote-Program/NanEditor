using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class TimeLineRenderer : MonoBehaviour
{
    LineRenderer lineRenderer;
    Text beatsText;
    public int id;
    public Color color
    {
        get { return lineRenderer.startColor; }
        set { lineRenderer.startColor = value; lineRenderer.endColor = value; }
    }
    public float width
    {
        get { return lineRenderer.startWidth; }
        set { lineRenderer.startWidth = value; lineRenderer.endWidth = value; }
    }
    public Vector3 position
    {
        set { gameObject.transform.localPosition = value; }
    }
    public void init(Color color, Vector3 positon, int beats = 0, int partition = 0)
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        beatsText = transform.Find("beats").gameObject.GetComponent<Text>();
        this.color = color;
        this.position = positon;
        this.id = beats;
        setBeats(beats, partition);
    }
    public void setBeats(int beats, int partition)
    {
        if (partition == 0)
        {
            beatsText.text = "";
            return;
        }
        beatsText.text = beats / partition + " + " + beats % partition + "/" + partition;
    }
}
