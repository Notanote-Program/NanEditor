using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager
{
    public List<KeyCode> keyDown;
    public List<KeyCode> keyPress;
    public Dictionary<Note, KeyCode> bindingKey;
    public void init()
    {
        keyDown = new List<KeyCode>();
        keyPress = new List<KeyCode>();
        bindingKey = new Dictionary<Note, KeyCode>();
    }
    public void update()
    {
        keyPress.Clear();
        keyDown.Clear();
        //android
/*        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                keyDown.Add(KeyCode.Space);
                Debug.Log("get input tap,time =" + Time.time);
            }
            else if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
            {
                keyPress.Add(KeyCode.Space);
            }
        }*/
        //pc
        if (Input.anyKeyDown)
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(keyCode))
                {
                    keyDown.Add(keyCode);
                }
            }
        }
        if (Input.anyKey)
        {
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKey(keyCode))
                {
                    keyPress.Add(keyCode);
                }
            }
        }
    }
}
