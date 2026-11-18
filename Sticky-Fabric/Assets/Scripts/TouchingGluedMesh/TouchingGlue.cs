using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingGlue : MonoBehaviour
{
    [SerializeField] LayerMask glue_mask;
    [SerializeField] Color glued_color;
    [SerializeField] Color unGlued_color;

    Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void OnCollisionEnter(Collision collision)
    {
        rend.material.color = glued_color;
        if (glue_mask == (glue_mask | (1 << collision.gameObject.layer)))
        {
            Debug.LogWarning("Tocando");
            rend.material.color = glued_color;
        }
        else
        {
            rend.material.color = unGlued_color;
        }
    }
    void OnCollisionExit(Collision collision)
    {
        rend.material.color = unGlued_color;
    }
}
