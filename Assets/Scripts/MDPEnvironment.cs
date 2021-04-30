<<<<<<< Updated upstream
﻿using System.Collections;
=======
﻿using System;
>>>>>>> Stashed changes
using System.Collections.Generic;
using UnityEngine;

public class MDPEnvironment
{
    public Tile[,] GameBoard { get; private set; }
    public bool IsTerminal { get; private set; }

    public int BoardSize { get; private set; }
    public const int DefaultBoardSize = 13;

    public Vector2Int King { get; private set; }

    public BoardManager.Team WhosTurn { get; private set; }

    public List<Vector2Int> Corners => new List<Vector2Int>(new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(GameBoard.GetLength(0) - 1, 0),
        new Vector2Int(0, GameBoard.GetLength(1) - 1), new Vector2Int(GameBoard.GetLength(0) - 1, GameBoard.GetLength(1) - 1) });



    public MDPEnvironment() : this(DefaultBoardSize) { }

    public MDPEnvironment(int boardSize)
    {
        BoardSize = boardSize;

        WhosTurn = BoardManager.Team.Attacking;
        GameBoard = new Tile[BoardSize, BoardSize];
        int centre = BoardSize / 2, last = BoardSize - 1;

        // Initialise all tiles to empty
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                GameBoard[x, y] = Tile.Empty;
            }
        }


        // Bottom
        GameBoard[centre - 2, 0] = Tile.Defending;
        GameBoard[centre - 1, 0] = Tile.Defending;
        GameBoard[centre, 0] = Tile.Defending;
        GameBoard[centre + 1, 0] = Tile.Defending;
        GameBoard[centre + 2, 0] = Tile.Defending;
        GameBoard[centre, 1] = Tile.Defending;
        // Left
        GameBoard[0, centre - 2] = Tile.Defending;
        GameBoard[0, centre - 1] = Tile.Defending;
        GameBoard[0, centre] = Tile.Defending;
        GameBoard[0, centre + 1] = Tile.Defending;
        GameBoard[0, centre + 2] = Tile.Defending;
        GameBoard[1, centre] = Tile.Defending;
        // Top
        GameBoard[centre - 2, last] = Tile.Defending;
        GameBoard[centre - 1, last] = Tile.Defending;
        GameBoard[centre, last] = Tile.Defending;
        GameBoard[centre + 1, last] = Tile.Defending;
        GameBoard[centre + 2, last] = Tile.Defending;
        GameBoard[centre, last - 1] = Tile.Defending;
        // Right
        GameBoard[last, centre - 2] = Tile.Defending;
        GameBoard[last, centre - 1] = Tile.Defending;
        GameBoard[last, centre] = Tile.Defending;
        GameBoard[last, centre + 1] = Tile.Defending;
        GameBoard[last, centre + 2] = Tile.Defending;
        GameBoard[last - 1, centre] = Tile.Defending;

        // Right
        GameBoard[centre + 1, centre] = Tile.Attacking;
        GameBoard[centre + 2, centre] = Tile.Attacking;
        GameBoard[centre + 1, centre + 1] = Tile.Attacking;
        GameBoard[centre + 1, centre - 1] = Tile.Attacking;
        // Left
        GameBoard[centre - 1, centre] = Tile.Attacking;
        GameBoard[centre - 2, centre] = Tile.Attacking;
        GameBoard[centre - 1, centre + 1] = Tile.Attacking;
        GameBoard[centre - 1, centre - 1] = Tile.Attacking;
        // Down
        GameBoard[centre, centre + 1] = Tile.Attacking;
        GameBoard[centre, centre + 2] = Tile.Attacking;
        // Up
        GameBoard[centre, centre - 1] = Tile.Attacking;
        GameBoard[centre, centre - 2] = Tile.Attacking;

        // Spawn the king 
        GameBoard[centre, centre] = Tile.King;
        King = new Vector2Int(centre, centre);
    }





    public Outcome ExecuteMove(Move m)
    {
        List<Vector2Int> piecesToKill = GetPiecesToKillWithMove(m);

        // Make the move
        Tile piece = GameBoard[m.From.x, m.From.y];
        GameBoard[m.To.x, m.To.y] = piece;
        GameBoard[m.From.x, m.From.y] = Tile.Empty;

        // Kill all pieces affected by this move
        foreach (Vector2Int v in piecesToKill)
        {
            GameBoard[v.x, v.y] = Tile.Empty;
        }

        bool gameOver = false;
        BoardManager.Team winningTeam = BoardManager.Team.Attacking;
        // Check if the king has reached one of the four corners
        if (piece == Tile.King)
        {
            // Keep track of where the king is
            King = m.To;

            if ((King.x == 0 || King.x == GameBoard.GetLength(0) - 1) && (King.y == 0 || King.y == GameBoard.GetLength(1) - 1))
            {
                gameOver = true;
                winningTeam = BoardManager.Team.Attacking;
            }
        }

        // Check if the king is pinned
        Vector2Int left = King + new Vector2Int(-1, 0), right = King + new Vector2Int(1, 0), up = King + new Vector2Int(0, 1), down = new Vector2Int(0, -1);
        if (IsWallOrPieceFromteam(left, BoardManager.Team.Defending) && IsWallOrPieceFromteam(right, BoardManager.Team.Defending) &&
            IsWallOrPieceFromteam(up, BoardManager.Team.Defending) && IsWallOrPieceFromteam(down, BoardManager.Team.Defending))
        {
            gameOver = true;
            winningTeam = BoardManager.Team.Defending;
        }



        // Check if either of the teams have run out of pieces
        /*
        if (attackingCount < 2)
        {
            // Defending wins
            EndGame(Team.Defending);
        }
        if (defendingCount < 2)
        {
            // Attacking wins
            EndGame(Team.Attacking);
        }
        */



        // Change whos turn it is
        WhosTurn = GetOppositeTeam(WhosTurn);

        return new Outcome(m, gameOver, winningTeam, piecesToKill);
    }


    private bool IsWall(Vector2Int pos)
    {
        return !Utils.IsWithinBounds(pos, GameBoard);
    }

    private bool IsPieceFromTeam(Vector2Int pos, BoardManager.Team team)
    {
        if (GetTeam(pos, out BoardManager.Team t))
        {
            return t == team;
        }

        return false;
    }

    private bool IsWallOrPieceFromteam(Vector2Int pos, BoardManager.Team team)
    {
        return IsWall(pos) || IsPieceFromTeam(pos, team);
    }

    public bool IsValidMove(Vector2Int from, Vector2Int to)
    {
        List<Vector2Int> moves = GetAllPossibleMoves(from, out bool hasMoves);

        if (hasMoves)
        {
            return moves.Contains(to);
        }

        return false;
    }

    public BoardManager.Team GetOppositeTeam(BoardManager.Team team)
    {
        if (team == BoardManager.Team.Attacking)
        {
            return BoardManager.Team.Defending;
        }
        else
        {
            return BoardManager.Team.Attacking;
        }
    }
    public bool GetTeam(Vector2Int tile, out BoardManager.Team team)
    {
<<<<<<< Updated upstream
        Tile t = GameBoard[tile.x, tile.y];
=======
        return GetTeam(Environment, tile, out team);
    }

    public static bool GetTeam(Tile[,] environment, Vector2Int tile, out BoardManager.Team team)
    {
        Tile t = environment[tile.x, tile.y];
>>>>>>> Stashed changes
        team = BoardManager.Team.Attacking;

        if (IsOnTeam(t, BoardManager.Team.Attacking))
        {
            team = BoardManager.Team.Attacking;
            return true;
        }
        else if (IsOnTeam(t, BoardManager.Team.Defending))
        {
            team = BoardManager.Team.Defending;
            return true;
        }

        return false;
    }


    public static bool IsOnTeam(Tile tile, BoardManager.Team team)
    {
        return (tile == Tile.Defending && team == BoardManager.Team.Defending) ||
            ((tile == Tile.Attacking || tile == Tile.King) && team == BoardManager.Team.Attacking);
    }


    public static List<Vector2Int> GetAllPossibleMoves(Tile[,] environment, Vector2Int tile, out bool hasMoves)
    {
        hasMoves = false;

        if (GetTeam(environment, tile, out BoardManager.Team team))
        {
            Tile t = environment[tile.x, tile.y];
            List<Vector2Int> moves = new List<Vector2Int>();

            if (t == Tile.King)
            {
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(1, 0));
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(-1, 0));
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(0, 1));
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(0, -1));
            }
            else
            {
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(1, 0));
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(-1, 0));
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(0, 1));
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(0, -1));
            }

            hasMoves = moves.Count > 0;
            return moves;
        }

        return null;
    }

    public List<Vector2Int> GetAllPossibleMoves(Vector2Int tile, out bool hasMoves)
    {
        hasMoves = false;

        if (GetTeam(tile, out BoardManager.Team team))
        {
            Tile t = GameBoard[tile.x, tile.y];
            List<Vector2Int> moves = new List<Vector2Int>();

            if (t == Tile.King)
            {
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(1, 0));
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(-1, 0));
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(0, 1));
                CheckPossibleMoveMoveKing(ref moves, tile + new Vector2Int(0, -1));
            }
            else
            {
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(1, 0));
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(-1, 0));
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(0, 1));
                CheckPossibleMove(ref moves, tile, team, new Vector2Int(0, -1));
            }

            hasMoves = moves.Count > 0;
            return moves;
        }

        return null;
    }



    private static bool CanMoveToPosition(Vector2Int pos, Vector2Int neighbour1, Vector2Int neighbour2, BoardManager.Team team)
    {
        BoardManager.Team opposite = GetOppositeTeam(team);

        // Position is within the board and there arent pieces trying to trap it
        return !Corners.Contains(pos) && Utils.IsWithinBounds(pos, GameBoard) &&
            (!IsWallOrPieceFromteam(neighbour1, opposite) || !IsWallOrPieceFromteam(neighbour2, opposite));
    }

    private void CheckPossibleMove(ref List<Vector2Int> moves, Vector2Int startingTile, BoardManager.Team team, Vector2Int direction)
    {
        CheckPossibleMove(ref moves, Environment, startingTile, team, direction);
    }

        private static void CheckPossibleMove(ref List<Vector2Int> moves, Tile[,] environment, Vector2Int startingTile, BoardManager.Team team, Vector2Int direction)
    {
        Vector2Int position = startingTile;
        while (true)
        {
            position += direction;

            // Ensure that the new position is within the bounds of the array and is empty
<<<<<<< Updated upstream
            if (!Utils.IsWithinBounds(position, GameBoard) || GameBoard[position.x, position.y] != Tile.Empty)
=======
            if (!Utils.IsWithinBounds(position, environment) || environment[position.x, position.y] != Tile.Empty)
>>>>>>> Stashed changes
            {
                break;
            }

            if (CanMoveToPosition(position, position + new Vector2Int(0, 1), position + new Vector2Int(0, -1), team) &&
                CanMoveToPosition(position, position + new Vector2Int(1, 0), position + new Vector2Int(-1, 0), team))
            {
                // If we get here then the move must be valid
                moves.Add(position);
            }
        }
    }

    private void CheckPossibleMoveMoveKing(ref List<Vector2Int> moves, Vector2Int pos)
    {
        CheckPossibleMoveMoveKing(ref moves, Environment, pos);
    }

    private static void CheckPossibleMoveMoveKing(ref List<Vector2Int> moves, Tile[,] environment, Vector2Int pos)
    {
        // Ensure move is within the board
<<<<<<< Updated upstream
        if (Utils.IsWithinBounds(pos, GameBoard) && GameBoard[pos.x, pos.y] == Tile.Empty)
=======
        if (Utils.IsWithinBounds(pos, environment) && environment[pos.x, pos.y] == Tile.Empty)
>>>>>>> Stashed changes
        {
            moves.Add(pos);
        }
    }

    public List<Vector2Int> GetPiecesToKillWithMove(Move m)
    {
        List<Vector2Int> toKill = new List<Vector2Int>();

        // Check all four directions for pieces to remove
        CheckPiecesToKillWithMove(ref toKill, m.To + new Vector2Int(1, 0), m.To + new Vector2Int(2, 0), m.Team);
        CheckPiecesToKillWithMove(ref toKill, m.To + new Vector2Int(-1, 0), m.To + new Vector2Int(-2, 0), m.Team);
        CheckPiecesToKillWithMove(ref toKill, m.To + new Vector2Int(0, 1), m.To + new Vector2Int(0, 2), m.Team);
        CheckPiecesToKillWithMove(ref toKill, m.To + new Vector2Int(0, -1), m.To + new Vector2Int(0, -2), m.Team);

        return toKill;
    }

    private void CheckPiecesToKillWithMove(ref List<Vector2Int> toKill, Vector2Int trappedPiece, Vector2Int possibleTeammate, BoardManager.Team team)
    {
        BoardManager.Team opposite = GetOppositeTeam(team);
        if (Utils.IsWithinBounds(trappedPiece, GameBoard) && IsPieceFromTeam(trappedPiece, opposite) && IsWallOrPieceFromteam(possibleTeammate, team))
        {
            // We cannot kill the king 
            if (GameBoard[trappedPiece.x, trappedPiece.y] != Tile.King)
            {
                toKill.Add(trappedPiece);
            }
        }
    }







