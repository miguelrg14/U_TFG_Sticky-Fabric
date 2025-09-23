using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;

public class GumObject : MonoBehaviour
{
    float scale = 0.1f;
    float speed = 0.5f;
    bool recalculateNormals = false;

    private Vector3[] baseVertices;
    private Perlin noise;
 
    void Start()
    {
        noise = new Perlin();
    }

    void Update()
    {
        Mesh mesh = GetComponent<MeshFilter>().mesh;

        if (baseVertices == null)
            baseVertices = mesh.vertices;

        var vertices = new Vector3[baseVertices.Length];

        float timex = Time.time * speed + 0.1365143f;
        float timey = Time.time * speed + 1.21688f;
        float timez = Time.time * speed + 2.5564f;
        for (var i = 0; i < vertices.Length; i++)
        {
            var vertex = baseVertices[i];

            vertex.x += noise.Noise(timex + vertex.x, timex + vertex.y, timex + vertex.z) * scale;
            vertex.y += noise.Noise(timey + vertex.x, timey + vertex.y, timey + vertex.z) * scale;
            vertex.z += noise.Noise(timez + vertex.x, timez + vertex.y, timez + vertex.z) * scale;
            vertices[i] = vertex;
        }

        mesh.vertices = vertices;

        if (recalculateNormals)
            mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    void OnCollisionEnter(Collision collision)
    {
        
    }
}
