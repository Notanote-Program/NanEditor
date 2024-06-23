using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

public class Utilities
{
    public static string toString<T>(T obj)
    {
        return JsonConvert.SerializeObject(obj);
    }

    public static T fromString<T>(string s)
    {
        return JsonConvert.DeserializeObject<T>(s);
    }

    public static Vector3 string2vector(string s)
    {
        s = s.Substring(1, s.Length - 2);
        string[] _s = s.Split(",");
        Vector3 vector = new Vector3(float.Parse(_s[0]), float.Parse(_s[1]), float.Parse(_s[2]));
        return vector;
    }

    public static IEnumerator LoadFromJsonAsync<T>(AsyncRequest<T> obj, string path,
        Config.LoadType type = Config.LoadType.External)
    {
        if (type == Config.LoadType.External)
        {
            StreamReader sr = new StreamReader(path);

            obj.asset = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
            obj.isDone = true;
            obj.progress = 1;
            //T t = JsonUtility.FromJson<T>(sr.ReadToEnd());
            sr.Close();
            yield return null;
        }
        else
        {
            string s = "";
            ResourceRequest rr = Resources.LoadAsync<TextAsset>(path);
            while (!rr.isDone)
            {
                obj.progress = rr.progress;
                yield return null;
            }

            TextAsset jsontext = rr.asset as TextAsset;
            if (jsontext != null)
                s = jsontext.text;
            obj.asset = JsonConvert.DeserializeObject<T>(s);
            obj.isDone = true;
            yield return null;
        }
    }

    public static T LoadFromJson<T>(string path, Config.LoadType type = Config.LoadType.External)
    {
        if (type == Config.LoadType.External)
        {
            T t = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            //T t = JsonUtility.FromJson<T>(sr.ReadToEnd());
            return t;
        }
        else
        {
            string s = "";
            TextAsset jsontext = Resources.Load<TextAsset>(path);
            if (jsontext != null)
                s = jsontext.text;
            T t = JsonConvert.DeserializeObject<T>(s);
            return t;
        }
    }

    public static void SaveJson<T>(T info, string path)
    {
        string s = JsonConvert.SerializeObject(info);
        StreamWriter sw = new StreamWriter(path);
        sw.Write(s);
        sw.Close();
    }

