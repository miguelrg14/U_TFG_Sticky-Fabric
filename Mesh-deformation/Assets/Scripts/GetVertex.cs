using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

// Attach this script to a camera and it will
// draw a debug line triangle triangle
// at the triangle where you place the mouse.
public class GetVertex : MonoBehaviour
{
    Transform sphere;

    void Start()
    {
        
    }

    void Update()
    {
        // Only if we hit something, do we continue
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out hit))
            return;

        // Just in case, also make sure the collider also has a renderer
        // material and texture
        var meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
            return;

        Mesh mesh = meshCollider.sharedMesh;
        var vertices = mesh.vertices;
        var triangles = mesh.triangles;

        // Extract local space vertices that were hit
        var p0 = vertices[triangles[hit.triangleIndex * 3 + 0]];
        var p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
        var p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

        // Transform local space vertices to world space
        Transform hitTransform = hit.collider.transform;
        p0 = hitTransform.TransformPoint(p0);
        p1 = hitTransform.TransformPoint(p1);
        p2 = hitTransform.TransformPoint(p2);

        // Display with Debug.DrawLine
        Debug.DrawLine(p0, p1);
        Debug.DrawLine(p1, p2);
        Debug.DrawLine(p2, p0);
        sphere.position = p0;
    }
}
