using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TheCube : MonoBehaviour
{
    public Transform targetTransform;
    private NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if(IsAgentOnNavMesh)
        {
            agent.enabled = true;

            Vector3 targetVector = targetTransform.transform.position;
            agent.SetDestination(targetVector);
        }
    }

    bool IsAgentOnNavMesh
    {
        get
        {
            float onMeshThreshold = 3;
            Vector3 agentPosition = agent.transform.position;
            NavMeshHit hit;

            // Check for nearest point on navmesh to agent, within onMeshThreshold
            if (NavMesh.SamplePosition(agentPosition, out hit, onMeshThreshold, NavMesh.AllAreas))
            {
                // Check if the positions are vertically aligned
                if (Mathf.Approximately(agentPosition.x, hit.position.x)
                    && Mathf.Approximately(agentPosition.z, hit.position.z))
                {
                    // Lastly, check if object is below navmesh
                    return agentPosition.y >= hit.position.y;
                }
            }

            return false;
        }
    }
}
