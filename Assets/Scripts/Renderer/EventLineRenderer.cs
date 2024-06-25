using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventLineRenderer : MonoBehaviour
{
    private const float minLength = 0.1f;
    private const float defaultWidth = 1;
    LineRenderer lineRenderer;
    public int id;
    public string type;
    private Material defaultMat;
    private Material selectedMat;
    private const string default_mat_path = "Materials/eventline_default";
    private const string selected_mat_path = "Materials/eventline_selected";
    public float width
    {
        get { return this.transform.localScale.x; }
        set 
        { 
            Vector3 oldScale = this.transform.localScale;
            this.transform.localScale = new Vector3(value,oldScale.y, oldScale.z);
            lineRenderer.startWidth = value;
            lineRenderer.endWidth = value;
        }
    }
    public Vector3 position
    {
        get { return this.transform.localPosition; }
        set { transform.localPosition = value; }
    }
    public float length
    {
        get { return this.transform.localScale.y; }
        set 
        {
            Vector3 oldScale = this.transform.localScale;
            this.transform.localScale = new Vector3(oldScale.x, Mathf.Max(minLength, value), oldScale.z);
        }
    }
    public void init(Vector3 position, float length,int id,string type, string tag = "eventline")
    {
        defaultMat = Resources.Load<Material>(default_mat_path);
        selectedMat = Resources.Load<Material>(selected_mat_path);
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.useWorldSpace = false;
        lineRenderer.material = defaultMat;
        this.position = position;
        this.length = length;
        this.width = defaultWidth;
        this.id = id;
        this.type = type;
        this.transform.tag = tag;
    }
    public void select(bool b)
    {
        if (b)
            lineRenderer.material = selectedMat;
        else
            lineRenderer.material = defaultMat;
    }
}
