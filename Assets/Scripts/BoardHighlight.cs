using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlight : MonoBehaviour
{
    public static BoardHighlight Instance { set; get; }

    public GameObject possibleMovesPrefab;
    public GameObject selectedTilePrefab;
    public GameObject pieceToRemovePrefab;

    public GameObject defendingPrefab;
    public GameObject attackingPrefab;
    public GameObject kingPrefab;

    public float yOffset = 0.01f;
    public float transparency = 0.5f;

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
        for (int y = 0; y < BoardManager.Instance.BOARD_SIZE; y++)
        {
            for (int x = 0; x < BoardManager.Instance.BOARD_SIZE; x++)
            {
                // If the tile should be highlighted
                if (moves[x, y])
                {
                    // Get the highlight prefab
                    GameObject go = GetHighlightObject(possibleMovesPrefab, "HighlightPossibleMove");

                    // Set it active
                    go.SetActive(true);
                    go.transform.position = new Vector3(x, yOffset, y);
                }
            }
        }
    }

    public void HighlightSelectedTile(int tileX, int tileY)
    {
        // Get the highlight prefab
        GameObject go = GetHighlightObject(selectedTilePrefab, "HighlightSelectedTile");

        // Set it active
        go.SetActive(true);
        go.transform.position = new Vector3(tileX, yOffset, tileY);
    }


    public void HighlightPiecesToRemove(List<Piece> toRemove)
    {
        HideHoverHighlight();

        if (toRemove != null)
        {
            foreach (Piece p in toRemove)
            {
                // Get the highlight prefab
                GameObject go = GetHighlightObject(pieceToRemovePrefab, "HighlightPieceToRemove");

                // Set it active
                go.SetActive(true);
                go.transform.position = new Vector3(p.CurrentX, yOffset, p.CurrentY);
            }
        }

    }


    public void HighlightHoverTile(Piece selected, int hoverX, int hoverY)
    {
        if (selected != null)
        {
            if (hoverX >= 0 && hoverX < BoardManager.Instance.BOARD_SIZE && hoverY >= 0 && hoverY < BoardManager.Instance.BOARD_SIZE)
            {
                // Set the correct prefab
                GameObject hover;
                if (selected.isAttacking)
                {
                    if (selected.isKing)
                    {
                        hover = GetHighlightObject(kingPrefab, "HighlightKing");
                    }
                    else
                    {
                        hover = GetHighlightObject(attackingPrefab, "HighlightAttacking");
                    }
                }
                else
                {
                    hover = GetHighlightObject(defendingPrefab, "HighlightDefending");
                }

                hover.SetActive(true);
                hover.transform.position = new Vector3(hoverX + (BoardManager.TILE_SIZE / 2), 0, hoverY + (BoardManager.TILE_SIZE / 2));

                SetTransparent(hover, transparency);

                return;
            }
        }

        HideHoverHighlight();
    }


    public void SetTransparent(GameObject g, float percent)
    {
        // Set each piece highlight to be transparent
        foreach (Transform t in g.transform)
        {
            if (t.GetComponent<MeshRenderer>())
            {
                // Can't assign alpha seperately so have to do it this way
                Color c = t.GetComponent<MeshRenderer>().material.color;
                c.a = percent;
                t.GetComponent<MeshRenderer>().material.color = c;
            }
        }
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
            foreach (Transform t in go.transform)
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
