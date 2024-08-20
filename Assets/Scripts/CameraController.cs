using UnityEngine;

public class CameraController : MonoBehaviour
{
    public void PositionCamera(int gridSize)
    {
        // Calculate the center of the grid
        Vector3 centerPosition = new Vector3((gridSize - 1) / 2f, 0, (gridSize - 1) / 2f);

        // Set the camera's position above the grid
        // Adjust the y-value to ensure the grid fits within the camera's view
        float cameraHeight = gridSize * 1.5f; // This multiplier can be adjusted based on the field of view
        transform.position = new Vector3(centerPosition.x, cameraHeight, centerPosition.z);

        // Set the camera to look directly downwards
        transform.rotation = Quaternion.Euler(90, 0, 0);

        // Optionally, adjust the camera's orthographic size if you're using an orthographic camera
        Camera.main.orthographic = true; // Ensure the camera is set to orthographic
        Camera.main.orthographicSize = gridSize / 2f;
    }
}
