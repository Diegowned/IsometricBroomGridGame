using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossController : MonoBehaviour {
    public GridManager grid;
    public List<BossPhase> phases;
    public BossVisuals visuals; // Assign in Inspector
    private Coroutine orbitCoroutine;

    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    private int currentPhaseIndex = 0;
    private int currentAttackIndex = 0;
    private bool isPhaseTransitioning = false;

    // Movement settings for the Boss
    public int bossX = 3, bossY = 3; 

    void Start() {
        currentHealth = maxHealth;
        transform.position = grid.GetWorldPos(bossX, bossY);
        StartCoroutine(BossLoop());
        StartCoroutine(PhaseBehaviorMonitor());
    }

IEnumerator PhaseBehaviorMonitor() {
        int lastPhaseIndex = -1;
        while (currentHealth > 0) {
            if (currentPhaseIndex != lastPhaseIndex) {
                lastPhaseIndex = currentPhaseIndex;
                BossPhase currentPhase = phases[currentPhaseIndex];

                if (currentPhase.continuousOrbit) {
                    if (orbitCoroutine == null) orbitCoroutine = StartCoroutine(OrbitRoutine());
                } else {
                    if (orbitCoroutine != null) {
                        StopCoroutine(orbitCoroutine);
                        orbitCoroutine = null;
                    }
                }
            }
            yield return new WaitForSeconds(0.5f);
        }
    }



    IEnumerator ContinuousOrbitChecker() {
        while (currentHealth > 0) {
            BossPhase currentPhase = phases[currentPhaseIndex];

            if (currentPhase.continuousOrbit && orbitCoroutine == null) {
                // Start orbiting if the phase calls for it and we aren't already
                orbitCoroutine = StartCoroutine(OrbitRoutine());
            } 
            else if (!currentPhase.continuousOrbit && orbitCoroutine != null) {
                // Stop orbiting if the phase changes to one without it
                StopCoroutine(orbitCoroutine);
                orbitCoroutine = null;
            }
            yield return new WaitForSeconds(0.5f); // Check periodically
        }
    }


IEnumerator OrbitRoutine() {
        while (true) {
            // Calculate the path once per loop
            List<Vector2Int> path = GetPerimeterPath(grid.width, grid.height);
            
            foreach (Vector2Int tile in path) {
                bossX = tile.x;
                bossY = tile.y;
                
                // Update visuals only, do NOT 'yield return' a Wait call here 
                // if you want attacks to trigger simultaneously.
                // Instead, we wait for a set time or use visuals.IsAtTarget()
                visuals.UpdateTargetPosition(grid.GetWorldPos(bossX, bossY));
                
                while (!visuals.IsAtTarget()) {
                    yield return null; // Wait for visual arrival without blocking BossLoop
                }
            }
        }
    }
private List<Vector2Int> GetPerimeterPath(int w, int h) {
        List<Vector2Int> path = new List<Vector2Int>();
        for (int x = 0; x < w; x++) path.Add(new Vector2Int(x, 0));
        for (int y = 1; y < h; y++) path.Add(new Vector2Int(w - 1, y));
        for (int x = w - 2; x >= 0; x--) path.Add(new Vector2Int(x, h - 1));
        for (int y = h - 2; y >= 1; y--) path.Add(new Vector2Int(0, y));
        return path;
    }

IEnumerator BossLoop() {
    while (currentHealth > 0) {
        BossPhase currentPhase = phases[currentPhaseIndex];
        BossAttack currentAttack = currentPhase.attackSequence[currentAttackIndex];

        visuals.SetVisualState(true);
        
        yield return StartCoroutine(currentAttack.Execute(this, grid));

        // End Attack Visuals
        visuals.SetVisualState(false);

        currentAttackIndex++;
        if (currentAttackIndex >= currentPhase.attackSequence.Count) {
            currentAttackIndex = 0;
        }

        yield return new WaitForSeconds(1f);
    }
}

public void MoveBoss(int x, int y) {
    bossX = x;
    bossY = y;
    visuals.UpdateTargetPosition(grid.GetWorldPos(x, y));
}

    public void TakeDamage(float amount) {
        currentHealth -= amount;
        Debug.Log("Boss hit! Remaining health: " + currentHealth);
        CheckPhaseTransition(); 
    }

    void CheckPhaseTransition() {
        if (currentPhaseIndex + 1 < phases.Count) {
            float healthPercent = currentHealth / maxHealth;
            if (healthPercent <= phases[currentPhaseIndex + 1].healthThreshold) {
                currentPhaseIndex++;
                currentAttackIndex = 0;
                Debug.Log("Switched to Phase " + currentPhaseIndex);
            }
        }
    }

// Helper methods for the Debug UI
public float GetCurrentHealth() => currentHealth;
public int GetCurrentPhaseIndex() => currentPhaseIndex;
public BossAttack GetCurrentAttack() {
    if (phases.Count > 0 && currentPhaseIndex < phases.Count) {
        var sequence = phases[currentPhaseIndex].attackSequence;
        if (currentAttackIndex < sequence.Count) {
            return sequence[currentAttackIndex];
        }
    }
    return null;
}

public IEnumerator MoveBossAndWait(int x, int y) {
    bossX = x; 
    bossY = y;
    
    // Tell visuals where to go
    visuals.UpdateTargetPosition(grid.GetWorldPos(x, y)); 
    
    // Wait until the visual object physically arrives at the destination
    while (!visuals.IsAtTarget()) {
        yield return null;
    }
}

}