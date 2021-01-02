using UnityEngine;

public class Controller : MonoBehaviour
{
    public static Controller Instance { get; private set; }

    public bool IsHoveringOverBoard { get; private set; }

    private Vector3 boardHoverPosition;
    public Vector3 BoardHoverPosition => boardHoverPosition;

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
                boardHoverPosition = hit.point;
            }
            else
            {
                IsHoveringOverBoard = false;

                // Reset them
                boardHoverPosition.x = -1;
                boardHoverPosition.y = -1;
            }
        }
    }

}
