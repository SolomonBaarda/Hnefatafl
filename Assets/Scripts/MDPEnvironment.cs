public class MDPEnvironment
{
    private Tile[,] environment;
    private Agent attacking, defending;

    public MDPEnvironment(Agent attacking, Agent defending)
    {
        this.attacking = attacking;
        this.defending = defending;
    }

    public MDPEnvironment(Piece[,] board)
    {
        environment = new Tile[board.GetLength(0), board.GetLength(1)];

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

                environment[x, y] = tile;
            }
        }
    }





    public enum Tile
    {
        Empty,
        Defending,
        Attacking,
        King
    }


}