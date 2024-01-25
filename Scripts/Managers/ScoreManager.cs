using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    private int _noteNum;
    private int _PerfectNum;
    private int _GoodNum;
    private int _EarlyNum;
    private int _LateNum;
    private int _BadNum;
    private int _MissNum;
    private int _Combo;
    private int _MaxCombo;
    [JsonIgnore]
    private float _DelaySum;
    [JsonIgnore]
    private int _DelayNum;
    [JsonIgnore]
    private Config.comboType _lastNoteRank;
    [JsonIgnore]
    private Dictionary<Note, Config.comboType> noteRank;
    public void init(int num)
    {
        _noteNum = num;
        _PerfectNum = 0;
        _GoodNum = 0;
        _BadNum = 0;
        _MissNum = 0;
        _Combo = 0;
        _MaxCombo = 0;
        noteRank = new Dictionary<Note, Config.comboType>();
    }
    public int noteNum
    {
        get { return _noteNum; }
        set { _noteNum = value; }
    }
    public int score
    {
        get { return getScore(); }
    }
    public float score_f
    {
        get { return getScoref(); }
    }
    public int Combo
    {
        get { return _Combo; }
        set { _Combo = value; }
    }
    public int PerfectNum
    {
        get { return _PerfectNum; }
        set
        {
            _PerfectNum = value;
        }   
    }
    public int GoodNum
    {
        get { return _EarlyNum + _LateNum; }
    }
    public int EarlyNum
    {
        get { return _EarlyNum; }
        set
        {
            _EarlyNum = value;
        }
    }
    public int LateNum
    {
        get { return _LateNum; }
        set
        {
            _LateNum = value;
        }
    }
    public int BadNum
    {
        get { return _BadNum; }
        set
        {
            _BadNum = value;
        }
    }
    public int MissNum
    {
        get { return _MissNum; }
        set
        {
            _MissNum = value;
        }
    }
    public int MaxCombo
    {
        get { return _MaxCombo; }
        set
        {
            _MaxCombo = value;
        }
    }
    [JsonIgnore]
    public float AverageDelay
    {
        get 
        { 
            if (_DelayNum == 0) 
                return 0; 
            else 
                return _DelaySum / _DelayNum; 
        }
    }
    [JsonIgnore]
    public Config.comboType lastNoteRank
    {
        get { return _lastNoteRank; }
    }
    private int getScore()
    {
        if (_noteNum <= 0)
            return 0;
        float score_per_note = Config.TotalScore / Mathf.Max(1, _noteNum);
        return Mathf.RoundToInt(score_per_note * _PerfectNum + score_per_note / 2 * _GoodNum);
    }
    private float getScoref()
    {
        if (_noteNum <= 0)
            return 0;
        float score_per_note = Config.TotalScore / Mathf.Max(1, _noteNum);
        return score_per_note * _PerfectNum + score_per_note / 2 * _GoodNum;
    }
    private void addCombo(Config.comboType type)
    {
        switch (type)
        {
            case Config.comboType.Perfect:
                _PerfectNum++;
                _Combo++;
                _MaxCombo = Mathf.Max(_MaxCombo, _Combo);
                break;
            case Config.comboType.Early:
                _EarlyNum++;
                _Combo++;
                _MaxCombo = Mathf.Max(_MaxCombo, _Combo);
                break;
            case Config.comboType.Late:
                _LateNum++;
                _Combo++;
                _MaxCombo = Mathf.Max(_MaxCombo, _Combo);
                break;
            case Config.comboType.Bad:
                _BadNum++;
                _Combo = 0;
                break;
            case Config.comboType.Miss:
                _MissNum++;
                _Combo = 0;
                break;
        }
        Debug.Log(type);
    }
    public void startRanking(Note note, float time)
    {
        float deltatime = note.time - time;       
        if (noteRank.ContainsKey(note))
        {
            noteRank[note] = getNoteRank(note,deltatime);
        }
        else
        {
            noteRank.Add(note, getNoteRank(note,deltatime));
        }
    }
    public void endRanking(Note note)
    {
        if(noteRank.ContainsKey(note))
        {
            addCombo(noteRank[note]);
            _lastNoteRank = noteRank[note];
            noteRank.Remove(note);
        }
    }
    private Config.comboType getNoteRank(Note note,float time)
    {
        float abs_time = Mathf.Abs(time);
        if(note.type == Config.Type.Drag)
        {
            if (abs_time > Config.range_normal.bad_duration)
                return Config.comboType.Miss;
            else
                return Config.comboType.Perfect;
        }
        else
        {
            if (abs_time <= Config.range_normal.perfect_duration)
            {
                _DelaySum -= time;
                _DelayNum++;
                return Config.comboType.Perfect;
            }               
            else if (abs_time <= Config.range_normal.good_duration)
            {
                _DelaySum -= time;
                _DelayNum++;
                if (time > 0)
                    return Config.comboType.Early;
                else
                    return Config.comboType.Late;
            }
            else if (abs_time <= Config.range_normal.bad_duration)
                return Config.comboType.Bad;
            else
                return Config.comboType.Miss;
        }
    }

    public void Save(string key)
    {
        PlayerPrefs.SetString(key, Utilities.toString(this));
    }
    public static ScoreManager Load(string key)
    {
        if(PlayerPrefs.HasKey(key))
        {
            return Utilities.fromString<ScoreManager>(PlayerPrefs.GetString(key));
        }
        else
        {
            return null;
        }
    }
}
