using UnityEngine;

public class PlayerCanCreate : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject prefab;
    public LayerMask ignoreLayerMask;
    public float maxSpawnDistance = 100.0f;

    public void OnAction()
    {
        Create();
    }

    private void Create()
    {
        var worldLocation = GetPosition();
        
        CreateOnPosition(worldLocation);
    }

    private Vector3 GetPosition()
    {
        var cameraTransform = playerCamera.transform;

        return Physics.Raycast(cameraTransform.position, cameraTransform.forward, out var hit, maxSpawnDistance, ~ignoreLayerMask) ? hit.point : Vector3.negativeInfinity;
    }

    private void CreateOnPosition(Vector3 spawnPoint)
    {
        if (spawnPoint.Equals(Vector3.negativeInfinity))
        {
            return;
        }

        Instantiate(prefab, spawnPoint, prefab.transform.rotation);
    }
}
