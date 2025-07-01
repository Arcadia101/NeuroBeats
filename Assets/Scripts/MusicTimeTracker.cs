using FMOD;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

/// <summary>
/// Singleton that tracks and exposes the current playback time of a specified FMOD event.
/// Use this to synchronize game logic with the audio timeline.
/// </summary>
public class MusicTimeTracker : MonoBehaviour
{
    public static MusicTimeTracker Instance { get; private set; }

    [Header("FMOD")]  
    [Tooltip("FMOD event to track. Assign your main music event here.")]
    [SerializeField]
    private EventReference fmodEvent;

    private EventInstance eventInstance;
    private bool isPlaying = false;

    /// <summary>
    /// Current playback time of the FMOD event, in seconds.
    /// </summary>
    public float CurrentSongTime { get; private set; }

    private void Awake()
    {
        // Ensure only one instance exists (Singleton)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Create and start the FMOD event instance
        eventInstance = RuntimeManager.CreateInstance(fmodEvent);
        eventInstance.start();
        isPlaying = true;
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        // Query the FMOD instance for its current timeline position (in milliseconds)
        int ms;
        RESULT result = eventInstance.getTimelinePosition(out ms);
        if (result == RESULT.OK)
        {
            // Convert ms to seconds and store
            CurrentSongTime = ms / 1000f;
        }
    }

    private void OnDestroy()
    {
        if (isPlaying)
        {
            // Stop and release FMOD event when this object is destroyed
            eventInstance.stop(STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }
}