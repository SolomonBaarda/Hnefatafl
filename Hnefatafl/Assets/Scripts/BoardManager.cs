using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }

    public Piece[,] Board { set; get; }
    private Piece selectedPiece;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;
    public const int BOARD_SIZE = 9;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> gamePiecePrefabs;
    private List<GameObject> activePieces;
    public GameObject tilePrefab;

    public bool isAttackingTurn;

    private void Start()
    {
        isAttackingTurn = true;

        Instance = this;
        CreateBoard();
        SpawnAllPieces();
    }

    private void Update()
    {
        UpdateTileSelection();
        DrawBoard();

        // Left click
        if (Input.GetMouseButtonDown(0))
        {
            if (selectionX >= 0 && selectionY >= 0)
            {
                if (selectedPiece == null)
                {
                    // Select the new piece
                    SelectPiece(selectionX, selectionY);
                }
                else
                {
                    // Move the piece
                    MovePiece(selectionX, selectionY);
                }
            }
        }
    }

    private void SelectPiece(int x, int y)
    {
        // Not a valid selection 
        if (Board[x, y] == null)
        {
            return;
        }
        if (Board[x, y].isAttacking != isAttackingTurn)
        {
            return;
        }

        bool hasAtLeastOneMove = false;
        allowedMoves = Board[x, y].PossibleMove();

        // Set hasAtLeastOneMove
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (allowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    break;
                }
            }
        }

        // Do not select the piece if it cannot move
        if (!hasAtLeastOneMove)
        {
            return;
        }

        selectedPiece = Board[x, y];
        BoardHighlight.Instance.HighlightSelectedTile(x, y);
        BoardHighlight.Instance.HighlightAllowedMoves(allowedMoves);
    }

    private void MovePiece(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            Piece p = Board[x, y];

            Board[selectedPiece.CurrentX, selectedPiece.CurrentY] = null;
            selectedPiece.transform.position = GetTileCentre(x, y);
            selectedPiece.SetPosition(x, y);
            Board[x, y] = selectedPiece;

            // Check if a piece needs to be removed
            UpdateBoard();

            isAttackingTurn = !isAttackingTurn;
        }

        BoardHighlight.Instance.HideHighlights();
        selectedPiece = null;
    }

    private void UpdateBoard()
    {
        Piece a, b, c, d, e;
        int attackingCount = 0, defendingCount = 0;

        // Loop through the board
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                // Count the number of active tiles
                if (Board[x, y] != null)
                {
                    if (Board[x, y].isAttacking)
                    {
                        attackingCount++;
                    }
                    else
                    {
                        defendingCount++;
                    }
                }

                // Check the 3x3
                if (y > 0 && x > 0 && y < BOARD_SIZE - 1 && x < BOARD_SIZE - 1)
                {
                    a = Board[x - 1, y];
                    b = Board[x + 1, y];
                    c = Board[x, y - 1];
                    d = Board[x, y + 1];

                    e = Board[x, y];

                    // Check if the king is surrounded on all four sides (can't move)
                    if (a != null && b != null && c != null && d != null && e != null)
                    {
                        if (e.isKing && !a.isAttacking && !b.isAttacking && !c.isAttacking && !d.isAttacking)
                        {
                            // Surrounded on all sides
                            // Defending wins 
                            EndGame(false);
                        }
                    }

                    // Check horizontal
                    if (a != null && b != null && e != null)
                    {
                        // Check if middle is different to the outside ones
                        if (e.isAttacking != a.isAttacking && a.isAttacking == b.isAttacking)
                        {
                            if (!e.isKing)
                            {
                                // Delete piece 
                                Kill(e.CurrentX, e.CurrentY);
                            }
                        }
                    }

                    // Check vertical
                    if (c != null && d != null && e != null)
                    {
                        // Check if middle is different to the outside ones
                        if (e.isAttacking != c.isAttacking && c.isAttacking == d.isAttacking)
                        {
                            if (!e.isKing)
                            {
                                // Delete piece 
                                Kill(e.CurrentX, e.CurrentY);
                            }
                        }
                    }
                }
            }
        }

        a = Board[0, 0];
        b = Board[0, BOARD_SIZE - 1];
        c = Board[BOARD_SIZE - 1, 0];
        d = Board[BOARD_SIZE - 1, BOARD_SIZE - 1];

        if (a != null)
        {
            if (a.isKing)
            {
                // King has reached the corner
                // Attacking wins
                EndGame(true);
            }
        }
        if (b != null)
        {
            if (b.isKing)
            {
                // King has reached the corner
                // Attacking wins
                EndGame(true);
            }
        }
        if (c != null)
        {
            if (c.isKing)
            {
                // King has reached the corner
                // Attacking wins
                EndGame(true);
            }
        }
        if (d != null)
        {
            if (d.isKing)
            {
                // King has reached the corner
                // Attacking wins
                EndGame(true);
            }
        }

        if (attackingCount < 2)
        {
            // Defending wins
            EndGame(false);
        }
        if (defendingCount < 2)
        {
            // Attacking wins
            EndGame(true);
        }

    }

    private void Kill(int x, int y)
    {
        Piece a = Board[x, y];
        Board[x, y] = null;
        activePieces.Remove(a.gameObject);
        Destroy(a.gameObject);

    }



    private void CreateBoard()
    {
        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                GameObject go = Instantiate(tilePrefab, new Vector3(x, -0.06f, y), Quaternion.identity) as GameObject;
                go.transform.SetParent(transform.Find("BoardVisual"));
            }
        }

    }


    private void SpawnAllPieces()
    {
        // Index 0: defending, 1: attacking, 2: king
        activePieces = new List<GameObject>(); activePieces = new List<GameObject>();

        Board = new Piece[BOARD_SIZE, BOARD_SIZE];

        // Spawn the black pieces
        SpawnPiece(0, 0, 3);
        SpawnPiece(0, 0, 4);
        SpawnPiece(0, 0, 5);
        SpawnPiece(0, 1, 4);

        SpawnPiece(0, 3, 0);
        SpawnPiece(0, 4, 0);
        SpawnPiece(0, 5, 0);
        SpawnPiece(0, 4, 1);

        SpawnPiece(0, 3, 8);
        SpawnPiece(0, 4, 8);
        SpawnPiece(0, 5, 8);
        SpawnPiece(0, 4, 7);

        SpawnPiece(0, 8, 3);
        SpawnPiece(0, 8, 4);
        SpawnPiece(0, 8, 5);
        SpawnPiece(0, 7, 4);

        // Spawn the white pieces
        SpawnPiece(1, 3, 3);
        SpawnPiece(1, 3, 4);
        SpawnPiece(1, 3, 5);
        SpawnPiece(1, 4, 3);
        SpawnPiece(1, 4, 5);
        SpawnPiece(1, 5, 3);
        SpawnPiece(1, 5, 4);
        SpawnPiece(1, 5, 5);

        // Spawn the king 
        SpawnPiece(2, 4, 4);
    }

    private void SpawnPiece(int index, int x, int y)
    {
        GameObject go = Instantiate(gamePiecePrefabs[index], GetTileCentre(x, y), Quaternion.identity) as GameObject;
        go.transform.SetParent(transform.Find("GamePieces"));

        Board[x, y] = go.GetComponent<Piece>();
        Board[x, y].SetPosition(x, y);

        activePieces.Add(go);
    }

    private Vector3 GetTileCentre(int tileX, int tileY)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * tileX) + TILE_OFFSET;
        origin.z += (TILE_SIZE * tileY) + TILE_OFFSET;

        return origin;
    }


    private void UpdateTileSelection()
    {
        if (Camera.main)
        {
            RaycastHit hit;
            // Raycast from mouse point to the plane
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane")))
            {

                // Get the x and y tile position
                selectionX = (int)hit.point.x;
                selectionY = (int)hit.point.z;
            }
            else
            {
                // Reset them
                selectionX = -1;
                selectionY = -1;
            }
        }
    }


    private void DrawBoard()
    {
        // Lines for board size
        Vector3 widthLine = Vector3.right * BOARD_SIZE;
        Vector3 heightLine = Vector3.forward * BOARD_SIZE;

        // Draw board gizmos
        for (int i = 0; i <= BOARD_SIZE; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + widthLine);

            for (int j = 0; j <= BOARD_SIZE; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heightLine);
            }
        }

        // Draw selection
        if (selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));
            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward * selectionY + Vector3.right * (selectionX + 1));
        }
    }



    private void EndGame(bool attackingTeamWon)
    {
        if (attackingTeamWon)
        {
            Debug.Log("Attacking team wins!");
        }
        else
        {
            Debug.Log("Defending team wins!");
        }

        foreach (GameObject go in activePieces)
        {
            Destroy(go);
        }

        isAttackingTurn = true;
        BoardHighlight.Instance.HideHighlights();
        SpawnAllPieces();
    }

}