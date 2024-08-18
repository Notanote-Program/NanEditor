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
    private int _performImgType;
    // 0: 普通图片，1: Note, 2: 判定线
    public int PerformImgType
    {
        get => _performImgType;
        set
        {
            _performImgType = value;
            img.enabled = PerformImgType == 0;
            noteRenderer.gameObject.SetActive(PerformImgType == 1);
            if (PerformImgType == 1) noteRenderer.updateBodyWidth(scaleX);
            if (PerformImgType == 2) judgeLineRenderer.setScaleX(scaleX);
        }
    }

    public float scaleX
    {
        get => transform.localScale.x;
        set
        {
            transform.localScale = new Vector3(value, transform.localScale.y, 1);
            if (PerformImgType == 1) noteRenderer.updateBodyWidth(value);
            if (PerformImgType == 2) judgeLineRenderer.setScaleX(value);
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
        if (PerformImgType == 1) noteRenderer.updateBodyWidth(x);
        if (PerformImgType == 2) judgeLineRenderer.setScaleX(x);
    }

    public Color color
    {
        get => img.color;
        set
        {
            img.color = value;
            if (PerformImgType == 1) noteRenderer.color = value;
            if (PerformImgType == 2) judgeLineRenderer.color = value;
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
                    PerformImgType = 1;
                    noteRenderer.init(0, _color, Config.Type.Tap);
                    noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    break;
                case "drag":
                    PerformImgType = 1;
                    noteRenderer.init(0, _color, Config.Type.Drag);
                    noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    break;
                case "cover":
                    PerformImgType = 0;
                    break;
                case "judgeline":
                    PerformImgType = 2;
                    judgeLineRenderer.init(_color, world2myposition(transform.position));
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
                            PerformImgType = 1;
                            noteRenderer.init(0, _color, Config.Type.Hold, NoteManager.GetDistance(duration, speed, isDownSide ? Config.LineType.Down : Config.LineType.Up));
                            noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                            break;
                        }
                    }

                    PerformImgType = 0;
                    img.sprite = Utilities.GetDefaultSprite();
                    break;
            }
        }
    }

    public void OnRelease()
    {
        PerformImgType = 0;
    }
}