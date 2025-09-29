using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private VisualElement root;
    private Button playButton;
    private Button settingsButton;
    private Button quitButton;

    void Start()
    {
        // Get the root visual element from the UI Document
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Find buttons by their names
        playButton = root.Q<Button>("play-button");
        settingsButton = root.Q<Button>("settings-button");
        quitButton = root.Q<Button>("quit-button");

        // Register button events (functionality can be added later)
        SetupButtonEvents();
    }

    private void SetupButtonEvents()
    {
        // Play button - will load game scene later
        playButton.clicked += OnPlayClicked;

        // Settings button - will open settings panel later
        settingsButton.clicked += OnSettingsClicked;

        // Quit button - will quit application
        quitButton.clicked += OnQuitClicked;
    }

    private void OnPlayClicked()
    {
        Debug.Log("Play button clicked!");
        // TODO: Load game scene or show player name input
        // SceneManager.LoadScene("GameScene");
    }

    private void OnSettingsClicked()
    {
        Debug.Log("Settings button clicked!");
        // TODO: Show settings panel
    }

    private void OnQuitClicked()
    {
        Debug.Log("Quit button clicked!");
        // TODO: Show quit confirmation or quit directly
        Application.Quit();

        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}