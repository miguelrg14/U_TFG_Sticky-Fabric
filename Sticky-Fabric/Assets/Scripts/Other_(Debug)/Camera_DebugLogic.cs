using UnityEngine;

public class Camera_DebugLogic : MonoBehaviour
{
    [Header("Mouse Position Ray")]
    public LayerMask layerMask      = ~0;           // Layer mask para filtrar las colisiones del raycast
    public Color lineColor          = Color.green;  // Color para la línea
    Vector3 mouseWorldPosition;
    bool isMouseWorldPositionValid  = false;

    [Header("Mesh")]
    public Color color_meshTriangle = Color.white;

    void Update()
    {
        UpdateMouseWorldPosition();
        DrawTriangleAtMousePosition();
    }

    /// <summary>
    /// Gets mouse position in the world to be able to draw gizmos of it.
    /// </summary>
    void UpdateMouseWorldPosition()
    {
        // Capturar la posición del ratón en la pantalla
        Vector3 mouseScreenPosition = Input.mousePosition;

        // Convertir la posición del ratón en la pantalla a un rayo
        Ray ray = Camera.main.ScreenPointToRay(mouseScreenPosition);

        // Realizar un raycast para detectar colisión con el plano
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, layerMask))
        {
            mouseWorldPosition = hitInfo.point;
            isMouseWorldPositionValid = true;

            // Dibujar una línea desde la cámara hasta el punto de intersección
            Debug.DrawLine(ray.origin, mouseWorldPosition, lineColor);
        }
        else
        {
            isMouseWorldPositionValid = false;
        }
    }

    /// <summary>
    /// Draws faces of game objects based on mesh collider functionality using mouse position in the world.
    /// </summary>
    void DrawTriangleAtMousePosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if  
        (
            Physics.Raycast(ray, out RaycastHit hit)    && 
            hit.collider is MeshCollider meshCollider   && 
            meshCollider.sharedMesh != null
        )
        {
            Mesh mesh = meshCollider.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;

            // Extract local space vertices that were hit
            Vector3 p0 = vertices[triangles[hit.triangleIndex * 3]];
            Vector3 p1 = vertices[triangles[hit.triangleIndex * 3 + 1]];
            Vector3 p2 = vertices[triangles[hit.triangleIndex * 3 + 2]];

            // Transform local space vertices to world space
            Transform hitTransform = hit.collider.transform;
            p0 = hitTransform.TransformPoint(p0);
            p1 = hitTransform.TransformPoint(p1);
            p2 = hitTransform.TransformPoint(p2);

            // Display with Debug.DrawLine
            Debug.DrawLine(p0, p1, color_meshTriangle);
            Debug.DrawLine(p1, p2, color_meshTriangle);
            Debug.DrawLine(p2, p0, color_meshTriangle);
        }
    }
}