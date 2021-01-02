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
    }

    public void GetNextMove(in UnityAction<Vector2Int, Vector2Int> callback)
    {
        IsWaitingForMove = true;

        WhosTurn.GetMove(this, callback);
    }

    public void ExecuteMove(Vector2Int from, Vector2Int to)
    {
        // Make the move





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


    public List<Vector2Int> GetAllPossibleMoves(Vector2Int tile, out bool hasMoves)
    {
        hasMoves = false;

        if (GetTeam(tile, out BoardManager.Team team))
        {
            Tile t = Environment[tile.x, tile.y];
            List<Vector2Int> moves = new List<Vector2Int>();

            if (t == Tile.King)
            {
                CheckDirectionMoveKing(ref moves, tile.x - 1, tile.y);
                CheckDirectionMoveKing(ref moves, tile.x + 1, tile.y);
                CheckDirectionMoveKing(ref moves, tile.x, tile.y - 1);
                CheckDirectionMoveKing(ref moves, tile.x, tile.y + 1);
            }
            else
            {
                CheckPossibleMovePieceXAxis(ref moves, tile, team, +1);
                CheckPossibleMovePieceXAxis(ref moves, tile, team, -1);
                CheckPossibleMovePieceYAxis(ref moves, tile, team, +1);
                CheckPossibleMovePieceYAxis(ref moves, tile, team, -1);
            }

            hasMoves = moves.Count > 0;
            return moves;
        }

        return null;
    }



    private void CheckPossibleMovePieceXAxis(ref List<Vector2Int> moves, Vector2Int tile, BoardManager.Team team, int direction)
    {
        int x = tile.x;
        while (true)
        {
            x += direction;

            // Ensure within board size
            if (x < 0 || x >= Environment.GetLength(0)) { break; }

            // Break out if there is a piece in the way
            if (Environment[x, tile.y] != Tile.Empty) { break; }

            // At the edge of the board
            if (tile.y == 0 || tile.y == Environment.GetLength(1))
            {
                int y;

                if (tile.y == 0)
                {
                    y = tile.y + 1;
                }
                else
                {
                    y = tile.y - 1;
                }

                if (GetTeam(new Vector2Int(x, y), out BoardManager.Team otherTeam))
                {
                    // Can't move here as would be trapped at edge of board
                    if (otherTeam != team)
                    {
                        continue;
                    }
                }
            }
            // In the middle of the board
            else
            {
                int firstY = tile.y - 1, secondY = tile.y + 1;
                if (firstY >= 0 && secondY >= 0 && firstY < Environment.GetLength(1) && secondY < Environment.GetLength(1))
                {
                    if (GetTeam(new Vector2Int(x, firstY), out BoardManager.Team first) && GetTeam(new Vector2Int(x, secondY), out BoardManager.Team second))
                    {
                        // Can't move here as would be trapped between two pieces
                        if (team != first && team != second)
                        {
                            continue;
                        }
                    }
                }
            }

            // If we get here then the move must be valid
            moves.Add(new Vector2Int(x, tile.y));
        }
    }


    private void CheckPossibleMovePieceYAxis(ref List<Vector2Int> moves, Vector2Int tile, BoardManager.Team team, int direction)
    {
        int y = tile.y;
        while (true)
        {
            y += direction;

            // Ensure within board size
            if (y < 0 || y >= Environment.GetLength(1)) { break; }

            // Break out if there is a piece in the way
            if (Environment[tile.x, y] != Tile.Empty) { break; }

            // At the edge of the board
            if (tile.x == 0 || tile.x == Environment.GetLength(0))
            {
                int x;

                if (tile.x == 0)
                {
                    x = tile.x + 1;
                }
                else
                {
                    x = tile.x - 1;
                }

                if (GetTeam(new Vector2Int(x, y), out BoardManager.Team otherTeam))
                {
                    // Can't move here as would be trapped at edge of board
                    if (otherTeam != team)
                    {
                        continue;
                    }
                }
            }
            // In the middle of the board
            else
            {
                int firstX = tile.x - 1, secondX = tile.x + 1;
                if (firstX >= 0 && secondX >= 0 && firstX < Environment.GetLength(0) && secondX < Environment.GetLength(0))
                {
                    if (GetTeam(new Vector2Int(firstX, y), out BoardManager.Team first) && GetTeam(new Vector2Int(secondX, y), out BoardManager.Team second))
                    {
                        // Can't move here as would be trapped between two pieces
                        if (team != first && team != second)
                        {
                            continue;
                        }
                    }
                }
            }

            // If we get here then the move must be valid
            moves.Add(new Vector2Int(tile.x, y));
        }
    }





    private void CheckDirectionMoveKing(ref List<Vector2Int> moves, int x, int y)
    {
        // Ensure move is within the board
        if (x >= 0 && x < Environment.GetLength(0) && y >= 0 && y < Environment.GetLength(1))
        {
            if (Environment[x, y] == Tile.Empty)
            {
                moves.Add(new Vector2Int(x, y));
            }
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

    public enum Tile
    {
        Empty,
        Defending,
        Attacking,
        King
    }




}