using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : BaseManager
{
    private enum NoteState
    {
        disabled,
        onBirth,
        living,
        onDestroy,
    }
    private Dictionary<Note, GameObject> noteObjectList;// rendering notes,always include selected notes
    private const string note_prefab_path = "Prefabs/Note";
    private List<Note> noteList;// all notes
    private List<Note> noteList_sortedBirthTime;
    private int nextNoteId;
    private List<GameObject> lineObjects;
    public Dictionary<Note, Config.ControlState> noteStateList;
    private List<Note> selectedNoteList;// selected notes
    public ScoreManager scoreManager;
    public GameObject playUI_manager;
    private JudgelineManager _judgelineManager;
    public List<Note> NoteList
    {
        get { return selectedNoteList; }
    }
    public void init(JudgelineManager _judgelineManager, List<GameObject> lines, List<Note> notes, int num, GameObject parent = null)
    {
        if (lines == null)
            this.lineObjects = new List<GameObject>();
        else
            this.lineObjects = new List<GameObject>(lines);
        if (notes == null)
            this.noteList = new List<Note>();
        else
            this.noteList = new List<Note>(notes);
        this.noteList.Sort((x, y) => x.time.CompareTo(y.time));
        noteList_sortedBirthTime = new List<Note>(noteList);
        noteList_sortedBirthTime.Sort((x, y) => (x.time - x.livingTime).CompareTo(y.time - y.livingTime));
        nextNoteId = 0;

        initPool(note_prefab_path, num, parent);
        noteObjectList = new Dictionary<Note, GameObject>();
        noteStateList = new Dictionary<Note, Config.ControlState>();
        selectedNoteList = new List<Note>();
        foreach (Note note in notes)
        {
            noteStateList.Add(note, Config.ControlState.init);
        }

        scoreManager = new ScoreManager();
        scoreManager.init(notes.Count);
        playUI_manager = GameObject.Find("playUI").gameObject;
        this._judgelineManager = _judgelineManager;
    }
    public void reset()
    {
        foreach(Note note in noteObjectList.Keys)
        {
            pool.release_item(noteObjectList[note]);
        }
        noteObjectList.Clear();
        selectedNoteList.Clear();
        nextNoteId = 0;

        scoreManager.init(noteList.Count);
    }
    private void addNote(Note note)
    {
        float distance = getDistance(note.livingTime, note.speed, note.lineSide);
        float length = getDistance(note.duration, note.speed, note.lineSide);
        if (!noteObjectList.ContainsKey(note))
        {
            noteStateList[note] = Config.ControlState.init;
            GameObject noteObject = pool.get_item();
            noteObject.GetComponent<NoteRenderer>().angle = lineObjects[note.lineId].GetComponent<JudgeLineRenderer>().angle;
            noteObject.transform.parent = lineObjects[note.lineId].transform;
            noteObject.transform.localScale = Vector3.one;
            noteObject.GetComponent<NoteRenderer>().init(distance, note.color, note.type, length, Config.ControlState.init);
            noteObjectList.Add(note, noteObject);
            selectedNoteList.Add(note);
        }
    }
    private void updateNote(Note note, float time)
    {
        if (!noteObjectList.ContainsKey(note))
            addNote(note);
        //Debug.Log(noteStateList[note]);
        noteObjectList[note].GetComponent<NoteRenderer>().setControlState(noteStateList[note]);
        noteObjectList[note].GetComponent<NoteRenderer>().distance = getDistance(note.time - time, note.speed, note.lineSide);
        if (note.type == Config.Type.Hold)
        {
            noteObjectList[note].GetComponent<NoteRenderer>().length = getDistance(Mathf.Min(note.duration, note.duration - time + note.time), note.speed, note.lineSide);
        }
        fade(note, time);
    }
    public void releaseNote(Note note, float delay = 0)
    {
        delay = Mathf.Max(0,delay)/1000.0f;
        selectedNoteList.Remove(note);
        if(delay == 0)
        {
            if (noteObjectList.ContainsKey(note))
            {
                GameObject noteObject = noteObjectList[note];
                noteObjectList.Remove(note);
                pool.release_item(noteObject);
            }
            return;
        }
        else
            StartCoroutine(deleteNote(note, delay));
    }
    private IEnumerator deleteNote(Note note, float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        if (noteObjectList.ContainsKey(note))
        {
            GameObject noteObject = noteObjectList[note];
            noteObjectList.Remove(note);
            pool.release_item(noteObject);
        }
    }
    public void update(float time)
    {
        //add new notes to selected notelist
        while (nextNoteId < noteList_sortedBirthTime.Count)
        {
            Note note = noteList_sortedBirthTime[nextNoteId];
            if (time > note.time - note.livingTime)
            {
                addNote(note);
                nextNoteId++;
            }
            else
                break;
        }
        //update controll state
        for (int i = selectedNoteList.Count - 1; i >= 0; i--)
        {
            Note note = selectedNoteList[i];
            if (note.type == Config.Type.Hold && noteStateList[note] == Config.ControlState.init && note.time + Config.range_normal.bad_duration < time)
            {
                noteStateList[note] = Config.ControlState.detach;
                startNoteRanking(note, float.PositiveInfinity);
                endNoteRanking(note);
                //releaseNote(note, note.time + note.duration - time);
            }
        }
        //update notes state
        foreach (Note note in noteObjectList.Keys)
        {
            if (getNoteState(note, time) == NoteState.living)
                updateNote(note, time);
        }
        //delete outdated notes
        for (int i = selectedNoteList.Count - 1; i >= 0; i--)
        {
            Note note = selectedNoteList[i];
            switch (getNoteState(note, time))
            {
                case NoteState.onDestroy:
                    releaseNote(note);
                    startNoteRanking(note, time);
                    endNoteRanking(note);
                    break;
            }
        }
    }
    private float getDistance(float duration, float speed, Config.LineType side)
    {
        return Mathf.Max(0, duration)/1000.0f * speed * (side == Config.LineType.Line1 ? 1 : -1);
    }
    private NoteState getNoteState(Note note, float time)
    {
        if (note.type == Config.Type.Hold)
        {
            if (note.time - note.livingTime > time)
                return NoteState.disabled;
            if (note.time - note.livingTime <= time && note.time - note.livingTime > time - Mathf.Max(Config.deltaTime, Time.deltaTime)*1000.0f)
                return NoteState.onBirth;
            if (note.time - note.livingTime <= time - Mathf.Max(Config.deltaTime, Time.deltaTime) * 1000.0f && note.time + note.duration + Config.range_normal.bad_duration > time)
                return NoteState.living;
            if (note.time + note.duration + Config.range_normal.bad_duration <= time)
            {
                return NoteState.onDestroy;
            }
        }
        else
        {
            if (note.time - note.livingTime > time)
                return NoteState.disabled;
            if (note.time - note.livingTime <= time && note.time - note.livingTime > time - Mathf.Max(Config.deltaTime, Time.deltaTime) * 1000.0f)
                return NoteState.onBirth;
            if (note.time - note.livingTime <= time - Mathf.Max(Config.deltaTime, Time.deltaTime) * 1000.0f && note.time + Config.range_normal.bad_duration > time)
                return NoteState.living;
            if (note.time + Config.range_normal.bad_duration <= time)
                return NoteState.onDestroy;
        }
        return NoteState.onDestroy;
    }
    public Vector3 getPosition(Note note)
    {
        if (note == null || !noteObjectList.ContainsKey(note))
        {
            return Vector3.zero;
        }

        //return noteObjectList[note].transform.position;
        Vector3? judgeRingPosition = _judgelineManager.getJudgeRingPosition(note.lineId, note.time);
        if (judgeRingPosition == null) return lineObjects[note.lineId].GetComponent<JudgeLineRenderer>().worldPosition;
        return Config.myposition2world((Vector3) judgeRingPosition);
    }
    private void fade(Note note, float time)
    {
        if (note.type == Config.Type.Hold)
        {
            if (note.time + note.duration < time)
            {
                float a = 1 - Mathf.Clamp((time - note.time - note.duration) / Config.range_normal.bad_duration, 0, 1);
                noteObjectList[note].GetComponent<NoteRenderer>().color = new Color(note.color.r, note.color.g, note.color.b, note.color.a * a);

            }
        }
        else if (note.time < time)
        {
            float a = 1 - Mathf.Clamp((time - note.time) / Config.range_normal.bad_duration, 0, 1);
            noteObjectList[note].GetComponent<NoteRenderer>().color = new Color(note.color.r, note.color.g, note.color.b, note.color.a * a);
        }
    }
    public void startNoteRanking(Note note, float time)
    {
        scoreManager.startRanking(note, time);
        playUI_manager.GetComponent<PlayUiManager>().setBias(note,time);
    }
    public void endNoteRanking(Note note)
    {
        scoreManager.endRanking(note);
        playUI_manager.GetComponent<PlayUiManager>().update(scoreManager);
    }
    public void finalRanking()
    {
        
        //TODO:
    }
}
