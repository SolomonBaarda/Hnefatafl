using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    // Value to check the team of the piece
    public bool isAttacking;
    // Value to check 
    public bool isKing = false;

    public void SetPosition(int x, int y)
    {
        this.CurrentX = x;
        this.CurrentY = y;
    }


    public virtual bool[,] PossibleMove()
    {
        // Array of valid moves for the piece 
        bool[,] r = new bool[BoardManager.BOARD_SIZE, BoardManager.BOARD_SIZE];

        // Temp values
        Piece p, a, b;
        int i;




        // TODO 
        // Don't allow the piece to move to the edge of the board if it will be destroyed
        // Same case as if between two players 




        // Right
        i = CurrentX;
        while (true)
        {
            i++;
            // Break out if not valid values
            if (i < 0 || i >= BoardManager.BOARD_SIZE)
            {
                break;
            }

            // Reference to the tile
            p = BoardManager.Instance.Board[i, CurrentY];
            // Possible valid move
            if (p == null)
            {
                // Only check if not on the border
                if (CurrentY > 0 && CurrentY < BoardManager.BOARD_SIZE - 1)
                {
                    // Get reference to tiles
                    a = BoardManager.Instance.Board[i, CurrentY - 1];
                    b = BoardManager.Instance.Board[i, CurrentY + 1];
                    if (a != null && b != null)
                    {
                        if (isAttacking != a.isAttacking && isAttacking != b.isAttacking)
                        {
                            // Not valid
                            r[i, CurrentY] = false;
                            continue;
                        }
                    }
                }
                // If we get here, the move is valid
                r[i, CurrentY] = true;
            }
            // Not valid move 
            else
            {
                break;
            }
        }

        // left
        i = CurrentX;
        while (true)
        {
            i--;
            // Break out if not valid values
            if (i < 0 || i >= BoardManager.BOARD_SIZE)
            {
                break;
            }

            // Reference to the tile
            p = BoardManager.Instance.Board[i, CurrentY];
            // Possible valid move
            if (p == null)
            {
                // Only check if not on the border
                if (CurrentY > 0 && CurrentY < BoardManager.BOARD_SIZE - 1)
                {
                    // Get reference to tiles
                    a = BoardManager.Instance.Board[i, CurrentY - 1];
                    b = BoardManager.Instance.Board[i, CurrentY + 1];
                    if (a != null && b != null)
                    {
                        if (isAttacking != a.isAttacking && isAttacking != b.isAttacking)
                        {
                            // Not valid
                            r[i, CurrentY] = false;
                            continue;
                        }
                    }
                }
                // If we get here, the move is valid
                r[i, CurrentY] = true;
            }
            // Not valid move 
            else
            {
                break;
            }
        }

        // Down
        i = CurrentY;
        while (true)
        {
            i++;
            // Break out if not valid values
            if (i < 0 || i >= BoardManager.BOARD_SIZE)
            {
                break;
            }

            // Reference to the tile
            p = BoardManager.Instance.Board[CurrentX, i];
            // Possible valid move
            if (p == null)
            {
                // Only check if not on the border
                if (CurrentX > 0 && CurrentX < BoardManager.BOARD_SIZE - 1)
                {
                    // Get reference to tiles
                    a = BoardManager.Instance.Board[CurrentX - 1, i];
                    b = BoardManager.Instance.Board[CurrentX + 1, i];
                    if (a != null && b != null)
                    {
                        if (isAttacking != a.isAttacking && isAttacking != b.isAttacking)
                        {
                            // Not valid
                            r[CurrentX, i] = false;
                            continue;
                        }
                    }
                }
                // If we get here, the move is valid
                r[CurrentX, i] = true;
            }
            // Not valid move 
            else
            {
                break;
            }
        }

        // Up
        i = CurrentY;
        while (true)
        {
            i--;
            // Break out if not valid values
            if (i < 0 || i >= BoardManager.BOARD_SIZE)
            {
                break;
            }

            // Reference to the tile
            p = BoardManager.Instance.Board[CurrentX, i];
            // Possible valid move
            if (p == null)
            {
                // Only check if not on the border
                if (CurrentX > 0 && CurrentX < BoardManager.BOARD_SIZE - 1)
                {
                    // Get reference to tiles
                    a = BoardManager.Instance.Board[CurrentX - 1, i];
                    b = BoardManager.Instance.Board[CurrentX + 1, i];
                    if (a != null && b != null)
                    {
                        if (isAttacking != a.isAttacking && isAttacking != b.isAttacking)
                        {
                            // Not valid
                            r[CurrentX, i] = false;
                            continue;
                        }
                    }
                }
                // If we get here, the move is valid
                r[CurrentX, i] = true;
            }
            // Not valid move 
            else
            {
                break;
            }
        }

        // Pieces not allowed to move to the far corners ever
        r[0, 0] = false;
        r[0, BoardManager.BOARD_SIZE - 1] = false;
        r[BoardManager.BOARD_SIZE - 1, 0] = false;
        r[BoardManager.BOARD_SIZE - 1, BoardManager.BOARD_SIZE - 1] = false;

        return r;
    }

}
