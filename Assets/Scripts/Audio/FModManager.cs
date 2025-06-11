using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using SubversionZero.Audio;

namespace SubversionZero.Audio
{
    public enum SfxKey
    {
        //for PlaySfx method
        PolaroidGrab,
        PolaroidAway,
        PolaroidPic,
        PolaroidHover,
        Door,
        CabinetOpen,
        CabinetClose,
        PlacePin,
        DestroyPin,
        UiHover,
        UiSelect,
        UiSlider,
        UiStart,
        //for PlayLoopingSfx method
        MovePin,
        SnowWind,
    }
    [Serializable]
    public struct SfxEntry
    {
        public SfxKey key;
        public EventReference eventPath;
    }

    [Serializable]
    public struct ParameterValue
    {
        public string name;
        public float value;
    }

    [Serializable]
    public struct LoopingSfxEntry
    {
        public SfxKey key;
        public EventReference eventPath;
        public List<ParameterValue> defaultParams; // Now supports multiple params
    }
}

public class FModManager : MonoBehaviour
{
    public static FModManager instance;
    private float currentClueValue = 0f;
    private Dictionary<SfxKey, EventInstance> _loopingSfx = new();

    [Serializable]
    public struct CharacterEvent
    {
        public Character character;
        public EventReference dialogueEventPath;
    }

    [Header("Per-Character FMOD Dialogue")]
    [SerializeField] List<CharacterEvent> characterEvents;
    Dictionary<Character, EventReference> _eventLookup;
    EventReference _currentEventPath;

    [Header("Pinboard Mode")]
    [Tooltip("Snapshot filter for pinboard")]
    [SerializeField] EventReference pinboardSnapshotEvent;

    [Tooltip("Any events to start when pinboard opens")]
    [SerializeField] List<EventReference> pinboardEnterEvents;

    [Tooltip("Any events to start when pinboard closes")]
    [SerializeField] List<EventReference> pinboardExitEvents;

    EventInstance _pinboardSnapshotInstance;
    readonly List<EventInstance> _pinboardInstances = new();

    [Serializable]
    public struct MusicTrack
    {
        public MusicState state;
        public EventReference musicEvent;
    }

    [Header("Music Tracks")]
    [SerializeField]
    List<MusicTrack> musicTracks;

    Dictionary<MusicState, EventReference> _musicLookup;
    EventInstance _currentMusicInstance;

    [Serializable]
    public struct RoomSnapshot
    {
        public string roomName;
        public EventReference snapshotEvent;
    }

    [Header("Room Snapshots")]
    [SerializeField] List<RoomSnapshot> roomSnapshots;

    Dictionary<string, EventReference> _roomSnapshotLookup;
    EventInstance _currentRoomSnapshotInstance;

    [Header("All SFX")]
    [SerializeField] List<SfxEntry> sfxEntries;
    Dictionary<SfxKey, EventReference> _sfxLookup;
    [SerializeField] List<LoopingSfxEntry> loopingEntries;
    Dictionary<SfxKey, LoopingSfxEntry> _loopingLookup;

    [Header("3D SFX")]
    [SerializeField] StudioEventEmitter drainEmitter;

    [Header("3D doors")]
    [SerializeField] StudioEventEmitter doorLivKitch;
    [SerializeField] StudioEventEmitter doorLivBath;
    [SerializeField] StudioEventEmitter doorLivBed;
    private Dictionary<string, StudioEventEmitter> _doorEmitterLookup;

    void Awake()
    {
        _sfxLookup = sfxEntries.ToDictionary(x => x.key, x => x.eventPath);
        _loopingLookup = loopingEntries.ToDictionary(x => x.key, x => x);
        _roomSnapshotLookup = roomSnapshots.ToDictionary(r => r.roomName, r => r.snapshotEvent);
        _doorEmitterLookup = new Dictionary<string, StudioEventEmitter>()
    {
        { "LivingRoom,Kitchen", doorLivKitch },
        { "LivingRoom,Bathroom", doorLivBath },
        { "LivingRoom,Bedroom",  doorLivBed },
        };
        instance = this;
        _eventLookup = characterEvents
            .ToDictionary(x => x.character, x => x.dialogueEventPath);
        _musicLookup = musicTracks
            .ToDictionary(x => x.state, x => x.musicEvent);

        // FModManager.instance.PlayMusic(MusicState.Investigation);

    }

    void Update()
        {
            // // Check if the E key is pressed
            // if (Input.GetKeyDown(KeyCode.E))
            // {
            //     FModManager.instance.PlayMusic(MusicState.Investigation);
            //     _currentMusicInstance.setParameterByName("EnoughClues", currentClueValue);
            //     Debug.Log($"EnoughClues set to: {currentClueValue}");
            // }
            // if (Input.GetKeyDown(KeyCode.R))
            // {
            //     FModManager.instance.PlayMusic(MusicState.Freeroam);
            // }

            // // Check if the F key is pressed
            // if (Input.GetKeyDown(KeyCode.F))
            // {
            //     currentClueValue = 1f - currentClueValue; // Toggle between 0 and 1
            //     if (_currentMusicInstance.isValid())
            //     {
            //         _currentMusicInstance.setParameterByName("EnoughClues", currentClueValue);
            //         Debug.Log($"EnoughClues set to: {currentClueValue}");
            //     }
            //     else
            //     {
            //         Debug.LogWarning("No active music instance to set parameter on.");
            //     }
            // }

            // Check if the W key is pressed
            // if (Input.GetKeyDown(KeyCode.W))
            // {
            //     FModManager.instance.StopDrain();
            //     Debug.Log("Stopping drain sound");
            // }
            // if (Input.GetKeyDown(KeyCode.E))
            // {
            //     FModManager.instance.PlayDrain();
            //     Debug.Log("Starting drain sound :o");
            // }
        }



