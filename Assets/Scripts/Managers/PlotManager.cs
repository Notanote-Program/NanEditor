using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlotManager : MonoBehaviour
{
    public Text T_dialog;
    public Text T_name;
    public Image T_character1, T_character2;
    public AudioSource T_typesound;

    private Plot plot;
    private int pos = 0;// 当前执行到的位置
    private bool is_generating = false;// 正在生成文本
    private bool display_all = false;// 直接生成块中所有文本
    void Start()
    {
        Init();
        ExcuteIns();
    }

    void Update()
    {
        if (getUserInput())
        {
            if(Input.GetKeyDown(KeyCode.R))
            {
                reset();
            }
            if (!is_generating)
            {
                ExcuteIns();
            }
            else
            {
                display_all = true;
            }
        }
    }
    private void Init()
    {
        string path = System.Environment.CurrentDirectory + "/Plots/story1/story1.json";
        plot = Utilities.LoadFromJson<Plot>(path);

    }
    private void reset()
    {
        pos = 0;
        T_character1.color = new Color(1, 1, 1, 0);
        T_character2.color = new Color(1, 1, 1, 0);
        T_dialog.text = "";
        T_name.text = "";
        T_typesound.Pause();
        is_generating = false;
        display_all = false;
}
    private bool getUserInput()
    {
        return Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Escape);
    }
    private void ExcuteIns()
    {
        while (pos < plot.lines.Count)
        {
            string[] lines = plot.lines[pos++].Split("|");
            if (lines.Length > 0)
            {
                switch (lines[0].Trim())
                {
                    case "dialog":
                        is_generating = true;
                        StartCoroutine(Dialog(pos - 1, lines));
                        return;
                    case "character1":
                        StartCoroutine(Character1(lines));
                        break;
                    case "character2":
                        StartCoroutine(Character2(lines));
                        break;
                    case "halt":
                        return;
                    default:
                        return;
                }
            }
            else
                return;
        }
    }
    private IEnumerator Dialog(int id, string[] texts)
    {
        T_dialog.text = "";
        T_name.text = texts[1];
        T_typesound.Play();
        for (int i = 2; i < texts.Length; i++)
        {
            string text = texts[i].Trim();           
            T_dialog.text = "\u3000\u3000";
            for (int j= 0; j < text.Length; j++)
            {
                T_dialog.text += text[j];
                yield return new WaitForSeconds(0.05f);
                if(display_all)
                {
                    T_dialog.text = "\u3000\u3000" + texts[^1].Trim();
                    is_generating = false;
                    display_all = false;
                    T_typesound.Pause();
                    yield break;// 终止生成
                }
            }
            yield return new WaitForSeconds(0.3f);
        }
        is_generating = false;
        display_all = false;
        T_typesound.Pause();
        yield return null;
    }
    private IEnumerator Character1(string[] texts)
    {
        string command = texts[1].Trim();
        Color color = T_character1.color;
        switch (command)
        {
            case "display":
                if (texts.Length > 2)
                {
                    string path = System.Environment.CurrentDirectory + "/Plots/story1/" + texts[2].Trim();
                    Debug.Log(path);
                    T_character1.sprite = Utilities.loadSprite(path, Config.LoadType.External);
                }
                else
                {
                    T_character1.sprite = Resources.Load<Sprite>("Textures/defaultimg");
                }
                for (int i = 0; i < 10; i++)
                {
                    T_character1.color = new Color(color.r, color.g, color.b, i / 9.0f);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            case "hide":
                color = T_character1.color;
                for (int i = 0; i < 10; i++)
                {
                    T_character1.color = new Color(color.r, color.g, color.b, color.a * (9 - i) / 9.0f);
                    yield return new WaitForSeconds(0.07f);
                }
                break;
        }

        yield return null;
    }
    private IEnumerator Character2(string[] texts)
    {
        Color color = T_character2.color;
        string command = texts[1];
        switch (command)
        {
            case "display":
                if (texts.Length > 2)
                {
                    string path = texts[2];
                    T_character2.sprite = Utilities.loadSprite(path, Config.LoadType.External);
                }
                else
                {
                    T_character2.sprite = Resources.Load<Sprite>("Textures/defaultimg");
                }
                for (int i = 0; i < 10; i++)
                {
                    T_character2.color = new Color(color.r, color.g, color.b, color.a * i / 9);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            case "hide":
                for (int i = 0; i < 10; i++)
                {
                    T_character2.color = new Color(color.r, color.g, color.b, color.a * (9 - i) / 9);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
        }

        yield return null;
    }
}
