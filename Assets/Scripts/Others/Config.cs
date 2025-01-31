using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config
{
    public const float TotalScore = 1000000.0f;
    public const float S_Score = 970000.0f;
    public const float A_Score = 950000.0f;
    public const float B_Score = 900000.0f;
    public const float C_Score = 850000.0f;
    public const float defaultMusicVolume = 0.25f;
    public const float defaultKeyVolume = 1.0f;

    public static JudgeRange range_normal = new JudgeRange();
    public static float deltaTime = 0.001f;
    public static Dictionary<string, Sprite> spriteList = new Dictionary<string, Sprite>(); // cache for sprites

    public static float delay
    {
        get
        {
            if (PlayerPrefs.HasKey("delay"))
                return PlayerPrefs.GetFloat("delay");
            else
                return 0;
        }
        set { PlayerPrefs.SetFloat("delay", value); }
    }

    static async void Init()
    {
    }

    public static float keyVolume
    {
        get
        {
            if (PlayerPrefs.HasKey("keyVolume"))
                return PlayerPrefs.GetFloat("keyVolume");
            else
                return 1;
        }
        set { PlayerPrefs.SetFloat("keyVolume", value); }
    }

    public static float musicVolume
    {
        get
        {
            if (PlayerPrefs.HasKey("musicVolume"))
                return PlayerPrefs.GetFloat("musicVolume");
            else
                return 1;
        }
        set { PlayerPrefs.SetFloat("musicVolume", value); }
    }

    public static int antiAliasing
    {
        get
        {
            if (PlayerPrefs.HasKey("antiAliasing"))
                return PlayerPrefs.GetInt("antiAliasing");
            else
            {
                return QualitySettings.antiAliasing;
            }
        }
        set
        {
            PlayerPrefs.SetInt("antiAliasing", value);
            QualitySettings.antiAliasing = value;
        }
    }

    public static int GraphicQuality
    {
        get
        {
            if (PlayerPrefs.HasKey("GraphicQuality"))
                return PlayerPrefs.GetInt("GraphicQuality");
            else
            {
                return QualitySettings.GetQualityLevel();
            }
        }
        set
        {
            PlayerPrefs.SetInt("GraphicQuality", value);
            QualitySettings.SetQualityLevel(value);
        }
    }

    public static int dspBufferSize
    {
        get
        {
            if (PlayerPrefs.HasKey("dspBufferSize"))
                return PlayerPrefs.GetInt("dspBufferSize");
            else
            {
                return AudioSettings.GetConfiguration().dspBufferSize;
            }
        }
        set
        {
            PlayerPrefs.SetInt("dspBufferSize", value);
            AudioConfiguration ac = AudioSettings.GetConfiguration();
            ac.dspBufferSize = value;
            if (!AudioSettings.Reset(ac))
                Debug.LogError("dspbuffer set failed");
        }
    }

    public static int lastSceneId = 0;

    public static string selectedChapter
    {
        get
        {
            if (PlayerPrefs.HasKey("selectedChapter"))
                return PlayerPrefs.GetString("selectedChapter");
            else
                return "";
        }
        set { PlayerPrefs.SetString("selectedChapter", value); }
    }

    public static string selectedChart
    {
        get
        {
            if (PlayerPrefs.HasKey("selectedChart"))
                return PlayerPrefs.GetString("selectedChart");
            else
                return "";
        }
        set { PlayerPrefs.SetString("selectedChart", value); }
    }

    public static bool selectSP
    {
        get
        {
            if (PlayerPrefs.HasKey("selectSP"))
            {
                if (PlayerPrefs.GetInt("selectSP") == 0)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
        set
        {
            if (value)
                PlayerPrefs.SetInt("selectSP", 1);
            else
                PlayerPrefs.SetInt("selectSP", 0);
        }
    }

    public static bool initSceneSucceed
    {
        get
        {
            if (PlayerPrefs.HasKey("loadSceneSucceed"))
            {
                if (PlayerPrefs.GetInt("loadSceneSucceed") == 0)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
        set
        {
            if (value)
                PlayerPrefs.SetInt("loadSceneSucceed", 1);
            else
                PlayerPrefs.SetInt("loadSceneSucceed", 0);
        }
    }

    public static bool autoplay
    {
        get
        {
            if (PlayerPrefs.HasKey("autoplay"))
            {
                if (PlayerPrefs.GetInt("autoplay") == 0)
                    return false;
                else
                    return true;
            }
            else
                return false;
        }
        set
        {
            if (value)
                PlayerPrefs.SetInt("autoplay", 1);
            else
                PlayerPrefs.SetInt("autoplay", 0);
        }
    }

    public static Config.LoadType loadType
    {
        get
        {
            if (PlayerPrefs.HasKey("loadType"))
            {
                int b = PlayerPrefs.GetInt("loadType");
                if (b == 0) // resource
                    return Config.LoadType.Resource;
                else
                    return Config.LoadType.External;
            }
            else
            {
                return Config.LoadType.Resource;
            }
        }
        set
        {
            if (value == LoadType.External)
                PlayerPrefs.SetInt("loadType", 1);
            else
                PlayerPrefs.SetInt("loadType", 0);
        }
    }

    public static Sprite getRankImage(int score)
    {
        string path = "Textures/Rank/";
        if (score == Config.TotalScore)
            path += "T_X";
        else if (score >= Config.S_Score)
            path += "T_S";
        else if (score >= Config.A_Score)
            path += "T_A";
        else if (score > Config.B_Score)
            path += "T_B";
        else if (score >= Config.C_Score)
            path += "T_C";
        else if (score > 0)
            path += "T_F";
        else if (score == 0)
            path += "T_N";
        else
            path += "T_?";
        return Utilities.loadSprite(path, Config.LoadType.Resource);
    }

    public enum Type
    {
        Tap,
        Drag,
        Hold
    }

    public enum comboType
    {
        Perfect,
        Early,
        Late,
        Bad,
        Miss
    }

    public enum ControlState
    {
        init,
        holding,
        detach
    }

    public enum EventType
    {
        Linear = 0,
        SineIn,
        SineOut,
        SineInOut,
        QuadIn,
        QuadOut,
        QuadInOut,
        CubicIn,
        CubicOut,
        CubicInOut,
        QuartIn,
        QuartOut,
        QuartInOut,
        QuintIn,
        QuintOut,
        QuintInOut,
        ExpoIn,
        ExpoOut,
        ExpoInOut,
        CircIn,
        CircOut,
        CircInOut,
        BackIn,
        BackOut,
        BackInOut,
        ElasticIn,
        ElasticOut,
        ElasticInOut,
        BounceIn,
        BounceOut,
        BounceInOut
    }

    public enum PathType
    {
        Straight,
        Bessel,
    }

    public enum LineType
    {
        Up = 0,
        Down,
    }

    public enum LoadType
    {
        Resource, //from resource file
        External //form External file
    }

    public enum EventlineType
    {
        Judgeline,
        PerformImg
    }

    public enum PasteTyte
    {
        Normal,
        Inherit,
        Smart
    }

    public enum PerformImgLayer
    {
        Background,
        AboveJudgementLine,
        AboveNote,
        AboveUI
    }

    public static Vector3 myposition2world(Vector3 mypos)
    {
        Vector3 screenpos = new Vector3((mypos.x / 16f + 0.5f) * Screen.width, (mypos.y / 9f + 0.5f) * Screen.height, mypos.z);
        return new Vector3(Camera.main.ScreenToWorldPoint(screenpos).x, Camera.main.ScreenToWorldPoint(screenpos).y, 0);
    }

    public static Vector3 world2myposition(Vector3 worldpos)
    {
        Vector3 screenpos = Camera.main.WorldToScreenPoint(worldpos);
        return new Vector3((screenpos.x / Screen.width - 0.5f) * 16f, (screenpos.y / Screen.height - 0.5f) * 9f, 0);
    }

    public static Sprite GetImgSprite(string rootFolder, string imgPath, LoadType loadType)
    {
        if (!rootFolder.Replace("\\", "/").EndsWith("/")) rootFolder += "/";
        if (!imgPath.StartsWith("$")) return Utilities.loadSprite(rootFolder + imgPath, loadType);
        imgPath = imgPath[1..];
        if (loadType != LoadType.External) return null;
        return imgPath switch
        {
            "cover" => Utilities.loadSprite(rootFolder + "../illustration", loadType),
            "cover_16x9" => Utilities.loadSprite(rootFolder + "../illustration_16x9", loadType),
            _ => null
        };
    }
}

public class JudgeRange
{
    public int perfect_duration; //ms
    public int good_duration; //ms
    public int bad_duration; //ms

    public JudgeRange(int perfect_duration = 70, int good_duration = 120, int bad_duration = 150)
    {
        this.perfect_duration = perfect_duration;
        this.good_duration = good_duration;
        this.bad_duration = bad_duration;
    }
}