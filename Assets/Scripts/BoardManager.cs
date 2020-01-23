using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { private set; get; }
    private bool[,] allowedMoves { set; get; }

    public Piece[,] Board { private set; get; }
    private List<GameObject> activePieces;
    private Piece selectedPiece;

    public const float TILE_SIZE = 1.0f;
    public const float TILE_OFFSET = 0.5f;
    public int BOARD_SIZE = 11;

    public bool isAttackingTurn;
    private bool isGameOver;
    public GameMode gameMode;

    private int selectionX = -1;
    private int selectionY = -1;
    private int validHoverX = -1;
    private int validHoverY = -1;

    public GameObject defendingPrefab;
    public GameObject attackingPrefab;
    public GameObject kingPrefab;
    public List<GameObject> gameTilePrefabs;
    private Piece king;

    public static event Action<Team> OnGameOver;

    public enum GameMode
    {
        Hnefatafl,
        Tablut,
    }

    public enum Team
    {
        Attacking,
        Defending,
    }

    private void Start()
    {
        Instance = this;

        LoadHUD();
        LoadGame();
    }

    public void LoadHUD()
    {
        if (!SceneManager.GetSceneByName("HUD").isLoaded)
        {
            SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
        }
    }

    public void LoadGame()
    {
        isAttackingTurn = true;
        isGameOver = false;

        if(gameMode.Equals(GameMode.Tablut))
        {
            BOARD_SIZE = 9;
        }
        else
        {
            BOARD_SIZE = 13;
        }

        CreateBoard();
        SetBoardPlane();
        SpawnAllPieces();
    }





    private void Update()
    {
        if (!isGameOver)
        {
            UpdateTileSelection();
            DrawBoard();

            // Left click
            if (Input.GetMouseButtonDown(0))
            {
                if (selectionX >= 0 && selectionY >= 0 && selectionX < BOARD_SIZE && selectionY < BOARD_SIZE)
                {
                    bool firstClick = false;

                    // Select new piece
                    if (Board[selectionX, selectionY] != null)
                    {
                        if (selectedPiece == null || selectedPiece != Board[selectionX, selectionY])
                        {
                            SelectPiece(selectionX, selectionY);
                            firstClick = true;
                        }

                    }

                    if (!firstClick)
                    {
                        // Move it if there is one selected
                        if (selectedPiece != null)
                        {
                            // Move the piece
                            MovePiece(selectionX, selectionY);
                        }
                    }
                }
            }

            // Do the highlights for hovering 
            if (selectedPiece != null)
            {
                if (selectionX >= 0 && selectionX < BOARD_SIZE && selectionY >= 0 && selectionY < BOARD_SIZE)
                {
                    if (allowedMoves[selectionX, selectionY])
                    {
                        // New hover piece has been selected
                        if (selectionX != validHoverX || selectionY != validHoverY)
                        {
                            // Set the currently shown hover tile
                            validHoverX = selectionX;
                            validHoverY = selectionY;

                            // Display the highlights 
                            BoardHighlight.Instance.HighlightPiecesToRemove(TilesToRemove(selectedPiece, selectionX, selectionY));
                            BoardHighlight.Instance.HighlightHoverTile(selectedPiece, selectionX, selectionY);
                        }
                        return;

                    }
                    // If we get here, the cell being hovered over is not valid, so reset the values and hide the highlight 
                    validHoverX = -1;
                    validHoverY = -1;
                    BoardHighlight.Instance.HideHoverHighlight();
                }
            }
        }

    }

    private void SelectPiece(int x, int y)
    {
        // Not a valid selection 
        if (Board[x, y] == null)
        {
            UnselectPiece();
            return;
        }
        if (Board[x, y].isAttacking != isAttackingTurn)
        {
            UnselectPiece();
            return;
        }

        bool hasAtLeastOneMove = false;

        // Set allowed moves
        bool[,] possibleAllowedMoves = Board[x, y].PossibleMove();

        // Set hasAtLeastOneMove
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                if (possibleAllowedMoves[i, j])
                {
                    hasAtLeastOneMove = true;
                    break;
                }
            }
        }

        // Do not select the piece if it cannot move
        if (!hasAtLeastOneMove)
        {
            UnselectPiece();
            return;
        }

        // Set selected piece
        selectedPiece = Board[x, y];
        allowedMoves = Board[x, y].PossibleMove();

        BoardHighlight.Instance.HideHighlights();

        // Enable highlights
        BoardHighlight.Instance.HighlightSelectedTile(x, y);
        BoardHighlight.Instance.HighlightAllowedMoves(allowedMoves);
    }

    private void UnselectPiece()
    {
        selectedPiece = null;
        allowedMoves = null;
        BoardHighlight.Instance.HideHighlights();
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

            // Check if the king has moved to the corner
            if (selectedPiece.isKing)
            {
                if ((selectedPiece.CurrentX == 0 || selectedPiece.CurrentX == BOARD_SIZE - 1) && (selectedPiece.CurrentY == 0 || selectedPiece.CurrentY == BOARD_SIZE - 1))
                {
                    // King has reached the corner
                    // Attacking wins
                    EndGame(true);
                }
            }

            // Check if a piece needs to be removed
            // Will be removed soon
            UpdateBoard();

            isAttackingTurn = !isAttackingTurn;
        }

        BoardHighlight.Instance.HideHighlights();
        selectedPiece = null;
    }

    private void UpdateBoard()
    {
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
            }
        }

        if (KingIsTrapped())
        {
            // Surrounded on all sides
            // Defending wins 
            EndGame(false);
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


    private bool KingIsTrapped()
    {
        // Initialise king
        if (king == null)
        {
            for (int y = 0; y < BOARD_SIZE; y++)
            {
                for (int x = 0; x < BOARD_SIZE; x++)
                {
                    if (Board[x, y] != null)
                    {
                        if (Board[x, y].isKing)
                        {
                            king = Board[x, y];
                            break;
                        }
                    }
                }
            }
        }
        // Reference to nearby pieces 
        Piece a, b, c, d;

        // Not at the edge
        if (king.CurrentX > 0 && king.CurrentX < BOARD_SIZE - 1 && king.CurrentY > 0 && king.CurrentY < BOARD_SIZE - 1)
        {
            // Check all four directions
            a = Board[king.CurrentX - 1, king.CurrentY];
            b = Board[king.CurrentX + 1, king.CurrentY];
            c = Board[king.CurrentX, king.CurrentY - 1];
            d = Board[king.CurrentX, king.CurrentY + 1];

            if (a != null && b != null && c != null && d != null)
            {
                if (!a.isAttacking && !b.isAttacking && !c.isAttacking && !d.isAttacking)
                {
                    // King is surrounded 
                    return true;
                }
            }
        }
        // At the edge
        else
        {
            // X axis
            if (king.CurrentX == 0 || king.CurrentX == BOARD_SIZE - 1)
            {
                if (king.CurrentX == 0)
                {
                    a = Board[king.CurrentX + 1, king.CurrentY];
                }
                else
                {
                    a = Board[king.CurrentX - 1, king.CurrentY];

                }
                b = Board[king.CurrentX, king.CurrentY - 1];
                c = Board[king.CurrentX, king.CurrentY + 1];

                if (a != null && b != null && c != null)
                {
                    if (!a.isAttacking && !b.isAttacking && !c.isAttacking)
                    {
                        // King is surrounded 
                        return true;
                    }
                }

            }
            // Y axis
            if (king.CurrentY == 0 || king.CurrentY == BOARD_SIZE - 1)
            {
                if (king.CurrentY == 0)
                {
                    a = Board[king.CurrentX, king.CurrentY + 1];
                }
                else
                {
                    a = Board[king.CurrentX, king.CurrentY - 1];
                }
                b = Board[king.CurrentX - 1, king.CurrentY];
                c = Board[king.CurrentX + 1, king.CurrentY];

                if (a != null && b != null && c != null)
                {
                    if (!a.isAttacking && !b.isAttacking && !c.isAttacking)
                    {
                        // King is surrounded 
                        return true;
                    }
                }
            }
        }

        // If we get here then the king is not trapped 
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
        GameObject plane = GameObject.FindWithTag("BoardPlane");
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
        activePieces = new List<GameObject>();

        Board = new Piece[BOARD_SIZE, BOARD_SIZE];

        // Spawn the black pieces
        SpawnPiece(defendingPrefab, 0, (BOARD_SIZE / 2) - 1);
        SpawnPiece(defendingPrefab, 0, BOARD_SIZE / 2);
        SpawnPiece(defendingPrefab, 0, (BOARD_SIZE / 2) + 1);
        SpawnPiece(defendingPrefab, 1, BOARD_SIZE / 2);

        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) - 1, 0);
        SpawnPiece(defendingPrefab, BOARD_SIZE / 2, 0);
        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) + 1, 0);
        SpawnPiece(defendingPrefab, BOARD_SIZE / 2, 1);

        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) - 1, BOARD_SIZE - 1);
        SpawnPiece(defendingPrefab, BOARD_SIZE / 2, BOARD_SIZE - 1);
        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) + 1, BOARD_SIZE - 1);
        SpawnPiece(defendingPrefab, BOARD_SIZE / 2, BOARD_SIZE - 2);

        SpawnPiece(defendingPrefab, BOARD_SIZE - 1, (BOARD_SIZE / 2) - 1);
        SpawnPiece(defendingPrefab, BOARD_SIZE - 1, BOARD_SIZE / 2);
        SpawnPiece(defendingPrefab, BOARD_SIZE - 1, (BOARD_SIZE / 2) + 1);
        SpawnPiece(defendingPrefab, BOARD_SIZE - 2, BOARD_SIZE / 2);

        if (BOARD_SIZE >= 11)
        {
            SpawnPiece(defendingPrefab, 0, (BOARD_SIZE / 2) - 2);
            SpawnPiece(defendingPrefab, 0, (BOARD_SIZE / 2) + 2);

            SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) - 2, 0);
            SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) + 2, 0);

            SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) - 2, BOARD_SIZE - 1);
            SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) + 2, BOARD_SIZE - 1);

            SpawnPiece(defendingPrefab, BOARD_SIZE - 1, (BOARD_SIZE / 2) - 2);
            SpawnPiece(defendingPrefab, BOARD_SIZE - 1, (BOARD_SIZE / 2) + 2);
        }

        // Spawn the white pieces
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 2, BOARD_SIZE / 2);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 1, BOARD_SIZE / 2);

        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) - 2);
        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) - 1);

        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 2, BOARD_SIZE / 2);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 1, BOARD_SIZE / 2);

        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) + 2);
        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) + 1);

        if (BOARD_SIZE >= 11)
        {
            // Add in the corners 
            SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 1, (BOARD_SIZE / 2) - 1);
            SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 1, (BOARD_SIZE / 2) - 1);
            SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 1, (BOARD_SIZE / 2) + 1);
            SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 1, (BOARD_SIZE / 2) + 1);
        }

        // Spawn the king 
        SpawnPiece(kingPrefab, BOARD_SIZE / 2, BOARD_SIZE / 2);
    }

    private void SpawnPiece(GameObject o, int x, int y)
    {
        GameObject go = Instantiate(o, GetTileCentre(x, y), Quaternion.identity) as GameObject;
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
        isGameOver = true;

        if (attackingTeamWon)
        {
            OnGameOver.Invoke(Team.Attacking);
            Debug.Log("Attacking team won.");
        }
        else
        {
            OnGameOver.Invoke(Team.Defending);
            Debug.Log("Defending team won.");
        }

        
    }


    public void ResetGame()
    {
        foreach (GameObject go in activePieces)
        {
            Destroy(go);
        }

        BoardHighlight.Instance.HideHighlights();
        SpawnAllPieces();
        isAttackingTurn = true;
        isGameOver = false;

        Debug.Log("Game has been reset.");
    }



}