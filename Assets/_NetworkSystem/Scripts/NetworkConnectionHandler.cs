using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion;
using Fusion.Sockets;

public class NetworkConnectionHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Runner")]
    private NetworkRunner networkRunner;

    [Header("Player Settings")]
    [SerializeField] private NetworkPrefabRef playerPrefab;

    [Header("Session Settings")]
    [SerializeField] private string sessionName = "HungerGamesRoom";
    [SerializeField] private int maxPlayers = 12;

    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "GameScene";  // Scene to load when game starts

    private void Awake()
    {
        // Asegurarse de que solo haya una instancia
        DontDestroyOnLoad(gameObject);
        Debug.Log("[NetworkConnectionHandler] Ready to connect");
    }

    #region Public Methods - Llamados desde Botones UI

    /// <summary>
    /// Inicia el juego como HOST (servidor + cliente)
    /// El host puede jugar Y controla la sesión
    /// </summary>
    public async void StartAsHost(string roomCode = null)
    {
        Debug.Log("=== INICIANDO COMO HOST ===");

        // Use custom room code if provided, otherwise use default
        if (!string.IsNullOrEmpty(roomCode))
        {
            sessionName = roomCode;
        }

        await StartGame(GameMode.Host);
    }

    /// <summary>
    /// Inicia el juego como CLIENT (solo cliente)
    /// Se conecta a una sesión existente
    /// </summary>
    public async void StartAsClient(string roomCode)
    {
        Debug.Log("=== INICIANDO COMO CLIENT ===");

        if (string.IsNullOrEmpty(roomCode))
        {
            Debug.LogError("[NetworkConnectionHandler] Room code is required to join!");
            return;
        }

        sessionName = roomCode;
        await StartGame(GameMode.Client);
    }

    /// <summary>
    /// Desconectar y volver al menu
    /// </summary>
    public async void Disconnect()
    {
        if (networkRunner != null)
        {
            Debug.Log("=== DESCONECTANDO ===");
            await networkRunner.Shutdown();
            networkRunner = null;
        }

        // Only load MainMenu if we're not already there
        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            SceneManager.LoadScene("MainMenu");
        }
    }

    #endregion

    #region Game Start Logic

    private async Task StartGame(GameMode mode)
    {
        // Crear el NetworkRunner
        if (networkRunner == null)
        {
            networkRunner = gameObject.AddComponent<NetworkRunner>();
            networkRunner.name = "NetworkRunner";
        }

        // El runner provee input solo si es Host o Client
        networkRunner.ProvideInput = (mode == GameMode.Host || mode == GameMode.Client);

        // Get the game scene index
        int gameSceneIndex = GetSceneIndex(gameSceneName);

        if (gameSceneIndex == -1)
        {
            Debug.LogError($"[NetworkConnectionHandler] Scene '{gameSceneName}' not found in Build Settings!");
            return;
        }

        Debug.Log($"[NetworkConnectionHandler] Starting game - Mode: {mode}, Scene: {gameSceneName} (index {gameSceneIndex}), Session: {sessionName}");

        // Configurar los parámetros de inicio
        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            Scene = SceneRef.FromIndex(gameSceneIndex),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            PlayerCount = maxPlayers
        };

        // Iniciar el juego
        var result = await networkRunner.StartGame(startGameArgs);

        if (result.Ok)
        {
            Debug.Log($"✓ Game started successfully as {mode}");
            // Scene will be loaded automatically by Fusion
        }
        else
        {
            Debug.LogError($"✗ Failed to start game: {result.ShutdownReason}");
        }
    }

    /// <summary>
    /// Check if we're fully connected and ready to start
    /// </summary>
    public bool IsConnected()
    {
        return networkRunner != null && networkRunner.IsRunning;
    }

    /// <summary>
    /// Check if this player is the host
    /// </summary>
    public bool IsHost()
    {
        return networkRunner != null && networkRunner.IsServer;
    }

    private int GetSceneIndex(string sceneName)
    {
        // Find scene index in build settings by name
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);

            if (sceneNameFromPath == sceneName)
            {
                return i;
            }
        }
        return -1;  // Not found
    }

    #endregion

    #region INetworkRunnerCallbacks - Eventos de Red

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"✓ Player {player.PlayerId} joined the session");

        // Solo el servidor/host spawna jugadores
        if (runner.IsServer && playerPrefab != null)
        {
            // Calcular posición de spawn
            Vector3 spawnPosition = GetSpawnPosition(player.PlayerId);

            Debug.Log($"Spawneando jugador {player.PlayerId} en posición {spawnPosition}");

            // Spawn del jugador
            NetworkObject networkPlayer = runner.Spawn(
                playerPrefab,
                spawnPosition,
                Quaternion.identity,
                player
            );

            Debug.Log($"✓ Jugador spawneado: {networkPlayer.name}");
        }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"✗ Player {player.PlayerId} left the session");
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        Debug.Log("✓ Connected to server successfully");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
        Debug.LogWarning($"✗ Desconectado del servidor. Razón: {reason}");
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.LogError($"✗ Falló la conexión. Razón: {reason}");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"NetworkRunner apagado. Razón: {shutdownReason}");
        networkRunner = null;
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        // El input se maneja en FusionInputProvider
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        // Aceptar todas las conexiones
        request.Accept();
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

    #endregion

    #region Helper Methods

    private Vector3 GetSpawnPosition(int playerId)
    {
        // Spawns en círculo alrededor del origen
        float angle = playerId * (360f / maxPlayers);
        float radius = 5f;
        
        float x = Mathf.Cos(angle * Mathf.Deg2Rad) * radius;
        float z = Mathf.Sin(angle * Mathf.Deg2Rad) * radius;
        
        return new Vector3(x, 1f, z);
    }

    #endregion
}