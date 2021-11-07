using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System;
using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    //gonna break these up later
    public enum ESound
    {
        BackgroundTrackSlowSomber,
        BackgroundTrackLively,
        WaterPickup,
        WaterDeposit,
        PuzzleCompleteSting,
        EagleString
    }

    [System.Serializable]
    public class SoundClip
    {
        [SerializeField] private string name;
        [SerializeField] private AudioManager.ESound sound;
        [SerializeField] private AudioClip clip;
        [SerializeField] private bool loop;

        [SerializeField] [Range(0f, 1f)] private float volume;
        [SerializeField] [Range(0.3f, 3f)] private float pitch;
        [SerializeField] [Range(0f, 500f)] private float minDistance;
        [SerializeField] [Range(0f, 500f)] private float maxDistance;


        [HideInInspector] private AudioSource _source;

        public string Name => name;
        public AudioManager.ESound Sound => sound;
        public AudioClip Clip => clip;
        public float Volume => volume;
        public float Pitch => pitch;
        public bool Loop => loop;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
        public AudioSource Source { get => _source; set => _source = value; }
    }

    [FormerlySerializedAs("OneShotSounds")] [SerializeField] private SoundClip[] oneShotSounds;
    [FormerlySerializedAs("BackGroundSounds")] [SerializeField] private SoundClip[] backGroundSounds;
    
    [SerializeField] public AudioZone[] AudioZones;

    //private Dictionary<ESound, float> soundTimerDictionary;
    private Dictionary<GameObject, Dictionary<ESound, float>> _gameObjectAudioTimerDictionary;
    private Dictionary<ESound, AudioSource> _audioSourceReferenceDictionary;
    private GameObject _oneShotGameObject;
    private AudioSource _oneShotAudioSource;
    private SoundClip _currentBackgroundTrack;
    private float _timeStamp;
    private float _volumeControlBackground;
    private float _volumeControlTimer;
    private bool _bgSwitching, _bgDown, _bgUp, _bgSwitchControl;
    private ESound _bgSoundSwitch;

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("There should never be 2 or more Audio Managers.");
        }
        Instance = this;
        _gameObjectAudioTimerDictionary = new Dictionary<GameObject, Dictionary<ESound, float>>();
        _audioSourceReferenceDictionary = new Dictionary<ESound, AudioSource>();

        foreach (var s in backGroundSounds)
        {
            s.Source = gameObject.AddComponent<AudioSource>();
            s.Source.clip = s.Clip;

            s.Source.volume = s.Volume;
            s.Source.pitch = s.Pitch;
            s.Source.loop = s.Loop;
        }

        _currentBackgroundTrack = Array.Find(backGroundSounds, soundClip => soundClip.Sound == ESound.BackgroundTrackSlowSomber);
        _currentBackgroundTrack.Source.Play();

        foreach (var az in AudioZones)
        {
            az.SetPuzzleStatus(false);
        }

        // legacy code
        //soundTimerDictionary = new Dictionary<ESound, float>();
        //soundTimerDictionary[ESound.Player_Hover] = 0f;
        //soundTimerDictionary[ESound.Mining] = 0f;
        //soundTimerDictionary[ESound.Damage_To_Building] = 0f;
        //soundTimerDictionary[ESound.Alien_Moves] = 0f;

        //foreach (KeyValuePair<ESound,float> es in soundTimerDictionary){
        //    Debug.Log(es.Key + " : " + es.Value);
        //}

        _bgSwitching = false;
        _bgDown = false;
        _bgUp = false;
        _bgSwitchControl = false;
    }

    private void Update()
    {
        if (_bgSwitching)
        {
            CheckBackgroundSound();
        }
    }

    public void PlayBackground(ESound sound)
    {
        var s = Array.Find(backGroundSounds, soundClip => soundClip.Sound == sound);
        if (s == null)
        {
            Debug.LogWarning("Sound: " + sound + " not found.");
            return;
        }
        s.Source.Play();
    }

    public void PlaySound(ESound sound, GameObject obj)
    {
        if (!ContainsSound(obj, sound))
        {
            var tmp = true;
            var sources = obj.GetComponents<AudioSource>();
            foreach (var source in sources)
            {
                _oneShotAudioSource = source;
                if (source.clip != GetAudio(sound).Clip) continue;
                tmp = false;
                break;
            }
            if (tmp)
            {
                var audioSource = obj.AddComponent<AudioSource>();
                var s = GetAudio(sound);
                audioSource.clip = s.Clip;
                audioSource.volume = s.Volume;
                audioSource.pitch = s.Pitch;
                audioSource.loop = s.Loop;
                audioSource.spatialBlend = 1f;
                audioSource.rolloffMode = AudioRolloffMode.Linear;
                audioSource.minDistance = s.MinDistance;
                audioSource.maxDistance = s.MaxDistance;
                if (_gameObjectAudioTimerDictionary.ContainsKey(obj))
                {
                    if (_gameObjectAudioTimerDictionary[obj].ContainsKey(sound))
                    {
                        _gameObjectAudioTimerDictionary[obj][sound] = Time.time;
                    }
                    else
                    {
                        _gameObjectAudioTimerDictionary[obj].Add(sound, Time.time);
                    }
                }
                else
                {
                    _gameObjectAudioTimerDictionary.Add(obj, new Dictionary<ESound, float>());
                    _gameObjectAudioTimerDictionary[obj].Add(sound, Time.time);
                }
            }
        }

        if (!CanPlaySound(obj, sound)) return;
        {
            var sources = obj.GetComponents<AudioSource>();
            foreach (var s in sources)
            {
                if (s.clip == GetAudio(sound).Clip)
                {
                    s.Play();
                }
            }
        }
    }


    public void PlaySound(ESound sound)
    {
        if (!_audioSourceReferenceDictionary.ContainsKey(sound))
        {
            var s = GetAudio(sound);
            s.Source = this.gameObject.AddComponent<AudioSource>();
            s.Source.clip = s.Clip;
            s.Source.volume = s.Volume;
            s.Source.pitch = s.Pitch;
            s.Source.loop = s.Loop;
            _audioSourceReferenceDictionary.Add(sound, s.Source);
        }
        _audioSourceReferenceDictionary[sound].PlayOneShot(GetAudio(sound).Clip);
    }

    public void StopSound(ESound sound)
    {
        var sources = this.gameObject.GetComponents<AudioSource>();
        if (sources.Length != 0)
        {
            foreach (var source in sources)
            {
                if (source.clip != GetAudio(sound).Clip) continue;
                source.Stop();
                break;
            }
        }
        else
        {
            Debug.LogError("The object has no audio sources!");
        }
    }

    public void StopSound(ESound sound, GameObject obj)
    {
        var sources = obj.GetComponents<AudioSource>();
        if (sources.Length != 0)
        {
            foreach (var source in sources)
            {
                if (source.clip != GetAudio(sound).Clip) continue;
                source.Stop();
                break;
            }
        }
        else
        {
            Debug.LogError("The object has no audio sources!");
        }
    }


    private SoundClip GetAudio(ESound sound)
    {
        var s = Array.Find(oneShotSounds, soundClip => soundClip.Sound == sound);
        return s ?? null;
    }

    private bool ContainsSound(GameObject obj, ESound sound)
    {
        if (Array.Find(oneShotSounds, soundClip => soundClip.Sound == sound) != null)
        {
            return _gameObjectAudioTimerDictionary.ContainsKey(obj) && _gameObjectAudioTimerDictionary[obj].ContainsKey(sound);
        }
        else
        {
            return false;
        }
    }

    private bool CanPlaySound(GameObject obj, ESound sound)
    {
        if (Array.Find(oneShotSounds, soundClip => soundClip.Sound == sound) != null)
        {
            switch (sound)
            {
                /*case ESound.Player_Hover:
                    if (_gameObjectAudioTimerDictionary.ContainsKey(obj))
                    {
                        var tmp = false;
                        var sources = obj.GetComponents<AudioSource>();
                        foreach (var s in sources)
                        {
                            if (s.clip != GetAudio(sound).Clip) continue;
                            if (!s.isPlaying)
                            {
                                s.Play();
                            }
                            else
                            {
                                tmp = true;
                                break;
                            }
                        }
                        if (_gameObjectAudioTimerDictionary[obj].ContainsKey(sound) && tmp)
                        {
                            var lastTimePlayed = _gameObjectAudioTimerDictionary[obj][sound];
                            var delay = GetAudio(sound).Clip.length;
                            //if (Time.time == 0f)
                            if (lastTimePlayed + delay < Time.time)
                            {
                                _gameObjectAudioTimerDictionary[obj][sound] = Time.time;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }*/
                /*case ESound.Mining:
                    if (_gameObjectAudioTimerDictionary.ContainsKey(obj))
                    {
                        var tmp = false;
                        var sources = obj.GetComponents<AudioSource>();
                        foreach (var s in sources)
                        {
                            if (s.clip != GetAudio(sound).Clip) continue;
                            if (!s.isPlaying)
                            {
                                s.Play();
                            }
                            else
                            {
                                tmp = true;
                                break;
                            }
                        }
                        if (_gameObjectAudioTimerDictionary[obj].ContainsKey(sound) && tmp)
                        {
                            var lastTimePlayed = _gameObjectAudioTimerDictionary[obj][sound];
                            var delay = GetAudio(sound).Clip.length;
                            //if (Time.time == 0f)
                            if (lastTimePlayed + delay < Time.time)
                            {
                                _gameObjectAudioTimerDictionary[obj][sound] = Time.time;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }*/
                /*case ESound.Alien_Moves:
                    if (_gameObjectAudioTimerDictionary.ContainsKey(obj))
                    {
                        var tmp = false;
                        var sources = obj.GetComponents<AudioSource>();
                        foreach (var s in sources)
                        {
                            if (s.clip == GetAudio(sound).Clip)
                            {
                                if (!s.isPlaying)
                                {
                                    s.Play();
                                }
                                else
                                {
                                    tmp = true;
                                    break;
                                }
                            }
                        }
                        if (_gameObjectAudioTimerDictionary[obj].ContainsKey(sound) && tmp)
                        {
                            var lastTimePlayed = _gameObjectAudioTimerDictionary[obj][sound];
                            var delay = GetAudio(sound).Clip.length;
                            //if (Time.time == 0f)
                            if (lastTimePlayed + delay < Time.time)
                            {
                                _gameObjectAudioTimerDictionary[obj][sound] = Time.time;
                                return true;
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true;
                    }*/
                default:
                    return true;
            }

        }
        else
        {
            Debug.Log("Sound: " + sound.ToString() + " cannot be found");
            return false;
        }
    }

    public void SwitchBackgroundTrack(ESound sound)
    {
        if (_currentBackgroundTrack.Sound == sound) return;
        _bgSoundSwitch = sound;
        _volumeControlTimer = 0;
        _timeStamp = Time.time;
        _volumeControlBackground = _currentBackgroundTrack.Source.volume;

        _bgDown = true;
        _bgSwitching = true;
    }

    private void CheckBackgroundSound()
    {
        if (Time.time >= _timeStamp + 1f && !_bgSwitchControl)
        {
            _bgSwitchControl = true;
            _currentBackgroundTrack.Source.Stop();
            
            _currentBackgroundTrack.Source.volume = _volumeControlBackground;

            _currentBackgroundTrack = Array.Find(backGroundSounds, soundClip => soundClip.Sound == _bgSoundSwitch);

            _bgDown = false;
            _bgUp = true;
            _volumeControlTimer = 0;
            _volumeControlBackground = _currentBackgroundTrack.Source.volume;
            _currentBackgroundTrack.Source.volume = 0;

            _currentBackgroundTrack.Source.Play();
        }
        else if (Time.time >= _timeStamp + 2f)
        {
            _timeStamp = 0f;
            _bgSwitchControl = false;
            _bgUp = false;
            _bgSwitching = false;
        }

        if (_bgDown)
        {
            VolumeControlDown();
        }
        else if (_bgUp)
        {
            VolumeControlUp();
        }
    }

    private void VolumeControlDown()
    {
        _volumeControlTimer += Time.deltaTime;
        _currentBackgroundTrack.Source.volume = Mathf.Lerp(_volumeControlBackground, 0f, _volumeControlTimer);
    }

    private void VolumeControlUp()
    {
        _volumeControlTimer += Time.deltaTime;
        _currentBackgroundTrack.Source.volume = Mathf.Lerp(0f, _volumeControlBackground, _volumeControlTimer);
    }


}
