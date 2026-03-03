using UnityEngine;
using System.Collections; // Needed for Coroutines

public class GridTile : MonoBehaviour {
    public Vector2Int coords;
    public bool isWalkable = true;

    [Header("Visuals")]
    public MeshRenderer meshRenderer;
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color attackingColor = Color.red;
    public Color fireColor = new Color(1f, 0.5f, 0f);
    public Color destructionWarningColor = new Color(0.6f, 0f, 1f);

    [Header("Arena Doors")]
    public bool isEntrance = false;
    public bool isExit = false;
    public bool isExitUnlocked = false;
    public Color entranceColor = Color.cyan;
    public Color exitLockedColor = Color.gray;
    public Color exitUnlockedColor = Color.green;

    private TileState currentState = TileState.Normal;

    void Awake() {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        SetState(TileState.Normal);
    }

    public void SetState(TileState newState) {
        if (currentState == TileState.Broken && newState != TileState.Normal) {
            return; 
        }

        currentState = newState;
        isWalkable = (newState != TileState.Broken);

        // Don't overwrite door colors with the normal color
        if (newState == TileState.Normal) {
            if (isEntrance) meshRenderer.material.color = entranceColor;
            else if (isExit) meshRenderer.material.color = isExitUnlocked ? exitUnlockedColor : exitLockedColor;
            else meshRenderer.material.color = normalColor;
            
            gameObject.SetActive(true);
            return;
        }

        switch (newState) {
            case TileState.Warning: meshRenderer.material.color = warningColor; break;
            case TileState.WarningDestruction: meshRenderer.material.color = destructionWarningColor; break;
            case TileState.Attacking: meshRenderer.material.color = attackingColor; break;
            case TileState.Broken:
                meshRenderer.material.color = Color.black; 
                gameObject.SetActive(false); 
                break;
        }
    }

    public TileState GetState() => currentState;

    // --- NEW: Setup Methods for Doors ---
    public void SetupAsEntrance() {
        isEntrance = true;
        SetState(TileState.Normal);
    }

    public void SetupAsExit() {
        isExit = true;
        isExitUnlocked = false;
        SetState(TileState.Normal);
    }

    public void UnlockExit() {
        if (isExit) {
            isExitUnlocked = true;
            SetState(TileState.Normal);
        }
    }

    // --- NEW: Animation Coroutines ---
    public IEnumerator SlideIn(float duration, float startYOffset) {
        Vector3 endPos = transform.position;
        Vector3 startPos = endPos + new Vector3(0, startYOffset, 0);
        transform.position = startPos;

        float elapsed = 0f;
        while (elapsed < duration) {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }

    public IEnumerator SlideOut(float duration, float endYOffset) {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, endYOffset, 0);

        float elapsed = 0f;
        while (elapsed < duration) {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
        Destroy(gameObject); // Cleanup old tile
    }
}