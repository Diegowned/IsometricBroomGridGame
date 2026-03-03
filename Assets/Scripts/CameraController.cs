using UnityEngine;

public class CameraController : MonoBehaviour {
    [Header("Targets")]
    public Transform player;
    public GridManager grid;

    [Header("Positioning")]
    public Vector3 offset = new Vector3(0, 15f, -10f); // Adjust for height and tilt
    public float smoothSpeed = 5f;

    [Header("Framing")]
    public float minHeight = 10f;
    public float maxHeight = 25f;
    public float gridPadding = 2f;

    void LateUpdate() {
        if (player == null || grid == null) return;

        // 1. Calculate the ideal position based on player + offset
        Vector3 targetPosition = player.position + offset;

        // 2. Dynamic Zoom: Adjust height (Y) based on player distance from grid center
        // This ensures the camera pulls back if the player goes to the edges
        Vector3 gridCenter = new Vector3((grid.width * grid.spacing) / 2, 0, (grid.height * grid.spacing) / 2);
        float distFromCenter = Vector3.Distance(player.position, gridCenter);
        
        // Tilt/Zoom logic: Higher distance = Higher Y
        targetPosition.y = Mathf.Clamp(offset.y + (distFromCenter * 0.5f), minHeight, maxHeight);

        // 3. Smooth Movement
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // 4. The 2.5D Tilt: Always look towards the center of the action
        // We look slightly ahead of the player or at the grid center
        Vector3 lookTarget = (player.position + gridCenter) / 2f;
        transform.LookAt(lookTarget);
    }
}