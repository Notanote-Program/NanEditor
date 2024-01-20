using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuilScreenSwitch : MonoBehaviour
{
    [SerializeField] private bool defaultValue;
    // Start is called before the first frame update
    void Start()
    {
        Screen.fullScreen = defaultValue;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11)) Screen.fullScreen = !Screen.fullScreen;
    }
}
