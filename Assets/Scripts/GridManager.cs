using UnityEngine;
using System.Collections;
public class GridManager : MonoBehaviour {
    public int width = 5;
    public int height = 5;
    public float spacing = 1.1f; // Distance between tile centers
    public GameObject tilePrefab;
    
    private GridTile[,] grid;

    void Awake() {
        GenerateGrid();
    }


    public void BreakTile(int x, int y, float duration) {
        if (x >= 0 && x < width && y >= 0 && y < height) {
            StartCoroutine(BreakRoutine(x, y, duration));
        }
    }

    private IEnumerator BreakRoutine(int x, int y, float duration) {
        // Set the state to Broken (this hides the tile and makes it non-walkable)
        SetTileState(x, y, TileState.Broken);

        yield return new WaitForSeconds(duration);

        // Return the tile to Normal state
        SetTileState(x, y, TileState.Normal);
    }

    void GenerateGrid() {
        grid = new GridTile[width, height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Vector3 pos = new Vector3(x * spacing, 0, y * spacing);
                GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                
                grid[x, y] = newTile.GetComponent<GridTile>();
                grid[x, y].coords = new Vector2Int(x, y);
            }
        }
    }

    public Vector3 GetWorldPos(int x, int y) {
        return new Vector3(x * spacing, 1f, y * spacing); // 1f is player height
    }

    public bool IsValidMove(int x, int y) {
        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        return grid[x, y].isWalkable;
    }
    

public void SetTileState(int x, int y, TileState state) {
    if (x >= 0 && x < width && y >= 0 && y < height) {
        // Even if a tile is currently Broken (Inactive), we update its state.
        // When the BreakRoutine finishes and sets it to Normal, it will 
        // respect the most recent state assigned.
        grid[x, y].SetState(state);
    }
}
public void SetRowState(int y, TileState state) {
    for (int x = 0; x < width; x++) {
        SetTileState(x, y, state);
    }
}

public void SetColumnState(int x, TileState state) {
    for (int y = 0; y < height; y++) {
        SetTileState(x, y, state);
    }
}

}