using UnityEngine;
using System;

/// <summary>
/// Singleton settings manager that handles all game settings
/// Persists settings using PlayerPrefs and applies them to Unity systems
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    // Settings Data
    [Serializable]
    public class SettingsData
    {
        public float masterVolume = 80f;
        public string resolution = "1920x1080";
        public bool fullscreen = true;
        public string framerate = "60";
        public float mouseSensitivity = 1.0f;
        public float fieldOfView = 90f;
        public bool showFPSCounter = false;
    }

    private SettingsData currentSettings = new SettingsData();

    // Public accessors for in-game use
    public static float MasterVolume => Instance?.currentSettings.masterVolume ?? 80f;
    public static string Resolution => Instance?.currentSettings.resolution ?? "1920x1080";
    public static bool Fullscreen => Instance?.currentSettings.fullscreen ?? true;
    public static string Framerate => Instance?.currentSettings.framerate ?? "60";
    public static float MouseSensitivity => Instance?.currentSettings.mouseSensitivity ?? 1.0f;
    public static float FieldOfView => Instance?.currentSettings.fieldOfView ?? 90f;
    public static bool ShowFPSCounter => Instance?.currentSettings.showFPSCounter ?? false;

    // Events for when settings change
    public static event Action OnSettingsChanged;
    public static event Action OnFOVChanged;
    public static event Action OnFPSCounterChanged;

    private const string SETTINGS_KEY = "GameSettings";

    private void Awake()
    {
        // Singleton pattern with DontDestroyOnLoad
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadSettings();
            ApplyAllSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Load settings from PlayerPrefs
    /// </summary>
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey(SETTINGS_KEY))
        {
            string json = PlayerPrefs.GetString(SETTINGS_KEY);
            currentSettings = JsonUtility.FromJson<SettingsData>(json);
            Debug.Log($"[GameSettings] Loaded settings: {json}");
        }
        else
        {
            Debug.Log("[GameSettings] No saved settings found, using defaults");
            SaveSettings(); // Save defaults
        }
    }

    /// <summary>
    /// Save settings to PlayerPrefs
    /// </summary>
    public void SaveSettings()
    {
        string json = JsonUtility.ToJson(currentSettings);
        PlayerPrefs.SetString(SETTINGS_KEY, json);
        PlayerPrefs.Save();
        Debug.Log($"[GameSettings] Saved settings: {json}");
    }

    /// <summary>
    /// Apply all settings to Unity systems
    /// </summary>
    public void ApplyAllSettings()
    {
        ApplyVolume();
        ApplyResolution();
        ApplyFramerate();
        OnSettingsChanged?.Invoke();
    }

    // Individual setting methods
    public void SetMasterVolume(float volume)
    {
        currentSettings.masterVolume = Mathf.Clamp(volume, 0f, 100f);
        ApplyVolume();
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetResolution(string resolution)
    {
        currentSettings.resolution = resolution;
        ApplyResolution();
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetFullscreen(bool fullscreen)
    {
        currentSettings.fullscreen = fullscreen;
        ApplyResolution(); // Fullscreen is applied together with resolution
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetFramerate(string framerate)
    {
        currentSettings.framerate = framerate;
        ApplyFramerate();
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        currentSettings.mouseSensitivity = Mathf.Clamp(sensitivity, 0.1f, 5.0f);
        SaveSettings();
        OnSettingsChanged?.Invoke();
    }

    public void SetFieldOfView(float fov)
    {
        currentSettings.fieldOfView = Mathf.Clamp(fov, 70f, 110f);
        SaveSettings();
        OnFOVChanged?.Invoke();
        OnSettingsChanged?.Invoke();
    }

    public void SetShowFPSCounter(bool show)
    {
        currentSettings.showFPSCounter = show;
        SaveSettings();
        OnFPSCounterChanged?.Invoke();
        OnSettingsChanged?.Invoke();
    }

    // Apply methods
    private void ApplyVolume()
    {
        AudioListener.volume = currentSettings.masterVolume / 100f;
        Debug.Log($"[GameSettings] Applied volume: {currentSettings.masterVolume}%");
    }

    private void ApplyResolution()
    {
        string[] parts = currentSettings.resolution.Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
        {
            FullScreenMode mode = currentSettings.fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            Screen.SetResolution(width, height, mode);
            Debug.Log($"[GameSettings] Applied resolution: {width}x{height}, Fullscreen: {currentSettings.fullscreen}");
        }
    }

    private void ApplyFramerate()
    {
        if (currentSettings.framerate == "Unlimited")
        {
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 0;
        }
        else if (int.TryParse(currentSettings.framerate, out int fps))
        {
            Application.targetFrameRate = fps;
            QualitySettings.vSyncCount = 0;
        }
        Debug.Log($"[GameSettings] Applied framerate: {currentSettings.framerate}");
    }

    // Get current settings for UI
    public SettingsData GetCurrentSettings()
    {
        return currentSettings;
    }
}
