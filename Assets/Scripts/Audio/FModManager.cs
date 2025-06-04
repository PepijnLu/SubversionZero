using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class FModManager : MonoBehaviour
{
    public static FModManager instance;
    private float currentClueValue = 0f;

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

    void Awake()
    {
        _roomSnapshotLookup = roomSnapshots.ToDictionary(r => r.roomName, r => r.snapshotEvent);

        instance = this;
        _eventLookup = characterEvents
            .ToDictionary(x => x.character, x => x.dialogueEventPath);
        _musicLookup = musicTracks
            .ToDictionary(x => x.state, x => x.musicEvent);

        FModManager.instance.PlayMusic(MusicState.Investigation);

    }

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

    void Update()
    {
        // Check if the E key is pressed
        if (Input.GetKeyDown(KeyCode.E))
        {
            FModManager.instance.PlayMusic(MusicState.Investigation);
            _currentMusicInstance.setParameterByName("EnoughClues", currentClueValue);
            Debug.Log($"EnoughClues set to: {currentClueValue}");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            FModManager.instance.PlayMusic(MusicState.Freeroam);
        }

        // Check if the F key is pressed
        if (Input.GetKeyDown(KeyCode.F))
        {
            currentClueValue = 1f - currentClueValue; // Toggle between 0 and 1
            if (_currentMusicInstance.isValid())
            {
                _currentMusicInstance.setParameterByName("EnoughClues", currentClueValue);
                Debug.Log($"EnoughClues set to: {currentClueValue}");
            }
            else
            {
                Debug.LogWarning("No active music instance to set parameter on.");
            }
        }

        // Check if the W key is pressed
        if (Input.GetKeyDown(KeyCode.W))
        {
            // FMODUnity.RuntimeManager.PlayOneShot("event:/sfx/tester");
        }
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

}