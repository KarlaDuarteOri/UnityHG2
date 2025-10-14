using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Linq;
using TMPro;

public class AlivePlayersCounter : NetworkBehaviour
{
    [Header("UI Reference")]
    public TextMeshProUGUI alivePlayersText;

    [Networked]
    private int aliveCount { get; set; }

    private ChangeDetector _changeDetector;

    [Networked]
    private int initialPlayerCount { get; set; } 

    public static AlivePlayersCounter Instance { get; private set; } 

    public override void Spawned()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        if (alivePlayersText == null)
            alivePlayersText = GameObject.Find("AlivePlayersText")?.GetComponent<TextMeshProUGUI>();

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            
            if (initialPlayerCount == 0)
            {
                initialPlayerCount = Runner.ActivePlayers.Count();
                aliveCount = initialPlayerCount;
            }
        }
        UpdateText();
    }

    public override void Render()
    {
        foreach (var change in _changeDetector.DetectChanges(this))
        {
            if (change == nameof(aliveCount))
            {
                UpdateText();
            }
        }
    }

    private void UpdateText()
    {
        if (alivePlayersText != null)
        {
            alivePlayersText.text = $"Alive: {aliveCount}";
        }
    }

    // Llamamos esta función cuando alguien muera
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_ReduceAliveCount()
    {
        if (aliveCount > 0)
        {
            aliveCount--;
            Debug.Log($"Jugador eliminado. Restantes: {aliveCount}");
        }
    }

    public void NotifyPlayerDeath()
    {
        RPC_ReduceAliveCount();
    }

    public int GetAliveCount()
    {
        return aliveCount;
    }

}