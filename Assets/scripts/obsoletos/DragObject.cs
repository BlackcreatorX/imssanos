using UnityEngine;

public class DragObject : MonoBehaviour
{
    // Energía inicial del objeto
    private Camera mainCamera;
    private GameObject selectedObject;
    private Vector3 offset;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleObjectDragging();
    }

    private void HandleObjectDragging()
    {
        if (Input.GetMouseButtonDown(0)) // Detecta clic izquierdo
        {
            RaycastHit2D hit = Physics2D.Raycast(mainCamera.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            
            if (hit.collider != null && hit.collider.CompareTag("TDoc"))
            {
                selectedObject = hit.collider.gameObject;
                offset = selectedObject.transform.position - mainCamera.ScreenToWorldPoint(Input.mousePosition);
            }
        }

        if (Input.GetMouseButton(0) && selectedObject != null)
        {
            Vector3 newPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition) + offset;
            newPosition.z = 0; // Asegurar que se mantenga en el mismo plano
            selectedObject.transform.position = newPosition;
        }

        if (Input.GetMouseButtonUp(0)) // Suelta el objeto
        {
            selectedObject = null;
        }
    }
}
