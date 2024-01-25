using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MoveableObject
{
    public Config.Type type = Config.Type.Tap;
    private GameObject NoteHead, NoteBody;
    private Color defaultColor = Color.white;
    private const float defaultWidth = 0.6f;
    public float distance
    {
        get { return transform.localPosition.y; }
        set { transform.localPosition = new Vector3(0, value, 0); }
    }
    public Sprite sprite
    {
        get { return NoteHead.GetComponent<SpriteRenderer>().sprite; }
        set { NoteHead.GetComponent<SpriteRenderer>().sprite = value; }
    }
    public Color color
    {
        get { return NoteHead.GetComponent<SpriteRenderer>().color; }
        set
        {
            NoteHead.GetComponent<SpriteRenderer>().color = value;
            NoteBody.GetComponent<LineRenderer>().endColor = value;
            NoteBody.GetComponent<LineRenderer>().startColor = value;
        }
    }
    public float length
    {
        get
        {
            if (type == Config.Type.Hold)
            {
                Vector3 endPos = NoteBody.GetComponent<LineRenderer>().GetPosition(1);
                return endPos.y;
            }
            else
                return 0;
        }
        set
        {
            if (type == Config.Type.Hold)
            {
                Vector3 endPos = NoteBody.GetComponent<LineRenderer>().GetPosition(1);
                endPos.y = value;
                NoteBody.GetComponent<LineRenderer>().SetPosition(1, endPos);
            }
        }
    }
    private Sprite getNoteSprite(Config.Type _type = Config.Type.Tap)
    {
        string path = "Textures/";
        switch (_type)
        {
            case Config.Type.Tap:
                path += "Tap";
                break;
            case Config.Type.Drag:
                path += "Drag";
                break;
            case Config.Type.Hold:
                path += "Hold";
                break;
        }
        Sprite sprite = Resources.Load<Sprite>(path);
        return sprite;
    }
    private void getGameObject()
    {
        NoteHead = transform.Find("NoteHead").gameObject;
        NoteBody = transform.Find("NoteBody").gameObject;
        sprite = getNoteSprite(type);
        if (type == Config.Type.Hold)
        {
            NoteBody.SetActive(true);
            NoteBody.GetComponent<LineRenderer>().useWorldSpace = false;
            NoteBody.GetComponent<LineRenderer>().positionCount = 2;
            NoteBody.GetComponent<LineRenderer>().SetPosition(0, Vector3.zero);
            NoteBody.GetComponent<LineRenderer>().SetPosition(1, new Vector3(0, length, 0));
        }
        else
        {
            NoteBody.SetActive(false);
        }
    }
    public void init(float _distance, Color _NoteColor, Config.Type _type = Config.Type.Tap, float length = 0, Config.ControlState state = Config.ControlState.init)
    {
        type = _type;
        getGameObject();
        distance = _distance;
        color = _NoteColor;
        NoteBody.GetComponent <LineRenderer>().useWorldSpace = false;
        setBodyWidth(this.transform.lossyScale.x);
        this.length = length;
        setControlState(state);
    }
    private void setBodyWidth(float width)
    {
        NoteBody.GetComponent<LineRenderer>().startWidth = width * defaultWidth;
        NoteBody.GetComponent<LineRenderer>().endWidth = width * defaultWidth;
    }
    public void setControlState(Config.ControlState state)
    {
        //Debug.Log(state);
        if (type != Config.Type.Hold)
            return;
        string path = "Materials/";
        switch (state)
        {
            case Config.ControlState.init:
                path += "hold_init";
                break;
            case Config.ControlState.holding:
                path += "hold_holding";
                break;
            case Config.ControlState.detach:
                path += "hold_detach";
                break;
        }
        Material material = Resources.Load<Material>(path);
        NoteBody.GetComponent<LineRenderer>().material = material;
    }
}
