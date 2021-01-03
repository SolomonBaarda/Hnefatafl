using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using System.Collections;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { private set; get; }

    public Piece[,] Board { private set; get; }

    public const float TILE_SIZE = 1.0f;
    public int BOARD_SIZE = 13;

    public GameState State
    {
        get
        {
            if (!Game.IsTerminal)
            {
                if (Game.WhosTurn.Team == Team.Attacking)
                {
                    return GameState.AttackingTurn;
                }
                else if (Game.WhosTurn.Team == Team.Defending)
                {
                    return GameState.DefendingTurn;
                }
            }
            return GameState.GameOver;
        }
    }

    public GameObject defendingPrefab;
    public GameObject attackingPrefab;
    public GameObject kingPrefab;
    private Piece king;

    public static event UnityAction<Team> OnGameWon;
    public static event UnityAction<GameState> OnTurnStart;

    public static LayerMask PLANE_MASK => LayerMask.GetMask("BoardPlane");
    [Header("References")]
    public Transform BoardPlane;
    public Transform PiecesParent;

    private MDPEnvironment Game;

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
        Instance = this;

        HumanAgent a = gameObject.AddComponent<HumanAgent>();
        a.Instantiate(Team.Attacking);

        HumanAgent d = gameObject.AddComponent<HumanAgent>();
        d.Instantiate(Team.Defending);

        Game = new MDPEnvironment(a, d, BOARD_SIZE);
        Board = new Piece[Game.Environment.GetLength(0), Game.Environment.GetLength(1)];


        SetBoardPlane();
        SpawnAllPieces();

        StartCoroutine(WaitForLoadScenes());
    }



    private IEnumerator WaitForLoadScenes()
    {
        if (!SceneManager.GetSceneByName("HUD").isLoaded)
        {
            SceneManager.LoadSceneAsync("HUD", LoadSceneMode.Additive);
        }

        if (!SceneManager.GetSceneByName("Background").isLoaded)
        {
            SceneManager.LoadSceneAsync("Background", LoadSceneMode.Additive);
        }

        while (!SceneManager.GetSceneByName("HUD").isLoaded && !SceneManager.GetSceneByName("Background").isLoaded)
        {
            yield return null;
        }

        StartCoroutine(Play());
    }


    private IEnumerator Play()
    {
        Debug.Log("Starting the game.");

        while (!Game.IsTerminal)
        {
            if (!Game.IsWaitingForMove)
            {
                //Debug.Log("Made get move request");
                Game.GetNextMove(MakeMove);
            }

            yield return null;
        }
    }

    private void MakeMove(Move m)
    {
        //Debug.Log("Recieved move from agent. (" + m.From.x + "," + m.From.y + "->" + m.To.x + "," + m.To.y + ")");

        // Update the MDP
        List<Vector2Int> piecesToKill = Game.ExecuteMove(m);

        // Make the move
        Piece p = Board[m.From.x, m.From.y];
        Board[m.From.x, m.From.y] = null;
        Board[m.To.x, m.To.y] = p;
        p.SetPosition(m.To.x, m.To.y, GetTileWorldPositionCentre(m.To.x, m.To.y));

        // Kill all pieces affected by this move
        foreach (Vector2Int v in piecesToKill)
        {
            Kill(v.x, v.y);
        }

        //Debug.Log("pieces to kill: " + piecesToKill.Count);


        /*
        // Check if the king has moved to the corner
        if (p.isKing)
        {
            if ((m.To.x == 0 || m.To.y == BOARD_SIZE - 1) && (selectedPiece.CurrentY == 0 || selectedPiece.CurrentY == BOARD_SIZE - 1))
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
            OnTurnStart.Invoke(State);
        }



        BoardHighlight.Instance.HideHighlights();
        
        */


    }


    /*
    private void Update()
    {
        if (State != GameState.GameOver)
        {
            if (Controller.Instance.IsHoveringOverBoard)
            {
                Vector2Int tile = GetTile(Controller.Instance.BoardHoverPosition);
                //Debug.Log("Hovering over tile " + tile.x + " " + tile.y);

                // Left click
                if (Controller.Instance.PressingLeftClick)
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

    */



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



    private void Kill(int x, int y)
    {
        Piece p = Board[x, y];
        Board[x, y] = null;

        Destroy(p.gameObject);
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
        // Loop over the environment
        for (int y = 0; y < Game.Environment.GetLength(1); y++)
        {
            for (int x = 0; x < Game.Environment.GetLength(0); x++)
            {
                // Instantiate the pieces
                switch (Game.Environment[x, y])
                {
                    case MDPEnvironment.Tile.Defending:
                        SpawnPiece(defendingPrefab, x, y);
                        break;
                    case MDPEnvironment.Tile.Attacking:
                        SpawnPiece(attackingPrefab, x, y);
                        break;
                    case MDPEnvironment.Tile.King:
                        SpawnPiece(kingPrefab, x, y);
                        break;
                }
            }
        }
    }

    private void SpawnPiece(GameObject o, int x, int y)
    {
        GameObject g = Instantiate(o, PiecesParent);
        Board[x, y] = g.GetComponent<Piece>();
        Board[x, y].SetPosition(x, y, GetTileWorldPositionCentre(x, y));
    }

    private Vector3 GetTileWorldPositionCentre(int tileX, int tileY)
    {
        return transform.position + new Vector3(TILE_SIZE * tileX + TILE_SIZE / 2, 0, TILE_SIZE * tileY + TILE_SIZE / 2);
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

    private void EndGame(Team won)
    {
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
        foreach(Piece p in PiecesParent.GetComponentsInChildren<Piece>())
        {
            Destroy(p.gameObject);
        }

        BoardHighlight.Instance.HideHighlights();

        if(Game.Attacking is HumanAgent h1) {
            h1.StopAllCoroutines();
        }
        if (Game.Defending is HumanAgent h2)
        {
            h2.StopAllCoroutines();
        }

        Game = new MDPEnvironment(Game.Attacking, Game.Defending, BOARD_SIZE);
        Board = new Piece[Game.Environment.GetLength(0), Game.Environment.GetLength(1)];

        SpawnAllPieces();

        StopAllCoroutines();
        StartCoroutine(WaitForLoadScenes());


        Debug.Log("Game has been reset.");
    }


}