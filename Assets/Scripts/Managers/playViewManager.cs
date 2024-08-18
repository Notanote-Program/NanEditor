using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class playViewManager : MonoBehaviour
{
    public GameObject playKernel;
    public GameObject playUI;
    private Animator anim;
    private Config.LoadType loadType;
    private bool start = false;
    private bool finish = false;
    private bool stoped = false;
    private bool inited = false;
    public Text T_score;
    public Text T_perfectNum;
    public Text T_goodNum;
    public Text T_badNum;
    public Text T_missNum;
    public Text T_songName;
    public Text T_specialTips;
    public Text T_newBest;
    public Image songImage;
    public Image rankImage;
    public Text T_delaySuggestion;

    Chart chart;
    /* this is where play_kernel begins */
    void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {
        init(Config.loadType);
/*        Thread thread_init = new Thread(init_thread);
        thread_init.Start(Config.loadType);*/
    }
    // Update is called once per frame
    void Update()
    {
        playerInput();
        if (start)
        {
            setPlayUI();
        }

        if (start && playKernel.GetComponent<Test>().finished && !finish)
        {
            ranking(playKernel.GetComponent<Test>().getScore());
        }
    }
    void init_thread(object data)
    {
        Config.LoadType loadType = (Config.LoadType)data;
        init(loadType);
    }    
    void init(Config.LoadType loadType = Config.LoadType.External)
    {
        this.loadType = loadType;
        string selectedChart = Config.selectedChart;
        if (Config.selectSP)
            selectedChart += "_SP";
        string selectedChapter = Config.selectedChapter;
        string path;
        if (loadType == Config.LoadType.Resource)
        {
            path = "Charts/" + selectedChapter + "/" + selectedChart + "/" + selectedChart;
        }
        else
        {
            path = System.Environment.CurrentDirectory + "/Charts/" + selectedChapter + "/" + selectedChart + "/" + selectedChart + ".json";
        }

        playKernel.GetComponent<Test>().chart_name = selectedChapter + "/" + selectedChart;
        if (loadType == Config.LoadType.Resource)
            playKernel.GetComponent<Test>().song_name = selectedChart;
        else
            playKernel.GetComponent<Test>().song_name = "*";
        chart = Chart.LoadChart(path, loadType);

        playKernel.GetComponent<Test>().init(chart, loadType);
        playKernel.GetComponent<Test>().setTime(1);// start from 1ms
        playKernel.GetComponent<Test>().setAuto(Config.autoplay);//set autoplay

        playUI.GetComponent<PlayUiManager>().init(chart.name);
        playUI.GetComponent<PlayUiManager>().setPartition(0);

        anim = GetComponent<Animator>();
        anim.updateMode = AnimatorUpdateMode.UnscaledTime;

        inited = true;
        Debug.Log("init finished");
    }
    private void setPlayUI()
    {
        playUI.GetComponent<PlayUiManager>().setPartition(playKernel.GetComponent<Test>().getChartPartition());
    }
    private void playerInput()
    {
        //if (Input.GetKeyDown(KeyCode.Space))
        if (Input.anyKeyDown)
        {
            startPlay();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pause();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            ranking(playKernel.GetComponent<Test>().getScore());
        }
    }
    void startPlay()
    {
        //TODO:
        if (!start)
        {
            Debug.Log("start");
            playKernel.GetComponent<Test>().Play();
            start = true;
        }
    }

    public void pause()
    {
        //TODO:
        if (start && !stoped && !finish)
        {
            stoped = true;
            anim.SetTrigger("pause");
            Time.timeScale = 0;
            playKernel.GetComponent<Test>().Pause();
        }
    }
    
    public void resume()
    {
        //TODO:
        if (stoped)
        {
            stoped = false;
            anim.SetTrigger("back");
            Time.timeScale = 1;
            playKernel.GetComponent<Test>().Resume();
        }
    }
    public void restart()
    {
        //TODO:
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void quit()
    {
        //TODO:
        Time.timeScale = 1;
        SceneManager.LoadScene(Config.lastSceneId);
    }
    bool saveScore(ScoreManager score)
    {
        string saveKey = "score_" + Config.selectedChapter + "_" + Config.selectedChart;
        if (Config.selectSP)
            saveKey += "_SP";
        ScoreManager savedScore = ScoreManager.Load(saveKey);
        if (savedScore == null || savedScore.score < score.score)
        {
            score.Save(saveKey);
            return true;
        }
        return false;
    }
    void ranking(ScoreManager score)
    {
        playKernel.GetComponent<Test>().Pause();
        finish = true;
        stoped = true;
        if (!playKernel.GetComponent<Test>().auto)
        {
            if (saveScore(score))// get a higher score
                T_newBest.text = "New Best Score!";
            else
                T_newBest.text = "";
        }
        else
            T_newBest.text = "AutoPlay is great!";
        T_songName.text = chart.name;
        T_score.text = "Score: " + score.score.ToString();
        T_perfectNum.text = "Perfect: " + score.PerfectNum.ToString();
        T_goodNum.text = "Good: " + score.GoodNum.ToString();
        T_badNum.text = "Bad: " + score.BadNum.ToString();
        T_missNum.text = "Miss: " + score.MissNum.ToString();
        if (score.score == Config.TotalScore)
            T_specialTips.text = "ALL PERFECT!";
        else if (score.MaxCombo == chart.noteNum)
            T_specialTips.text = "FULL COMBO!";
        else 
            T_specialTips.text = "";
        T_delaySuggestion.text = "Average Delay: " + Utilities.round(score.AverageDelay,2);
        rankImage.sprite = getRankImage(score.score);
        songImage.sprite = getSongImage();

        playUI.GetComponent<PlayUiManager>().setAnim("quit");
        anim.SetTrigger("ranking");
    }
    Sprite getSongImage()
    {
        string imgpath;
        if (loadType == Config.LoadType.Resource)
        {
            imgpath = "Charts/" + Config.selectedChapter + "/" + Config.selectedChart + "/illustration";
        }
        else
        {
            imgpath = System.Environment.CurrentDirectory + "/Charts/" + Config.selectedChapter + "/" + Config.selectedChart + "/illustration";
        }
        return Utilities.loadSprite(imgpath, loadType);
    }
    Sprite getRankImage(int score)
    {
        return Config.getRankImage(score);
    }
}
