using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int CurrentX { set; get; }
    public int CurrentY { set; get; }

    // Value to check determine the team of the piece
    public bool isAttacking;


    public void SetPosition(int x, int y)
    {
        this.CurrentX = x;
        this.CurrentY = y;
    }


    public bool[,] PossibleMove()
    {
        // Array of valid moves for the piece 
        bool[,] r = new bool[BoardManager.BOARD_SIZE, BoardManager.BOARD_SIZE];

        // Temp values
        Piece p;
        int i;

        // Moving right
        i = CurrentX;
        while (true)
        {
            i++;
            if (i >= BoardManager.BOARD_SIZE)
            {
                break;
            }
            p = BoardManager.Instance.Board[i, CurrentY];
            if (p == null)
            {
                r[i, CurrentY] = true;
            }
            else
            {
                break;
            }
        }

        // Move left
        i = CurrentX;
        while (true)
        {
            i--;
            if (i < 0)
            {
                break;
            }
            p = BoardManager.Instance.Board[i, CurrentY];
            if (p == null)
            {
                r[i, CurrentY] = true;
            }
            else
            {
                break;
            }
        }

        // Move up
        i = CurrentY;
        while (true)
        {
            i++;
            if (i >= BoardManager.BOARD_SIZE)
            {
                break;
            }
            p = BoardManager.Instance.Board[CurrentX, i];
            if (p == null)
            {
                r[CurrentX, i] = true;
            }
            else
            {
                break;
            }
        }

        // Move down
        i = CurrentY;
        while (true)
        {
            i--;
            if (i < 0)
            {
                break;
            }
            p = BoardManager.Instance.Board[CurrentX, i];
            if (p == null)
            {
                r[CurrentX, i] = true;
            }
            else
            {
                break;
            }
        }


        return r;
    }

}
