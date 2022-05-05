using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetArrow : MonoBehaviour
{
    public Transform from;
    public Transform to;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = from.position;
        Vector3 dir = to.position - from.position;
        dir.y = 0;
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
