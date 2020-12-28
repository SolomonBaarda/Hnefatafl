using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; private set; }

    public bool IsHoveringOverBoard { get; private set; }

    private Vector2 boardSelectionPosition;
    private Vector2Int boardTileSelection;
    public Vector2 BoardSelectionPosition => boardSelectionPosition;
    public Vector2Int BoardTileSelection => boardTileSelection;


    public bool LeftClick => Input.GetMouseButtonDown(0);
    public bool RightClick => Input.GetMouseButtonDown(1);


    private void Awake()
    {
        Instance = this;
    }


    private void Update()
    {
        if (Camera.main)
        {
            // Raycast from mouse point to the plane
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 25.0f, BoardManager.PLANE_MASK))
            {
                IsHoveringOverBoard = true;

                // Get the x and y world position of the mouse
                boardSelectionPosition.x = hit.point.x;
                boardSelectionPosition.y = hit.point.z;
                // Get the x and y tile position
                boardTileSelection.x = (int)boardSelectionPosition.x;
                boardTileSelection.y = (int)boardSelectionPosition.y;
            }
            else
            {
                IsHoveringOverBoard = false;

                // Reset them
                boardSelectionPosition.x = -1;
                boardSelectionPosition.y = -1;

                boardTileSelection.x = -1;
                boardTileSelection.y = -1;
            }
        }
    }


}
