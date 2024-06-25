using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class SelectUiManager : MonoBehaviour
{
    private Config.LoadType loadType;

    private Animator anim;//select animations
    private Animator bg_anim;//background animations

    private AudioSource audio_source;

    public Image lastSongView, presentSongView, nextSongView;
    private List<Sprite> images = new List<Sprite>();// songImage
    private List<AudioClip> audioList = new List<AudioClip>();
    private List<string> names = new List<string>();// songName
    private List<string> composers = new List<string>();// composerName
    private List<string> scores = new List<string>();// scores

    private Chapter_Meta chapterMeta;

    private MutexLock mutex_select = new MutexLock();//if player is able to select a song 
    private MutexLock mutex_bg = new MutexLock();

    AsyncOperation scene_operation = null;// operation to load scene

    private bool freeMode//if player can operate
    {
        get { return !mutex_select.islocked && !mutex_bg.islocked; }
    }
    private bool finish_loading_scene = false;
    public int chart_id
    {
        get { return _chart_id; }
        set { _chart_id = (value + chapterMeta.chartNum) % chapterMeta.chartNum; }
    }//selected chart_id
    private int _chart_id = 0;

    public Text T_songName,T_songName_loading;
    public Text T_composer;
    public Text T_score;
    public Text T_chapterName;
    public Image rankImage;

    public AudioSource select_audio,switch_audio;
    // Start is called before the first frame update
    void Start()
    {
        setLoadType();
        //generateTestMeta();
        init(Config.loadType);
    }
    private void init(Config.LoadType loadType = Config.LoadType.External)
    {
        Config.lastSceneId = 1;//select scene
        Config.selectSP = false;// init in normal mode
        this.loadType = loadType;        
        initAnim();
        initAudio();
        loadCharts();
        mutex_bg.Lock();
        mutex_select.Lock();
        StartCoroutine(enter());
    }

    // Update is called once per frame
    void Update()
    {
        playerInput();
        
/*        if(scene_operation != null && scene_operation.progress >= 0.9f && !finish_loading_scene)
        {
            Debug.Log("done");
            finish_loading_scene = true;
            StartCoroutine(enter_scene(scene_id,1.0f));
        }*/
    }

    private IEnumerator enter()// enter this scene
    {
        float delay = Utilities.getAnimTime(anim, "enter").length;
        yield return new WaitForSeconds(delay);
        mutex_bg.Unlock();
        mutex_select.Unlock();
    }
    private void setLoadType()
    {
        Config.selectedChapter = "notanote一测谱面";
        //Config.selectedChapter = "Chapter1";
        //todo:set loadtype from config.selectedchapter
        Config.loadType = Config.LoadType.Resource;
        //Config.loadType = Config.LoadType.External;
    }
    private void initAnim()
    {
        anim = GetComponent<Animator>();
        bg_anim = GameObject.Find("bgView").GetComponent<Animator>();
    }
    private void initAudio()
    {
        audio_source = GetComponent<AudioSource>();
        audio_source.volume = Config.defaultMusicVolume * Config.musicVolume;
    }
    private void loadCharts()

    {
        string selectedChapter = Config.selectedChapter;
        string path;

        /* Load meta */
        if (loadType == Config.LoadType.Resource)
        {
            path = "Charts/" + selectedChapter + "/Chapter_Meta";
        }
        else
        {
            path = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/Chapter_Meta.json";
        }
        chapterMeta = Utilities.LoadFromJson<Chapter_Meta>(path, loadType);
        if (chapterMeta == null)
            Debug.LogError("meta not found!");
        else if (chapterMeta.chartNum == 0)
            Debug.LogError("no valid chart!");

        /* Set chapterName */
        T_chapterName.text = chapterMeta.chapterName;

        
        for (int i = 0; i < chapterMeta.chartNum; i++)
        {
            if (chapterMeta.chartMetas[i].chartName == Config.selectedChart)
            {
                chart_id = i;
            }
            /* Load images */
            string imgpath;
            if (loadType == Config.LoadType.Resource)
            {
                //chartpath = "Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/" + chapterMeta.chartMetas[i].chartName ;
                imgpath = "Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/Illustration";
            }
            else
            {
                //chartpath = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/" + chapterMeta.chartMetas[i].chartName +".json";
                imgpath = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/Illustration";
            }
            images.Add(Utilities.loadSprite(imgpath, loadType));

            /* Load audios*/
            string audiopath,songname;
            if (loadType == Config.LoadType.Resource)
            {
                audiopath = "Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName;
                songname = "clip";
            }
            else
            {                
                audiopath = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName;
                songname = "*";
            }
            AudioLoader audioLoader = new AudioLoader();
            AudioClip clip = audioLoader.LoadAudioClip(audiopath, songname,loadType);
            audioList.Add(clip);
        }

        /* Load Audio */
        for (int i = 0; i < chapterMeta.chartNum; i++)
        {
            if (chapterMeta.chartMetas[i].chartName == Config.selectedChart)
            {
                chart_id = i;
            }

            string imgpath;
            if (loadType == Config.LoadType.Resource)
            {
                //chartpath = "Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/" + chapterMeta.chartMetas[i].chartName ;
                imgpath = "Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/Illustration";
            }
            else
            {
                //chartpath = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/" + chapterMeta.chartMetas[i].chartName +".json";
                imgpath = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/" + chapterMeta.chartMetas[i].chartName + "/Illustration";
            }
            images.Add(Utilities.loadSprite(imgpath, loadType));
        }

        syncImage();
        setChartInfo(chart_id);
    }
    public void switchScene(int scene_id)
    {
        //TODO:
        audio_source.enabled = false;
        Config.spriteList.Clear();// important!
        Config.selectedChart = chapterMeta.chartMetas[chart_id].chartName;
        mutex_select.Lock();
        mutex_bg.Lock();
        enter_loading(scene_id);
        switch(scene_id)
        {
            case 2:// enter play view
                select_audio.Play();
                Config.initSceneSucceed = false;// wait until sprites loading finished
                StartCoroutine(preloadSprites(0.0f));
                StartCoroutine(enter_scene(scene_id, 3.0f));
                break;
            case 4:// enter setting view
                switch_audio.Play();
                Config.initSceneSucceed = true;
                StartCoroutine(enter_scene(scene_id, 1.0f));
                break;
        }

    }
    /* enter loading status */
    private void enter_loading(int scene_id)
    {
        switch(scene_id)
        {
            case 2:// enter loading game
                anim.SetTrigger("enter_game");
                bg_anim.SetTrigger("enter_game");
                break;
            case 4:// enter settings view
                anim.SetTrigger("quit");
                bg_anim.SetTrigger("quit");
                break;
        }
    }
    /* enter the scene */
    private IEnumerator enter_scene(int scene_id, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        while (!Config.initSceneSucceed)
        {
            yield return null;
        }
        scene_operation = Utilities.loadScene(scene_id);
        while (scene_operation.progress < 0.89f)
        {
            yield return null;
        }
/*        anim.SetTrigger("fading");
        Debug.Log("switch scene");
        yield return new WaitForSeconds(0.5f);//fading time*/
        scene_operation.allowSceneActivation = true;
        while (!scene_operation.isDone)
        {
            yield return null;
        }
        yield return null;
    }

    /* pre-load sprites */
    IEnumerator preloadSprites(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        string path;
        string selectedChart = Config.selectedChart;
        if (Config.selectSP)
            selectedChart += "_SP";
        if (loadType == Config.LoadType.Resource)
        {
            path = "Charts/" + Config.selectedChapter + "/" + selectedChart + "/" + selectedChart;
        }
        else
        {
            path = System.Environment.CurrentDirectory + "/Charts/" + Config.selectedChapter + "/" + selectedChart + "/" + selectedChart + ".json";
        }

        AsyncRequest<Chart> ar = new AsyncRequest<Chart>(null);
        StartCoroutine(Utilities.LoadFromJsonAsync<Chart>(ar, path, loadType));
        while (!ar.isDone)
            yield return new WaitForSeconds(1.0f);
        Chart chart = ar.asset;
        if (chart == null)
        {
            Debug.LogError("chart not found. path:" + path);
        }

        string imgpath;
        if (loadType == Config.LoadType.Resource)
        {
            imgpath = "Charts/" + Config.selectedChapter + "/" + selectedChart + "/imgs/";
        }
        else
        {
            imgpath = System.Environment.CurrentDirectory + "/Charts/" + Config.selectedChapter + "/" + selectedChart + "/imgs/";
        }
        for (int i = 0; i < chart.performImgList.Count; i++)
        {            
            if (chart.performImgList[i].path == null)
                chart.performImgList[i].path = "";
            if (!Config.spriteList.ContainsKey(chart.performImgList[i].path))
            {
                /*                Debug.Log(imgpath + chart.performImgList[i].path);
                                Config.spriteList[chart.performImgList[i].path] = Utilities.loadSprite(imgpath + chart.performImgList[i].path, loadType);
                                yield return null;*/
                AsyncRequest<Sprite> sar = new AsyncRequest<Sprite>(null);
                StartCoroutine(Utilities.LoadSpriteAsync(sar, imgpath + chart.performImgList[i].path, loadType));
                while (!sar.isDone)
                    yield return null;

                /*ResourceRequest rr = Resources.LoadAsync<Sprite>(imgpath + chart.performImgList[i].path);
                while(!rr.isDone)
                    yield return null;*/                
                Config.spriteList[chart.performImgList[i].path] = sar.asset;
            }
            
        }
        Config.initSceneSucceed = true;
        yield return null;
    }
    private void playerInput()
    {
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (freeMode)
            {
                mutex_select.Lock();
                shift_right();
            }

        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (freeMode)
            {
                mutex_select.Lock();
                shift_left();
            }

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (freeMode)
            {
                mutex_bg.Lock();
                mutex_select.Lock();
                switchScene(2);// jump to playView
            }
        }
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (freeMode)
            {
                mutex_bg.Lock();
                setSP(!Config.selectSP);
            }
        }
    }
    /* set special mode */
    public void setSP(bool b)
    {
        if (b != Config.selectSP)
        {
            if (!b || (b && chapterMeta.chartMetas[chart_id].SP))
            {
                switch_to_special(b);
                setChartInfo(chart_id);
            }
            else
                mutex_bg.Unlock();
        }
        else
        {
            mutex_bg.Unlock();
        }
    }
    private void switch_to_special(bool b)
    {
        Config.selectSP = b;
        if (b)//to special
        {
            bg_anim.SetTrigger("special");
        }
        else//to normal
        {
            bg_anim.SetTrigger("default");
        }
        //TODO:
        StartCoroutine(setlock(mutex_bg, false, 0.5f));
    }
    private void shift_left(float duration = 0.4f)
    {
        if(switch_audio && switch_audio.clip)
        {
            switch_audio.time = 0;
            switch_audio.Play();
        }
        var clip = Utilities.getAnimTime(anim, "shift_left");
        if (!clip)
            return;
        float time = clip.length;
        anim.speed = time / duration;
        time = duration;
        anim.SetTrigger("shift_left");
        StartCoroutine(getChartInfo(1, time * 0.4f));
        StartCoroutine(getNextImage(1, time * 0.4f));
        StartCoroutine(shiftImage(1, time));
    }
    private void shift_right(float duration = 0.4f)
    {
        if (switch_audio && switch_audio.clip)
        {
            switch_audio.time = 0;
            switch_audio.Play();
        }
        var clip = Utilities.getAnimTime(anim, "shift_right");
        if (!clip)
            return;
        float time = clip.length;
        anim.speed = time / duration;
        time = duration;
        anim.SetTrigger("shift_right");
        StartCoroutine(getChartInfo(0, time * 0.4f));
        StartCoroutine(getNextImage(0, time * 0.4f));
        StartCoroutine(shiftImage(0, time));
    }
    public IEnumerator shiftImage(int left, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        if (left == 1)
        {
            chart_id = chart_id + 1;
            syncImage();
        }
        else
        {
            chart_id = chart_id - 1;
            syncImage();
        }
        anim.SetTrigger("back");
        mutex_select.Unlock();
    }
    public IEnumerator getNextImage(int left, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        if (left == 1)
        {
            lastSongView.sprite = images[Mathf.Clamp((chart_id + 2) % chapterMeta.chartNum, 0, chapterMeta.chartNum - 1)];
        }
        else
        {
            nextSongView.sprite = images[Mathf.Clamp((chart_id - 2 + 2 * chapterMeta.chartNum) % chapterMeta.chartNum, 0, chapterMeta.chartNum - 1)];
        }
    }
    public IEnumerator getChartInfo(int left, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (left == 1)
        {
            setChartInfo(Mathf.Clamp((chart_id + 1) % chapterMeta.chartNum, 0, chapterMeta.chartNum - 1));
        }
        else
        {
            setChartInfo(Mathf.Clamp((chart_id - 1 + chapterMeta.chartNum) % chapterMeta.chartNum, 0, chapterMeta.chartNum - 1));
        }
    }
    public IEnumerator setlock(MutexLock _lock, bool b, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (b)
            _lock.Lock();
        else
            _lock.Unlock();
    }
    public void button_shift(bool left)// use button to shift
    {
        Debug.Log(left);

        if (left)
        {
            if (freeMode)
            {
                mutex_select.Lock();
                shift_left();
            }
        }
        else
        {
            if (freeMode)
            {
                mutex_select.Lock();
                shift_right();
            }
        }

    }
    private void setChartInfo(int id)
    {
        if (!chapterMeta.chartMetas[id].SP && Config.selectSP)
        {
            switch_to_special(false);
        }
        T_composer.text = chapterMeta.chartMetas[id].Composer;
        T_songName.text = chapterMeta.chartMetas[id].songName;
        T_songName_loading.text = chapterMeta.chartMetas[id].songName;
        string scoreKey = "score_" + Config.selectedChapter + "_" + chapterMeta.chartMetas[id].chartName;
        if (Config.selectSP)
            scoreKey += "_SP";
        int score = 0;
        if (ScoreManager.Load(scoreKey) != null)
            score = ScoreManager.Load(scoreKey).score;
        T_score.text = "BestScore: " + score.ToString();
        rankImage.sprite = Config.getRankImage(score);
        setSongSlice(id);
    }
    private void setSongSlice(int id)
    {
        if (audioList[id] != null)
            StartCoroutine(setAudioClip(id, 0.1f));
        else
        {
            audio_source.Stop();
            audio_source.clip = null;
        }

    }
    private IEnumerator setAudioClip(int id, float delay = 0)
    {
        float volume = audio_source.volume;
        //volume down
        for (int i=1;i<=10;i++)
        {
            audio_source.volume = volume / 10.0f * (10 - i);
            yield return new WaitForSeconds(delay / 10);
        }
        audio_source.Pause();       
        audio_source.clip = audioList[id];
        audio_source.time = 0;
        audio_source.Play();
        //volume up
        for (int i = 1; i <= 10; i++)
        {
            audio_source.volume = volume / 10.0f * i;
            yield return new WaitForSeconds(delay / 10);
        }
    }
    private void syncImage()// sync image to chart_id
    {
        presentSongView.sprite = images[chart_id];
        lastSongView.sprite = images[Mathf.Clamp((chart_id - 1 + chapterMeta.chartNum) % chapterMeta.chartNum, 0, chapterMeta.chartNum - 1)];
        nextSongView.sprite = images[Mathf.Clamp((chart_id + 1) % chapterMeta.chartNum, 0, chapterMeta.chartNum - 1)];
    }
    private void generateTestMeta()
    {
        Config.selectedChapter = "Chapter1";
        string path = System.Environment.CurrentDirectory + "/Charts/" + Config.selectedChapter + "/Chapter_Meta.json";
        Debug.Log(path);
        Chapter_Meta meta = new Chapter_Meta();
        Chart_Meta[] chartmeta = new Chart_Meta[5];
        for (int i = 0; i < 5; i++)
        {
            chartmeta[i] = new Chart_Meta();
            //chartmeta[i].score = new ScoreManager();
            //chartmeta[i].score.init(114514);
        }
        chartmeta[0].chartName = "test1";
        chartmeta[1].chartName = "test3";
        chartmeta[2].chartName = "Altersist";
        chartmeta[3].chartName = "marenol";
        chartmeta[4].chartName = "enchanted love";
        foreach (Chart_Meta chart in chartmeta)
        {
            meta.chartMetas.Add(chart);
        }
        Utilities.SaveJson<Chapter_Meta>(meta, path);
    }
    public void gotoSettings()
    {
        if (freeMode)
        {
            Debug.Log("settings");
            switchScene(4);
        }
    }
    public void gotoPlayView()
    {
        if (freeMode)
        {
            switchScene(2);
        }
    }
}
