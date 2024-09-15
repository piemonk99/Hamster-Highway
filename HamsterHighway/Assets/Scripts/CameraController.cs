using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public bool MayDrag = true;

    private Camera cam;
    private Vector3 dragPosition;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        if (!MayDrag || EventSystem.current.IsPointerOverGameObject() || !enabled)
            return;

        if (Input.GetMouseButtonDown(0))
            dragPosition = cam.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(0))
        {   
            transform.position += new Vector3(dragPosition.x - cam.ScreenToWorldPoint(Input.mousePosition).x, 0, 0);
        }
    }
}