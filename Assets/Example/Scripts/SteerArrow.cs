using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerArrow : MonoBehaviour
{
    public CarController car;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = car.transform.position;
        /*
        float angle = car.getSteer() * car.maxSteer;
        transform.rotation = Quaternion.LookRotation(car.transform.forward);
        transform.Rotate(new Vector3(0, angle, 0));
        */
        Vector3 dir = car.transform.TransformDirection(car.getSteerDir());
        transform.rotation = Quaternion.LookRotation(dir);
    }
}
