using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.UI;

public class JudgeLineRenderer : MoveableObject
{
    LineRenderer Line1;
    LineRenderer Line2;
    GameObject JudgeCircle;
    Text textId;
    public Material LineMaterial;
    public Sprite JudgeCircleSprite;
    private float _width = 0.06f;
    private const float line_length = 10000.0f;
    private const float radius = 0.5f;
    
    public float scale
    {
        get { return transform.localScale.x; }
        set 
        {
            transform.localScale = new Vector3(value, value, 1); 
        }
    }
    public float width
    {
        get { return _width; }
        set 
        {
            _width = value;
            adjustLine();
        }
    }
    public Color color_line1
    {
        get { return Line1.startColor; }
        set 
        {
            Line1.startColor = value;
            Line1.endColor = value;
        }
    }
    public Color color_line2
    {
        get { return Line2.startColor; }
        set
        {
            Line2.startColor = value;
            Line2.endColor = value;
        }
    }
    public Color color_judgecircle
    {
        get { return JudgeCircle.transform.GetComponent<SpriteRenderer>().color; }
        set { JudgeCircle.transform.GetComponent<SpriteRenderer>().color = value; }
    }
    public Color color
    {
        set
        {
            Line1.startColor = value;
            Line1.endColor = value;
            Line2.startColor = value;
            Line2.endColor = value;
            JudgeCircle.transform.GetComponent<SpriteRenderer>().color = value;
        }
    }
    private void initJudgeCircle()
    {
        JudgeCircle = this.transform.Find("JudgeCircle").gameObject;
        JudgeCircle.GetComponent<SpriteRenderer>().sprite = JudgeCircleSprite;
    }
    private void initline()
    {
        GameObject line1 = this.transform.Find("Line1").gameObject;
        if(!line1.GetComponent<LineRenderer>())
            line1.AddComponent<LineRenderer>();
        Line1 = line1.GetComponent<LineRenderer>();
        GameObject line2 = this.transform.Find("Line2").gameObject;
        if (!line2.GetComponent<LineRenderer>())
            line2.AddComponent<LineRenderer>();
        Line2 = line2.GetComponent<LineRenderer>();

        Line1.material = LineMaterial;
        Line2.material = LineMaterial;
        Line1.sortingLayerName = "judgeline";
        Line2.sortingLayerName = "judgeline";
        //Line1.sortingLayerID = 3;
        //Line2.sortingLayerID = 3;
        position = Vector3.zero;
        angle = 0;
        scale = 1;
        //Debug.Log("init line");
        Line1.positionCount = 2;
        Line2.positionCount = 2;

        Line1.useWorldSpace = false;
        Line2.useWorldSpace = false;

        Line1.startWidth = _width * scale;
        Line1.endWidth = _width * scale;
        Line2.startWidth = _width * scale;
        Line2.endWidth = _width * scale;

        Line1.SetPosition(0, new Vector3(0, radius, 0));
        Line1.SetPosition(1, new Vector3(0, line_length, 0));
        Line2.SetPosition(0, new Vector3(0, -radius, 0));
        Line2.SetPosition(1, new Vector3(0, -line_length, 0));
    }
    private void initTextId()
    {        
        this.transform.Find("Canvas").GetComponent<Canvas>().worldCamera = Camera.main;
        textId = this.transform.Find("Canvas").transform.Find("Text").GetComponent<Text>();
    }
    private void adjustLine()
    {
        Vector3 originpos = Line1.GetPosition(1);
        if (scale!=0 && !OutOfScreen(world2myposition(transform.TransformPoint(originpos))))
        {
            Line1.SetPosition(1, new Vector3(0, originpos.y * 2, 0));
        }
        originpos = Line2.GetPosition(1);
        if (scale!=0 && !OutOfScreen(world2myposition(transform.TransformPoint(originpos))))
        {
            Line2.SetPosition(1, new Vector3(0, originpos.y * 2, 0));
        }

        Line1.startWidth = _width * scale;
        Line1.endWidth = _width * scale;
        Line2.startWidth = _width * scale;
        Line2.endWidth = _width * scale;
    }
    private bool LineInScreen(Vector3 pos1, Vector3 pos2)
    {
        if(!OutOfScreen(pos1) || !OutOfScreen(pos2))
            return false;
        return true;
    }
    public void setposition(Vector3 pos)
    {
        position = pos;
        //textId.transform.position = myposition2world(position);
        Vector3 screenPos = Camera.main.WorldToScreenPoint(myposition2world(pos));
        Vector3 worldPoint;
        //屏幕转UI  ui(当前的canvas)  _camera_UiCamera(UI的摄像机)
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(textId.GetComponent<RectTransform>(), screenPos, Camera.main, out worldPoint))
        {
            textId.transform.position = worldPoint;
        }
        adjustLine();
    }

    public void setscale(float _scale)
    {
        scale = _scale;
        adjustLine();
    }
    public void setangle(float _angle)
    {
        angle = _angle;
        adjustLine();
    }
    public void init(Color _color, Vector3 _position, float _scale = 1, float _angle = 0)
    {
        initJudgeCircle();
        initline();
        initTextId();
        position = _position;
        textId.transform.position = myposition2world(position);
        color = _color;
        setscale(_scale);
        angle = _angle;
    }
    public void showId(bool b = false, int id = 0)
    {
        textId.gameObject.SetActive(b);
        if (b)
        {
            textId.text = id.ToString();
        }
    }
}
