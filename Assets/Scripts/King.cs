using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class King : Piece
{

    public override bool[,] PossibleMove()
    {
        // Array of valid moves for the piece 
        bool[,] r = new bool[BoardManager.Instance.BOARD_SIZE, BoardManager.Instance.BOARD_SIZE];

        Piece p;

        // Check 3x3 around the king 
        for(int y = CurrentY - 1; y <= CurrentY + 1; y++)
        {        
            for(int x = CurrentX - 1; x <= CurrentX + 1; x++)
            {
                // Ensure within the board
                if(x >= 0 && x < BoardManager.Instance.BOARD_SIZE && y >= 0 && y < BoardManager.Instance.BOARD_SIZE)
                {
                    if (x == CurrentX || y == CurrentY)
                    {
                        // Get the tile
                        p = BoardManager.Instance.Board[x, y];
                        // Valid move
                        if (p == null)
                        {
                            r[x, y] = true;
                        }
                    }
                }
            }
        }

        return r;
    }
}
