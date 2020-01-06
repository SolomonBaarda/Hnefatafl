using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlight : MonoBehaviour
{
    public static BoardHighlight Instance { set; get; }

    public GameObject highlightPossibleMovesPrefab;
    public GameObject highlightSelectedTilePrefab;
    private List<GameObject> highlights;

    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
    }

    private GameObject GetHighlightObject(GameObject highlightPrefab)
    {
        // Find the highlight prefab type
        GameObject go = highlights.Find(g => !g.activeSelf);

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
        for (int i = 0; i < BoardManager.BOARD_SIZE; i++)
        {
            for (int j = 0; j < BoardManager.BOARD_SIZE; j++)
            {
                // If the tile should be highlighted
                if (moves[i, j])
                {
                    // Get the highlight prefab
                    GameObject go = GetHighlightObject(highlightPossibleMovesPrefab);

                    // Set it active
                    go.SetActive(true);
                    go.transform.position = new Vector3(i, 0, j);
                }
            }
        }
    }

    public void HighlightSelectedTile(int tileX, int tileY)
    {
        // Get the highlight prefab
        GameObject go = GetHighlightObject(highlightSelectedTilePrefab);

        // Set it active
        go.SetActive(true);
        go.transform.position = new Vector3(tileX, 0, tileY);
    }


    public void HideHighlights()
    {
        foreach (GameObject go in highlights)
        {
            go.SetActive(false);
        }
    }

}
