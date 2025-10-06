using UnityEngine;
using UnityEngine.UI;
using Fusion;
using System.Linq;

public class PlayerCounter : NetworkBehaviour
{
    public Text alivePlayersText;
    
    [Networked] private int aliveCount { get; set; }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            aliveCount = Runner.ActivePlayers.Count();
        }
        UpdateText();
    }

    public override void Render()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (alivePlayersText != null)
        {
            alivePlayersText.text = aliveCount.ToString();
            //alivePlayersText.text = $"{aliveCount}
            // o en texto
        }
    }

    // Llamamos esta función cuando alguien muera
    public void ReduceAliveCount()
    {
        if (Object.HasStateAuthority && aliveCount > 0)
        {
            aliveCount--;
        }
    }
}