using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public float velocity = 10;
    public float maxVelocity = 30;
    public float turningAngle = 90;
    public float MaxTurningAngle = 180;
    public float jumpPower = 10;

    // Update is called once per frame
    private void Update()
    {
        // Update the speed if boosting
        float speed = velocity;
        float turn = turningAngle;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = maxVelocity;
            turn = MaxTurningAngle;
        }

        // Update player angle
        float angle = 0;
        if (Input.GetKey(KeyCode.A))
        {
            angle -= turn * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            angle += turn * Time.deltaTime;
        }
        transform.Rotate(new Vector3(0, angle, 0));

        // Update player speed
        Vector3 direction = new Vector3();
        if (Input.GetKey(KeyCode.W))
        {
            direction.z += speed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            direction.z -= speed * Time.deltaTime;
        }
        transform.Translate(direction, Space.Self);

        // Make the player jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Collider collider = GetComponent<Collider>();

            // Check if the player is on the ground my raycasting to the ground
            //collider.bounds are the bounds collider relative to the world. I wanted a 0.1 margin, and 0.18 is the radius of my collider.
            if (IsOnGround())
            {
                GetComponent<Rigidbody>().AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
            }

        }

    }

    private bool IsOnGround()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.5f);
    }



}
