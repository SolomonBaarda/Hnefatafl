﻿using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { private set; get; }
    private bool[,] allowedMoves;

    public Piece[,] Board { private set; get; }
    private List<GameObject> activePieces;
    private Piece selectedPiece;

    public const float TILE_SIZE = 1.0f;
    public int BOARD_SIZE = 13;

    public GameState State { get; private set; }

    public GameObject defendingPrefab;
    public GameObject attackingPrefab;
    public GameObject kingPrefab;
    private Piece king;

    public static event Action<Team> OnGameWon;
    public static event Action<GameState> OnTurnStart;

    public static LayerMask PLANE_MASK => LayerMask.GetMask("BoardPlane");
    public Transform BoardPlane;
    public Transform PiecesParent;

    public enum Team
    {
        Attacking,
        Defending,
    }

    public enum GameState
    {
        AttackingTurn,
        DefendingTurn,
        GameOver,
    }

    private void Start()
    {
        LoadGame();
    }

    public void LoadGame()
    {
        Instance = this;

        State = GameState.AttackingTurn;

        SetBoardPlane();
        SpawnAllPieces();

        if (!SceneManager.GetSceneByName("HUD").isLoaded)
        {
            SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
        }

        if (!SceneManager.GetSceneByName("Background").isLoaded)
        {
            SceneManager.LoadSceneAsync("Background", LoadSceneMode.Additive);
        }
    }


    private void Update()
    {
        if (State != GameState.GameOver)
        {
            if (Controller.Instance.IsHoveringOverBoard)
            {
                Vector2Int tile = GetTile(Controller.Instance.BoardHoverPosition);
                //Debug.Log("Hovering over tile " + tile.x + " " + tile.y);

                // Left click
                if (Controller.Instance.LeftClick)
                {
                    bool firstClick = false;

                    // Select new piece
                    if (Board[tile.x, tile.y] != null)
                    {
                        if (selectedPiece == null || selectedPiece != Board[tile.x, tile.y])
                        {
                            SelectPiece(tile.x, tile.y);
                            firstClick = true;
                        }
                    }

                    if (!firstClick)
                    {
                        // Move it if there is one selected
                        if (selectedPiece != null)
                        {
                            // Move the piece
                            MovePiece(tile.x, tile.y);
                        }
                    }
                }


                // Do the highlights for hovering 
                if (selectedPiece != null)
                {
                    if (allowedMoves[tile.x, tile.y])
                    {
                        // Display the highlights 
                        BoardHighlight.Instance.HighlightPiecesToRemove(TilesToRemove(selectedPiece, tile.x, tile.y));
                        BoardHighlight.Instance.HighlightHoverTile(selectedPiece, tile.x, tile.y);
                    }
                    else
                    {
                        // If we get here, the cell being hovered over is not valid, so hide the highlight 
                        BoardHighlight.Instance.HideHoverHighlight();
                    }
                }
            }
        }
    }

    private static bool AtLeastOneValidMove(in bool[,] possibleMoves)
    {
        for (int y = 0; y < possibleMoves.GetLength(1); y++)
        {
            for (int x = 0; x < possibleMoves.GetLength(0); x++)
            {
                if (possibleMoves[x, y])
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void SelectPiece(int x, int y)
    {
        // Not a valid selection 
        if (Board[x, y] == null || (Board[x, y].isAttacking && State == GameState.DefendingTurn)
            || (!Board[x, y].isAttacking && State == GameState.AttackingTurn))
        {
            UnselectPiece();
            return;
        }

        // Get the possible moves
        bool[,] possibleAllowedMoves = Board[x, y].PossibleMove();

        // This piece is a valid selection
        if (AtLeastOneValidMove(possibleAllowedMoves))
        {
            // Set selected piece
            selectedPiece = Board[x, y];
            allowedMoves = possibleAllowedMoves;

            BoardHighlight.Instance.HideHighlights();

            // Enable highlights
            BoardHighlight.Instance.HighlightSelectedTile(x, y);
            BoardHighlight.Instance.HighlightAllowedMoves(allowedMoves);
        }
        // Do not select the piece if it cannot move
        else
        {
            UnselectPiece();
        }
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
            selectedPiece.SetPosition(x, y, GetTileWorldPositionCentre(x, y));
            Board[x, y] = selectedPiece;

            // Check if the king has moved to the corner
            if (selectedPiece.isKing)
            {
                if ((selectedPiece.CurrentX == 0 || selectedPiece.CurrentX == BOARD_SIZE - 1) && (selectedPiece.CurrentY == 0 || selectedPiece.CurrentY == BOARD_SIZE - 1))
                {
                    // King has reached the corner
                    // Attacking wins
                    EndGame(Team.Attacking);
                }
            }

            // Check if a piece needs to be removed
            // Will be removed soon
            UpdateBoard();

            if (State != GameState.GameOver)
            {
                // Update the players turn
                UpdatePlayerTurn();
            }

        }

        BoardHighlight.Instance.HideHighlights();
        selectedPiece = null;
    }

    private void UpdatePlayerTurn()
    {
        if (State == GameState.AttackingTurn)
        {
            State = GameState.DefendingTurn;
        }
        else if (State == GameState.DefendingTurn)
        {
            State = GameState.AttackingTurn;
        }

        OnTurnStart(State);
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
            EndGame(Team.Defending);
        }

        if (attackingCount < 2)
        {
            // Defending wins
            EndGame(Team.Defending);
        }
        if (defendingCount < 2)
        {
            // Attacking wins
            EndGame(Team.Attacking);
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
        // Set the scale to fit the current board size
        BoardPlane.localScale = new Vector3(BOARD_SIZE * 0.1f, 1, BOARD_SIZE * 0.1f);
        // Move the plane so that it is in the centre of the board
        BoardPlane.position = GetBoardCentre();
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
        SpawnPiece(defendingPrefab, 0, (BOARD_SIZE / 2) - 2);
        SpawnPiece(defendingPrefab, 0, (BOARD_SIZE / 2) + 2);
        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) - 2, 0);
        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) + 2, 0);
        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) - 2, BOARD_SIZE - 1);
        SpawnPiece(defendingPrefab, (BOARD_SIZE / 2) + 2, BOARD_SIZE - 1);
        SpawnPiece(defendingPrefab, BOARD_SIZE - 1, (BOARD_SIZE / 2) - 2);
        SpawnPiece(defendingPrefab, BOARD_SIZE - 1, (BOARD_SIZE / 2) + 2);


        // Spawn the white pieces
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 2, BOARD_SIZE / 2);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 1, BOARD_SIZE / 2);
        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) - 2);
        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) - 1);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 2, BOARD_SIZE / 2);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 1, BOARD_SIZE / 2);
        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) + 2);
        SpawnPiece(attackingPrefab, BOARD_SIZE / 2, (BOARD_SIZE / 2) + 1);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 1, (BOARD_SIZE / 2) - 1);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 1, (BOARD_SIZE / 2) - 1);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) - 1, (BOARD_SIZE / 2) + 1);
        SpawnPiece(attackingPrefab, (BOARD_SIZE / 2) + 1, (BOARD_SIZE / 2) + 1);


        // Spawn the king 
        SpawnPiece(kingPrefab, BOARD_SIZE / 2, BOARD_SIZE / 2);
    }

    private void SpawnPiece(GameObject o, int x, int y)
    {
        GameObject g = Instantiate(o);
        g.transform.parent = PiecesParent;

        Board[x, y] = g.GetComponent<Piece>();
        Board[x, y].SetPosition(x, y, GetTileWorldPositionCentre(x, y));

        activePieces.Add(g);
    }

    private Vector3 GetTileWorldPositionCentre(int tileX, int tileY)
    {
        return transform.position + new Vector3(TILE_SIZE * tileX + TILE_SIZE/2, 0, TILE_SIZE * tileY + TILE_SIZE / 2);
    }

    public Vector3 GetBoardCentre()
    {
        Vector3 centre = GetTileWorldPositionCentre(BOARD_SIZE / 2, BOARD_SIZE / 2);

        return centre;
    }

    public Vector2Int GetTile(Vector3 worldPosition)
    {
        return new Vector2Int((int)(worldPosition.x / TILE_SIZE), (int)(worldPosition.z / TILE_SIZE));
    }




    /*

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

    */


    private void EndGame(Team won)
    {
        State = GameState.GameOver;

        if (won.Equals(Team.Attacking))
        {
            OnGameWon.Invoke(Team.Attacking);
            Debug.Log("Attacking team won.");
        }
        else if (won.Equals(Team.Defending))
        {
            OnGameWon.Invoke(Team.Defending);
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
        State = GameState.AttackingTurn;

        Debug.Log("Game has been reset.");
    }





}