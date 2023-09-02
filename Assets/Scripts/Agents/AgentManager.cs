using System;
using System.Collections.Generic;
using Exuli;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    public static AgentManager Instance;

    private static List<Agent> _allAgents = new List<Agent>();
    public List<Agent> canSeePlayer = new List<Agent>();
    public Player player;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public Transform GetPlayer()
    {
        if (player != null) return player.transform;
        
        player = FindObjectOfType<Player>();
        if (player == null) // if still
            throw new NotImplementedException("El player no esta en escena");

        return player.transform;
    } 

    public void AddAgent(Agent agent)
    {
        if(!_allAgents.Contains(agent))
            _allAgents.Add(agent);
    }

    public void RemoveAgent(Agent agent)
    {
        if(_allAgents.Contains(agent))
            _allAgents.Remove(agent);
    }

    public void AlertPlayer()
    {
        foreach (var agent in _allAgents) 
            agent.AlertPlayerFinded();
    }

    public void CanSeePlayer(Agent agent, bool ican)
    {
        if (ican)
        {
            if(!canSeePlayer.Contains(agent))
                canSeePlayer.Add(agent);
        }
        else
        {
            if(canSeePlayer.Contains(agent))
                canSeePlayer.Remove(agent);
        }
    }

    public bool AnyoneSeePlayer() => canSeePlayer.Count > 0;
}
