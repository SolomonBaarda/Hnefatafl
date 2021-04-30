using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PolicyIterationAgent : IAgent
{
    public BoardManager.Team Team { get; private set; }

    public const int MovesAheadToCheckDepth = 4;


    public void Instantiate(BoardManager.Team team)
    {
        Team = team;
    }



    public void GetMove(MDP e, UnityAction<Move> callback)
    {

    }


    private static Dictionary<MDPEnvironment.Tile[,], float> InitialisePolicies(MDPEnvironment e, int movesForThisAgentDepth)
    {
        HashSet<MDPEnvironment.Tile> allStates; 

        Dictionary<MDPEnvironment.Tile[,], float> states = new Dictionary<MDPEnvironment.Tile[,], float>();


    }



}
