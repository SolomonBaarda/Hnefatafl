using UnityEngine;

public class Piece : MonoBehaviour
{
    public int CurrentX { get; private set; }
    public int CurrentY { get; private set; }


    // Value to check the team of the piece
    public bool isAttacking;
    // Value to check 
    public bool isKing = false;

    public void SetPosition(int x, int y, Vector3 worldPosition)
    {
        CurrentX = x;
        CurrentY = y;

        transform.position = worldPosition;
    }

    public virtual bool[,] PossibleMove()
    {
        // Array of valid moves for the piece 
        bool[,] r = new bool[BoardManager.Instance.BOARD_SIZE, BoardManager.Instance.BOARD_SIZE];

        CheckPossibleMoveXAxis(+1, ref r);
        CheckPossibleMoveXAxis(-1, ref r);
        CheckPossibleMoveYAxis(+1, ref r);
        CheckPossibleMoveYAxis(-1, ref r);

        // Pieces are never allowed to move to the corners
        r[0, 0] = false;
        r[0, BoardManager.Instance.BOARD_SIZE - 1] = false;
        r[BoardManager.Instance.BOARD_SIZE - 1, 0] = false;
        r[BoardManager.Instance.BOARD_SIZE - 1, BoardManager.Instance.BOARD_SIZE - 1] = false;

        return r;
    }


    private void CheckPossibleMoveXAxis(int direction, ref bool[,] r)
    {
        Piece current, a, b;

        // direction = +/- 
        int x = CurrentX;
        while (true)
        {
            x += direction;
            // Break out if not valid
            if(x < 0 || x >= BoardManager.Instance.BOARD_SIZE)
            {
                break;
            }

            current = BoardManager.Instance.Board[x, CurrentY];
            // Break out if there is a piece in the way
            if(current != null)
            {
                break;
            }
            else
            {
                // At the edge (only check one direction)
                if(CurrentY == 0 || CurrentY == BoardManager.Instance.BOARD_SIZE - 1)
                {
                    if(CurrentY == 0)
                    {
                        // Check below only
                        a = BoardManager.Instance.Board[x, CurrentY + 1];
                    }
                    else
                    {
                        // Check above only
                        a = BoardManager.Instance.Board[x, CurrentY - 1];
                    }
                    if(a != null)
                    {
                        if(isAttacking != a.isAttacking)
                        {
                            // Can't move here in this case
                            r[x, CurrentY] = false;
                            continue;
                        }
                    }
                }
                // In the middle (check both sides)
                else
                {
                    a = BoardManager.Instance.Board[x, CurrentY - 1];
                    b = BoardManager.Instance.Board[x, CurrentY + 1];
                    if(a != null && b != null)
                    {
                        if(isAttacking != a.isAttacking && isAttacking != b.isAttacking)
                        {
                            // Can't move here in this case
                            r[x, CurrentY] = false;
                            continue;
                        }
                    }
                }
            }
            // If we get here then the move must be valid
            r[x, CurrentY] = true;
        }
    }

    private void CheckPossibleMoveYAxis(int direction, ref bool[,] r)
    {
        Piece current, a, b;

        // direction = +/- 
        int y = CurrentY;
        while (true)
        {
            y += direction;
            // Break out if not valid
            if (y < 0 || y >= BoardManager.Instance.BOARD_SIZE)
            {
                break;
            }

            current = BoardManager.Instance.Board[CurrentX, y];
            // Break out if there is a piece in the way
            if (current != null)
            {
                break;
            }
            else
            {
                // At the edge (only check one direction)
                if (CurrentX == 0 || CurrentX == BoardManager.Instance.BOARD_SIZE - 1)
                {
                    if (CurrentX == 0)
                    {
                        // Check right only
                        a = BoardManager.Instance.Board[CurrentX + 1, y];
                    }
                    else
                    {
                        // Check left only
                        a = BoardManager.Instance.Board[CurrentX - 1, y];
                    }
                    if (a != null)
                    {
                        if (isAttacking != a.isAttacking)
                        {
                            // Can't move here in this case
                            r[CurrentX, y] = false;
                            continue;
                        }
                    }
                }
                // In the middle (check both sides)
                else
                {
                    a = BoardManager.Instance.Board[CurrentX - 1, y];
                    b = BoardManager.Instance.Board[CurrentX + 1, y];
                    if (a != null && b != null)
                    {
                        if (isAttacking != a.isAttacking && isAttacking != b.isAttacking)
                        {
                            // Can't move here in this case
                            r[CurrentX, y] = false;
                            continue;
                        }
                    }
                }
            }
            // If we get here then the move must be valid
            r[CurrentX, y] = true;
        }
    }

}
