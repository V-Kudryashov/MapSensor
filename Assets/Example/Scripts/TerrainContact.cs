using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainContact : MonoBehaviour
{
    public bool active;
    public bool reset;
    public float reward = -4;
    private CarAgent carAgent;
    private void OnCollisionEnter(Collision collision)
    {
        if (!active)
            return;
        CarAgent car = collision.gameObject.GetComponent<CarAgent>();
        if (car != null)
            carAgent = car;
        if (carAgent != null)
        {
            carAgent.terrainContact(reset, reward);
        }
    }
}
