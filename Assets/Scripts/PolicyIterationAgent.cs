using UnityEngine;
using UnityEngine.Events;

public class PolicyIterationAgent : IAgent
{
    public BoardManager.Team Team { get; private set; }


    public void Instantiate(BoardManager.Team team)
    {
        Team = team;
    }



    public void GetMove(MDPEnvironment e, UnityAction<Move> callback)
    {

    }
}
