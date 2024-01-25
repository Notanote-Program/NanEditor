using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public int d_delay = 10;
    public int max_dalay = 500;
    public int min_dalay = -500;
    public Animator UI_anim,BG_anim;

    public Slider keyVolume, MusicVolume, AALevel, graphicLevel, dspBuffer;
    public Text delay;
    public Toggle autoplay;

    public AudioSource quit_audio;
    private AudioSource audio_source;
    MutexLock input_lock;
    // Start is called before the first frame update
    void Start()
    {
        init();
        
    }

    // Update is called once per frame
    void Update()
    {
        userInput();
    }
    private void init()
    {
        audio_source = GetComponent<AudioSource>();
        audio_source.volume = Config.defaultMusicVolume * Config.musicVolume;
        input_lock = new MutexLock();
        input_lock.Lock();
        StartCoroutine(enter());
        setUI();
    }
    private IEnumerator enter()// enter this scene
    {
        UI_anim.SetTrigger("enter");
        BG_anim.SetTrigger("enter");
        float delay = Utilities.getAnimTime(UI_anim, "enter").length;
        yield return new WaitForSeconds(delay);
        input_lock.Unlock();
    }
    private void setUI()
    {
        keyVolume.value = Config.keyVolume;
        MusicVolume.value = Config.musicVolume;
        AALevel.value = QualitySettings.antiAliasing;
        graphicLevel.value = QualitySettings.GetQualityLevel();
        delay.text = Config.delay.ToString();
        dspBuffer.value = Mathf.Log(Config.dspBufferSize,2);
        autoplay.isOn = Config.autoplay;
        Debug.Log(Config.dspBufferSize);
    }
    public void userInput()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            quit();
    }
    public void quit()
    {
        if (!input_lock.islocked)
        {
            input_lock.Lock();
            quit_audio.Play();
            UI_anim.SetTrigger("quit");
            BG_anim.SetTrigger("quit");
            float time1 = Utilities.getAnimTime(UI_anim, "quit").length;
            float time2 = Utilities.getAnimTime(BG_anim, "quit").length;
            StartCoroutine(enterScene(Config.lastSceneId, Mathf.Max(time1, time2)));//return to last scene
        }
    }
    private IEnumerator enterScene(int id,float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(id);
    }
    public void setMusicVolume(float t)
    {
        Config.musicVolume = Mathf.Clamp01(t);
        audio_source.volume = Config.defaultMusicVolume * Config.musicVolume;
    }
    public void setKeyVolume(float t)
    {
        Config.keyVolume = Mathf.Clamp01(t);
    }
    public void addDelay()
    {
        Config.delay = Mathf.Clamp(Config.delay + d_delay, min_dalay, max_dalay);
        delay.text = Config.delay.ToString();
    }
    public void subDelay()
    {
        Config.delay = Mathf.Clamp(Config.delay - d_delay,min_dalay,max_dalay);
        delay.text = Config.delay.ToString();
    }
    public void setAntiAlias(float t)
    {
        Config.antiAliasing = (int)Mathf.Pow(2, t);
    }
    public void setGraphicQuality(float t)
    {
        Config.GraphicQuality = (int)t;
    }
    public void setDspBuffer(float t)
    {
        Config.dspBufferSize = (int)Mathf.Pow(2, t);
    }
    public void setAutoplay(bool b)
    {
        Config.autoplay = b;
    }
}
