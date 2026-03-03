using UnityEngine;

public class GridTile : MonoBehaviour {
    public Vector2Int coords;
    public bool isWalkable = true;

    [Header("Visuals")]
    public MeshRenderer meshRenderer;
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color attackingColor = Color.red;
    public Color fireColor = new Color(1f, 0.5f, 0f); // Orange

    private TileState currentState = TileState.Normal;

    void Awake() {
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        SetState(TileState.Normal);
    }

public Color destructionWarningColor = new Color(0.6f, 0f, 1f);

public void SetState(TileState newState) {
    // Priority Check: If we are broken, don't accept 'Warning' or 'Attacking' states.
    // Only allow 'Normal' to restore a broken tile.
    if (currentState == TileState.Broken && newState != TileState.Normal) {
        return; 
    }

    currentState = newState;
    isWalkable = (newState != TileState.Broken);

    switch (newState) {
        case TileState.Normal:
            meshRenderer.material.color = normalColor;
            gameObject.SetActive(true);
            break;
        case TileState.Warning:
            meshRenderer.material.color = warningColor;
            break;
        case TileState.WarningDestruction:
            meshRenderer.material.color = destructionWarningColor;
            break;
        case TileState.Attacking:
            meshRenderer.material.color = attackingColor;
            break;
        case TileState.Broken:
            meshRenderer.material.color = Color.black; // Optional: for debug
            gameObject.SetActive(false); 
            break;
    }
}

    public TileState GetState() => currentState;
}