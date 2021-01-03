using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Outcome
{
    public Move Move;
    public bool GameOver;
    public BoardManager.Team WinningTeam;
    public List<Vector2Int> PiecesKilled;

    public Outcome(Move move, bool gameOver, BoardManager.Team winningTeam, List<Vector2Int> piecesKilled)
    {
        Move = move;
        GameOver = gameOver;
        WinningTeam = winningTeam;
        PiecesKilled = piecesKilled;
    }
}
