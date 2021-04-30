using UnityEngine;
using UnityEngine.Events;

public interface IAgent
{
    BoardManager.Team Team { get; }

    void Instantiate(BoardManager.Team team);

    void GetMove(MDP e, UnityAction<Move> callback);
}
