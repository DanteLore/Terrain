using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBehaviour : MonoBehaviour
{
    private Transform player;

    new private Rigidbody rigidbody;

    private DayNightCycle dayNightCycle;

    public float TurnSpeed = 1.5f;
    public float MoveSpeed = 1.0f;
    public float PersonalSpaceThreshold = 2.0f;
    public float FollowRangeThreshold = 20.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        dayNightCycle = GameObject.FindObjectOfType<DayNightCycle>();
        rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        EmergencyFallPrevention();
        MoveAround();
        Headlights();
    }

    private void Headlights()
    {
        foreach(Light light in transform.GetComponentsInChildren<Light>())
        {
            light.enabled = dayNightCycle.IsNight;
        }
    }

    Vector3 target;

    private void MoveAround()
    {
        /// TODO: This method is shamefully bad... but it makes robots do fun stuff.  Refactor to a state machine kinda setup...

        float playerDistance = Vector3.Distance(transform.position, player.position);

        if(playerDistance < FollowRangeThreshold)
        {
            target = player.position;
            
            if(playerDistance <= PersonalSpaceThreshold)
            {
                // Stare in a spooky way
                TurnTowards(player.position);
            }
            else
            {
                // Follow
                float speed = MoveSpeed * Mathf.InverseLerp(PersonalSpaceThreshold, FollowRangeThreshold, playerDistance);
                MoveTowards(player.position, speed);
            }
        }
        else
        {
            if(target == null)
            {
                // New target
                target = RandomTarget();
            }
            var targetDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(target.x, target.z));

            if(targetDistance <= PersonalSpaceThreshold)
            {
                target = RandomTarget();
                targetDistance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(target.x, target.z));
            }

            // Random wander
            float speed = MoveSpeed * Mathf.InverseLerp(PersonalSpaceThreshold, FollowRangeThreshold, targetDistance);
            MoveTowards(target, MoveSpeed / 2);
        }
    }

    private Vector3 RandomTarget()
    {
        Vector2 pos = Random.insideUnitCircle * FollowRangeThreshold;
        return transform.position + new Vector3(pos.x, 0, pos.y);
    }

    private void MoveTowards(Vector3 target, float speed)
    {
        TurnTowards(target);

        // Push forwards a bit

        // Choice here between moving the position and hoping not to fall through the ground vs applying force and trying to control some crazy movements!
        rigidbody.MovePosition(transform.position + transform.forward * speed * Time.deltaTime);
        //rigidbody.AddForce(transform.forward * speed);
    }

    private void TurnTowards(Vector3 target)
    {
        // Rotate to face target
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target - transform.position), TurnSpeed * Time.deltaTime);}

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
