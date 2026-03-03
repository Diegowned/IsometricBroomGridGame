using UnityEngine;

public class BossVisuals : MonoBehaviour {
    public MeshRenderer bossRenderer;
    public Color idleColor = Color.gray;
    public Color attackingColor = Color.red;
    public float moveSpeed = 5f;

    private Vector3 targetWorldPos;

    public void UpdateTargetPosition(Vector3 newPos) {
        targetWorldPos = newPos;
    }

    public void SetVisualState(bool isAttacking) {
        if (bossRenderer != null) {
            bossRenderer.material.color = isAttacking ? attackingColor : idleColor;
        }
    }

    void Update() {
        transform.position = Vector3.Lerp(transform.position, targetWorldPos, Time.deltaTime * moveSpeed);
    }

    public bool IsAtTarget() {
    // Returns true if the visual object is very close to the logical position
    return Vector3.Distance(transform.position, targetWorldPos) < 0.05f;
}
}