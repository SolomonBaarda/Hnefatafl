using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HumanAgent : MonoBehaviour, IAgent
{
    public BoardManager.Team Team { get; private set; }

    public void Instantiate(BoardManager.Team team)
    {
        Team = team;
    }

    public void GetMove(MDPEnvironment e, UnityAction<Move> callback)
    {
        StartCoroutine(WaitForMove(e, callback));
    }

    private IEnumerator WaitForMove(MDPEnvironment e, UnityAction<Move> callback)
    {
        if (e.IsTerminal)
        {
            Debug.LogError("Trying to get move from terminal state.");
        }

        Vector2Int notSet = new Vector2Int(-1, -1);
        Vector2Int selected = notSet;

        // Wait until the player makes a selection
        while (true)
        {
            if (Controller.Instance.IsHoveringOverBoard)
            {
                Vector2Int hoveringTile = BoardManager.Instance.GetTile(Controller.Instance.BoardHoverPosition);
                //Debug.Log("Hovering over tile " + hoveringTile.x + " " + hoveringTile.y);

                // Left click
                if (Controller.Instance.LeftClick)
                {
                    bool firstClick = false;

                    // Select new piece
                    MDPEnvironment.Tile t = e.Environment[hoveringTile.x, hoveringTile.y];
                    if (t != MDPEnvironment.Tile.Empty)
                    {
                        // Ensure the player is trying to select one of their own pieces
                        if (MDPEnvironment.IsOnTeam(t, Team))
                        {
                            if (selected.Equals(notSet) || !selected.Equals(hoveringTile))
                            {
                                // Check if this piece can make any moves
                                List<Vector2Int> moves = e.GetAllPossibleMoves(hoveringTile, out bool hasMoves);
                                if (hasMoves)
                                {
                                    selected = hoveringTile;
                                    firstClick = true;

                                    //Debug.Log("Selected piece at " + selected.x + "," + selected.y + " with " + moves.Count + " moves.");
                                }
                            }
                        }
                    }

                    if (!firstClick)
                    {
                        // Move it if there is one selected and it is a valid move
                        if (!selected.Equals(notSet) && e.IsValidMove(selected, hoveringTile))
                        {
                            // Move the piece
                            callback.Invoke(new Move(selected, hoveringTile, Team));
                            break;
                        }

                    }
                }
            }

            yield return null;
        }
    }

}
