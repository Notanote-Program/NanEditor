using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

public class PerformImgRenderer : MoveableObject
{
    [SerializeField] private SpriteRenderer img;
    [SerializeField] private NoteRenderer noteRenderer;
    private float holdLength;
    private bool _enableNoteRenderer;

    public bool EnableNoteRenderer
    {
        get => _enableNoteRenderer;
        set
        {
            _enableNoteRenderer = value;
            noteRenderer.gameObject.SetActive(_enableNoteRenderer);
        }
    }

    public float scaleX
    {
        get { return transform.localScale.x; }
        set
        {
            transform.localScale = new Vector3(value, transform.localScale.y, 1);
            if (EnableNoteRenderer) noteRenderer.updateBodyWidth(value);
        }
    }

    public float scaleY
    {
        get { return transform.localScale.y; }
        set
        {
            transform.localScale = new Vector3(transform.localScale.x, value, 1);
        }
    }

    public void SetScaleRespectively(float x, float y)
    {
        transform.localScale = new Vector3(x, y, 1);
        if (EnableNoteRenderer) noteRenderer.updateBodyWidth(x);
    }

    public Color color
    {
        get { return img.color; }
        set { img.color = value; }
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
        img.sortingLayerName = sortingLayerName;
        img.sortingOrder = sortingOrder;
        if (internalReference != null)
        {
            switch (internalReference)
            {
                case "tap":
                case "tap_hl": // TODO: 多押
                    EnableNoteRenderer = true;
                    noteRenderer.init(0, _color, Config.Type.Tap);
                    noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    break;
                case "drag":
                    EnableNoteRenderer = true;
                    noteRenderer.init(0, _color, Config.Type.Drag);
                    noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                    break;
                default:
                    if (internalReference.StartsWith("hold_") && float.TryParse(internalReference["hold_".Length..], out var length))
                    {
                        EnableNoteRenderer = true;
                        noteRenderer.init(0, _color, Config.Type.Hold, length);
                        noteRenderer.setSortingLayerAndOrder(sortingLayerName, sortingOrder);
                        break;
                    }

                    EnableNoteRenderer = false;
                    img.sprite = Utilities.GetDefaultSprite();
                    break;
            }
        }
    }

    public void OnRelease()
    {
        EnableNoteRenderer = false;
    }
}