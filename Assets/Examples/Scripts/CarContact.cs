using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CarAgent))]
public class CarContact : MonoBehaviour
{
    public bool active;
    public bool reset;
    public float reward = -1;
    private CarAgent carAgent;
    private void OnCollisionEnter(Collision collision)
    {
        if (!active)
            return;
        if (collision.gameObject.tag != "Car")
            return;
        GetComponent<CarAgent>().terrainContact(reset, reward);
    }
}
