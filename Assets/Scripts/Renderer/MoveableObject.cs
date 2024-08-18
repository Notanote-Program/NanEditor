using System.Collections;
using System.Collections.Generic;
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
            transform.rotation = Quaternion.Euler(0,0,value);
        }
    }

    public Vector3 myposition2world(Vector3 mypos)
    {
        return Config.myposition2world(mypos);
    }
    public Vector3 world2myposition(Vector3 worldpos)
    {
        return  Config.world2myposition(worldpos);
    }
    public bool OutOfScreen(Vector3 mypos)
    {
        if(Mathf.Abs(mypos.x)>=1 || Mathf.Abs(mypos.y)>=1)
            return true;
        return false;
    }
}
