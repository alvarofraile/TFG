using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWorldPosition : MonoBehaviour
{
    private static MouseWorldPosition instance;
    [SerializeField] private LayerMask mousePlaneLayerMask;

    [Header("Visualization")]
    [Tooltip("Change before playing")]
    [SerializeField] private bool visualizeMousePosition;
    [SerializeField] private GameObject mousePositionVisual;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        if (visualizeMousePosition)
        {
            mousePositionVisual.SetActive(true);
        }
        else
        {
            mousePositionVisual.SetActive(false);
        }
    }

    private void FixedUpdate()
    {
        if (visualizeMousePosition)
        {
            this.transform.position = GetPosition();
        }
    }

    public static Vector3 GetPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, instance.mousePlaneLayerMask);
        return raycastHit.point;
    }
}
