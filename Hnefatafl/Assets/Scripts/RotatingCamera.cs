﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingCamera : MonoBehaviour
{
    public float lookSpeed = 50f;
    public int maxOffset = 30;
    public Vector3 defaultView = new Vector3(90, 0, 0);


    // Update is called once per frame
    private void Update()
    {
        // Update the position of the camera
        float pos = (int)(BoardManager.Instance.BOARD_SIZE / 2) + BoardManager.TILE_OFFSET;
        // Works but need a better method for height
        float height = (int)(BoardManager.Instance.BOARD_SIZE / 2) + 1;
        transform.localPosition = new Vector3(pos, height, pos);

        // Update view if the player is looking around
        if (Input.GetMouseButton(1))
        {
            float h = lookSpeed * Input.GetAxis("Mouse X") * Time.deltaTime;
            float v = lookSpeed * Input.GetAxis("Mouse Y") * Time.deltaTime;
            transform.Rotate(h, v, 0f);

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
}
