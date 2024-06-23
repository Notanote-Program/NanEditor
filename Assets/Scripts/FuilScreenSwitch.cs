using System.Linq;
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
            Resolution newRes = Utilities.GetFullScreenResolution();
            float ratio = newRes.width * 1f / newRes.height;
            if (Mathf.Abs(ratio - 16f / 9f) < 0.001)
            {
            }
            else if (ratio > 16f / 9f)
            {
                newRes.width = Mathf.RoundToInt(newRes.height * 16f / 9f);
            }
            else
            {
                newRes.height = Mathf.RoundToInt(newRes.width * 9f / 16f);
            }

            if (!value)
            {
                newRes.width = (int) (newRes.width / 1.5f);
                newRes.height = (int) (newRes.height / 1.5f);
            }

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