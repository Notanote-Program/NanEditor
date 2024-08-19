using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteRenderer : MoveableObject
{
    public Config.Type type = Config.Type.Tap;
    [SerializeField] private SpriteRenderer NoteHead;
    [SerializeField] private LineRenderer NoteBody;
    private Color defaultColor = Color.white;
    private const float defaultWidth = 0.6f;
    public float distance
    {
        get { return transform.localPosition.y; }
        set { transform.localPosition = new Vector3(0, value, 0); }
    }
    public Sprite sprite
    {
        get { return NoteHead.sprite; }
        set { NoteHead.sprite = value; }
    }
    public Color color
    {
        get { return NoteHead.color; }
        set
        {
            NoteHead.color = value;
            NoteBody.endColor = value;
            NoteBody.startColor = value;
        }
    }
    public float length
    {
        get
        {
            if (type == Config.Type.Hold)
            {
                Vector3 endPos = NoteBody.GetPosition(1);
                return endPos.y;
            }
            else
                return 0;
        }
        set
        {
            if (type == Config.Type.Hold)
            {
                Vector3 endPos = NoteBody.GetPosition(1);
                endPos.y = value;
                NoteBody.SetPosition(1, endPos);
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
        sprite = getNoteSprite(type);
        if (type == Config.Type.Hold)
        {
            NoteBody.gameObject.SetActive(true);
            NoteBody.useWorldSpace = false;
            NoteBody.positionCount = 2;
            NoteBody.SetPosition(0, Vector3.zero);
            NoteBody.SetPosition(1, new Vector3(0, length, 0));
        }
        else
        {
            NoteBody.gameObject.SetActive(false);
        }
    }
    public void init(float _distance, Color _NoteColor, Config.Type _type = Config.Type.Tap, float length = 0, Config.ControlState state = Config.ControlState.init)
    {
        type = _type;
        getGameObject();
        distance = _distance;
        color = _NoteColor;
        NoteBody.useWorldSpace = false;
        setBodyWidth(this.transform.lossyScale.x);
        this.length = length;
        setControlState(state);
    }
    public void setBodyWidth(float scale)
    {
        if (type == Config.Type.Hold)
        {
            NoteBody.startWidth = scale * defaultWidth;
            NoteBody.endWidth = scale * defaultWidth;
        }
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
        NoteBody.material = material;
    }

    public void setSortingLayerAndOrder(string sortingLayerName, int sortingOrder)
    {
        NoteHead.sortingLayerName = sortingLayerName;
        NoteHead.sortingOrder = sortingOrder;
        NoteBody.sortingLayerName = sortingLayerName;
        NoteBody.sortingOrder = sortingOrder;
    }
}
