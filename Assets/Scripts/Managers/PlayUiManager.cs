using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayUiManager : MonoBehaviour
{
    public Text Tscore;
    public Text Tcombo;
    public Text Tnote_rank;
    public Text T_songName;
    public Slider timeLine;
    public GameObject biasDisplay;
    Animator anim;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void init(string songName)
    {
        T_songName.text = songName;
        anim = GetComponent<Animator>();
    }
    public void update(ScoreManager score_manager)
    {
        Tscore.text = score_manager.score.ToString();
        Tcombo.text = score_manager.Combo.ToString();        
        Tnote_rank.text = noteRankToString(score_manager.lastNoteRank);
    }
    public void setPartition(float p)
    {
        timeLine.value = p;
    }
    public void setAnim(string s)
    {
        anim.SetTrigger(s);
    }
    public void setBias(Note note, float time)
    {
        // Debug.Log(time);
        // Debug.Log(note.time);
        float delta_time = time - note.time;
        if (note.type == Config.Type.Drag && Mathf.Abs(delta_time) <= Config.range_normal.bad_duration)// drag is always perfect
            delta_time = 0;
        if(biasDisplay!=null)
            biasDisplay.GetComponent<biasDisplay>().addPointer(delta_time);
    }
    private string noteRankToString(Config.comboType type)
    {
        switch(type)
        {
            case Config.comboType.Bad:
                return "Bad";
                break;
            case Config.comboType.Early:
                return "Early";
                break;
            case Config.comboType.Late:
                return "Late";
                break;
            case Config.comboType.Perfect:
                return "Perfect";
                break;
            case Config.comboType.Miss:
                return "Miss";
                break;
        }
        return "";
    }
}
