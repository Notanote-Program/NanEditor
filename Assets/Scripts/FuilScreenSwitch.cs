using UnityEngine;

public class FuilScreenSwitch : MonoBehaviour
{
    [SerializeField] private bool defaultValue;
#if !UNITY_EDITOR
    private bool isFullscreen
    {
        get => Screen.fullScreen;
        set
        {
            Resolution newRes = Screen.resolutions[^1];
            Screen.SetResolution(newRes.width, newRes.height, value);
            Screen.fullScreen = value;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        isFullscreen = defaultValue;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F11)) isFullscreen = !isFullscreen;
    }
#endif
}