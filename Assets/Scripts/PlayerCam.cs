using UnityEngine;
using DG.Tweening;

/// <summary>
/// Class that handles camera rotation and camera effects
/// </summary>
public class PlayerCam : MonoBehaviour
{
    // Sensitivity of the mouse on the X axis
    public float sensX;
    // Sensitivity of the mouse on the Y axis
    public float sensY;
    // Transform used to rotate the player with the camera
    public Transform orientation;
    // Transform used to rotate the camera
    public Transform camHolder;
    // Rotation of the camera on the X axis
    float xRotation;
    // Rotation of the camera on the Y axis
    float yRotation;

    /// <summary>
    /// Lock cursor movement and hide it
    /// </summary>
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Get mouse movement on X and Y axis and use them to calculate rotation of the camera
    /// </summary>
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensX;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    /// <summary>
    /// Adjust camera Field of View depending on the input parameter
    /// </summary>
    /// <param name="endValue"> the Field of View to which camera should be resized </param>
    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    /// <summary>
    /// Tilt the camera to the required angle
    /// </summary>
    /// <param name="zTilt"> Angle to which the camera should be rotated </param>
    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }
}
