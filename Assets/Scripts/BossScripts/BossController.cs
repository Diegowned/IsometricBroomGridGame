using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class BossController : MonoBehaviour {
    public GridManager grid;
    public List<BossPhase> phases;
    public BossVisuals visuals; 
    private Coroutine orbitCoroutine;

    [Header("Stats")]
    public float maxHealth = 100f;
    private float currentHealth;
    
    [Header("UI")]
    public Image healthBarFill;

    private int currentPhaseIndex = 0;
    private int currentAttackIndex = 0;
    private bool isPhaseTransitioning = false;

    public int bossX = 3, bossY = 3; 

void Start() {
        currentHealth = maxHealth;
        UpdateHealthBar(); 
        
        visuals.UpdateTargetPosition(grid.GetWorldPos(bossX, bossY));
        
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
                orbitCoroutine = StartCoroutine(OrbitRoutine());
            } 
            else if (!currentPhase.continuousOrbit && orbitCoroutine != null) {
                StopCoroutine(orbitCoroutine);
                orbitCoroutine = null;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }


IEnumerator OrbitRoutine() {
        while (true) {
            List<Vector2Int> path = GetPerimeterPath(grid.width, grid.height);
            
            foreach (Vector2Int tile in path) {
                bossX = tile.x;
                bossY = tile.y;
                

                visuals.UpdateTargetPosition(grid.GetWorldPos(bossX, bossY));
                
                while (!visuals.IsAtTarget()) {
                    yield return null; 
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

            // Turn Red
            visuals.SetVisualState(true);
            
            // --- NEW: Do a quick anticipation bounce! ---
            if (visuals != null) visuals.TriggerBounce();
            
            // Wait for the attack logic to finish
            yield return StartCoroutine(currentAttack.Execute(this, grid));

            // End Attack Visuals (Turn Gray)
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
        
        UpdateHealthBar(); 
        
        if (visuals != null) {
            visuals.TriggerShake(amount);
        }

        if (currentHealth <= 0) {
            if (grid != null) grid.EnemyDefeated(this);
            
            Canvas healthCanvas = GetComponentInChildren<Canvas>();
            if (healthCanvas != null) healthCanvas.gameObject.SetActive(false);
            
            Destroy(gameObject); 
        } else {
            CheckPhaseTransition(); 
        }
    }

    private void UpdateHealthBar() {
        if (healthBarFill != null) {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
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
        visuals.UpdateTargetPosition(grid.GetWorldPos(x, y)); 
        while (!visuals.IsAtTarget()) {
            yield return null;
        }
    }
}