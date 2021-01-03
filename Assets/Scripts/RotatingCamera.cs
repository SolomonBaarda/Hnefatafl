using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RotatingCamera : MonoBehaviour
{
    public float lookSpeedX = 25f;
    public float lookSpeedY = 25f;
    public int maxOffset = 30;
    public Vector3 defaultView = new Vector3(90, 0, 0);

    private void Start()
    {
        // Call the set camera position method once the scene has loaded
        SceneManager.sceneLoaded += SetCameraPosition;
    }

    // Update is called once per frame
    private void Update()
    {
        // Look around using right mouse button
        if (Input.GetMouseButton(1))
        {
            // Get the x and y rotation
            float x = lookSpeedX * Input.GetAxisRaw("Mouse X") * Time.deltaTime;
            float y = lookSpeedY * Input.GetAxisRaw("Mouse Y") * Time.deltaTime;
            transform.Rotate(-y, x, 0f);

            Vector3 rotation = transform.localRotation.eulerAngles;
            rotation.x = Mathf.Clamp(rotation.x, defaultView.x - maxOffset, defaultView.x + maxOffset);
            transform.localRotation = Quaternion.Euler(rotation);
        }
        // Reset view
        else
        {
            transform.localRotation = Quaternion.Euler(defaultView);
        }
    }

    private int RoundToClosestValue(float number, int[] values)
    {
        // currently unused method 

        float[] differences = new float[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            differences[i] = Mathf.Abs(number - values[i]);
        }

        int index = 0;
        float smallest = differences[index];
        for (int i = 0; i < differences.Length; i++)
        {
            if (differences[i] < smallest)
            {
                smallest = differences[i];
                index = i;
            }
        }

        return values[index];
    }


    private void SetCameraPosition(Scene scene, LoadSceneMode mode)
    {
        // Set the position to be above the centre of the board
        transform.position = BoardManager.Instance.GetBoardCentreWorldPosition() + new Vector3(0, (BoardManager.Instance.BOARD_SIZE / 2) + 3, 0);
    }

    private void OnDestroy()
    {
        // Remove the method from the event call
        SceneManager.sceneLoaded -= SetCameraPosition;
    }

}
