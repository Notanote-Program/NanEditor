using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Dropdown resolutionDropdown;
    private bool _isOn;

    private void Awake()
    {
        settingsButton.onClick.AddListener(OpenSettingsPanel);
        closeButton.onClick.AddListener(PlayerPrefs.Save);
        closeButton.onClick.AddListener(CloseSettingsPanel);
        resolutionDropdown.AddOptions(new List<string>
        {
            "х╚фа (16:9)",
            "2560*1440",
            "1920*1080",
            "1600*900",
            "1280*720",
            "960*540"
        });
        resolutionDropdown.value = PlayerPrefs.GetInt("resolution", 0);
        resolutionDropdown.onValueChanged.AddListener(i => PlayerPrefs.SetInt("resolution", i));
        resolutionDropdown.onValueChanged.AddListener(RefreshResolution);
#if UNITY_EDITOR
        resolutionDropdown.interactable = false;
        PlayerPrefs.SetInt("resolution", 0);
        PlayerPrefs.Save();
#else
        resolutionDropdown.onValueChanged.Invoke(resolutionDropdown.value);
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        CloseSettingsPanel();
    }

    private void OpenSettingsPanel()
    {
        if (_isOn) return;
        settingsPanel.SetActive(_isOn = true);
    }

    private void CloseSettingsPanel()
    {
        if (!_isOn) return;
        settingsPanel.SetActive(_isOn = false);
    }

    [UsedImplicitly] 
    private static Resolution _originalResolution = new Resolution()
    {
        width = -1,
        height = -1
    };
    
    private static void RefreshResolution(int resolution)
    {
        Resolution newRes;
        bool fullScreen;
        switch (resolution)
        {
            case 0: //full screen (16:9)
                fullScreen = true;
                newRes = Utilities.GetFullScreenResolution();
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

                break;
            case 1:
                fullScreen = false;
                newRes = new Resolution()
                {
                    width = 2560,
                    height = 1440
                };
                break;
            case 2:
                fullScreen = false;
                newRes = new Resolution()
                {
                    width = 1920,
                    height = 1080
                };
                break;
            case 3:
                fullScreen = false;
                newRes = new Resolution()
                {
                    width = 1600,
                    height = 900
                };
                break;
            case 4:
                fullScreen = false;
                newRes = new Resolution()
                {
                    width = 1280,
                    height = 720
                };
                break;
            case 5:
                fullScreen = false;
                newRes = new Resolution()
                {
                    width = 960,
                    height = 540
                };
                break;
            default:
                fullScreen = false;
                newRes = new Resolution()
                {
                    width = 1920,
                    height = 1080
                };
                break;
        }
        // if (halfResolution)
        // {
        //     newRes.width /= 2;
        //     newRes.height /= 2;
        // }

        Screen.fullScreen = fullScreen;
        Screen.SetResolution(newRes.width, newRes.height, fullScreen, Screen.currentResolution.refreshRate);
    }
}