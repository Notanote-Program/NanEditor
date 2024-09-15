using System;
using UnityEngine;

public class PerformImgRenderer : ColoredMoveableObject, IReleasablePoolItem
{
    [SerializeField] private SpriteRenderer img;
    [SerializeField] private NoteRenderer noteRenderer;
    [SerializeField] private JudgeLineRenderer judgeLineRenderer;
    private float holdLength;

    private PerformImgType _type;

    private PerformImgType Type
    {
        get => _type;
        set
        {
            _type = value;
            img.enabled = Type == PerformImgType.Normal;
            noteRenderer.gameObject.SetActive(Type == PerformImgType.Note);
            judgeLineRenderer.gameObject.SetActive(Type == PerformImgType.JudgeLine);
            if (Type == PerformImgType.Note) noteRenderer.setBodyWidth(scaleX);
            if (Type == PerformImgType.JudgeLine) judgeLineRenderer.scaleSingle = 1f;
        }
    }

    protected override void RefreshTransformScale(bool hasX, bool hasY)
    {
        base.RefreshTransformScale(hasX, hasY);
        if (hasX)
        {
            float x = scaleX;
            if (Type == PerformImgType.Note) noteRenderer.setBodyWidth(x);
            if (Type == PerformImgType.JudgeLine) judgeLineRenderer.setLineLength(1);
        }
    }

    // Color
    protected override Color GetColor() => img.color;

    protected override void SetColor(Color color)
    {
        img.color = color;
        if (Type == PerformImgType.Note) noteRenderer.Color = color;
        if (Type == PerformImgType.JudgeLine) judgeLineRenderer.Color = color;
    }

    public void setSortingLayerNameAndOrderLocal(string sortingLayerName, int sortingOrder)
    {
        img.sortingLayerName = sortingLayerName;
        img.sortingOrder = sortingOrder;
    }

    public void init(Sprite _sprite, Color _color, Vector3 _position, float _scaleX = 1, float _scaleY = 1,
        Config.PerformImgLayer layer = Config.PerformImgLayer.Background, float _angle = 0, int sortingOrder = 500,
        string internalReference = null)
    {
        //Debug.Log(path);
        this.Color = _color;
        this.position = _position;
        this.scale = new Vector2(_scaleX, _scaleY);
        this.angle = _angle;
        img.sprite = _sprite;
        string sortingLayerName = layer switch
        {
            Config.PerformImgLayer.Background => "background",
            Config.PerformImgLayer.AboveJudgementLine => "aboveJudgementLine",
            Config.PerformImgLayer.AboveNote => "aboveNote",
            Config.PerformImgLayer.AboveUI => "aboveUI",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, null)
        };
        setSortingLayerNameAndOrderLocal(sortingLayerName, sortingOrder);
        if (internalReference != null)
        {
            switch (internalReference)
            {
                case "tap":
                case "tap_hl": // TODO: 多押
                    Type = PerformImgType.Note;
                    noteRenderer.init(0, _color, Config.Type.Tap);
                    noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    break;
                case "drag":
                    Type = PerformImgType.Note;
                    noteRenderer.init(0, _color, Config.Type.Drag);
                    noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    break;
                case "cover":
                case "cover_16x9":
                    Type = PerformImgType.Normal;
                    break;
                case "judgeline":
                    Type = PerformImgType.JudgeLine;
                    judgeLineRenderer.init(_color, transform.position, isWorldPosition: true);
                    judgeLineRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    judgeLineRenderer.scaleX = _scaleX;
                    break;
                default:
                    if (internalReference.StartsWith("hold_"))
                    {
                        string head = "hold_";
                        bool isHl = false;
                        if (internalReference.StartsWith(head + "hl_"))
                        {
                            head += "hl_";
                            isHl = true; // Preserve
                        }

                        string[] content = internalReference[head.Length..].Split(",");
                        bool isDownSide = false;
                        if (content.Length is 2 or 3 && float.TryParse(content[0].Trim(), out var duration) &&
                            float.TryParse(content[1].Trim(), out var speed) &&
                            (content.Length == 2 || bool.TryParse(content[2].Trim(), out isDownSide)))
                        {
                            Type = PerformImgType.Note;
                            noteRenderer.init(0, _color, Config.Type.Hold,
                                NoteManager.GetDistance(duration, speed,
                                    isDownSide ? Config.LineType.Down : Config.LineType.Up));
                            noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                            break;
                        }
                    }

                    Type = PerformImgType.Normal;
                    img.sprite = Utilities.GetDefaultSprite();
                    break;
            }
        }
    }

    public void OnRelease()
    {
        Type = PerformImgType.Normal;
    }

    private enum PerformImgType
    {
        Normal = 0,
        Note = 1,
        JudgeLine = 2
    }
}