    /// Call this when a new line of dialogue starts.
    public void StartDialogue(Character who)
    {
        if (_eventLookup.TryGetValue(who, out var path))
            _currentEventPath = path;
        else
            Debug.LogWarning($"[FMOD] No dialogue event for {who}");
    }

    /// <summary>Call this for each syllable with its exact type code.</summary>
    public void PlaySyllableSound(int syllableType)
    {
        if (_currentEventPath.IsNull)
        {
            Debug.LogWarning("[FMOD] No current event set.");
            return;
        }

        var inst = RuntimeManager.CreateInstance(_currentEventPath);
        // Always send the exact endType
        inst.setParameterByName("SyllableType", syllableType);
        inst.start();
        inst.release();
    }


    /// <summary>Stop tracking dialogue (optional cleanup).</summary>
    public void StopDialogue()
    {
        _currentEventPath = default;
    }

    /// <summary>Start pinboard snapshot + any enter-events.</summary>
    public void StartPinboard()
    {
        // 1) snapshot
        if (_pinboardSnapshotInstance.isValid())
        {
            _pinboardSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _pinboardSnapshotInstance.release();
        }

        _pinboardSnapshotInstance = RuntimeManager.CreateInstance(pinboardSnapshotEvent);
        _pinboardSnapshotInstance.start();

        // 2) clear old instances
        foreach (var inst in _pinboardInstances)
        {
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                inst.release();
            }
        }
        _pinboardInstances.Clear();

        // 3) start new enter-events
        foreach (var ev in pinboardEnterEvents)
        {
            var inst = RuntimeManager.CreateInstance(ev);
            inst.start();
            _pinboardInstances.Add(inst);
        }
    }

    /// <summary>Stop pinboard snapshot + all enter-events.</summary>
    public void StopPinboard()
    {
        // 1) stop snapshot (runs its release)
        if (_pinboardSnapshotInstance.isValid())
        {
            _pinboardSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _pinboardSnapshotInstance.release();
        }

        // 2) stop & release all enter-events
        foreach (var inst in _pinboardInstances)
        {
            if (inst.isValid())
            {
                inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                inst.release();
            }
        }
        _pinboardInstances.Clear();

        // 3) play exit-events 
        foreach (var ev in pinboardExitEvents)
        {
            var inst = RuntimeManager.CreateInstance(ev);
            inst.start();
            inst.release();
        }
    }

    /// Play or switch to the given music state, fading out any previous track.
    public void PlayMusic(MusicState state)
    {
        // Stop previous
        if (_currentMusicInstance.isValid())
            _currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

        // If no track for this state, bail
        if (!_musicLookup.TryGetValue(state, out var evt))
            return;

        // Start new
        _currentMusicInstance = RuntimeManager.CreateInstance(evt);
        _currentMusicInstance.start();
    }

    /// Stop whatever music is playing right now.
    public void StopMusic()
    {
        if (_currentMusicInstance.isValid())
        {
            _currentMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _currentMusicInstance.release();
        }
    }

    public void EnterRoom(string roomName)
    {
        if (_currentRoomSnapshotInstance.isValid())
        {
            _currentRoomSnapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _currentRoomSnapshotInstance.release();
        }

        if (_roomSnapshotLookup.TryGetValue(roomName, out var snapshotEvent))
        {
            _currentRoomSnapshotInstance = RuntimeManager.CreateInstance(snapshotEvent);
            _currentRoomSnapshotInstance.start();
            Debug.Log($"Switching room snapshot to '{roomName}'");
        }
        else
        {
            Debug.LogWarning($"[FMOD] No snapshot defined for room '{roomName}'");
        }
    }
    public void PlaySfx(SfxKey key)
    {
        if (!_sfxLookup.TryGetValue(key, out var path))
        {
            Debug.LogWarning($"[FMOD] No SFX registered for {key}");
            return;
        }
        RuntimeManager.PlayOneShot(path);
    }

    public void PlayLoopingSfx(SfxKey key)
    {
        if (_loopingSfx.ContainsKey(key) || !_loopingLookup.TryGetValue(key, out var entry))
            return;

        var inst = RuntimeManager.CreateInstance(entry.eventPath);

        if (entry.defaultParams != null)
        {
            foreach (var param in entry.defaultParams)
            {
                if (!string.IsNullOrEmpty(param.name))
                    inst.setParameterByName(param.name, param.value);
            }
        }

        inst.start();
        _loopingSfx[key] = inst;
    }
    /// <summary>Stops and releases a looping FMOD instance by key.</summary>
    public void StopLoopingSfx(SfxKey key)
    {
        if (_loopingSfx.TryGetValue(key, out var inst))
        {
            inst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            inst.release();
            _loopingSfx.Remove(key);
        }
    }

    public void SetLoopParameter(SfxKey key, string param, float value)
    {
        if (_loopingSfx.TryGetValue(key, out var inst))
            inst.setParameterByName(param, value);
    }

    public void PlayDoorSound(string connectingRooms)
    {
        if (_doorEmitterLookup.TryGetValue(connectingRooms, out var emitter) && emitter != null)
        {
            emitter.Play();
        }
        else
        {
            Debug.LogWarning($"[FMOD] No door emitter found for key '{connectingRooms}'");
        }
    }

    public void PlayDrain()
    {
        drainEmitter.Play();
    }
    public void StopDrain()
    {
        drainEmitter.Stop();
    }
    // public void Test()
    // {
    //     // 3) start new enter-events
    //     foreach (var ev in testEvents)
    //     {
    //         var inst = RuntimeManager.CreateInstance(ev);
    //         inst.start();
    //         _testInstances.Add(inst);
    //     }
    // }

}