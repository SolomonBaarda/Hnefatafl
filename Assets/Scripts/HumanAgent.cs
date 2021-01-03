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
        Vector2Int hoveringTile = notSet;

        // Wait until the player makes a selection
        while (true)
        {
            if (Controller.Instance.IsHoveringOverBoard)
            {
                Vector2Int newHoveringTile = BoardManager.Instance.GetTile(Controller.Instance.BoardHoverPosition);
                bool newHoverTile = !hoveringTile.Equals(newHoveringTile);
                hoveringTile = newHoveringTile;

                //Debug.Log("Hovering over tile " + hoveringTile.x + " " + hoveringTile.y);

                // Select a piece
                if (Controller.Instance.LeftClick)
                {
                    MDPEnvironment.Tile t = e.Environment[hoveringTile.x, hoveringTile.y];

                    // Ensure the player is trying to select one of their own pieces
                    if (t != MDPEnvironment.Tile.Empty && MDPEnvironment.IsOnTeam(t, Team))
                    {
                        if (selected.Equals(notSet) || !selected.Equals(hoveringTile))
                        {
                            // Check if this piece can make any moves
                            List<Vector2Int> moves = e.GetAllPossibleMoves(hoveringTile, out bool hasMoves);
                            if (hasMoves)
                            {
                                selected = hoveringTile;


                                // Enable the highlights for this selected tile
                                BoardHighlight.Instance.HideAllHighlights();
                                BoardHighlight.Instance.HighlightAllowedMoves(moves);
                                BoardHighlight.Instance.HighlightSelectedTile(selected);

                                //Debug.Log("Selected piece at " + selected.x + "," + selected.y + " with " + moves.Count + " moves.");
                                // Continue so that we wait for the next mouse press before trying to move again
                                continue;
                            }

                        }
                    }


                    // Move it if there is one selected and it is a valid move
                    if (!selected.Equals(notSet) && e.IsValidMove(selected, hoveringTile))
                    {
                        // Make the move
                        callback.Invoke(new Move(selected, hoveringTile, Team));

                        BoardHighlight.Instance.HideAllHighlights();
                        break;
                    }
                }

                // Now do the hover highlights
                if (newHoverTile)
                {
                    BoardHighlight.Instance.HideAllHoverHighlights();

                    if (!selected.Equals(notSet) && e.IsValidMove(selected, hoveringTile))
                    {
                        MDPEnvironment.Tile selectedTileType = e.Environment[selected.x, selected.y];
                        BoardHighlight.Instance.HighlightHoverForTile(hoveringTile, selectedTileType);
                    }
                }
            }

            yield return null;
        }
    }

}
