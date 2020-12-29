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



    /*
    public MDPEnvironment(Piece[,] board)
    {
        Environment = new Tile[board.GetLength(0), board.GetLength(1)];

        // Create the MDP environment from the current game board
        for (int y = 0; y < board.GetLength(1); y++)
        {
            for (int x = 0; x < board.GetLength(0); x++)
            {
                Tile tile = Tile.Empty;

                if (board[x, y] != null)
                {
                    // Attacking
                    if (board[x, y].isAttacking)
                    {
                        // King
                        if (board[x, y].isKing)
                        {
                            tile = Tile.King;
                        }
                        // Normal piece
                        else
                        {
                            tile = Tile.Attacking;
                        }
                    }
                    // Defending
                    else
                    {
                        tile = Tile.Defending;
                    }
                }

                Environment[x, y] = tile;
            }
        }
    }
    */

    public void GetNextMove(in UnityAction<Vector2Int, Vector2Int> callback)
    {
        IsWaitingForMove = true;

        WhosTurn.GetMove(this, callback);
    }

    public void ExecuteMove(Vector2Int from, Vector2Int to)
    {
        IsWaitingForMove = false;


    }


    public enum Tile
    {
        Empty,
        Defending,
        Attacking,
        King
    }


}