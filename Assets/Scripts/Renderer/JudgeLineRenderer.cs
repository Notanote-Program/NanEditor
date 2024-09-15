using UnityEngine;
using UnityEngine.UI;

public class JudgeLineRenderer : ColoredMoveableObject
{
    [SerializeField] private SpriteRenderer judgeCircle;
    [SerializeField] private LineRenderer line1;
    [SerializeField] private LineRenderer line2;
    [SerializeField] private Text textId;
    public Material LineMaterial;
    public Sprite JudgeCircleSprite;
    private float _width = 0.06f;
    private const float line_length = 10000.0f;
    private const float radius = 0.5f;

    public Color ColorJudgeCircle
    {
        get => judgeCircle.color;
        set => judgeCircle.color = value;
    }

    // Color
    protected override Color GetColor() => judgeCircle.color;

    protected override void SetColor(Color color)
    {
        line1.startColor = color;
        line1.endColor = color;
        line2.startColor = color;
        line2.endColor = color;
        judgeCircle.color = color;
    }

    private void initJudgeCircle()
    {
        judgeCircle.sprite = JudgeCircleSprite;
    }

    private void initline()
    {
        line1.material = LineMaterial;
        line2.material = LineMaterial;
        line1.sortingLayerName = "judgeline";
        line2.sortingLayerName = "judgeline";
        //Line1.sortingLayerID = 3;
        //Line2.sortingLayerID = 3;
        position = Vector3.zero;
        angle = 0;
        scaleSingle = 1;
        //Debug.Log("init line");
        line1.positionCount = 2;
        line2.positionCount = 2;

        line1.useWorldSpace = false;
        line2.useWorldSpace = false;

        line1.startWidth = _width * scaleSingle;
        line1.endWidth = _width * scaleSingle;
        line2.startWidth = _width * scaleSingle;
        line2.endWidth = _width * scaleSingle;

        line1.SetPosition(0, new Vector3(0, radius, 0));
        line1.SetPosition(1, new Vector3(0, line_length, 0));
        line2.SetPosition(0, new Vector3(0, -radius, 0));
        line2.SetPosition(1, new Vector3(0, -line_length, 0));
    }

    private void initTextId()
    {
        this.transform.Find("Canvas").GetComponent<Canvas>().worldCamera = Camera.main;
    }

    private void adjustLine()
    {
        adjustLineY();
        adjustLineX();
    }

    private void adjustLineY()
    {
        Vector3 originpos = line1.GetPosition(1);
        if (scaleSingle != 0 && !OutOfScreen(world2myposition(transform.TransformPoint(originpos))))
        {
            line1.SetPosition(1, new Vector3(0, originpos.y * 2, 0));
        }

        originpos = line2.GetPosition(1);
        if (scaleSingle != 0 && !OutOfScreen(world2myposition(transform.TransformPoint(originpos))))
        {
            line2.SetPosition(1, new Vector3(0, originpos.y * 2, 0));
        }
    }

    private void adjustLineX()
    {
        line1.startWidth = _width * scaleSingle;
        line1.endWidth = _width * scaleSingle;
        line2.startWidth = _width * scaleSingle;
        line2.endWidth = _width * scaleSingle;
    }

    public void setLineLength(float scale)
    {
        line1.startWidth = _width * scale;
        line1.endWidth = _width * scale;
        line2.startWidth = _width * scale;
        line2.endWidth = _width * scale;
    }

    private bool LineInScreen(Vector3 pos1, Vector3 pos2)
    {
        if (!OutOfScreen(pos1) || !OutOfScreen(pos2))
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
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(textId.GetComponent<RectTransform>(), screenPos,
                Camera.main, out worldPoint))
        {
            textId.transform.position = worldPoint;
        }

        adjustLine();
    }

    protected override void RefreshTransformScale(bool hasX, bool hasY)
    {
        base.RefreshTransformScale(hasX, hasY);
        if (hasX && hasY)
        {
            adjustLine();
        }
        else if (hasX)
        {
            adjustLineX();
        }
        else
        {
            adjustLineY();
        }
    }

    public void setangle(float _angle)
    {
        angle = _angle;
        adjustLine();
    }

    public void setSortingLayerAndOrder(string sortingLayerName, int sortingOrder)
    {
        judgeCircle.sortingLayerName = sortingLayerName;
        judgeCircle.sortingOrder = sortingOrder;
        line1.sortingLayerName = sortingLayerName;
        line1.sortingOrder = sortingOrder;
        line2.sortingLayerName = sortingLayerName;
        line2.sortingOrder = sortingOrder;
    }

    public void init(Color _color, Vector3 _position, float _angle = 0, bool isWorldPosition = false)
    {
        initJudgeCircle();
        initline();
        initTextId();
        if (isWorldPosition)
        {
            worldPosition = _position;
        }
        else
        {
            position = _position;
        }

        textId.transform.position = worldPosition;
        Color = _color;
        scaleSingle = 1f;
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