using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MDPEnvironment
{
    public Tile[,] Environment { get; private set; }
    public bool IsTerminal { get; private set; }
    public bool IsWaitingForMove { get; private set; }

    public IAgent Attacking { get; private set; }
    public IAgent Defending { get; private set; }
    public IAgent WhosTurn { get; private set; }

    private Vector2Int king;

    private List<Vector2Int> Corners => new List<Vector2Int>(new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(Environment.GetLength(0) - 1, 0),
        new Vector2Int(0, Environment.GetLength(1) - 1), new Vector2Int(Environment.GetLength(0) - 1, Environment.GetLength(1) - 1) });

    public enum Tile
    {
        Empty,
        Defending,
        Attacking,
        King
    }

    public MDPEnvironment(IAgent attacking, IAgent defending, int boardSize)
    {
        Attacking = attacking;
        Defending = defending;

        WhosTurn = Attacking;

        CreateEnvironment(boardSize);
    }

    private void CreateEnvironment(int boardSize)
    {
        Environment = new Tile[boardSize, boardSize];
        int centre = boardSize / 2, last = boardSize - 1;

        // Initialise all tiles to empty
        for (int y = 0; y < boardSize; y++)
        {
            for (int x = 0; x < boardSize; x++)
            {
                Environment[x, y] = Tile.Empty;
            }
        }


        // Bottom
        Environment[centre - 2, 0] = Tile.Defending;
        Environment[centre - 1, 0] = Tile.Defending;
        Environment[centre, 0] = Tile.Defending;
        Environment[centre + 1, 0] = Tile.Defending;
        Environment[centre + 2, 0] = Tile.Defending;
        Environment[centre, 1] = Tile.Defending;
        // Left
        Environment[0, centre - 2] = Tile.Defending;
        Environment[0, centre - 1] = Tile.Defending;
        Environment[0, centre] = Tile.Defending;
        Environment[0, centre + 1] = Tile.Defending;
        Environment[0, centre + 2] = Tile.Defending;
        Environment[1, centre] = Tile.Defending;
        // Top
        Environment[centre - 2, last] = Tile.Defending;
        Environment[centre - 1, last] = Tile.Defending;
        Environment[centre, last] = Tile.Defending;
        Environment[centre + 1, last] = Tile.Defending;
        Environment[centre + 2, last] = Tile.Defending;
        Environment[centre, last - 1] = Tile.Defending;
        // Right
        Environment[last, centre - 2] = Tile.Defending;
        Environment[last, centre - 1] = Tile.Defending;
        Environment[last, centre] = Tile.Defending;
        Environment[last, centre + 1] = Tile.Defending;
        Environment[last, centre + 2] = Tile.Defending;
        Environment[last - 1, centre] = Tile.Defending;

        // Right
        Environment[centre + 1, centre] = Tile.Attacking;
        Environment[centre + 2, centre] = Tile.Attacking;
        Environment[centre + 1, centre + 1] = Tile.Attacking;
        Environment[centre + 1, centre - 1] = Tile.Attacking;
        // Left
        Environment[centre - 1, centre] = Tile.Attacking;
        Environment[centre - 2, centre] = Tile.Attacking;
        Environment[centre - 1, centre + 1] = Tile.Attacking;
        Environment[centre - 1, centre - 1] = Tile.Attacking;
        // Down
        Environment[centre, centre + 1] = Tile.Attacking;
        Environment[centre, centre + 2] = Tile.Attacking;
        // Up
        Environment[centre, centre - 1] = Tile.Attacking;
        Environment[centre, centre - 2] = Tile.Attacking;

        // Spawn the king 
        Environment[centre, centre] = Tile.King;

        king = new Vector2Int(centre, centre);
    }

    public void GetNextMove(in UnityAction<Move> callback)
    {
        IsWaitingForMove = true;

        WhosTurn.GetMove(this, callback);
    }

    public Outcome ExecuteMove(Move m)
    {
        List<Vector2Int> piecesToKill = GetPiecesToKillWithMove(m);

        // Make the move
        Tile piece = Environment[m.From.x, m.From.y];
        Environment[m.To.x, m.To.y] = piece;
        Environment[m.From.x, m.From.y] = Tile.Empty;

        // Kill all pieces affected by this move
        foreach (Vector2Int v in piecesToKill)
        {
            Environment[v.x, v.y] = Tile.Empty;
        }

        bool gameOver = false;
        BoardManager.Team winningTeam = BoardManager.Team.Attacking;
        // Check if the king has reached one of the four corners
        if (piece == Tile.King)
        {
            // Keep track of where the king is
            king = m.To;

            if ((king.x == 0 || king.x == Environment.GetLength(0) - 1) && (king.y == 0 || king.y == Environment.GetLength(1) - 1))
            {
                gameOver = true;
                winningTeam = BoardManager.Team.Attacking;
            }
        }

        // Check if the king is pinned
        Vector2Int left = king + new Vector2Int(-1, 0), right = king + new Vector2Int(1, 0), up = king + new Vector2Int(0, 1), down = new Vector2Int(0, -1);
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
        if (WhosTurn.Team == BoardManager.Team.Attacking)
        {
            WhosTurn = Defending;
        }
        else
        {
            WhosTurn = Attacking;
        }

        IsWaitingForMove = false;

        return new Outcome(m, gameOver, winningTeam, piecesToKill);
    }


    private bool IsWall(Vector2Int pos)
    {
        return !Utils.IsWithinBounds(pos, Environment);
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
        Tile t = Environment[tile.x, tile.y];
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


    public List<Vector2Int> GetAllPossibleMoves(Vector2Int tile, out bool hasMoves)
    {
        hasMoves = false;

        if (GetTeam(tile, out BoardManager.Team team))
        {
            Tile t = Environment[tile.x, tile.y];
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



    private bool CanMoveToPosition(Vector2Int pos, Vector2Int neighbour1, Vector2Int neighbour2, BoardManager.Team team)
    {
        BoardManager.Team opposite = GetOppositeTeam(team);

        // Position is within the board and there arent pieces trying to trap it
        return !Corners.Contains(pos) && Utils.IsWithinBounds(pos, Environment) &&
            (!IsWallOrPieceFromteam(neighbour1, opposite) || !IsWallOrPieceFromteam(neighbour2, opposite));
    }


    private void CheckPossibleMove(ref List<Vector2Int> moves, Vector2Int startingTile, BoardManager.Team team, Vector2Int direction)
    {
        Vector2Int position = startingTile;
        while (true)
        {
            position += direction;

            // Ensure that the new position is within the bounds of the array and is empty
            if (!Utils.IsWithinBounds(position, Environment) || Environment[position.x, position.y] != Tile.Empty)
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
        // Ensure move is within the board
        if (Utils.IsWithinBounds(pos, Environment) && Environment[pos.x, pos.y] == Tile.Empty)
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
        if (Utils.IsWithinBounds(trappedPiece, Environment) && IsPieceFromTeam(trappedPiece, opposite) && IsWallOrPieceFromteam(possibleTeammate, team))
        {
            // We cannot kill the king 
            if (Environment[trappedPiece.x, trappedPiece.y] != Tile.King)
            {
                toKill.Add(trappedPiece);
            }
        }
    }


}