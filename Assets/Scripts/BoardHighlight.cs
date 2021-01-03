using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardHighlight : MonoBehaviour
{
    public static BoardHighlight Instance { set; get; }

    private List<GameObject> highlights = new List<GameObject>();

    public Transform HighlightsParent;
    public float yOffset = 0.01f;
    public float transparency = 0.5f;

    [Header("Hover Prefabs")]
    public GameObject possibleMovesPrefab;
    public GameObject selectedTilePrefab;
    public GameObject pieceToRemovePrefab;

    private GameObject kingPrefab, attackingPrefab, defendingPrefab;

    public const string Hover_Tag = "HoverHighlight";

    private void Awake()
    {
        Instance = this;
    }


    // TODO fix the hover highlight prefabs - not equal as changed alpha material colour value



    private void Start()
    {
        kingPrefab = Instantiate(BoardManager.Instance.kingPrefab, transform);
        SetTransparent(kingPrefab, transparency);
        kingPrefab.SetActive(false);

        attackingPrefab = Instantiate(BoardManager.Instance.attackingPrefab, transform);
        SetTransparent(attackingPrefab, transparency);
        attackingPrefab.SetActive(false);

        defendingPrefab = Instantiate(BoardManager.Instance.defendingPrefab, transform);
        SetTransparent(defendingPrefab, transparency);
        defendingPrefab.SetActive(false);
    }

    private GameObject GetHighlightObject(GameObject highlightPrefab)
    {
        // Find a non active instance of the prefab
        GameObject go = highlights.Find(g => !g.activeInHierarchy && g.Equals(highlightPrefab));

        // Add one if there are none
        if (go == null)
        {
            go = Instantiate(highlightPrefab, HighlightsParent);
            highlights.Add(go);
        }

        return go;
    }

    public void HighlightAllowedMoves(List<Vector2Int> allowed)
    {
        allowed.ForEach((x) => EnableHighlight(x, possibleMovesPrefab));
    }

    public void HighlightSelectedTile(Vector2Int tile)
    {
        EnableHighlight(tile, selectedTilePrefab);
    }

    public void HighlightPiecesToRemove(List<Vector2Int> toRemove)
    {
        toRemove.ForEach((x) => EnableHighlight(x, pieceToRemovePrefab));
    }

    private GameObject EnableHighlight(Vector2Int tile, GameObject prefab)
    {
        GameObject g = GetHighlightObject(prefab);
        g.SetActive(true);

        // Set the position of the highlight to be the centre of the tile + y offset
        g.transform.position = BoardManager.Instance.GetTileWorldPositionCentre(tile.x, tile.y) + new Vector3(0, yOffset, 0);

        return g;
    }

    public void HighlightHoverForTile(Vector2Int tile, MDPEnvironment.Tile type)
    {
        HideAllHoverHighlights();

        switch (type)
        {
            case MDPEnvironment.Tile.Defending:
                SetHoverHighlightPreview(tile, defendingPrefab);
                break;
            case MDPEnvironment.Tile.Attacking:
                SetHoverHighlightPreview(tile, attackingPrefab);
                break;
            case MDPEnvironment.Tile.King:
                SetHoverHighlightPreview(tile, kingPrefab);
                break;
        }
    }

    private void SetHoverHighlightPreview(Vector2Int tile, GameObject prefab)
    {
        GameObject g = EnableHighlight(tile, BoardManager.Instance.kingPrefab);
        g.transform.tag = Hover_Tag;
    }


    private void SetTransparent(GameObject g, float percent)
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


    public void HideAllHighlights()
    {
        highlights.ForEach((x) => x.SetActive(false));
    }

    public void HideAllHoverHighlights()
    {
        highlights.FindAll((x) => x.transform.tag == Hover_Tag).ForEach((x) => x.SetActive(false));
    }

}
