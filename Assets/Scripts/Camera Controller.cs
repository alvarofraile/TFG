using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour
{

    private const float MIN_FOLLOW_Y_DISTANCE = 2f;
    private const float MAX_FOLLOW_X_DISTANCE = 12f;

    [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;

    private CinemachineTransposer cinemachineTransposer;
    private Vector3 targetFollowOffset;

    [SerializeField] private float cameraMovementSpeed = 10f;
    [SerializeField] private float cameraRotationSpeed = 100f;
    [SerializeField] private float cameraZoomSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        cinemachineTransposer = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>();
        targetFollowOffset = cinemachineTransposer.m_FollowOffset;
    }

    // Update is called once per frame
    void Update()
    {
        ControlMovement();
        ControlRotation();
        ControlZoom();
    }

    private void ControlMovement()
    {
        Vector3 cameraInput = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W))
        {
            cameraInput.z += 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            cameraInput.z -= 1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            cameraInput.x += 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            cameraInput.x -= 1f;
        }

        Vector3 cameraMovementVector = transform.forward * cameraInput.z + transform.right * cameraInput.x;
        transform.position += cameraMovementVector.normalized * cameraMovementSpeed * Time.deltaTime;
    }

    private void ControlRotation()
    {
        Vector3 cameraRotationVector = new Vector3(0, 0, 0);

        if(Input.GetKey(KeyCode.Q))
        {
            cameraRotationVector.y += 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            cameraRotationVector.y -= 1f;
        }

        transform.eulerAngles += cameraRotationVector * cameraRotationSpeed * Time.deltaTime;
    }

    private void ControlZoom()
    {
        float zoomAmount = 1f;

        if(Input.mouseScrollDelta.y > 0)
        {
            targetFollowOffset.y -= zoomAmount;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            targetFollowOffset.y += zoomAmount;
        }

        targetFollowOffset.y = Mathf.Clamp(targetFollowOffset.y, MIN_FOLLOW_Y_DISTANCE, MAX_FOLLOW_X_DISTANCE);

        cinemachineTransposer.m_FollowOffset = Vector3.Lerp(cinemachineTransposer.m_FollowOffset, targetFollowOffset, Time.deltaTime * cameraZoomSpeed);
    }
}
