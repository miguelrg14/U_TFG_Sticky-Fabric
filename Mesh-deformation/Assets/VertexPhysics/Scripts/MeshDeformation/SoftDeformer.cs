using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*  Unity Vertex-based soft body physics demo by Tiny Angle Labs
 * 
 * 
 * contact: tinyanglelabs@outlook.com
 * 
 * 
 */

public class SoftDeformer : MonoBehaviour
{
    public SkinnedMeshRenderer SM;
    public Mesh deformingMesh;
    public Mesh bakedMesh;
    public Mesh originalMesh;
    public Rigidbody rbody;

    public float distanceThreshold = .1f;
    public float restoringForceMultiplier = 1;

    Vector3[] originalVertices, displacedVertices, animatedVertices;
    Vector3[] vertexVelocities;

    // Use this for initialization
    void Start()
    {
        bakedMesh = new Mesh();
        originalMesh = SM.sharedMesh;
        deformingMesh = (Mesh)Object.Instantiate(originalMesh);
        SM.sharedMesh = deformingMesh;
        originalVertices = deformingMesh.vertices;
        animatedVertices = originalVertices;

        displacedVertices = new Vector3[originalVertices.Length];
        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
        vertexVelocities = new Vector3[originalVertices.Length];
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < displacedVertices.Length; i++)
        {
            if (((displacedVertices[i] - originalVertices[i]).magnitude > 0) | (vertexVelocities[i].magnitude > 0))
            {
                Vector3 restoringForce = restoringForceMultiplier * (displacedVertices[i] - originalVertices[i]);
                float forceScale = Mathf.Pow(Mathf.Abs(restoringForce.magnitude), 2);
                vertexVelocities[i] += forceScale * (restoringForce.normalized) * -2f;

                displacedVertices[i] = originalVertices[i] + vertexVelocities[i];
            }
        }

        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    public void AddDeformingForce( Vector3 point, float force, Vector3 direction)
    {
        SM.BakeMesh(bakedMesh);
        animatedVertices = bakedMesh.vertices;

        Vector3 normalsAvg = Vector3.zero;
        int N_deformedVerts = 0;

        if (rbody)
            rbody.AddForce(direction.normalized * 100);

        for (int i = 0; i < displacedVertices.Length; i++)
        {
            Vector3 pointToVertex = animatedVertices[i] - point;
            if (Mathf.Abs(pointToVertex.magnitude) < distanceThreshold)
            {
                normalsAvg += deformingMesh.normals[i];
                float attenuatedForce = force / (1f + Mathf.Pow( pointToVertex.sqrMagnitude,1) * 100);
                float velocity = attenuatedForce * Time.deltaTime;
                vertexVelocities[i] += pointToVertex.normalized * velocity * .1f;
                N_deformedVerts++;
                if (N_deformedVerts > 100)
                    break;
            }
        }
        
    }
   
}
