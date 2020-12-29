﻿using UnityEngine;
using UnityEngine.Events;

public interface IAgent
{
    BoardManager.Team Team { get; }

    void Instantiate(BoardManager.Team team);

    void GetMove(MDPEnvironment e, UnityAction<Vector2Int, Vector2Int> callback);
}