    public static void SaveBinary<T>(T info, string path)
    {
        string s = JsonConvert.SerializeObject(info);
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.OpenOrCreate);
        bf.Serialize(file, s);
        file.Close();
        Debug.Log("saved");
    }

    public static T LoadBinary<T>(string path)
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Open(path, FileMode.Open);
        String s = (String)bf.Deserialize(file);
        T info = JsonConvert.DeserializeObject<T>(s);
        file.Close();
        return info;
    }

    public static float getPartition(float t, Config.EventType type)
    {
        t = Mathf.Clamp01(t);
        float v = t;
        switch (type)
        {
            case Config.EventType.Linear:
                // Linear, do nothing
                break;
            case Config.EventType.SineIn:
                v = 1 - Mathf.Cos(t * Mathf.PI / 2);
                break;
            case Config.EventType.SineOut:
                v = Mathf.Sin(t * Mathf.PI / 2);
                break;
            case Config.EventType.SineInOut:
                v = (1 - Mathf.Cos(Mathf.PI * t)) / 2;
                break;
            case Config.EventType.CubicIn:
                v = Mathf.Pow(t, 3f);
                break;
            case Config.EventType.CubicOut:
                v = Mathf.Pow(t - 1, 3f) + 1;
                break;
            case Config.EventType.CubicInOut:
                v = t < 0.5f ? Mathf.Pow(t, 3f) * 4 : 1 + Mathf.Pow(t - 1, 3f) * 4;
                break;
            case Config.EventType.QuadIn:
                v = Mathf.Pow(t, 2f);
                break;
            case Config.EventType.QuadOut:
                v = -Mathf.Pow(t - 1, 2f) + 1;
                break;
            case Config.EventType.QuadInOut:
                v = t < 0.5f ? Mathf.Pow(t, 2f) * 2 : 1 - Mathf.Pow(t - 1, 2f) * 2;
                break;
            case Config.EventType.QuartIn:
                v = Mathf.Pow(t, 4f);
                break;
            case Config.EventType.QuartOut:
                v = -Mathf.Pow(t - 1, 4f) + 1;
                break;
            case Config.EventType.QuartInOut:
                v = t < 0.5f ? Mathf.Pow(t, 4f) * 8 : 1 - Mathf.Pow(t - 1, 4f) * 8;
                break;
            case Config.EventType.QuintIn:
                v = t * t * t * t * t;
                break;
            case Config.EventType.QuintOut:
                v = 1 - t;
                v = 1 - v * v * v * v * v;
                break;
            case Config.EventType.QuintInOut:
                if (t < 0.5f)
                {
                    v = 16 * t * t * t * t * t;
                }
                else
                {
                    v = 1 - t;
                    v *= 2;
                    v = 1 - v * v * v * v * v / 2;
                }

                break;
            case Config.EventType.ExpoIn:
                v = Math.Abs(t) < 0.001f ? 0 : Mathf.Pow(2, 10 * t - 10);
                break;
            case Config.EventType.ExpoOut:
                v = Math.Abs(t - 1f) < 0.001f ? 1 : 1 - Mathf.Pow(2, -10 * t);
                break;
            case Config.EventType.ExpoInOut:
                v = Math.Abs(t) < 0.001f ? 0 :
                    Math.Abs(t - 1f) < 0.001f ? 1 :
                    t < 0.5 ? Mathf.Pow(2, 20 * t - 10) / 2 : (2 - Mathf.Pow(2, -20 * t + 10)) / 2;
                break;
            case Config.EventType.CircIn:
                v = 1 - Mathf.Sqrt(1 - t * t);
                break;
            case Config.EventType.CircOut:
                v -= 1f;
                v = Mathf.Sqrt(1 - v * v);
                break;
            case Config.EventType.CircInOut:
                if (t < 0.5)
                {
                    v = (1 - Mathf.Sqrt(1 - 4 * t * t)) / 2;
                }
                else
                {
                    v = 1 - t;
                    v = (Mathf.Sqrt(1 - 4 * v * v) + 1) / 2;
                }

                break;
            case Config.EventType.BackIn:
                v = Math.Abs(t) < 0.001f ? 0 :
                    Math.Abs(t - 1f) < 0.001f ? 1 :
                    -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * 2 / 3 * Mathf.PI);
                break;
            case Config.EventType.BackOut:
                v = Math.Abs(t) < 0.001f ? 0 :
                    Math.Abs(t - 1f) < 0.001f ? 1 :
                    Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * 2 / 3 * Mathf.PI) + 1;
                break;
            case Config.EventType.BackInOut:
                v = Math.Abs(t) < 0.001f ? 0 :
                    Math.Abs(t - 1f) < 0.001f ? 1 :
                    t < 0.5 ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * 4 / 9 * Mathf.PI)) / 2 :
                    (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * 4 / 9 * Mathf.PI)) / 2 + 1;
                break;
            case Config.EventType.ElasticIn:
                v = Math.Abs(t) < 0.001f ? 0 :
                    Math.Abs(t - 1f) < 0.001f ? 1 :
                    -Mathf.Pow(2, 10 * t - 10) * Mathf.Sin((t * 10 - 10.75f) * 2 / 3 * Mathf.PI);

                break;
            case Config.EventType.ElasticOut:
                v = Math.Abs(t) < 0.001f ? 0 :
                    Math.Abs(t - 1f) < 0.001f ? 1 :
                    Mathf.Pow(2, -10 * t) * Mathf.Sin((t * 10 - 0.75f) * 2 / 3 * Mathf.PI) + 1;
                break;
            case Config.EventType.ElasticInOut:
                v = Math.Abs(t) < 0.001f
                    ? 0
                    : Math.Abs(t - 1f) < 0.001f
                        ? 1
                        : t < 0.5
                            ? -(Mathf.Pow(2, 20 * t - 10) * Mathf.Sin((20 * t - 11.125f) * 4 / 9 * Mathf.PI)) / 2
                            : (Mathf.Pow(2, -20 * t + 10) * Mathf.Sin((20 * t - 11.125f) * 4 / 9 * Mathf.PI)) / 2 + 1;
                break;
            case Config.EventType.BounceIn:
                v = (float)(1 - EaseOutBounce(1 - t));
                break;
            case Config.EventType.BounceOut:
                v = (float)EaseOutBounce(t);
                break;
            case Config.EventType.BounceInOut:
                v = (float)(t < 0.5
                    ? (1 - EaseOutBounce(1 - 2 * t)) / 2
                    : (1 + EaseOutBounce(2 * t - 1)) / 2);
                break;
        }

        return v;

        static double EaseOutBounce(double x)
        {
            const double n1 = 7.5625;
            const double d1 = 2.75;

            return x < 1 / d1
                ? n1 * x * x
                : x < 2 / d1
                    ? n1 * (x -= 1.5 / d1) * x + 0.75
                    : x < 2.5 / d1
                        ? n1 * (x -= 2.25 / d1) * x + 0.9375
                        : n1 * (x -= 2.625 / d1) * x + 0.984375;
        }
    }

    public static Vector3 getBesselPosition(float t, List<Vector3> positions)
    {
        Vector3 pos = Vector3.zero;
        for (int i = 0; i < positions.Count; i++)
        {
            pos += Combination(positions.Count - 1, i) * Mathf.Pow(t, i) * Mathf.Pow((1 - t), positions.Count - i - 1) *
                   positions[i];
        }

        return pos;
    }

    public static Vector3 getStraightPosition(float t, List<Vector3> positions)
    {
        int k = (int)Mathf.Clamp((positions.Count - 1) * t, 0, positions.Count - 1);
        if (k == positions.Count - 1)
            return positions[k];
        float p = t * (positions.Count - 1) - k;
        Vector3 pos = positions[k] * (1 - p) + positions[k + 1] * p;
        return pos;
    }

    public static float Combination(int n, int a)
    {
        float ans = 1;
        for (int i = 0; i < a; i++)
        {
            ans *= n - i;
            ans /= i + 1;
        }

        return ans;
    }

    public static GameObject getNearestObject(string tag)
    {
        Collider2D[] col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        GameObject selected = null;
        if (col.Length > 0)
        {
            float shortestLength = -1;
            foreach (Collider2D c in col)
            {
                if (c.transform.tag == tag)
                {
                    float length = Vector3.Distance(Camera.main.ScreenToWorldPoint(Input.mousePosition),
                        c.transform.position);
                    if (length < shortestLength || shortestLength < 0)
                    {
                        selected = c.transform.gameObject;
                        shortestLength = length;
                    }
                }
            }
        }

        return selected;
    }

    public static List<GameObject> getObjects(string tag)
    {
        Collider2D[] col = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        List<GameObject> selected = new List<GameObject>();
        if (col.Length > 0)
        {
            foreach (Collider2D c in col)
            {
                if (c.transform.tag == tag)
                {
                    selected.Add(c.transform.gameObject);
                }
            }
        }

        return selected;
    }

    public static Texture2D LoadTexture2D(string path)
    {
        if (!File.Exists(path))
            return null;
        Debug.Log(path);
        Texture2D texture2D = new Texture2D(0, 0);
        if (!texture2D.LoadImage(File.ReadAllBytes(path), false))
        {
            return null;
        }

        return texture2D;
        // using ImageAsset imageAsset = new ImageAsset();
        // imageAsset.Load(path);
        // return imageAsset.ToTexture2D();
    }

    public static Sprite loadSprite(string imgpath, Config.LoadType loadType)
    {
        if (loadType == Config.LoadType.External)
            imgpath += ".png";
        Texture2D texture = new Texture2D(50, 50);
        if (imgpath != null)
        {
            if (loadType == Config.LoadType.Resource)
            {
                Sprite sprite = Resources.Load<Sprite>(imgpath);
                return sprite != null ? sprite : Resources.Load<Sprite>("Textures/defaultimg");
            }
/*                texture = Resources.Load<Texture2D>(imgpath);*/

            texture = Utilities.LoadTexture2D(imgpath);
            return texture != null
                ? Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f))
                : Resources.Load<Sprite>("Textures/defaultimg");
        }

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    public static IEnumerator LoadSpriteAsync(AsyncRequest<Sprite> ar, string imgpath, Config.LoadType loadType)
    {
        if (loadType == Config.LoadType.External)
            imgpath += ".png";
        Texture2D texture = new Texture2D(50, 50);
        if (imgpath != null)
        {
            if (loadType == Config.LoadType.Resource)
            {
                ResourceRequest rr = Resources.LoadAsync<Sprite>(imgpath);
                while (!rr.isDone)
                {
                    ar.progress = rr.progress;
                    yield return null;
                }

                if (rr.asset == null)
                {
                    rr = Resources.LoadAsync<Sprite>("Textures/defaultimg");
                    while (!rr.isDone)
                    {
                        ar.progress = rr.progress;
                        yield return null;
                    }
                }

                ar.asset = rr.asset as Sprite;
                ar.isDone = true;
                yield return null;
            }
            /*                texture = Resources.Load<Texture2D>(imgpath);*/
            else
            {
                texture = Utilities.LoadTexture2D(imgpath);
                if (texture != null)
                    ar.asset = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f));
                else
                    ar.asset = Resources.Load<Sprite>("Textures/defaultimg");
                ar.progress = 1;
                ar.isDone = true;
                yield return null;
            }
        }

        ar.asset = Resources.Load<Sprite>("Textures/defaultimg");
        ar.progress = 1;
        ar.isDone = true;
        yield return null;
    }

    public static AnimationClip getAnimTime(Animator anim, string name)
    {
        foreach (var clip in anim.runtimeAnimatorController.animationClips)
        {
            if (clip.name == name)
                return clip;
        }

        return null;
    }

    public static float round(float f, int t)
    {
        float d = Mathf.Pow(10.0f, t);
        return (int)(f * d) / d;
    }

    public static AsyncOperation loadScene(int id)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(id);
        operation.allowSceneActivation = false;
        return operation;
    }
    
    [UsedImplicitly]
    public static Resolution GetFullScreenResolution()
    {
        Resolution newRes = Screen.resolutions.OrderByDescending(it => it.width).ToList()[0];
        if (newRes.width < newRes.height)
        {
            (newRes.height, newRes.width) = (newRes.width, newRes.height);
        }

        return newRes;
    }
}

public class AsyncRequest<T>
{
    public T asset;
    public bool isDone = false;
    public float progress = 0;

    public AsyncRequest(T asset, bool isDone = false, float progress = 0)
    {
        this.asset = asset;
        this.isDone = isDone;
        this.progress = progress;
    }
}

public class MutexLock
{
    public bool islocked
    {
        get { return _islocked; }
    }

    private bool _islocked = false;

    public void Unlock()
    {
        _islocked = false;
    }

    public void Lock()
    {
        _islocked = true;
    }
}