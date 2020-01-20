using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingCamera : MonoBehaviour
{
    public GameObject following;
    public float cameraOffsetHorizontal = 6;
    public float cameraOffertVertical = 3;

    // Update is called once per frame
    void Update()
    {
        // Set the position
        Vector3 pos = new Vector3(0, cameraOffertVertical, -cameraOffsetHorizontal);
        transform.localPosition = pos;

        // Set the rotation
        transform.rotation = following.transform.rotation;
    }
}
