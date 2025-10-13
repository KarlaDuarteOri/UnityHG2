using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Controls the in-game pause menu opened with ESC key
/// Handles Resume, Settings, and Leave Game functionality
/// </summary>
public class PauseMenuController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference pauseAction;

    private VisualElement root;
    private VisualElement pauseMenu;
    private VisualElement settingsPanel;

    // Pause menu buttons
    private Button resumeButton;
    private Button settingsButton;
    private Button leaveButton;
    private Button closeSettingsButton;

    // Settings UI elements
    private DropdownField resolutionDropdown;
    private DropdownField framerateDropdown;
    private Toggle fullscreenToggle;
    private Toggle fpsCounterToggle;
    private Slider masterVolumeSlider;
    private Slider mouseSensitivitySlider;
    private Slider fovSlider;
    private Label masterVolumeValue;
    private Label mouseSensitivityValue;
    private Label fovValue;

    private GameSettings gameSettings;
    private bool isPaused = false;
    private NetworkConnectionHandler networkHandler;

    private void Awake()
    {
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Find pause menu elements
        pauseMenu = root.Q<VisualElement>("pause-menu");
        settingsPanel = root.Q<VisualElement>("pause-settings-panel");

        resumeButton = root.Q<Button>("resume-button");
        settingsButton = root.Q<Button>("pause-settings-button");
        leaveButton = root.Q<Button>("leave-button");
        closeSettingsButton = root.Q<Button>("close-pause-settings-button");

        // Find settings elements
        resolutionDropdown = root.Q<DropdownField>("pause-resolution-dropdown");
        framerateDropdown = root.Q<DropdownField>("pause-framerate-dropdown");
        fullscreenToggle = root.Q<Toggle>("pause-fullscreen-toggle");
        fpsCounterToggle = root.Q<Toggle>("pause-fps-counter-toggle");
        masterVolumeSlider = root.Q<Slider>("pause-master-volume-slider");
        mouseSensitivitySlider = root.Q<Slider>("pause-mouse-sensitivity-slider");
        fovSlider = root.Q<Slider>("pause-fov-slider");
        masterVolumeValue = root.Q<Label>("pause-master-volume-value");
        mouseSensitivityValue = root.Q<Label>("pause-mouse-sensitivity-value");
        fovValue = root.Q<Label>("pause-fov-value");

        // Find GameSettings
        gameSettings = FindFirstObjectByType<GameSettings>();

        // Find network handler
        networkHandler = FindFirstObjectByType<NetworkConnectionHandler>();

        // Setup button events
        SetupEvents();

        // Initialize settings panel
        InitializeSettings();

        // Hide menu by default and ensure cursor is locked for gameplay
        pauseMenu.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.None;

        // Lock cursor at game start
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }

    private void OnEnable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed += OnPausePerformed;
            pauseAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (pauseAction != null)
        {
            pauseAction.action.performed -= OnPausePerformed;
            pauseAction.action.Disable();
        }
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"[PauseMenu] ESC pressed, isPaused={isPaused}");

        // Toggle pause
        if (isPaused)
        {
            // If settings is open, close settings instead of unpausing
            if (settingsPanel.style.display == DisplayStyle.Flex)
            {
                Debug.Log("[PauseMenu] Closing settings panel");
                CloseSettings();
            }
            else
            {
                Debug.Log("[PauseMenu] Calling Resume()");
                Resume();
            }
        }
        else
        {
            Pause();
        }
    }

    private void SetupEvents()
    {
        resumeButton.clicked += Resume;
        settingsButton.clicked += OpenSettings;
        leaveButton.clicked += LeaveGame;
        closeSettingsButton.clicked += CloseSettings;

        // Settings events
        masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
        mouseSensitivitySlider.RegisterValueChangedCallback(OnMouseSensitivityChanged);
        fovSlider.RegisterValueChangedCallback(OnFOVChanged);
        fullscreenToggle.RegisterValueChangedCallback(OnFullscreenChanged);
        fpsCounterToggle.RegisterValueChangedCallback(OnFPSCounterChanged);
        resolutionDropdown.RegisterValueChangedCallback(OnResolutionChanged);
        framerateDropdown.RegisterValueChangedCallback(OnFramerateChanged);
    }

    private void InitializeSettings()
    {
        // Initialize dropdowns
        resolutionDropdown.choices = new List<string>
        {
            "1920x1080", "1366x768", "1280x720", "1024x768", "800x600"
        };
        framerateDropdown.choices = new List<string>
        {
            "30", "60", "120", "Unlimited"
        };

        // Load current settings
        LoadCurrentSettings();
    }

    private void LoadCurrentSettings()
    {
        var settings = gameSettings.GetCurrentSettings();

        masterVolumeSlider.SetValueWithoutNotify(settings.masterVolume);
        UpdateVolumeLabel(settings.masterVolume);

        resolutionDropdown.SetValueWithoutNotify(settings.resolution);
        framerateDropdown.SetValueWithoutNotify(settings.framerate);
        fullscreenToggle.SetValueWithoutNotify(settings.fullscreen);

        mouseSensitivitySlider.SetValueWithoutNotify(settings.mouseSensitivity);
        UpdateMouseSensitivityLabel(settings.mouseSensitivity);

        fovSlider.SetValueWithoutNotify(settings.fieldOfView);
        UpdateFOVLabel(settings.fieldOfView);

        fpsCounterToggle.SetValueWithoutNotify(settings.showFPSCounter);
    }

    private void Pause()
    {
        isPaused = true;
        pauseMenu.style.display = DisplayStyle.Flex;

        // Unlock cursor for menu navigation
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;

        Debug.Log("[PauseMenu] Game paused - cursor unlocked");

        // Note: We don't pause time in multiplayer games
        // Time.timeScale = 0f;
    }

    private void Resume()
    {
        isPaused = false;
        pauseMenu.style.display = DisplayStyle.None;
        settingsPanel.style.display = DisplayStyle.None;

        // Lock cursor for gameplay - do this in a specific order to force Unity to comply
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;

        // Force cursor to center of screen (helps with locking)
        StartCoroutine(ForceCursorLockNextFrame());

        Debug.Log("[PauseMenu] Game resumed - cursor locked");

        // Time.timeScale = 1f;
    }

    private System.Collections.IEnumerator ForceCursorLockNextFrame()
    {
        // Wait a frame for UI to fully hide
        yield return null;

        // Force lock again to ensure it sticks
        UnityEngine.Cursor.visible = false;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }

    private void OpenSettings()
    {
        settingsPanel.style.display = DisplayStyle.Flex;
        LoadCurrentSettings(); // Refresh settings UI
    }

    private void CloseSettings()
    {
        settingsPanel.style.display = DisplayStyle.None;
    }

    private void LeaveGame()
    {
        // Disconnect from network
        if (networkHandler != null)
        {
            networkHandler.Disconnect();
        }

        // Return to main menu
        SceneManager.LoadScene("MainMenu");
    }

    // Settings event handlers
    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        UpdateVolumeLabel(evt.newValue);
        gameSettings.SetMasterVolume(evt.newValue);
    }

    private void OnMouseSensitivityChanged(ChangeEvent<float> evt)
    {
        UpdateMouseSensitivityLabel(evt.newValue);
        gameSettings.SetMouseSensitivity(evt.newValue);
    }

    private void OnFOVChanged(ChangeEvent<float> evt)
    {
        UpdateFOVLabel(evt.newValue);
        gameSettings.SetFieldOfView(evt.newValue);
    }

    private void OnFullscreenChanged(ChangeEvent<bool> evt)
    {
        gameSettings.SetFullscreen(evt.newValue);
    }

    private void OnFPSCounterChanged(ChangeEvent<bool> evt)
    {
        gameSettings.SetShowFPSCounter(evt.newValue);
    }

    private void OnResolutionChanged(ChangeEvent<string> evt)
    {
        gameSettings.SetResolution(evt.newValue);
    }

    private void OnFramerateChanged(ChangeEvent<string> evt)
    {
        gameSettings.SetFramerate(evt.newValue);
    }

    // Label update methods
    private void UpdateVolumeLabel(float value)
    {
        masterVolumeValue.text = $"{value:F0}%";
    }

    private void UpdateMouseSensitivityLabel(float value)
    {
        mouseSensitivityValue.text = $"{value:F1}";
    }

    private void UpdateFOVLabel(float value)
    {
        fovValue.text = $"{value:F0}°";
    }
}
