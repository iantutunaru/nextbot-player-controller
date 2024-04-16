using UnityEngine;

/// <summary>
/// Class used to move the camera to follow player movement
/// </summary>
public class MoveCamera : MonoBehaviour
{
    // Transform representing current camera position
    public Transform cameraPosition;

    /// <summary>
    /// Update camera position with player movement
    /// </summary>
    void Update()
    {
        transform.position = cameraPosition.position;
    }
}
