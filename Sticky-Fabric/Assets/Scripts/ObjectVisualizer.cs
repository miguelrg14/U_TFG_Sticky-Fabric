using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ObjectVisualizer : MonoBehaviour
{
    public bool start_preview = false;

    /// --------------  GameObject (General) -------------- 
    [Header("GameObject")]
    public Color gizmoColor_GameObject      = Color.white;
    public float radius_gameObject          = 0.1f;


    void OnDrawGizmos()
    {
        /// --------------  GameObject Gizmos -------------- 
        Gizmos.color        = gizmoColor_GameObject;
        Gizmos.DrawSphere(transform.position, radius_gameObject);
    }
}
