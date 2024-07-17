using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class Test : MonoBehaviour
{
    public AudioClip defaultHitSound;
    public bool isEditor;
    public bool auto;
    public string chart_name, song_name;
    // Start is called before the first frame update
    public GameObject judgeline_manager;
    public GameObject effect_manager;
    public GameObject note_manager;
    public GameObject img_manager;
    private AudioSource songAudio;
    public float pitch// audio speed
    {
        get { return songAudio.pitch; }
        set { songAudio.pitch = Mathf.Clamp(value, 0.3f, 2); }
    }
    private InputManager input_manager;
    private float startTime;// relative time point to start in the chart, in ms
    private float playTime;// physical time point to play the chart, in ms
    private bool onPlay;
    public Chart chart;
    private Config.LoadType loadType;
    private bool musicOnly = false;// in this mod, the chart wont update
    private bool showId = false;// show judgeline id
    public bool finished = false;
    void Start()
    {
        if (isEditor)
        {
            HitSoundManager.Instance.UpdateVolume(Config.keyVolume * Config.defaultKeyVolume);
            LoadExternalHitSound();
        }
    }

    private async void LoadExternalHitSound()
    {
        AudioClip tap = defaultHitSound, drag = defaultHitSound;
        if (File.Exists(Application.dataPath + "/../SFX/tap.wav"))
        {
            tap = await AudioLoader.LoadWavExternal(Application.dataPath + "/../SFX/tap.wav");
        }
        if (File.Exists(Application.dataPath + "/../SFX/drag.wav"))
        {
            drag = await AudioLoader.LoadWavExternal(Application.dataPath + "/../SFX/drag.wav");
        }
        
        HitSoundManager.Instance.RefreshHitSounds(tap, drag);
        
        Debug.Log("Loaded");
    }
    // Update is called once per frame
    void Update()
    {
        if (onPlay)
        {
            float time = (Time.time * 1000.0f - playTime) * pitch + startTime;
            if (!songAudio.isPlaying && time - chart.offset - Config.delay > 0)
            {
                songAudio.Play();
            }
            if (!musicOnly)
            {
                input_manager.update();
                if (!auto)
                    judge(time);
                else
                    autoPlay(time);                
                note_manager.GetComponent<NoteManager>().update(time);
                judgeline_manager.GetComponent<JudgelineManager>().update(time,showId);
                img_manager.GetComponent<PerformImgManager>().update(time);
            }
            if (time - chart.offset - Config.delay > songAudio.clip.length * 1000 + 1)
            {
                Pause();
                finished = true;
                //finalize();
            }
            else
                finished = false;
        }
    }
    public float getChartTime()
    {
        if(onPlay)
            return (Time.time*1000.0f - playTime) * pitch + startTime;
        else
            return startTime;
    }
    public float getChartPartition()
    {
        return Mathf.Clamp01(getChartTime() / (songAudio.clip.length * 1000.0f + chart.offset + Config.delay));
    }
    public void reset(Chart chart)
    {
        this.chart = chart;
        onPlay = false;
        if (judgeline_manager)
            Destroy(judgeline_manager);
        if (note_manager)
            Destroy(note_manager);
        if (img_manager)
            Destroy(img_manager);

        initJudgelineManager(chart);// must be inited first
        initNoteManager(chart);
        //img_manager.GetComponent<PerformImgManager>().init(chart.performImgList, chart.performImgList.Count, img_manager, loadType, chart_name);
        initPerformImgManager(chart);
        initAudio();
        setTime(startTime);
    }
    public void setTime(float time = 0,bool showId = false)
    {
        startTime = time;

        note_manager.GetComponent<NoteManager>().reset();
        judgeline_manager.GetComponent<JudgelineManager>().reset();
        img_manager.GetComponent<PerformImgManager>().reset();

        note_manager.GetComponent<NoteManager>().update(time);
        judgeline_manager.GetComponent<JudgelineManager>().update(time,showId);
        img_manager.GetComponent<PerformImgManager>().update(time);
    }
    public void Play(bool _musicOnly = true,bool _showId = false)
    {
        this.musicOnly = !_musicOnly;
        this.showId = _showId;
        if (!onPlay && songAudio.clip != null)
        {
            setTime(startTime);
            onPlay = true;
            playTime = Time.time * 1000.0f;
            if (startTime - chart.offset - Config.delay < 0)
                songAudio.time = 0;
            else
            {
                songAudio.time = Mathf.Clamp((startTime - chart.offset - Config.delay)/1000.0f,0,songAudio.clip.length);
                songAudio.Play();
            }
        }
    }
    public void Resume(bool _musicOnly = true, bool _showId = false)
    {
        this.musicOnly = !_musicOnly;
        this.showId = _showId;
        if (!onPlay && songAudio.clip != null)
        {
            onPlay = true;
            playTime = Time.time * 1000.0f;
            if (startTime - chart.offset - Config.delay < 0)
                songAudio.time = 0;
            else
            {
                songAudio.time = Mathf.Clamp((startTime - chart.offset - Config.delay) / 1000.0f, 0, songAudio.clip.length);
                songAudio.Play();
            }
        }
    }
    public void Pause()
    {
        if (onPlay)
        {
            startTime = (Time.time*1000.0f - playTime) * pitch + startTime;
            onPlay = false;
            songAudio.Pause();
        }
    }
    public void init(Chart chart, Config.LoadType loadType = Config.LoadType.Resource)
    {
        this.chart = chart;
        this.loadType = loadType;
        startTime = 0;
        onPlay = false;
        initJudgelineManager(chart);// must be inited first
        initNoteManager(chart);
        initPerformImgManager(chart);
        initEffectManager();
        initInputManager();
        initAudio();
        Debug.Log("player init finished");
    }
    private void initEffectManager()
    {
        effect_manager = new GameObject();
        effect_manager.transform.parent = this.transform;
        effect_manager.AddComponent<EffectManager>();
        effect_manager.GetComponent<EffectManager>().init(1000, effect_manager);
    }
    private void initJudgelineManager(Chart chart)
    {
        judgeline_manager = new GameObject();
        judgeline_manager.transform.parent = this.transform;
        judgeline_manager.AddComponent<JudgelineManager>();
        judgeline_manager.GetComponent<JudgelineManager>().init(chart.judgelineList, chart.judgelineList.Count, judgeline_manager);
    }
    private void initPerformImgManager(Chart chart)
    {
        img_manager = new GameObject();
        img_manager.transform.parent = this.transform;
        img_manager.AddComponent<PerformImgManager>();
        img_manager.GetComponent<PerformImgManager>().init(chart.performImgList, chart.performImgList.Count, img_manager, loadType, chart_name);
    }
    private void initNoteManager(Chart chart)
    {
        note_manager = new GameObject();
        note_manager.transform.parent = this.transform;
        note_manager.AddComponent<NoteManager>();
        List<Note> notes = new List<Note>();
        for(int i=0;i<chart.judgelineList.Count;i++)
        {
            JudgeLine line = chart.judgelineList[i];
            foreach (Note note in line.noteList)
            {
                note.lineId = i;
                notes.Add(note);
            }
        }
        note_manager.GetComponent<NoteManager>().init(judgeline_manager.GetComponent<JudgelineManager>(), judgeline_manager.GetComponent<JudgelineManager>().judgelineObjectList, notes, chart.noteNum, note_manager);
    }
    private void initInputManager()
    {
        input_manager = new InputManager();
        input_manager.init();
    }
    private void initAudio()
    {
        string path;
        if(loadType == Config.LoadType.Resource)
        {
            path = "Charts/" + chart_name;
        }
        else
        {
            path = System.Environment.CurrentDirectory + "/Charts/" + chart_name;
        }
        songAudio = gameObject.GetComponent<AudioSource>();
        loadAudio(path);
        songAudio.Pause();
        songAudio.volume = Config.musicVolume * Config.defaultMusicVolume;
    }
    private void loadAudio(string path)
    {
        AudioLoader loader = GetComponent<AudioLoader>();
        loader.LoadAudio(songAudio, path, song_name, loadType);
    }
        private void judge(float time)
        {
            List<Note> notes = note_manager.GetComponent<NoteManager>().NoteList;
            notes.Sort((x, y) => -x.time.CompareTo(y.time));
            for (int i = notes.Count - 1; i >= 0; i--)
            {
                Note note = notes[i];
                if (note.fake)
                {
                    judgeFake(note);
                }
                else
                {
                    judgeReal(note);
                }
            }

            return;

            void judgeReal(Note note)
            {
                float deltaTime = note.time - time;
                switch (note.type)
                {
                    case Config.Type.Tap:
                        if (Mathf.Abs(deltaTime) <= Config.range_normal.bad_duration)
                        {
                            if (input_manager.keyDown.Count > 0)
                            {
                                input_manager.keyDown.RemoveAt(0);
                                if (Mathf.Abs(deltaTime) < Config.range_normal.perfect_duration)
                                {
                                    effect_manager.GetComponent<EffectManager>().show(
                                        note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                        "perfect");
                                }
                                else if (Mathf.Abs(deltaTime) < Config.range_normal.good_duration)
                                {
                                    effect_manager.GetComponent<EffectManager>().show(
                                        note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                        "good");
                                }
                                else
                                {
                                    effect_manager.GetComponent<EffectManager>().show(
                                        note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                        "bad");
                                }

                                note_manager.GetComponent<NoteManager>().releaseNote(note);
                                note_manager.GetComponent<NoteManager>().startNoteRanking(note, time);
                                note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                            }
                        }

                        break;
                    case Config.Type.Drag:
                        if (state(note) != Config.ControlState.init)
                        {
                            break;
                        }

                        if (Mathf.Abs(deltaTime) < Config.range_normal.bad_duration)
                        {
                            if (input_manager.keyPress.Count > 0)
                            {
                                float delayTime = Mathf.Max(0, deltaTime);
                                effect_manager.GetComponent<EffectManager>().show(
                                    note_manager.GetComponent<NoteManager>().getPosition(note), note.color, delayTime, null,
                                    "perfect", 1);
                                note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.detach;
                                note_manager.GetComponent<NoteManager>().releaseNote(note, delayTime);
                                note_manager.GetComponent<NoteManager>().startNoteRanking(note, time);
                                note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                            }
                        }
                        else if (time > note.time)
                        {
                            note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.detach;
                            note_manager.GetComponent<NoteManager>().releaseNote(note);
                            note_manager.GetComponent<NoteManager>().startNoteRanking(note, Single.PositiveInfinity);
                            note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                        }

                        break;
                    case Config.Type.Hold:
                        if (state(note) == Config.ControlState.detach)
                        {
                            note_manager.GetComponent<NoteManager>().releaseNote(note, note.time + note.duration - time);
                        }

                        if (state(note) == Config.ControlState.init)
                        {
                            if (Mathf.Abs(deltaTime) < Config.range_normal.bad_duration &&
                                input_manager.keyDown.Count > 0)
                            {
                                //input_manager.bindingKey.Add(note, input_manager.keyDown[0]);
                                input_manager.keyDown.RemoveAt(0);
                                if (Mathf.Abs(deltaTime) < Config.range_normal.perfect_duration)
                                {
                                    effect_manager.GetComponent<EffectManager>().show(
                                        note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                        "perfect");
                                }
                                // else if (Mathf.Abs(note.time - time) < Config.range_normal.good_duration)
                                // {
                                //     _effectManager.show(
                                //         note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                //         "good");
                                // }
                                // else
                                // {
                                //     _effectManager.show(
                                //         note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                //         "bad");
                                // }
                                else
                                {
                                    effect_manager.GetComponent<EffectManager>().show(
                                        note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0, null,
                                        "good");
                                }

                                note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.holding;
                                note_manager.GetComponent<NoteManager>().startNoteRanking(note, time);
                            }
                            else if (note.time + Config.range_normal.bad_duration < time)
                            {
                                note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.detach;
                                note_manager.GetComponent<NoteManager>().startNoteRanking(note, float.PositiveInfinity);
                                note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                            }
                        }
                        else if (time < note.time + note.duration - Config.range_normal.bad_duration &&
                                 state(note) == Config.ControlState.holding)
                        {
                            //if(input_manager.bindingKey.ContainsKey(note) && input_manager.keyPress.Contains(input_manager.bindingKey[note]))
                            if (input_manager.keyPress.Count > 0)
                            {
                                //Debug.Log(input_manager.bindingKey[note]);
                            }
                            else
                            {
                                //input_manager.bindingKey.Remove(note);
                                note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.detach;
                                note_manager.GetComponent<NoteManager>().startNoteRanking(note, float.PositiveInfinity);
                                note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                                note_manager.GetComponent<NoteManager>().releaseNote(note, note.time + note.duration - time);
                            }
                        }
                        else if (time >= note.time + note.duration - Config.range_normal.bad_duration &&
                                 state(note) == Config.ControlState.holding)
                        {
                            //_effectManager.show(_note_manager.GetComponent<NoteManager>().getPosition(note), note.color, 0);
                            note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                            note_manager.GetComponent<NoteManager>().releaseNote(note, note.time + note.duration - time);
                        }

                        break;
                }
            }

            void judgeFake(Note note)
            {
                float deltaTime = MathF.Max(Config.deltaTime, Time.deltaTime) * 1000.0f * pitch;
                if (note.type == Config.Type.Hold)
                {
                    if (state(note) == Config.ControlState.detach)
                    {
                        note_manager.GetComponent<NoteManager>().releaseNote(note, note.time + note.duration - time);
                    }

                    if (note.time < time + deltaTime && state(note) == Config.ControlState.init)
                    {
                        //Debug.Log("on holding");
                        // float delayTime = time + deltaTime - note.time;
                        note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.holding;
                    }

                    if (note.time + note.duration <= time && state(note) == Config.ControlState.holding)
                    {
                        // float delayTime = time + deltaTime - note.time - note.duration;
                        // _effectManager.show(_note_manager.GetComponent<NoteManager>().getPosition(note), note.color, delayTime);
                        note_manager.GetComponent<NoteManager>().releaseNote(note, 0);
                    }
                }
                else if (note.time < (time + deltaTime) && state(note) == Config.ControlState.init)
                {
                    // float delayTime = time + deltaTime - note.time;
                    // int audioType = 0; // default: tap sound
                    // if (note.type == Config.Type.Drag)
                    //     audioType = 1;
                    note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.detach;
                    note_manager.GetComponent<NoteManager>().releaseNote(note, 0);
                }
            }
        }
    private void autoPlay(float time)
    {
        for (int i = note_manager.GetComponent<NoteManager>().NoteList.Count - 1; i >= 0; i--)
        {
            Note note = note_manager.GetComponent<NoteManager>().NoteList[i];
            float delta_time = MathF.Max(Config.deltaTime, Time.deltaTime) * 1000.0f * pitch;
            if (note.type == Config.Type.Hold)
            {
                if(state(note) == Config.ControlState.detach)
                {
                    note_manager.GetComponent<NoteManager>().releaseNote(note, note.time + note.duration - time);
                }
                if (note.time < time + delta_time && state(note) == Config.ControlState.init)
                {
                    //Debug.Log("on holding");
                    float delayTime = time + delta_time - note.time;
                    note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.holding;
                    effect_manager.GetComponent<EffectManager>().show(note_manager.GetComponent<NoteManager>().getPosition(note), note.color, delayTime);
                    note_manager.GetComponent<NoteManager>().startNoteRanking(note, time);
                }
                if (note.time + note.duration <= time && state(note) == Config.ControlState.holding)
                {
                    float delayTime = time + delta_time - note.time - note.duration;
                    //effect_manager.GetComponent<EffectManager>().show(note_manager.GetComponent<NoteManager>().getPosition(note), note.color, delayTime);
                    note_manager.GetComponent<NoteManager>().endNoteRanking(note);
                    note_manager.GetComponent<NoteManager>().releaseNote(note, 0);
                }
            }
            else if (note.time < (time + delta_time) && state(note) == Config.ControlState.init)
            {
                float delayTime = time + delta_time - note.time;
                effect_manager.GetComponent<EffectManager>().show(
                    note_manager.GetComponent<NoteManager>().getPosition(note), note.color, delayTime, null, "perfect",
                    note.type == Config.Type.Drag ? 1 : 0);
                note_manager.GetComponent<NoteManager>().noteStateList[note] = Config.ControlState.detach;
                note_manager.GetComponent<NoteManager>().releaseNote(note, 0);
                note_manager.GetComponent<NoteManager>().startNoteRanking(note, time);
                note_manager.GetComponent<NoteManager>().endNoteRanking(note);
            }
        }
    }
    private Config.ControlState state(Note note)
    {
        return note_manager.GetComponent<NoteManager>().noteStateList[note];
    }
    private void finalize()
    {
        note_manager.GetComponent<NoteManager>().finalRanking();
    }
    public ScoreManager getScore()
    {
        return note_manager.GetComponent<NoteManager>().scoreManager;
    }
    public void setAuto(bool b)
    {
        auto = b;
    }
}
