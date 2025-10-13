using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    [Header("Network Settings")]
    [SerializeField] private GameObject networkRunnerPrefab;  // Assign in inspector
    private NetworkConnectionHandler networkHandler;

    private VisualElement root;
    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    // Player Name Field
    private TextField playerNameField;

    // Host/Join Panel Elements
    private VisualElement hostJoinPanel;
    private Button closeHostJoinButton;
    private VisualElement hostJoinChoice;
    private VisualElement joinRoomScreen;
    private Button hostButton;
    private Button joinButton;
    private Button joinRoomButton;
    private Button backToChoiceButton;
    private TextField roomCodeField;
    private Label connectionStatus;

    // Lobby Panel Elements
    private VisualElement lobbyPanel;
    private Button closeLobbyButton;
    private Label lobbyRoomCode;
    private Label playerCountLabel;
    private VisualElement playerList;
    private Button startGameButton;
    private Button leaveLobbyButton;

    // Settings Panel Elements
    private VisualElement settingsPanel;
    private Button closeSettingsButton;
    private DropdownField resolutionDropdown;
    private DropdownField framerateDropdown;
    private Slider masterVolumeSlider;
    private Slider mouseSensitivitySlider;
    private Slider fovSlider;
    private Label masterVolumeValue;
    private Label mouseSensitivityValue;
    private Label fovValue;

    void Start()
    {
        // Get the root visual element from the UI Document
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Find main menu elements
        playButton = root.Q<Button>("play-button");
        settingsButton = root.Q<Button>("settings-button");
        quitButton = root.Q<Button>("quit-button");

        // Find player name field
        playerNameField = root.Q<TextField>("player-name-field");

        // Find host/join panel elements
        hostJoinPanel = root.Q<VisualElement>("host-join-panel");
        closeHostJoinButton = root.Q<Button>("close-host-join-button");
        hostJoinChoice = root.Q<VisualElement>("host-join-choice");
        joinRoomScreen = root.Q<VisualElement>("join-room-screen");
        hostButton = root.Q<Button>("host-button");
        joinButton = root.Q<Button>("join-button");
        joinRoomButton = root.Q<Button>("join-room-button");
        backToChoiceButton = root.Q<Button>("back-to-choice-button");
        roomCodeField = root.Q<TextField>("room-code-field");
        connectionStatus = root.Q<Label>("connection-status");

        // Find lobby panel elements
        lobbyPanel = root.Q<VisualElement>("lobby-panel");
        closeLobbyButton = root.Q<Button>("close-lobby-button");
        lobbyRoomCode = root.Q<Label>("lobby-room-code");
        playerCountLabel = root.Q<Label>("player-count");
        playerList = root.Q<VisualElement>("player-list");
        startGameButton = root.Q<Button>("start-game-button");
        leaveLobbyButton = root.Q<Button>("leave-lobby-button");

        // Find settings panel elements
        settingsPanel = root.Q<VisualElement>("settings-panel");
        closeSettingsButton = root.Q<Button>("close-settings-button");
        resolutionDropdown = root.Q<DropdownField>("resolution-dropdown");
        framerateDropdown = root.Q<DropdownField>("framerate-dropdown");
        masterVolumeSlider = root.Q<Slider>("master-volume-slider");
        mouseSensitivitySlider = root.Q<Slider>("mouse-sensitivity-slider");
        fovSlider = root.Q<Slider>("fov-slider");
        masterVolumeValue = root.Q<Label>("master-volume-value");
        mouseSensitivityValue = root.Q<Label>("mouse-sensitivity-value");
        fovValue = root.Q<Label>("fov-value");

        // Setup events and initialize
        SetupButtonEvents();
        InitializeSettingsPanel();
    }

    private void SetupButtonEvents()
    {
        // Main menu buttons
        playButton.clicked += OnPlayClicked;
        settingsButton.clicked += OnSettingsClicked;
        quitButton.clicked += OnQuitClicked;

        // Host/Join panel buttons
        closeHostJoinButton.clicked += OnCloseHostJoinClicked;
        hostButton.clicked += OnHostClicked;
        joinButton.clicked += OnJoinClicked;
        joinRoomButton.clicked += OnJoinRoomClicked;
        backToChoiceButton.clicked += OnBackToChoiceClicked;

        // Lobby panel buttons
        closeLobbyButton.clicked += OnCloseLobbyClicked;
        startGameButton.clicked += OnStartGameClicked;
        leaveLobbyButton.clicked += OnLeaveLobbyClicked;

        // Settings panel buttons
        closeSettingsButton.clicked += OnCloseSettingsClicked;

        // Settings sliders events
        masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeChanged);
        mouseSensitivitySlider.RegisterValueChangedCallback(OnMouseSensitivityChanged);
        fovSlider.RegisterValueChangedCallback(OnFOVChanged);
    }

    private void InitializeSettingsPanel()
    {
        // Initialize Resolution dropdown
        resolutionDropdown.choices = new List<string>
        {
            "1920x1080",
            "1366x768",
            "1280x720",
            "1024x768",
            "800x600"
        };
        resolutionDropdown.value = "1920x1080";

        // Initialize Framerate dropdown
        framerateDropdown.choices = new List<string>
        {
            "30",
            "60",
            "120",
            "Unlimited"
        };
        framerateDropdown.value = "60";

        // Initialize slider values
        UpdateVolumeLabel(masterVolumeSlider.value);
        UpdateMouseSensitivityLabel(mouseSensitivitySlider.value);
        UpdateFOVLabel(fovSlider.value);

        // Load saved player name if exists
        string savedPlayerName = PlayerPrefs.GetString("PlayerName", "Player");
        playerNameField.value = savedPlayerName;

        // Register player name focus out event
        playerNameField.RegisterCallback<FocusOutEvent>(OnPlayerNameFocusLost);
    }

    private void OnPlayClicked()
    {
        Debug.Log($"[MainMenu] Play button clicked! Player name: {playerNameField.value}");

        // Validate player name
        if (string.IsNullOrWhiteSpace(playerNameField.value))
        {
            Debug.LogWarning("[MainMenu] Player name is empty!");
            playerNameField.value = "Player";
        }

        // Save player name before opening multiplayer panel
        PlayerPrefs.SetString("PlayerName", playerNameField.value);
        PlayerPrefs.Save();

        // Show Host/Join panel with choice screen
        hostJoinPanel.style.display = DisplayStyle.Flex;
        hostJoinChoice.style.display = DisplayStyle.Flex;
        joinRoomScreen.style.display = DisplayStyle.None;
        connectionStatus.text = "";
    }

    private void OnCloseHostJoinClicked()
    {
        hostJoinPanel.style.display = DisplayStyle.None;
        // Reset to choice screen
        hostJoinChoice.style.display = DisplayStyle.Flex;
        joinRoomScreen.style.display = DisplayStyle.None;
        connectionStatus.text = "";
    }

    private void OnHostClicked()
    {
        connectionStatus.text = "Starting host...";

        // Create NetworkRunner if it doesn't exist
        if (networkHandler == null)
        {
            if (networkRunnerPrefab != null)
            {
                GameObject runnerObj = Instantiate(networkRunnerPrefab);
                networkHandler = runnerObj.GetComponent<NetworkConnectionHandler>();
            }
            else
            {
                Debug.LogError("[MainMenu] NetworkRunner prefab not assigned!");
                connectionStatus.text = "Error: Network not configured!";
                return;
            }
        }

        // Generate a random room code
        string roomCode = GenerateRoomCode();
        Debug.Log($"[MainMenu] Starting as Host with room code: {roomCode}");

        // Start hosting and show lobby panel
        networkHandler.StartAsHost(roomCode);

        // Show lobby panel
        ShowLobby(roomCode, true);
    }

    private void OnJoinClicked()
    {
        // Hide choice screen, show join room screen
        hostJoinChoice.style.display = DisplayStyle.None;
        joinRoomScreen.style.display = DisplayStyle.Flex;
        connectionStatus.text = "";
        // Clear room code field
        roomCodeField.value = "";
    }

    private void OnJoinRoomClicked()
    {
        if (string.IsNullOrWhiteSpace(roomCodeField.value))
        {
            connectionStatus.text = "Please enter a room code!";
            return;
        }
        connectionStatus.text = "Joining room...";

        // Create NetworkRunner if it doesn't exist
        if (networkHandler == null)
        {
            if (networkRunnerPrefab != null)
            {
                GameObject runnerObj = Instantiate(networkRunnerPrefab);
                networkHandler = runnerObj.GetComponent<NetworkConnectionHandler>();
            }
            else
            {
                Debug.LogError("[MainMenu] NetworkRunner prefab not assigned!");
                connectionStatus.text = "Error: Network not configured!";
                return;
            }
        }

        // Join room and get room code back
        string roomCode = roomCodeField.value.ToUpper();
        networkHandler.StartAsClient(roomCode);

        // Show lobby panel (not host, so no START GAME button)
        ShowLobby(roomCode, false);
    }

    private void OnBackToChoiceClicked()
    {
        // Go back to choice screen
        joinRoomScreen.style.display = DisplayStyle.None;
        hostJoinChoice.style.display = DisplayStyle.Flex;
        connectionStatus.text = "";
        // Clear room code field
        roomCodeField.value = "";
    }

    private void OnSettingsClicked()
    {
        settingsPanel.style.display = DisplayStyle.Flex;
    }

    private void OnCloseSettingsClicked()
    {
        settingsPanel.style.display = DisplayStyle.None;
    }

    private void OnQuitClicked()
    {
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Settings event handlers
    private void OnMasterVolumeChanged(ChangeEvent<float> evt)
    {
        UpdateVolumeLabel(evt.newValue);
        // TODO: Apply volume change
    }

    private void OnMouseSensitivityChanged(ChangeEvent<float> evt)
    {
        UpdateMouseSensitivityLabel(evt.newValue);
        // TODO: Apply sensitivity change
    }

    private void OnFOVChanged(ChangeEvent<float> evt)
    {
        UpdateFOVLabel(evt.newValue);
        // TODO: Apply FOV change
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

    private void OnPlayerNameFocusLost(FocusOutEvent evt)
    {
        // Save player name when field loses focus
        PlayerPrefs.SetString("PlayerName", playerNameField.value);
        PlayerPrefs.Save();
    }

    // Lobby methods
    private void ShowLobby(string roomCode, bool isHost)
    {
        // Hide host/join panel
        hostJoinPanel.style.display = DisplayStyle.None;

        // Show lobby panel
        lobbyPanel.style.display = DisplayStyle.Flex;

        // Set room code
        lobbyRoomCode.text = roomCode;

        // Show START GAME button only for host
        startGameButton.style.display = isHost ? DisplayStyle.Flex : DisplayStyle.None;

        // Disable START GAME button until connection completes
        if (isHost)
        {
            startGameButton.SetEnabled(false);
            // Start checking for connection
            StartCoroutine(WaitForConnection());
        }

        // Initialize player list with just the local player
        UpdatePlayerList(new List<string> { playerNameField.value });
        UpdatePlayerCount(1, 12);
    }

    private System.Collections.IEnumerator WaitForConnection()
    {
        // Wait up to 10 seconds for connection
        float timeout = 10f;
        float elapsed = 0f;

        while (elapsed < timeout)
        {
            if (networkHandler != null && networkHandler.IsConnected())
            {
                // Connected! Enable the button
                startGameButton.SetEnabled(true);
                Debug.Log("[MainMenu] Connection ready - START GAME button enabled");
                yield break;
            }

            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        // Timeout - show error
        Debug.LogError("[MainMenu] Connection timeout! Could not connect to Photon servers.");
    }

    public void UpdatePlayerList(List<string> playerNames)
    {
        // Clear existing player list
        playerList.Clear();

        // Add each player to the list
        for (int i = 0; i < playerNames.Count; i++)
        {
            var playerItem = new VisualElement();
            playerItem.AddToClassList("player-list-item");

            var nameLabel = new Label(playerNames[i]);
            nameLabel.AddToClassList("player-name-label");

            playerItem.Add(nameLabel);

            // Add host badge to first player (host)
            if (i == 0)
            {
                var hostBadge = new Label("HOST");
                hostBadge.AddToClassList("player-host-badge");
                playerItem.Add(hostBadge);
            }

            playerList.Add(playerItem);
        }
    }

    public void UpdatePlayerCount(int current, int max)
    {
        playerCountLabel.text = $"Players: {current}/{max}";
    }

    private void OnStartGameClicked()
    {
        // Game auto-starts when hosting, this button won't actually be used
        Debug.Log("[MainMenu] Start game clicked (game already started)");
    }

    private void OnCloseLobbyClicked()
    {
        // Just hide the lobby panel without disconnecting
        lobbyPanel.style.display = DisplayStyle.None;
    }

    private void OnLeaveLobbyClicked()
    {
        // Disconnect from network
        if (networkHandler != null)
        {
            networkHandler.Disconnect();
        }

        // Hide lobby panel
        lobbyPanel.style.display = DisplayStyle.None;

        // Show host/join panel again (reset to choice screen)
        hostJoinPanel.style.display = DisplayStyle.Flex;
        hostJoinChoice.style.display = DisplayStyle.Flex;
        joinRoomScreen.style.display = DisplayStyle.None;
        connectionStatus.text = "";
    }

    private string GenerateRoomCode()
    {
        // Generate a 6-character alphanumeric room code
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] code = new char[6];

        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[Random.Range(0, chars.Length)];
        }

        return new string(code);
    }
}
