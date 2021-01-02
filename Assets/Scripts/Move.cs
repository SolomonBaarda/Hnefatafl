using System.Collections.Generic;
using UnityEngine;

public struct Move
{
    public Vector2Int From;
    public Vector2Int To;
    public BoardManager.Team Team;

    public Move(Vector2Int from, Vector2Int to, BoardManager.Team team)
    {
        From = from;
        To = to;
        Team = team;
    }
}
