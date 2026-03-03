using UnityEngine;
using System.Collections;

public class BossVisuals : MonoBehaviour {
    public MeshRenderer bossRenderer;
    public Color idleColor = Color.gray;
    public Color attackingColor = Color.red;
    public float moveSpeed = 5f;

    [Header("Damage Feedback")]
    public float baseShakeDuration = 0.2f;
    public float shakeMultiplier = 0.3f; 
    public float maxShakeMagnitude = 1.5f; 

    [Header("Attack Anticipation")]
    public float bounceHeight = 1f;
    public float bounceDuration = 0.3f;
    
    private Vector3 targetWorldPos;
    
    // Visual offsets
    private Vector3 shakeOffset = Vector3.zero;
    private Vector3 bounceOffset = Vector3.zero;
    
    private Coroutine shakeCoroutine;
    private Coroutine bounceCoroutine;

    void Awake() {
        targetWorldPos = transform.position;
    }

    public void UpdateTargetPosition(Vector3 newPos) {
        targetWorldPos = newPos;
    }

    public void SetVisualState(bool isAttacking) {
        if (bossRenderer != null) {
            bossRenderer.material.color = isAttacking ? attackingColor : idleColor;
        }
    }

    // --- NEW: Trigger the bounce animation ---
    public void TriggerBounce() {
        if (bounceCoroutine != null) StopCoroutine(bounceCoroutine);
        bounceCoroutine = StartCoroutine(BounceRoutine());
    }

    private IEnumerator BounceRoutine() {
        float elapsed = 0f;
        while (elapsed < bounceDuration) {
            // A Sine wave creates a perfect smooth curve from 0 to 1 and back to 0
            float curve = Mathf.Sin((elapsed / bounceDuration) * Mathf.PI);
            float y = curve * bounceHeight;
            
            bounceOffset = new Vector3(0f, y, 0f);
            
            elapsed += Time.deltaTime;
            yield return null; 
        }
        
        bounceOffset = Vector3.zero; 
    }

    public void TriggerShake(float damageAmount) {
        if (shakeCoroutine != null) StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeRoutine(damageAmount));
    }

    private IEnumerator ShakeRoutine(float damageAmount) {
        float elapsed = 0f;
        float currentMagnitude = Mathf.Clamp(shakeMultiplier * damageAmount, 0f, maxShakeMagnitude);

        while (elapsed < baseShakeDuration) {
            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float z = Random.Range(-1f, 1f) * currentMagnitude;
            
            shakeOffset = new Vector3(x, 0f, z);
            elapsed += Time.deltaTime;
            yield return null; 
        }
        shakeOffset = Vector3.zero; 
    }

    void Update() {
        // --- CHANGED: Strip away both offsets to find the logical ground position ---
        Vector3 logicalGroundPos = transform.position - shakeOffset - bounceOffset;
        Vector3 basePos = Vector3.Lerp(logicalGroundPos, targetWorldPos, Time.deltaTime * moveSpeed);
        
        // Add the visual offsets back on top
        transform.position = basePos + shakeOffset + bounceOffset;
    }

    public bool IsAtTarget() {
        // Ignore visual offsets when checking if we reached our destination tile
        return Vector3.Distance(transform.position - shakeOffset - bounceOffset, targetWorldPos) < 0.05f;
    }
}