using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainContact : MonoBehaviour
{
    public bool active;
    public bool reset;
    public float reward = -4;
    private void OnCollisionEnter(Collision collision)
    {
        if (!active)
            return;
        CarAgent carAhent = collision.gameObject.GetComponent<CarAgent>();
        if (carAhent == null)
            return;
            carAhent.terrainContact(reset, reward);
    }
}