<<<<<<< Updated upstream


    public enum GameState
    {
        AttackingTurn,
        DefendingTurn,
        AttackingWon,
        DefendingWon,
        Draw,
    }

    public enum Tile
    {
        Empty,
        Defending,
        Attacking,
        King
    }

}
=======
    private static HashSet<Tile[,]> ConstructAllPossibleStates(MDPEnvironment e, int movesForThisAgentDepth)
    {
        HashSet<Tile[,]> states = new HashSet<Tile[,]>();



    }

    private static void AddAllPossibleOutcomes(Tile[,] environment, BoardManager.Team turn, Vector2Int from, List<Vector2Int> destinations, ref HashSet<Tile[,]> outcomes)
    {
        foreach (Vector2Int move in e.GetAllPossibleMoves())
        {

        }

    }








    public static long Hash(Tile[,] environment, BoardManager.Team turn)
    {
        // For board of 13*13 we need 170 digits of base 4 numbers
        // this would allow for 169 tile positions and 1 turn value

        string base4 = "";

        // The first length * height digits (in base 4) will be the values of the game tiles
        for (int y = 0; y < environment.GetLength(1); y++)
        {
            for (int x = 0; x < environment.GetLength(0); x++)
            {
                // Use the enum tile value for each tile
                string.Concat(base4, environment[x, y]);
            }
        }

        // Now encode whos turn it is
        string.Concat(base4, turn);

        // We use base 4 as there are 4 possibilities for each tile type
        return Utils.Base4ToLong(base4);
    }




}
>>>>>>> Stashed changes
