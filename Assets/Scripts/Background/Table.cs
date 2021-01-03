using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Table : MonoBehaviour
{
    public GameObject cornerTile;
    public GameObject regularTile;
    public GameObject table;

    public float boardVisualThickness = 0.5f;
    public float boardVisualExtraEdge = 5;

    private void Start()
    {
        transform.position = BoardManager.Instance.GetBoardCentreWorldPosition();

        CreateBoard();
        CreateTable();
    }


    private void CreateBoard()
    {
        // Index 0: regular tile, 1: corners
        GameObject type;

        for (int y = 0; y < BoardManager.Instance.BOARD_SIZE; y++)
        {
            for (int x = 0; x < BoardManager.Instance.BOARD_SIZE; x++)
            {
                // Set corners to different visual
                if ((x == 0 || x == BoardManager.Instance.BOARD_SIZE - 1) && (y == 0 || y == BoardManager.Instance.BOARD_SIZE - 1))
                {
                    type = cornerTile;
                }
                else
                {
                    type = regularTile;
                }

                // Create the tile
                GameObject go = Instantiate(type, new Vector3(x, transform.position.y + boardVisualThickness / 2, y), Quaternion.identity) as GameObject;
                go.transform.SetParent(transform.Find("BoardVisual"));
            }
        }

    }


    private void CreateTable()
    {
        GameObject go = Instantiate(table, transform.position, Quaternion.identity) as GameObject;
        go.transform.SetParent(transform.Find("TableVisual"));

        // Set scale
        go.transform.localScale = new Vector3(BoardManager.Instance.BOARD_SIZE + boardVisualExtraEdge, 1, BoardManager.Instance.BOARD_SIZE + boardVisualExtraEdge);
        // Set position
        Vector3 pos = go.transform.position;
        pos.y -= boardVisualThickness + (transform.localScale.y / 2);
        go.transform.position = pos;
    }


}
