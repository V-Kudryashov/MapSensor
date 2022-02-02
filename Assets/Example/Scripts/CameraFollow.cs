using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform car;
    [Range(0, 1)]
    public float smooth = 0.1f;
    public Vector3 offset;
    public bool updateOffset = true;
    private float disance;

    void Start()
    {
        offset = new Vector3(0, 5, 0);
        if (updateOffset)
            offset = gameObject.transform.position - car.position;
    }

    void Update()
    {
        updatePos();
    }
    private void LateUpdate()
    {
    }
    private void updatePos()
    {
        Vector3 newPosition = car.position + offset;
        //newPosition.y = gameObject.transform.position.y;
        newPosition = Vector3.Lerp(gameObject.transform.position, newPosition, smooth);
        gameObject.transform.position = newPosition;
    }
}
