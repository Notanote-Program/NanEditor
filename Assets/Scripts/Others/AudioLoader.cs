using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class AudioLoader : MonoBehaviour
{
    private List<string> availableType = new List<string>() { "wav", "ogg" };
    public void LoadAudio(AudioSource audio, string path, string songName, Config.LoadType loadType)
    {
        if (loadType == Config.LoadType.Resource)
        {
            AudioClip song = Resources.Load<AudioClip>(path + "/" + songName);
            if (song != null)
                audio.clip = song;
        }
        else
        {
            if (Directory.Exists(path))
            {
                DirectoryInfo direction = new DirectoryInfo(path);
                foreach (string type in availableType)
                {
                    FileInfo[] files = direction.GetFiles(songName + "." + type);
                    if (files.Length > 0)
                    {
                        string _path = files[0].FullName;
                        StartCoroutine(_LoadAudio(_path, type, audio));
                        //_LoadAudio(_path, type, audio);
                        break;
                    }
                }
            }
        }
    }
    private static IEnumerator _LoadAudio(string url, string type, AudioSource audio)
    {
        WWW www = new WWW("file:///" + url);
        yield return www;

        if (www != null && www.isDone)
        {
            switch (type)
            {
                case "wav":
                    audio.clip = www.GetAudioClip(true, false, AudioType.WAV);
                    break;
                case "ogg":
                    audio.clip = www.GetAudioClip(true, false, AudioType.OGGVORBIS);
                    break;
            }
        }
    }
    public AudioClip LoadAudioClip(string path, string songName, Config.LoadType loadType)
    {
        if (loadType == Config.LoadType.Resource)
        {
            return Resources.Load<AudioClip>(path + "/" + songName);
        }
        else
        {
            //todo
            return null;
        }
    }
    
    
    public static async UniTask<AudioClip> LoadWavExternal(string path)
    {
        await UniTask.SwitchToMainThread();
        Uri.TryCreate(path, UriKind.Absolute, out Uri uri);
        UnityWebRequest uwr = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.WAV);
        await uwr.SendWebRequest();
        if (uwr.error != null) throw new ArgumentException();
        AudioClip audioClip = DownloadHandlerAudioClip.GetContent(uwr);
        return audioClip;
    }
}
