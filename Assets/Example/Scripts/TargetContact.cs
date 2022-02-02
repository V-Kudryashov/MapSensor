using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetContact : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        CarAgent carAgent = other.transform.GetComponentInParent<CarAgent>();
        if (carAgent != null)
        {
            carAgent.targetContact();
        }
    }
}
