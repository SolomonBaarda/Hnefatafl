using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MDP
{
    public MDPEnvironment State { get; private set; }

    public bool IsWaitingForMove { get; private set; }
    public bool IsGameOver { get { return State.IsTerminal; } }

    public IAgent Attacking { get; private set; }
    public IAgent Defending { get; private set; }
    public IAgent WhosTurn
    {
        get
        {
            if (State.WhosTurn == BoardManager.Team.Defending)
            {
                return Defending;
            }
            else
            {
                return Attacking;
            }
        }
    }

    public MDP(IAgent attacking, IAgent defending)
    {
        Attacking = attacking;
        Defending = defending;

        State = new MDPEnvironment();
    }

    public void GetNextMove(in UnityAction<Move> callback)
    {
        IsWaitingForMove = true;

        WhosTurn.GetMove(this, callback);
    }

    public Outcome ExecuteMove(Move m)
    {
        IsWaitingForMove = false;
        return State.ExecuteMove(m);
    }










}