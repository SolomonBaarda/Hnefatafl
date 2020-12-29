using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class HumanAgent : MonoBehaviour, IAgent
{
    public BoardManager.Team Team { get; private set; }

    public void Instantiate(BoardManager.Team team)
    {
        Team = team;
    }


    public void GetMove(MDPEnvironment e, UnityAction<Vector2Int, Vector2Int> callback)
    {
        StartCoroutine(WaitForMove(e, callback));
    }

    private IEnumerator WaitForMove(MDPEnvironment e, UnityAction<Vector2Int, Vector2Int> callback)
    {
        if (e.IsTerminal)
        {
            Debug.LogError("Trying to get move from terminal state");
        }

        Vector2Int notSet = new Vector2Int(-1, -1);
        Vector2Int selected = notSet;
        bool moveRecieved = true;

        // Wait until the player makes a selection
        while (!moveRecieved)
        {
            if (Controller.Instance.IsHoveringOverBoard)
            {
                Vector2Int tile = BoardManager.Instance.GetTile(Controller.Instance.BoardHoverPosition);
                //Debug.Log("Hovering over tile " + tile.x + " " + tile.y);

                // Left click
                if (Controller.Instance.LeftClick)
                {
                    bool firstClick = false;

                    // Select new piece
                    MDPEnvironment.Tile t = e.Environment[tile.x, tile.y];
                    if (t != MDPEnvironment.Tile.Empty)
                    {
                        // Ensure the player is trying to select one of their own pieces
                        if (((t == MDPEnvironment.Tile.Attacking || t == MDPEnvironment.Tile.King) && Team == BoardManager.Team.Attacking)
                            || (t == MDPEnvironment.Tile.Defending && Team == BoardManager.Team.Defending))
                        {
                            if (selected.Equals(notSet) || !selected.Equals(tile))
                            {
                                selected = tile;
                                firstClick = true;
                            }
                        }
                    }

                    if (!firstClick)
                    {
                        // Move it if there is one selected
                        if (!selected.Equals(notSet))
                        {
                            // Move the piece
                            callback.Invoke(selected, tile);
                            moveRecieved = true;
                        }
                    }
                }
            }
        }

        // Wait until the next frame 
        if (!moveRecieved)
        {
            yield return null;
        }
    }

}
