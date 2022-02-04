using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBehaviour : MonoBehaviour
{
    private Transform player;

    public float TurnSpeed = 3.0f;
    public float MoveSpeed = 3.0f;
    public float PersonalSpaceThreshold = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    // Update is called once per frame
    void Update()
    {
        EmergencyFallPrevention();
        FollowPlayer();
    }

    private void FollowPlayer()
    {
        // Rotate to face player
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.position - transform.position), TurnSpeed * Time.deltaTime);
        
        // Move forards a bit, but not too close to the player
        float distance = Vector3.Distance(transform.position, player.position);
        if(distance > PersonalSpaceThreshold)
        {
            float speed = MoveSpeed * Mathf.InverseLerp(PersonalSpaceThreshold, 100, distance);

            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }

    private void EmergencyFallPrevention()
    {
        // Just in case we fall through the mesh
        if(transform.position.y < -10)
        {
            transform.position = player.position + (Vector3.forward * -20f) + (Vector3.up * 50f);
            Debug.Log("Robot fell through the floor :(");
        }
    }
}
