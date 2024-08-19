using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PerformImgRenderer : MoveableObject, IReleasablePoolItem
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
            if (Type == PerformImgType.Note) noteRenderer.updateBodyWidth(scaleX);
            if (Type == PerformImgType.JudgeLine) judgeLineRenderer.setScaleX(scaleX);
        }
    }

    public float scaleX
    {
        get => transform.localScale.x;
        set
        {
            transform.localScale = new Vector3(value, transform.localScale.y, 1);
            if (Type == PerformImgType.Note) noteRenderer.updateBodyWidth(value);
            if (Type == PerformImgType.JudgeLine) judgeLineRenderer.setScaleX(value);
        }
    }

    public float scaleY
    {
        get => transform.localScale.y;
        set => transform.localScale = new Vector3(transform.localScale.x, value, 1);
    }

    public void SetScaleRespectively(float x, float y)
    {
        transform.localScale = new Vector3(x, y, 1);
        if (Type == PerformImgType.Note) noteRenderer.updateBodyWidth(x);
        if (Type == PerformImgType.JudgeLine) judgeLineRenderer.setScaleX(x);
    }

    public Color color
    {
        get => img.color;
        set
        {
            img.color = value;
            if (Type == PerformImgType.Note) noteRenderer.color = value;
            if (Type == PerformImgType.JudgeLine) judgeLineRenderer.color = value;
        }
    }

    public void setSortingLayerNameAndOrderLocal(string sortingLayerName, int sortingOrder)
    {
        img.sortingLayerName = sortingLayerName;
        img.sortingOrder = sortingOrder;
    }

    public void init(Sprite _sprite, Color _color, Vector3 _position, float _scaleX = 1, float _scaleY = 1,
        Config.PerformImgLayer layer = Config.PerformImgLayer.Background, float _angle = 0, int sortingOrder = 500,
        string internalReference = "")
    {
        //Debug.Log(path);
        this.color = _color;
        this.position = _position;
        this.SetScaleRespectively(_scaleX, _scaleY);
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
                    judgeLineRenderer.setScaleX(_scaleX);
                    break;
                default:
                    if (internalReference.StartsWith("hold_"))
                    {
                        string[] content = internalReference["hold_".Length..].Split(",");
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
        Type = 0;
    }

    private enum PerformImgType
    {
        Normal = 0,
        Note = 1,
        JudgeLine = 2
    }
}