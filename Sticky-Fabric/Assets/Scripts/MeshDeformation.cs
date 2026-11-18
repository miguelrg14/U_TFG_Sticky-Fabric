using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;
using static UnityEngine.GraphicsBuffer;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class MeshCreator : MonoBehaviour
{
    [Header("Mesh Creation")]
    public float            radius_vertexCollider = 0.03f;
    public int              subdivisions = 2; // Número de subdivisiones

    [Header("Mesh logic")]
    //public Transform targetTransform; // Transform objetivo
    public float            k = 10.0f;
    public float            b = 0f;

    public float            elasticity = 0.5f; // Elasticidad del vértice / Umbral de distancia para despegar
    public LayerMask        collisionLayer; // Capa para detectar colisiones

    // Malla
    private Mesh mesh;
    private Vector3[]       vertices;
    private Vector3[]       originalVertices;
    private Vector3[]       velocities;
    private Vector3[]       previousVelocities;
    // Punto cercano al que se pega del otro objeto
    private Vector3[]       closestPoint;

    [Header("Gizmos")]
    public Color            color_Vertex    = Color.magenta;

    void Start()
    {
        CreateMesh();
    }
    void Update()
    {
        MeshDeformation_Logic();
    }

    #region //  MESH CREATION
    /// <summary>
    ///     Crea la malla proceduralmente
    /// </summary>
    void CreateMesh()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>(); // Obtener el MeshFilter adjunto a este GameObject
        mesh = new Mesh(); // Crear una nueva malla

        GenerateSubdividedSquare(subdivisions); // Generar el cuadrado subdividido
        meshFilter.mesh = mesh; // Asignar la malla al MeshFilter

        // Inicializar arrays de velocidades
        velocities = new Vector3[vertices.Length];
        previousVelocities = new Vector3[vertices.Length];
    }
    /// <summary>
    ///     Gestión del planteamiento de los vértices en el mundo y el número de subdivisiones que tendrá la malla
    /// </summary>
    /// <param name="subdivisions"></param>
    void GenerateSubdividedSquare(int subdivisions)
    {
        int vertCount = (subdivisions + 1) * (subdivisions + 1);
        int triCount = subdivisions * subdivisions * 2;

        vertices = new Vector3[vertCount];
        int[] triangles = new int[triCount * 3];

        float stepSize = 1.0f / subdivisions;

        // Generar los vértices
        for (int y = 0; y <= subdivisions; y++)
        {
            for (int x = 0; x <= subdivisions; x++)
            {
                vertices[y * (subdivisions + 1) + x] = new Vector3(x * stepSize - 0.5f, y * stepSize - 0.5f, 0);
            }
        }

        // Generar los triángulos
        int triIndex = 0;
        for (int y = 0; y < subdivisions; y++)
        {
            for (int x = 0; x < subdivisions; x++)
            {
                int start = y * (subdivisions + 1) + x;

                triangles[triIndex] = start;
                triangles[triIndex + 1] = start + 1;
                triangles[triIndex + 2] = start + subdivisions + 1;

                triangles[triIndex + 3] = start + 1;
                triangles[triIndex + 4] = start + subdivisions + 2;
                triangles[triIndex + 5] = start + subdivisions + 1;

                triIndex += 6;
            }
        }

        // Asignar vértices y triángulos a la malla
        mesh.vertices = vertices;
        mesh.triangles = triangles;

        // Inicializo los arrays de vértices
        originalVertices = new Vector3[vertices.Length];

        // Guardo las posiciones de los vértices en su inicio
        for (int i = 0; i < vertices.Length; i++)
        {
            originalVertices[i] = vertices[i];
        }

        // Objeto pegado
        closestPoint            = new Vector3[vertices.Length];

        // Recalcular normales para la iluminación
        mesh.RecalculateNormals();

        if (GetComponent<MeshCollider>() != null)
            GetComponent<MeshCollider>().sharedMesh = mesh;
    }
    #endregion

    #region //  MESH LOGIC
    /// <summary>
    ///     Lógica para revisar los vértices de la malla para su deformación
    /// </summary>
    void MeshDeformation_Logic()
    {
        // Revisa todos los vértices de la malla
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3     currentPosition = vertices[i];                      //  Manejo de vértices - Posición actual del vértice en la malla
            Collider[]  meshColliders   = Physics.OverlapSphere(transform.position + currentPosition, radius_vertexCollider, collisionLayer);   //  Manejo de colliders - Detecta colisiones cercanas al vértice creando colliders de esferas en cada uno (Se crea aquí continuamente por si se mueve la entidad para que la malla se adapte a su nueva posición también de manera inteligente)
            
            Vector3     targetPosition  = originalVertices[i];              //  Objetivo al que debe moverse el vértice (Punto al que está pegado o punto al que debe volver al pasarse de elasticidad)
            Vector3     d               = targetPosition - currentPosition; //  Distancia hasta el punto original en "vector3"
            float       distance        = d.magnitude;                      //  Distancia hasta el punto original en "float"
            Vector3     direction       = d.normalized;                     //  Dirección que debe tomar para llegar al punto

            // Lógica al pegar la malla si toca uno de los vértices (/uno de los colliders creados en cada vértice)
            if (meshColliders.Length > 0)
            {
                closestPoint[i] = meshColliders[0].ClosestPoint(transform.position + currentPosition);  // Cálculo el punto más cercano a donde colisiona para pegar el vértice

                // Pegar si entra en los límites de elasticidad
                if (distance < elasticity)
                {
                    targetPosition = closestPoint[i];
                    targetPosition = transform.InverseTransformPoint(targetPosition);

                    Glue_Vertex(i, targetPosition); // MOVIMIENTO VÉRTICE - hacia la posición que le envío como target
                }
            }
            // Lógica de despegar el vértice de la malla + excepciones por si se suelta la malla antes de tiempo para que vuelva a su posición original
            else
            {
                // Si la posición del vector del vértice que se está manejando es distinta a la posición original => Despegar
                if (vertices[i] != originalVertices[i])
                {
                    UnGlue_Vertex(i, distance, direction); // MOVIMIENTO VÉRTICE - volver hacia la posición original que le envío como target
                }
            }
        }
    }

    /// <summary>
    ///     Pega el vértice al otro objeto
    /// </summary>
    /// <param name="index"></param>
    /// <param name="vertex_LocalPosition"></param>
    void Glue_Vertex(int index, Vector3 vertex_LocalPosition)
    {
        vertices[index] = vertex_LocalPosition; // Asigna la posición global al vértice modificado
        UpdateMesh();
    }
    /// <summary>
    ///     Devuelve al vértice a su posición original mediante una fórmula de elasticidad
    /// </summary>
    /// <param name="index"></param>
    /// <param name="distance"></param>
    /// <param name="direction"></param>
    void UnGlue_Vertex(int index, float distance, Vector3 direction)
    {        
        Vector3 force = k * (distance - b) * direction - b * k * (velocities[index] - previousVelocities[index]);   // Aplicar la fórmula directamente sin umbral

        vertices[index] += force * Time.deltaTime;

        previousVelocities[index] = velocities[index];
        velocities[index] = force;
        ///
        UpdateMesh();
    }

    /// <summary>
    ///     Actualizar la malla con los nuevos vértices
    /// </summary>
    void UpdateMesh()
    {
        mesh.vertices = vertices;   // Asigna los vértices modificados de nuevo a la malla
        mesh.RecalculateNormals();  // Recalcula las normales para asegurar una apariencia correcta
    }

    /// <summary>
    ///     Enseña correctamente por código el vértice para debuguear
    /// </summary>
    /// <param name="previousTxt"></param>
    /// <param name="vector"></param>
    /// <param name="vectorNumber"></param>
    void Debug_PrintVertex(string previousTxt, Vector3 vector, int vectorNumber)
    {
        Debug.Log(previousTxt + " | Vértice: " + vectorNumber + ": (" + vector.x + ", " + vector.y + ", " + vector.z + ")");
    }
    #endregion

    void OnDrawGizmos()
    {
        if (mesh == null)
            return;

        Vector3[] vertices = mesh.vertices; // Obtener los vértices de la malla

        // Dibujar una esfera en cada vértice
        Gizmos.color = color_Vertex; // Color de las esferas
        foreach (Vector3 vertex in vertices)
        {
            Gizmos.DrawSphere(transform.position + vertex, radius_vertexCollider); // Dibujar esfera en el vértice
        }
    }
}
