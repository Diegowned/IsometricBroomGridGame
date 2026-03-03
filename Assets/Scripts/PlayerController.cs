using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
    public GridManager gridManager;
    public int currentX = 0, currentY = 0;
    public int health = 3;
    public bool isAttacking = false;

[Header("Visual Aiming")]
public LineRenderer aimLine;
public float maxLineDistance = 50f;
public LayerMask bossLayer; // Set this to the "Boss" layer in the Inspector


    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform firePoint; // Where the bullet spawns (e.g., player's hand)
    public float fireRate = 0.2f;
    private float nextFireTime;
    public LayerMask aimLayerMask; // Set this to "AimPlane" in the Inspector

    [Header("Invincibility")]
    public float invincibilityDuration = 1.5f;
    public float blinkInterval = 0.1f;
    private bool isInvincible = false;
    public MeshRenderer playerRenderer;

    void Start() {
        transform.position = gridManager.GetWorldPos(currentX, currentY);
        if (playerRenderer == null) playerRenderer = GetComponentInChildren<MeshRenderer>();
    }

void Update() {
        HandleMovementInput(); // WASD Movement
        HandleRotation();       // Twin-stick aiming
        HandleShooting();       // Mouse/Controller Fire
        UpdateAimLine();
        
        CheckTileSafety();
    }

private void HandleRotation() {
    // 1. Controller Input (Right Stick)
    float horizontal = Input.GetAxis("RightStickHorizontal");
    float vertical = Input.GetAxis("RightStickVertical");
    Vector2 stickDirection = new Vector2(horizontal, vertical);

    if (stickDirection.sqrMagnitude > 0.1f) {
        float targetAngle = Mathf.Atan2(horizontal, vertical) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, targetAngle, 0);
    }
    else {
        // 2. Mouse Input using the Invisible Plane
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
// 1. Raycast against the invisible Aim Plane to rotate the player
    if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimLayerMask)) {
        Vector3 targetPoint = hit.point;
        targetPoint.y = transform.position.y; 
        
        Vector3 direction = (targetPoint - transform.position).normalized;
        if (direction != Vector3.zero) {
            transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}

    private void HandleShooting() {
        // Fire with Mouse Button 0 or Controller R2/Right Trigger
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime) {
            nextFireTime = Time.time + fireRate;
            Shoot();
        }
    }

    private void Shoot() {
        GameObject bullet = Instantiate(projectilePrefab, firePoint.position, transform.rotation);
        // Ensure bullet has a simple script to move forward
    }

    private void CheckTileSafety() {
    // If the current tile is broken or invalid, find safety
    if (!gridManager.IsValidMove(currentX, currentY)) {
        TakeDamage(1);
        MoveToSafeTile();
    }
}

private void MoveToSafeTile() {
    // Search neighbors: Up, Down, Left, Right
    Vector2Int[] directions = {
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(-1, 0), new Vector2Int(1, 0)
    };

    foreach (var dir in directions) {
        int checkX = currentX + dir.x;
        int checkY = currentY + dir.y;

        if (gridManager.IsValidMove(checkX, checkY)) {
            currentX = checkX;
            currentY = checkY;
            transform.position = gridManager.GetWorldPos(currentX, currentY);
            return; // Found a safe spot
        }
    }
}
    
    // If no adjacent tiles are safe, you could expand search or trigger Game Over

    public void TakeDamage(int amount) {
        if (isInvincible || health <= 0) return;

        health -= amount;
        Debug.Log("Player Health: " + health);

        if (health <= 0) {
            Debug.Log("Game Over");
            return;
        }

        StartCoroutine(BlinkRoutine());
    }

    private IEnumerator BlinkRoutine() {
        isInvincible = true;
        float timer = 0;
        while (timer < invincibilityDuration) {
            playerRenderer.enabled = !playerRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }
        playerRenderer.enabled = true;
        isInvincible = false;
    }

private void HandleMovementInput() {
    if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) 
        TryMove(0, 1);
    if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) 
        TryMove(0, -1);
    if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) 
        TryMove(-1, 0);
    if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) 
        TryMove(1, 0);
}

private void TryMove(int xDir, int yDir) {
    int targetX = currentX + xDir;
    int targetY = currentY + yDir;

  
    if (gridManager.IsValidMove(targetX, targetY)) {
        currentX = targetX;
        currentY = targetY;

        transform.position = gridManager.GetWorldPos(currentX, currentY);
    }
}

    IEnumerator AttackRoutine() {
        isAttacking = true;
        yield return new WaitForSeconds(0.5f);
        isAttacking = false;
    }

private void UpdateAimLine() {
    if (aimLine == null) return;

    // Start of the line is always at the fire point
    aimLine.SetPosition(0, firePoint.position);

    // Cast a ray forward from the player's current facing direction
    RaycastHit hit;
    if (Physics.Raycast(firePoint.position, transform.forward, out hit, maxLineDistance, bossLayer)) {
        // If it hits the boss, stop the line at the hit point
        aimLine.SetPosition(1, hit.point);
    } else {
        // Otherwise, extend it to its maximum length
        aimLine.SetPosition(1, firePoint.position + transform.forward * maxLineDistance);
    }
}

}