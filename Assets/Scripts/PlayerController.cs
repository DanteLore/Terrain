using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{    
    public event System.Action<PlayerController, Collider> TargetChanged;

    public TerrainGenerator terrainGenerator;

    public float maxTargetRange = 20f;

    private Collider _target;
    public Collider Target
    {
        get { return _target; }
        private set
        {
            _target = value;

            if(TargetChanged != null)
                TargetChanged(this, _target);
        }
    }

    public TerrainChunk CurrentChunk
    {
        get 
        {
            return terrainGenerator.GetChunkForPosition(transform.position);
        }
    }

    void Start()
    {
        transform.position = new Vector3(0, 200, 0);
    }

    void Update()
    {
        EmergencyFallPrevention();
        ManageTarget();
    }

    private void ManageTarget()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        if(Physics.Raycast(ray, out hit, maxTargetRange) && hit.collider.gameObject.layer != LayerMask.NameToLayer("Terrain"))
        {
            if(hit.collider != Target)
            {
                Target = hit.collider;
            }
        }
        else
        {
            Target = null;
        }
    }

    private void EmergencyFallPrevention()
    {
        // Just in case we fall through the mesh
        if(transform.position.y < -10)
        {
            transform.position = new Vector3(0, 200f, 0);
            Debug.Log("Turtles all the way down, man!");
        }
    }
}
