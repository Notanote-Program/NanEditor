using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    public Vector3 position
    {
        get { return world2myposition(transform.position); }
        set { transform.position = myposition2world(value); }
    }

    public Vector3 worldPosition
    {
        get { return transform.position; }
        set { transform.position = value; }
    }

    private float _angle = 0;

    public float angle
    {
        get { return _angle; }
        set
        {
            _angle = value;
            transform.rotation = Quaternion.Euler(0, 0, value);
        }
    }
    
    private Vector2 _scale = new Vector2(1f, 1f);
        
    public float scaleSingle
    {
        get { return _scale.x; }
        set
        {
            _scale = new Vector2(value, value);
            RefreshTransformScale(true, true);
        }
    }
    
    public Vector2 scale
    {
        get { return _scale; }
        set
        {
            _scale = value;
            RefreshTransformScale(true, true);
        }
    }
        
    public float scaleX
    {
        get { return _scale.x; }
        set
        {
            _scale = new Vector2(value, _scale.y);
            RefreshTransformScale(true, false); 
        }
    }
    public float scaleY
    {
        get { return _scale.y; }
        set
        {
            _scale = new Vector2(_scale.x, value);
            RefreshTransformScale(false, true); 
        }
    }

    protected virtual void RefreshTransformScale(bool hasX, bool hasY)
    {
        transform.localScale = new Vector3(_scale.x, _scale.y, 1); 
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 myposition2world(Vector3 mypos)
    {
        return Config.myposition2world(mypos);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector3 world2myposition(Vector3 worldpos)
    {
        return Config.world2myposition(worldpos);
    }

    public bool OutOfScreen(Vector3 mypos)
    {
        return Mathf.Abs(mypos.x) >= 8 || Mathf.Abs(mypos.y) >= 4.5f;
    }
}