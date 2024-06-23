using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PerformImgRenderer : MoveableObject
{
    private GameObject img;
    public float scaleX
    {
        get { return transform.localScale.x; }
        set 
        {
            transform.localScale = new Vector3(value, transform.localScale.y, 1); 
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
    public Color color
    {
        get { return img.GetComponent<SpriteRenderer>().color; }
        set { img.GetComponent<SpriteRenderer>().color = value; }
    }
    public void init(Sprite _sprite, Color _color, Vector3 _position, float _scaleX = 1, float _scaleY = 1, float _angle = 0, int sortingOrder = 500, Config.LoadType loadType = Config.LoadType.Resource)
    {
        //Debug.Log(path);
        img = this.transform.gameObject;
        this.color = _color;
        this.position = _position;
        this.SetScaleRespectively(_scaleX, _scaleY);
        this.angle = _angle;
        if (_sprite != null)
            img.GetComponent<SpriteRenderer>().sprite = _sprite;
        else
            img.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/defaultimg");
        img.GetComponent<SpriteRenderer>().sortingOrder = sortingOrder;        
    }
}
