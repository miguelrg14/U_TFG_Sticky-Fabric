using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveObject : MonoBehaviour
{
    public Transform position1; // Primera posición
    public Transform position2; // Segunda posición
    public float speed = 1.0f; // Velocidad de movimiento

    private Vector3 nextPosition;

    void Start()
    {
        nextPosition = position1.position;
    }

    void Update()
    {
        if (transform.position == position1.position)
        {
            nextPosition = position2.position;
        }
        else if (transform.position == position2.position)
        {
            nextPosition = position1.position;
        }

        transform.position = Vector3.MoveTowards(transform.position, nextPosition, speed * Time.deltaTime);
    }
}
