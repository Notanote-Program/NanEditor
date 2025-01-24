using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MaTech.Audio;

	public class HitSoundManager : MonoSingleton<HitSoundManager>
	{
	    private static Dictionary<int, AudioSource[]> _unityAudios;
	    private static Dictionary<int, AudioSample[]> _maAudios;
	    private static Dictionary<int, int> _audioIndexes;
	
	    private static float _hitSoundVolume = 1f;
	
	    [SerializeField] private AudioClip[] hitSounds;
	    [SerializeField] private int[] hitSoundsCount;
	    private bool inited = false;
	
	    protected override void OnAwake()
	    {
		    InitMaAudio().Forget();
	    }
	
	    public void UpdateVolume(float volume)
	    {
	        _hitSoundVolume = volume;
	    }

	    private async UniTaskVoid InitMaAudio()
	    {
		    _maAudios = new Dictionary<int, AudioSample[]>();
		    _audioIndexes = new Dictionary<int, int>();
	
		    MaAudio.LoadForUnity();
		    await RefreshMaAudio();
	    }
	
	    public async void RefreshHitSounds(AudioClip tap, AudioClip drag)
	    {
	        Resources.UnloadUnusedAssets();
	        hitSounds[0] = tap;
	        hitSounds[1] = drag;
	        await RefreshMaAudio();
	    }
	    
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
	
	    public void Play(int soundIndex, float rewriteVolume = -1)
	    {
	        var orgVlm = _hitSoundVolume;
	        if (rewriteVolume >= 0) _hitSoundVolume = rewriteVolume;
	
	        PlayByMaAudio(soundIndex);
	
	        _hitSoundVolume = orgVlm;
	    }
	    
	    private void PlayByUnityAudio(int soundIndex)
	    {
	        if (_hitSoundVolume <= 0.01f) return;
	
	        var index = _audioIndexes[soundIndex] + 1;
	        if (index >= hitSoundsCount[soundIndex]) index = 0;
	        var source = _unityAudios[soundIndex][index];
	        _audioIndexes[soundIndex] = index;
	        
	        float[] f = new float[source.clip.channels * source.clip.samples];
	        source.clip.GetData(f, 0);
	        source.volume = _hitSoundVolume;
	        source.PlayScheduled(AudioSettings.dspTime);
	    }
	
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
	}