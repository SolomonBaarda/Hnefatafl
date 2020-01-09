using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlight : MonoBehaviour
{
    public static BoardHighlight Instance { set; get; }

    public GameObject highlightPossibleMovesPrefab;
    public GameObject highlightSelectedTilePrefab;
    public GameObject highlightPieceToRemovePrefab;
    private List<GameObject> highlights;

    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(GameObject highlightPrefab, string tag)
    {
        // Find the highlight prefab type
        GameObject go = highlights.Find(g => !g.activeSelf && g.CompareTag(tag));

        // Add it if it is not there
        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            go.transform.SetParent(transform.Find("BoardHighlights"));
            highlights.Add(go);
        }

        // Return it
        return go;
    }

    public void HighlightAllowedMoves(bool[,] moves)
    {
        // Loop though all tiles
        for (int y = 0; y < BoardManager.BOARD_SIZE; y++)
        {
            for (int x = 0; x < BoardManager.BOARD_SIZE; x++)
            {
                // If the tile should be highlighted
                if (moves[x, y])
                {
                    // Get the highlight prefab
                    GameObject go = GetHighlightObject(highlightPossibleMovesPrefab, "HighlightPossibleMove");

                    // Set it active
                    go.SetActive(true);
                    go.transform.position = new Vector3(x, 0, y);
                }
            }
        }
    }

    public void HighlightSelectedTile(int tileX, int tileY)
    {
        // Get the highlight prefab
        GameObject go = GetHighlightObject(highlightSelectedTilePrefab, "HighlightSelectedTile");

        // Set it active
        go.SetActive(true);
        go.transform.position = new Vector3(tileX, 0, tileY);
    }


    public void HighlightPiecesToRemove(List<Piece> toRemove)
    {
        HideHoverHighlight();

        foreach(Piece p in toRemove)
        {
            // Get the highlight prefab
            GameObject go = GetHighlightObject(highlightPieceToRemovePrefab, "HighlightPieceToRemove");

            // Set it active
            go.SetActive(true);
            go.transform.position = new Vector3(p.CurrentX, 0.01f, p.CurrentY);
        }
    }


    public void HighlightHoverTile(Piece selected, int hoverX, int hoverY, bool[,] validMoves)
    {
        print(hoverX + ", " + hoverY);
        if (hoverX >= 0 && hoverX < BoardManager.BOARD_SIZE && hoverY >= 0 && hoverY < BoardManager.BOARD_SIZE)
        {
            if (selected != null)
            {
                if (validMoves[hoverX, hoverY])
                {
                    if (hoverX != selected.CurrentX && hoverY != selected.CurrentY)
                    {
                        GameObject hover;
                        if (selected.isAttacking)
                        {
                            //hover = attackingTileHover;
                        }
                        else
                        {
                            //hover = defendingTileHover;
                        }

                        hover.SetActive(true);
                        hover.transform.position = new Vector3(hoverX + (BoardManager.TILE_SIZE / 2), 0, hoverY + (BoardManager.TILE_SIZE / 2));
                        return;
                    }
                }
            }
        }

        HideHoverHighlight();
    }


    public void HideHighlights()
    {
        foreach (GameObject go in highlights)
        {
            go.SetActive(false);
        }
    }

    public void HideHoverHighlight()
    {
        // The child of the prefab must contain the "HoverHighlight" tag to be disabled

        // Loop through all highlights 
        foreach (GameObject go in highlights)
        {
            // Check the children of each
            foreach(Transform t in go.transform)
            {
                // Disable all objects tagged 
                if (t.CompareTag("HoverHighlight"))
                {
                    go.SetActive(false);
                }
            }

        }
    }

}
