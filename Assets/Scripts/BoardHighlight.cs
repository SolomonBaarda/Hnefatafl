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

    [Header("Tile Prefabs")]
    public GameObject PrefabPossibleMoves;
    public GameObject PrefabSelectedTile;
    public GameObject PrefabTilesToKill;

    [Header("Piece Prefabs")]
    public GameObject PrefabKingPreview;
    public GameObject PrefabAttackingPreview;
    public GameObject PrefabDefendingPreview;

    public const string Hover_Tag = "HoverHighlight";

    private void Awake()
    {
        Instance = this;
    }

    private GameObject EnableHighlight(Vector2Int tile, GameObject prefab, string name)
    {
        GameObject g = GetHighlightObject(prefab, name);
        g.SetActive(true);

        // Set the position of the highlight to be the centre of the tile + y offset
        g.transform.position = BoardManager.Instance.GetTileWorldPositionCentre(tile.x, tile.y) + new Vector3(0, yOffset, 0);

        return g;
    }

    private GameObject GetHighlightObject(GameObject highlightPrefab, string name)
    {
        // Find a non active instance of the prefab
        GameObject g = highlights.Find(x => !x.activeInHierarchy && x.name.Equals(name));

        // Add one if there are none
        if (g == null)
        {
            g = Instantiate(highlightPrefab, HighlightsParent);
            g.name = name;
            highlights.Add(g);
        }

        return g;
    }

    public void HighlightAllowedMoves(List<Vector2Int> allowed)
    {
        allowed.ForEach((x) => EnableHighlight(x, PrefabPossibleMoves, "HighlightAllowedMoves"));
    }

    public void HighlightSelectedTile(Vector2Int tile)
    {
        EnableHighlight(tile, PrefabSelectedTile, "HighlightSelectedTile");
    }

    public void HighlightPiecesToKill(List<Vector2Int> toRemove)
    {
        toRemove.ForEach((x) => EnableHighlight(x, PrefabTilesToKill, "HighlightPiecesToKill").tag = Hover_Tag);
    }

    public void HighlightHoverForTile(Vector2Int tile, MDPEnvironment.Tile type)
    {
        HideAllHoverHighlights();

        switch (type)
        {
            case MDPEnvironment.Tile.Defending:
                EnableHighlight(tile, PrefabDefendingPreview, "HighlightDefendingPiece").tag = Hover_Tag;
                break;
            case MDPEnvironment.Tile.Attacking:
                EnableHighlight(tile, PrefabAttackingPreview, "HighlightAttackingPiece").tag = Hover_Tag;
                break;
            case MDPEnvironment.Tile.King:
                EnableHighlight(tile, PrefabKingPreview, "HighlightKingPiece").tag = Hover_Tag;
                break;
        }
    }

    public void HideAllHighlights()
    {
        highlights.ForEach((x) => x.SetActive(false));
    }

    public void HideAllHoverHighlights()
    {
        highlights.FindAll((x) => x.transform.CompareTag(Hover_Tag)).ForEach((x) => x.SetActive(false));
    }

}
