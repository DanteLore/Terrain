using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{    
    public event System.Action<PlayerController, Collider> TargetChanged;

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

    void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        if(Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask("Rocks")))
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
}
