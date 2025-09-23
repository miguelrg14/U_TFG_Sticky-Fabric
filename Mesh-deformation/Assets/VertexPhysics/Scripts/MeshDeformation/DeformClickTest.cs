using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformClickTest : MonoBehaviour
{

    public float force = 10f;
    public float forceOffset = 0.1f;
    public bool clickDisabled = false;

    public LayerMask hitMask;

    void Update()
    {
        if (Input.GetMouseButton(0) & (!clickDisabled))
        {
            HandleInput();
            clickDisabled = true;
            StartCoroutine(DelayClick());
        }
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(inputRay, out hit, 20, hitMask))
        {
            SoftDeformer softDeformer = hit.collider.GetComponent<SoftDeformer>();
          
            if (softDeformer)
            {
                Vector3 point = hit.point;
                point += hit.normal * forceOffset;
                softDeformer.AddDeformingForce(point, force,(hit.point - transform.position).normalized);

            }
        }
    }

    IEnumerator DelayClick()
    {
        yield return new WaitForSeconds(.1f);
        clickDisabled = false;
    }

}
