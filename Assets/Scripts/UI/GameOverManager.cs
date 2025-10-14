using UnityEngine;
using TMPro;
using Fusion;
using UnityEngine.UI;

public class GameOverManager : NetworkBehaviour
{
    [Header("Prefab Reference")]
    [SerializeField] private GameObject gameOverCanvasPrefab;

    private GameObject gameOverPanel;
    private TextMeshProUGUI winnerText;
    private Button mainMenuButton;
    private GameObject canvasInstance;

    [Networked] private bool gameEnded { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            CreateGameOverUI();
        }
    }

    private void CreateGameOverUI()
    {
        if (gameOverCanvasPrefab != null && canvasInstance == null)
        {
            canvasInstance = Instantiate(gameOverCanvasPrefab);
            DontDestroyOnLoad(canvasInstance);

            Debug.Log("Canvas instanciado: " + (canvasInstance != null));

            gameOverPanel = canvasInstance.transform.Find("GameOverPanel")?.gameObject;
            Debug.Log("GameOverPanel encontrado: " + (gameOverPanel != null));

            if (gameOverPanel == null)
            {
                gameOverPanel = FindInChildren(canvasInstance, "GameOverPanel");
                Debug.Log("GameOverPanel (recursivo): " + (gameOverPanel != null));
            }

            winnerText = canvasInstance.transform.Find("WinnerText")?.GetComponent<TextMeshProUGUI>();
            if (winnerText == null)
            {
                winnerText = FindInChildren<TextMeshProUGUI>(canvasInstance, "WinnerText");
            }
            Debug.Log("WinnerText encontrado: " + (winnerText != null));

            mainMenuButton = canvasInstance.transform.Find("MainMenuButton")?.GetComponent<Button>();
            if (mainMenuButton == null)
            {
                mainMenuButton = FindInChildren<Button>(canvasInstance, "MainMenuButton");
            }
            Debug.Log("MainMenuButton encontrado: " + (mainMenuButton != null));

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);
            else
                Debug.LogError("No se encontró GameOverPanel en el canvas");
        }
        else
        {
            Debug.LogError("gameOverCanvasPrefab es null o canvasInstance ya existe");
        }
    }

    private T FindInChildren<T>(GameObject parent, string name) where T : Component
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
                return child.GetComponent<T>();
            
            T foundInGrandchild = FindInChildren<T>(child.gameObject, name);
            if (foundInGrandchild != null)
                return foundInGrandchild;
        }
        return null;
    }

    private GameObject FindInChildren(GameObject parent, string name)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.name == name)
                return child.gameObject;
            
            GameObject foundInGrandchild = FindInChildren(child.gameObject, name);
            if (foundInGrandchild != null)
                return foundInGrandchild;
        }
        return null;
    }

    public void EndGame(string winnerName)
    {
        if (gameEnded) return;
        
        gameEnded = true;
        Debug.Log("EndGame llamado para: " + winnerName);
        RPC_ShowGameOver(winnerName);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowGameOver(string winnerName)
    {
        Debug.Log("RPC_ShowGameOver recibido: " + winnerName);
        ShowGameOverPanel(winnerName);
    }

    private void ShowGameOverPanel(string winnerName)
    {
        Debug.Log("ShowGameOverPanel llamado: " + winnerName);

        if (canvasInstance == null && gameOverCanvasPrefab != null)
        {
            CreateGameOverUI();
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("GameOverPanel activado");

            if (winnerText != null)
            {
                winnerText.text = $"CONGRATULATIONS!\n\n{winnerName} WINS!";
                winnerText.color = Color.yellow;
                Debug.Log("Texto actualizado: " + winnerText.text);
            }
            else
            {
                Debug.LogError("WinnerText es null");
            }
        }
        else
        {
            Debug.LogError("GameOverPanel es null");
        }
        
        UnlockCursor();
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Debug.Log("Cursor liberado");
    }

    private void ReturnToMainMenu()
    {
        Debug.Log("Volviendo al menú principal");
        if (canvasInstance != null)
            Destroy(canvasInstance);
            
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public static void TriggerGameOver(string winnerName)
    {
        Debug.Log("TriggerGameOver llamado: " + winnerName);
        GameOverManager manager = FindFirstObjectByType<GameOverManager>();
        if (manager != null)
        {
            manager.EndGame(winnerName);
        }
        else
        {
            Debug.LogError("No se encontró GameOverManager en la escena");
        }
    }

    private void OnDestroy()
    {
        if (canvasInstance != null)
            Destroy(canvasInstance);
    }
}