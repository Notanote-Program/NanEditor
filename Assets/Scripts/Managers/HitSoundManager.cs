#if UNITY_EDITOR || !UNITY_ANDROID || true
#define DISABLE_NATIVE_AUDIO
#endif
#define USE_MA_AUDIO
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Notanote.Others;
using UnityEngine;
#if !DISABLE_NATIVE_AUDIO || USE_MA_AUDIO
using Cysharp.Threading.Tasks;
#endif

#if USE_MA_AUDIO
using MaTech.Audio;
#endif

	public class HitSoundManager : MonoSingleton<HitSoundManager>
	{
	    private static Dictionary<int, AudioSource[]> _unityAudios;
#if USE_MA_AUDIO
	        private static Dictionary<int, AudioSample[]> _maAudios;
#endif
	    private static Dictionary<int, int> _audioIndexes;
	
	    private static float _hitSoundVolume = 1f;
	
	    [SerializeField] private AudioClip[] hitSounds;
	    [SerializeField] private int[] hitSoundsCount;
	    private bool inited = false;
	
	    protected override void OnAwake()
	    {
#if USE_MA_AUDIO
	            InitMaAudio().Forget();
#else
	            InitUnityAudio();
#endif
	    }
	
	    public void UpdateVolume(float volume)
	    {
	        _hitSoundVolume = volume;
	    }
	
	    private void InitUnityAudio()
	    {
	        _unityAudios = new Dictionary<int, AudioSource[]>();
	        _audioIndexes = new Dictionary<int, int>();
	
	        foreach (AudioSource[] audioSources in _unityAudios.Values)
	        {
	            foreach (AudioSource audioSource in audioSources)
	            {
	                Destroy(audioSource.gameObject);
	            }
	        }
	        
	        _unityAudios.Clear();
	        _audioIndexes.Clear();
	        
	        for (var i = 0; i < hitSounds.Length; i++)
	        {
	            _unityAudios.Add(i, new AudioSource[hitSoundsCount[i]]);
	            _audioIndexes.Add(i, 0);
	            for (var j = 0; j < hitSoundsCount[i]; j++)
	            {
	                var obj = new GameObject($"Unity Audio - HitSound {i}-{j}");
	                obj.transform.SetParent(transform);
	                obj.transform.position = new Vector3(0, 0, -10);
	                var comp = obj.AddComponent<AudioSource>();
	                comp.loop = false;
	                comp.playOnAwake = false;
	                comp.clip = hitSounds[i];
	                _unityAudios[i][j] = comp;
	            }
	        }
	    }
	
#if USE_MA_AUDIO
	        private async UniTaskVoid InitMaAudio()
	        {
	            _maAudios = new Dictionary<int, AudioSample[]>();
	            _audioIndexes = new Dictionary<int, int>();
	
	            MaAudio.LoadForUnity();
	            await RefreshMaAudio();
	        }
#endif
	
	    public async void RefreshHitSounds(AudioClip tap, AudioClip drag)
	    {
	        Resources.UnloadUnusedAssets();
	        hitSounds[0] = tap;
	        hitSounds[1] = drag;
#if USE_MA_AUDIO
		    await RefreshMaAudio();
#else
	        RefreshUnityAudio();
#endif
	    }
	    
	    private void RefreshUnityAudio()
	    {
	        for (int i = 0; i < _unityAudios.Count; i++)
	        {
	            AudioSource[] audioSources = _unityAudios[i];
	            foreach (AudioSource audioSource in audioSources)
	            {
	                audioSource.clip = hitSounds[i];
	            }
	        }
	    }
	
#if USE_MA_AUDIO
	        private async UniTask RefreshMaAudio()
	        {
	            _maAudios.Clear();
	            _audioIndexes.Clear();
	
	            for (int i = 0; i < hitSounds.Length; i++)
	            {
	                _maAudios.Add(i, new AudioSample[hitSoundsCount[i]]);
	                _audioIndexes.Add(i, 0);
	                for (var j = 0; j < hitSoundsCount[i]; j++)
	                {
	                    if (!hitSounds[i]) continue;
	                    if (_maAudios[i][j] != null) _maAudios[i][j].Unload();
	                    _maAudios[i][j] = await AudioSample.LoadFromAudioClip(hitSounds[i]);
	
	                    if (_maAudios[i][j] == null) continue;
	                    _maAudios[i][j].Volume = _hitSoundVolume;
	                }
	            }
	        }
#endif
	
	    // private string UrlEncodePath(string path)
	    // {
	    //     string str = "file://" + string.Join("/", path.Replace("\\", "/").Split('/').Select(UrlEncode));
	    //     Debug.Log(str);
	    //     return str;
	    // }
	    //
	    // private string UrlEncode(string str)
	    // {
	    //     StringBuilder stringBuilder = new StringBuilder();
	    //     TextElementEnumerator textElementEnumerator = StringInfo.GetTextElementEnumerator(str);
	    //     UTF8Encoding utf8Encoding = new UTF8Encoding(false);
	    //     while (textElementEnumerator.MoveNext())
	    //     {
	    //         string qwq = textElementEnumerator.GetTextElement();
	    //         if (qwq.Length == 1)
	    //         {
	    //             char c = qwq[0];
	    //             if (c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9' or '-' or '_' or '.' or '~')
	    //             {
	    //                 stringBuilder.Append(c);
	    //                 continue;
	    //             }
	    //         }
	    //
	    //         stringBuilder.Append(string.Join("",
	    //             utf8Encoding.GetBytes(qwq).Select(b => "%" + Convert.ToString(b, 16))));
	    //     }
	    //
	    //     return stringBuilder.ToString();
	    // }
	
	    public void Play(int soundIndex, float rewriteVolume = -1)
	    {
	        var orgVlm = _hitSoundVolume;
	        if (rewriteVolume >= 0) _hitSoundVolume = rewriteVolume;
	
#if USE_MA_AUDIO
		    PlayByMaAudio(soundIndex);
#else
	        PlayByUnityAudio(soundIndex);
#endif
	
	        _hitSoundVolume = orgVlm;
	    }
	    
	    private void PlayByUnityAudio(int soundIndex)
	    {
	        if (_hitSoundVolume <= 0.01f) return;
	
	        var index = _audioIndexes[soundIndex] + 1;
	        if (index >= hitSoundsCount[soundIndex]) index = 0;
	        var source = _unityAudios[soundIndex][index];
	        _audioIndexes[soundIndex] = index;
	
	        // Debug.Log(_hitSoundVolume);
	        // Debug.Log(source.clip.samples);
	        float[] f = new float[source.clip.channels * source.clip.samples];
	        source.clip.GetData(f, 0);
	        // Debug.Log($"[{string.Join(", ", f.Take(Mathf.Min(f.Length, 20)))}]");
	        source.volume = _hitSoundVolume;
	        source.PlayScheduled(AudioSettings.dspTime);
	    }
	
#if USE_MA_AUDIO
	        private void PlayByMaAudio(int soundIndex)
	        {
	            if (_hitSoundVolume <= 0.01f) return;
	
	            var index = _audioIndexes[soundIndex] + 1;
	            if (index >= hitSoundsCount[soundIndex]) index = 0;
	            var source = _maAudios[soundIndex][index];
	            _audioIndexes[soundIndex] = index;
	
	            source.Volume = _hitSoundVolume;
	            source.Channel = (ushort)(soundIndex * 10 + index); // 自动分配音轨
	            source.PlayImmediate();
	        }
#endif
	}