using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public GameObject settingsPanel;
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
            "全屏 (16:9)",
            "2560*1440",
            "1920*1080",
            "1600*900",
            "1280*720",
            "960*540"
        });
        resolutionDropdown.value = PlayerPrefs.GetInt("resolution", 0);
        resolutionDropdown.onValueChanged.AddListener(i => PlayerPrefs.SetInt("resolution", i));
        resolutionDropdown.onValueChanged.AddListener(i =>
        {
            var (newRes, fullScreen) = Utilities.GetResolution(i);
            Screen.fullScreen = fullScreen;
            Screen.SetResolution(newRes.width, newRes.height, fullScreen, Screen.currentResolution.refreshRate);
        });
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
}