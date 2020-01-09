using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{

    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }

    public Piece[,] Board { set; get; }
    private Piece selectedPiece;

    public const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    public const int BOARD_SIZE = 11;

    private int selectionX = -1;
    private int selectionY = -1;
    private int validHoverX = -1;
    private int validHoverY = -1;

    public List<GameObject> gamePiecePrefabs;
    private List<GameObject> activePieces;
    public List<GameObject> gameTilePrefabs;

    public bool isAttackingTurn;

    private void Start()
    {
        isAttackingTurn = true;
        Instance = this;

        CreateBoard();
        SetBoardPlane();
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

        if (selectedPiece != null)
        {
            if (selectionX >= 0 && selectionX < BOARD_SIZE && selectionY >= 0 && selectionY < BOARD_SIZE)
            {
                if (allowedMoves[selectionX, selectionY])
                {
                    // New hover piece has been selected
                    if (selectionX != validHoverX || selectionY != validHoverY)
                    {
                        validHoverX = selectionX;
                        validHoverY = selectionY;

                        BoardHighlight.Instance.HighlightPiecesToRemove(TilesToRemove(selectedPiece, selectionX, selectionY));
                        //BoardHighlight.Instance.HighlightHoverTile(selectedPiece, selectionX, selectionY, allowedMoves);
                    }
                    return;

                }
                validHoverX = -1;
                validHoverY = -1;
                BoardHighlight.Instance.HideHoverHighlight();
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

        // Set allowed moves
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

        // Set selected piece
        selectedPiece = Board[x, y];

        // Enable highlights
        BoardHighlight.Instance.HighlightSelectedTile(x, y);
        BoardHighlight.Instance.HighlightAllowedMoves(allowedMoves);
    }

    private void MovePiece(int x, int y)
    {
        if (allowedMoves[x, y])
        {
            // Kill any pieces
            foreach (Piece p in TilesToRemove(Board[selectedPiece.CurrentX, selectedPiece.CurrentY], x, y))
            {
                Kill(p.CurrentX, p.CurrentY);
            }

            // Make the move
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
        // TODO
        // Redo this updateBoard method
        // keep it in this method, but simplify it
        // allow for expansion to add a "preview move" function, that returns any cells 
        // that would be destroyed if the piece was to move there


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

                // Pointer to current tile
                e = Board[x, y];

                // Only bother to check if the cell contains a piece 
                if (e != null)
                {
                    // Check the 3x3 around the piece
                    if (y > 0 && x > 0 && y < BOARD_SIZE - 1 && x < BOARD_SIZE - 1)
                    {
                        a = Board[x - 1, y];
                        b = Board[x + 1, y];
                        c = Board[x, y - 1];
                        d = Board[x, y + 1];

                        // Check if the king is surrounded on all four sides (can't move)
                        if (e.isKing)
                        {
                            if (a != null && b != null && c != null && d != null)
                            {
                                if (e.isKing && !a.isAttacking && !b.isAttacking && !c.isAttacking && !d.isAttacking)
                                {
                                    // Surrounded on all sides
                                    // Defending wins 
                                    EndGame(false);
                                }
                            }
                        }
                    }
                    // The piece is at the edge of the board
                    // Check if it is pinned by a piece from the other team
                    else
                    {
                        // Horizontal
                        if (x == 0)
                        {
                            if (e.isKing)
                            {
                                a = Board[x + 1, y];
                                b = Board[x, y - 1];
                                c = Board[x, y + 1];

                                if (a != null && b != null && c != null)
                                {
                                    if (!a.isAttacking && !b.isAttacking && !c.isAttacking)
                                    {
                                        // Surrounded on all sides
                                        // Defending wins 
                                        EndGame(false);
                                    }
                                }
                            }

                        }
                        else if (x == BOARD_SIZE - 1)
                        {
                            if (e.isKing)
                            {
                                a = Board[x - 1, y];
                                b = Board[x, y - 1];
                                c = Board[x, y + 1];

                                if (a != null && b != null && c != null)
                                {
                                    if (!a.isAttacking && !b.isAttacking && !c.isAttacking)
                                    {
                                        // Surrounded on all sides
                                        // Defending wins 
                                        EndGame(false);
                                    }
                                }
                            }
                        }

                        // Vertical 
                        if (y == 0)
                        {
                            if (e.isKing)
                            {
                                a = Board[x, y + 1];
                                b = Board[x + 1, y];
                                c = Board[x - 1, y];

                                if (a != null && b != null && c != null)
                                {
                                    if (!a.isAttacking && !b.isAttacking && !c.isAttacking)
                                    {
                                        // Surrounded on all sides
                                        // Defending wins 
                                        EndGame(false);
                                    }
                                }
                            }
                        }
                        else if (y == BOARD_SIZE - 1)
                        {
                            if (e.isKing)
                            {
                                a = Board[x, y - 1];
                                b = Board[x + 1, y];
                                c = Board[x - 1, y];

                                if (a != null && b != null && c != null)
                                {
                                    if (!a.isAttacking && !b.isAttacking && !c.isAttacking)
                                    {
                                        // Surrounded on all sides
                                        // Defending wins 
                                        EndGame(false);
                                    }
                                }
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


    private bool KingIsTrapped(Piece start, int x, int y)
    {
        // TODO in progress

        if (start != null)
        {
            if (!start.isAttacking)
            {
                if (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE)
                {
                    Piece a, b, c, d;

                    // Not on the edge
                    if (x >= 1 && x < BOARD_SIZE - 1 && y >= 1 && y < BOARD_SIZE - 1)
                    {
                        a = Board[x - 1, y];
                        b = Board[x + 1, y];
                        c = Board[x, y - 1];
                        d = Board[x, y + 1];

                    }
                    else
                    {

                    }
                }
            }
        }

        return false;
    }


    private List<Piece> TilesToRemove(Piece start, int x, int y)
    {
        List<Piece> toRemove = new List<Piece>();

        if (start != null)
        {
            if (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE)
            {
                // Check all four directions for pieces to remove
                CheckTilesToRemoveX(start, x, y, +1, ref toRemove);
                CheckTilesToRemoveX(start, x, y, -1, ref toRemove);
                CheckTilesToRemoveY(start, x, y, +1, ref toRemove);
                CheckTilesToRemoveY(start, x, y, -1, ref toRemove);

            }
        }
        return toRemove;
    }



    private void CheckTilesToRemoveX(Piece start, int x, int y, int direction, ref List<Piece> toRemove)
    {
        // Reference to "middle" and "far" pieces
        Piece m, f;

        // Ensure not on the edge
        if (x >= 2 && x <= BOARD_SIZE - 3)
        {
            // Get reference to the pieces
            m = Board[x + direction, y];
            f = Board[x + (2 * direction), y];

            if (m != null && f != null)
            {
                if (start.isAttacking == f.isAttacking && start.isAttacking != m.isAttacking)
                {
                    if (!m.isKing)
                    {
                        // Middle piece needs to be removed
                        toRemove.Add(m);
                    }
                }
            }
        }
        else
        {
            // On the left side
            if (x == 1)
            {
                if (direction < 0)
                {
                    m = Board[x + direction, y];

                    if (m != null)
                    {
                        if (start.isAttacking != m.isAttacking)
                        {
                            if (!m.isKing)
                            {
                                // Middle piece needs to be removed
                                toRemove.Add(m);
                            }
                        }
                    }
                }

            }
            // On the right side
            else if (x == BOARD_SIZE - 2)
            {
                if (direction > 0)
                {
                    m = Board[x + direction, y];

                    if (m != null)
                    {
                        if (start.isAttacking != m.isAttacking)
                        {
                            if (!m.isKing)
                            {
                                // Middle piece needs to be removed
                                toRemove.Add(m);
                            }
                        }
                    }
                }
            }
        }

    }

    private void CheckTilesToRemoveY(Piece start, int x, int y, int direction, ref List<Piece> toRemove)
    {
        // Reference to "middle" and "far" pieces
        Piece m, f;

        // Ensure not on the edge
        if (y >= 2 && y <= BOARD_SIZE - 3)
        {
            // Get reference to the pieces
            m = Board[x, y + direction];
            f = Board[x, y + (2 * direction)];

            if (m != null && f != null)
            {
                if (start.isAttacking == f.isAttacking && start.isAttacking != m.isAttacking)
                {
                    if (!m.isKing)
                    {
                        // Middle piece needs to be removed
                        toRemove.Add(m);
                    }
                }
            }
        }
        else
        {
            // On the top
            if (y == 1)
            {
                if (direction < 0)
                {
                    m = Board[x, y + direction];

                    if (m != null)
                    {
                        if (start.isAttacking != m.isAttacking)
                        {
                            if (!m.isKing)
                            {
                                // Middle piece needs to be removed
                                toRemove.Add(m);
                            }
                        }
                    }
                }

            }
            // On the bottom
            else if (y == BOARD_SIZE - 2)
            {
                if (direction > 0)
                {
                    m = Board[x, y + direction];

                    if (m != null)
                    {
                        if (start.isAttacking != m.isAttacking)
                        {
                            if (!m.isKing)
                            {
                                // Middle piece needs to be removed
                                toRemove.Add(m);
                            }
                        }
                    }
                }
            }
        }

    }


    private void Kill(int x, int y)
    {
        Piece a = Board[x, y];
        Board[x, y] = null;
        activePieces.Remove(a.gameObject);
        Destroy(a.gameObject);

    }

    private void SetBoardPlane()
    {
        GameObject plane = GameObject.Find("BoardPlane");
        // Set the scale to fit the current board size
        plane.transform.localScale = new Vector3(BOARD_SIZE * 0.1f, 1, BOARD_SIZE * 0.1f);
        // Move the plane so that it is in the centre of the board
        plane.transform.position = new Vector3((float)(BOARD_SIZE) / 2, 0, (float)(BOARD_SIZE) / 2);
    }

    private void CreateBoard()
    {
        // Index 0: regular tile, 1: corners
        GameObject type;

        for (int y = 0; y < BOARD_SIZE; y++)
        {
            for (int x = 0; x < BOARD_SIZE; x++)
            {
                // Set corners to different visual
                if ((x == 0 || x == BOARD_SIZE - 1) && (y == 0 || y == BOARD_SIZE - 1))
                {
                    type = gameTilePrefabs[1];
                }
                else
                {
                    type = gameTilePrefabs[0];
                }

                // Create the tile
                GameObject go = Instantiate(type, new Vector3(x, -0.06f, y), Quaternion.identity) as GameObject;
                go.transform.SetParent(transform.Find("BoardVisual"));
            }
        }

    }

    private void SpawnAllPieces()
    {
        // Index 0: defending, 1: attacking, 2: king
        activePieces = new List<GameObject>();

        Board = new Piece[BOARD_SIZE, BOARD_SIZE];

        // Spawn the black pieces
        SpawnPiece(0, 0, (BOARD_SIZE / 2) - 1);
        SpawnPiece(0, 0, BOARD_SIZE / 2);
        SpawnPiece(0, 0, (BOARD_SIZE / 2) + 1);
        SpawnPiece(0, 1, BOARD_SIZE / 2);

        SpawnPiece(0, (BOARD_SIZE / 2) - 1, 0);
        SpawnPiece(0, BOARD_SIZE / 2, 0);
        SpawnPiece(0, (BOARD_SIZE / 2) + 1, 0);
        SpawnPiece(0, BOARD_SIZE / 2, 1);

        SpawnPiece(0, (BOARD_SIZE / 2) - 1, BOARD_SIZE - 1);
        SpawnPiece(0, BOARD_SIZE / 2, BOARD_SIZE - 1);
        SpawnPiece(0, (BOARD_SIZE / 2) + 1, BOARD_SIZE - 1);
        SpawnPiece(0, BOARD_SIZE / 2, BOARD_SIZE - 2);

        SpawnPiece(0, BOARD_SIZE - 1, (BOARD_SIZE / 2) - 1);
        SpawnPiece(0, BOARD_SIZE - 1, BOARD_SIZE / 2);
        SpawnPiece(0, BOARD_SIZE - 1, (BOARD_SIZE / 2) + 1);
        SpawnPiece(0, BOARD_SIZE - 2, BOARD_SIZE / 2);

        if (BOARD_SIZE >= 11)
        {
            SpawnPiece(0, 0, (BOARD_SIZE / 2) - 2);
            SpawnPiece(0, 0, (BOARD_SIZE / 2) + 2);

            SpawnPiece(0, (BOARD_SIZE / 2) - 2, 0);
            SpawnPiece(0, (BOARD_SIZE / 2) + 2, 0);

            SpawnPiece(0, (BOARD_SIZE / 2) - 2, BOARD_SIZE - 1);
            SpawnPiece(0, (BOARD_SIZE / 2) + 2, BOARD_SIZE - 1);

            SpawnPiece(0, BOARD_SIZE - 1, (BOARD_SIZE / 2) - 2);
            SpawnPiece(0, BOARD_SIZE - 1, (BOARD_SIZE / 2) + 2);
        }

        // Spawn the white pieces
        SpawnPiece(1, (BOARD_SIZE / 2) - 2, BOARD_SIZE / 2);
        SpawnPiece(1, (BOARD_SIZE / 2) - 1, BOARD_SIZE / 2);

        SpawnPiece(1, BOARD_SIZE / 2, (BOARD_SIZE / 2) - 2);
        SpawnPiece(1, BOARD_SIZE / 2, (BOARD_SIZE / 2) - 1);

        SpawnPiece(1, (BOARD_SIZE / 2) + 2, BOARD_SIZE / 2);
        SpawnPiece(1, (BOARD_SIZE / 2) + 1, BOARD_SIZE / 2);

        SpawnPiece(1, BOARD_SIZE / 2, (BOARD_SIZE / 2) + 2);
        SpawnPiece(1, BOARD_SIZE / 2, (BOARD_SIZE / 2) + 1);

        if (BOARD_SIZE >= 11)
        {
            // Add in the corners 
            SpawnPiece(1, (BOARD_SIZE / 2) - 1, (BOARD_SIZE / 2) - 1);
            SpawnPiece(1, (BOARD_SIZE / 2) + 1, (BOARD_SIZE / 2) - 1);
            SpawnPiece(1, (BOARD_SIZE / 2) - 1, (BOARD_SIZE / 2) + 1);
            SpawnPiece(1, (BOARD_SIZE / 2) + 1, (BOARD_SIZE / 2) + 1);
        }

        // Spawn the king 
        SpawnPiece(2, BOARD_SIZE / 2, BOARD_SIZE / 2);
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