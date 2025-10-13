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

    public override void Spawned()
    {
        if (alivePlayersText == null)
            alivePlayersText = GameObject.Find("AlivePlayersText")?.GetComponent<TextMeshProUGUI>();

        _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (Object.HasStateAuthority)
        {
            aliveCount = Runner.ActivePlayers.Count();
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
            //alivePlayersText.text = aliveCount.ToString();
            alivePlayersText.text = $"Vivos: {aliveCount}";
            // o en texto
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
        if (Object.HasStateAuthority)
        {
            RPC_ReduceAliveCount();
        }
        else
        {
            RPC_ReduceAliveCount();
        }
    }

    public int GetAliveCount()
    {
        return aliveCount;
    }

}