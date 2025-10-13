using UnityEngine;

/// <summary>
/// Simple FPS counter that displays in the top-right corner
/// Visibility controlled by GameSettings
/// </summary>
public class FPSCounter : MonoBehaviour
{
    private float deltaTime = 0.0f;
    private GUIStyle style;
    private bool isVisible = false;
    private Rect rect;

    private void Awake()
    {
        // Subscribe to settings changes
        GameSettings.OnFPSCounterChanged += UpdateVisibility;
        UpdateVisibility();
    }

    private void Start()
    {
        // Setup GUI style
        style = new GUIStyle();
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = 16;
        style.normal.textColor = Color.yellow;
        style.fontStyle = FontStyle.Bold;

        // Position in top-right corner
        int w = Screen.width, h = Screen.height;
        rect = new Rect(w - 100, 10, 90, 30);
    }

    private void Update()
    {
        // Calculate FPS
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
    }

    private void OnGUI()
    {
        if (!isVisible) return;

        // Update rect position in case of resolution change
        int w = Screen.width;
        rect.x = w - 100;

        float fps = 1.0f / deltaTime;
        string text = $"{Mathf.Ceil(fps)} FPS";

        // Draw outline for better visibility
        GUI.color = Color.black;
        GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, style);

        GUI.color = Color.white;
        GUI.Label(rect, text, style);
    }

    private void UpdateVisibility()
    {
        isVisible = GameSettings.ShowFPSCounter;
    }

    private void OnDestroy()
    {
        GameSettings.OnFPSCounterChanged -= UpdateVisibility;
    }
}
