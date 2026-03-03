using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour {
    // --- NEW: List of Arenas ---
    [Header("Arena Sequence")]
    public List<ArenaData> arenas;
    private int currentArenaIndex = 0;

    // These are now driven by the current ArenaData, so they are hidden from the Inspector
    [HideInInspector] public int width;
    [HideInInspector] public int height;
    
    public float spacing = 1.1f;
    public GameObject tilePrefab;
    public GridTile[,] grid;
    
    [Header("Arena Doors")]
    public GridTile entranceTile;
    public GridTile exitTile;

    [Header("Arena Transition")]
    public float transitionSlideOffset = -10f; 
    public float transitionDuration = 0.5f;
    public float tileStaggerDelay = 0.05f; 
    
    [Header("Enemies")]
    public List<BossController> activeEnemies = new List<BossController>();

    private bool isTransitioning = false;

    void Start() {
        if (arenas.Count == 0) {
            Debug.LogError("No Arenas assigned to the GridManager!");
            return;
        }
        StartCoroutine(InitialSetupRoutine());
    }

// --- RESTORED ANIMATION HELPER ---
    private IEnumerator DelayedSlideIn(GridTile tile, float delay) {
        tile.gameObject.SetActive(false); // Hide the tile while it waits
        yield return new WaitForSeconds(delay); // Wait for the staggered delay
        tile.gameObject.SetActive(true);
        yield return StartCoroutine(tile.SlideIn(transitionDuration, transitionSlideOffset));
    }

    // --- NEW: Wrapper coroutine for the start of the game ---
    private IEnumerator InitialSetupRoutine() {
        // 1. Build the very first arena
        yield return StartCoroutine(GenerateGridAnimated());
        
        // 2. Drop the enemies for the first arena
        yield return StartCoroutine(SpawnEnemies());
    }

    public void EnemyDefeated(BossController enemy) {
        activeEnemies.Remove(enemy);
        if (activeEnemies.Count == 0 && !isTransitioning) {
            // --- NEW: Check if you beat the game ---
            if (currentArenaIndex >= arenas.Count - 1) {
                Debug.Log("YOU WIN! All arenas cleared.");
                // Trigger a win screen here later
            } else {
                UnlockArenaExit();
            }
        }
    }

    private void UnlockArenaExit() {
        if (exitTile != null) {
            exitTile.UnlockExit();
            Debug.Log("Arena Cleared! Exit Unlocked.");
        }
    }

    public void StartArenaTransition(PlayerController player) {
        if (isTransitioning) return;
        StartCoroutine(TransitionRoutine(player));
    }

    private IEnumerator TransitionRoutine(PlayerController player) {
        isTransitioning = true;
        player.isTransitioning = true; 
        
        if (entranceTile != null) StartCoroutine(entranceTile.SlideOut(transitionDuration, transitionSlideOffset));
        if (exitTile != null) StartCoroutine(exitTile.SlideOut(transitionDuration, transitionSlideOffset));

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                if (grid != null && grid[x, y] != null) {
                    StartCoroutine(grid[x, y].SlideOut(transitionDuration, transitionSlideOffset));
                }
            }
        }

        yield return new WaitForSeconds(transitionDuration + 0.5f); 

        // --- NEW: Move to the next arena data ---
        currentArenaIndex++;
        ArenaData nextArena = arenas[currentArenaIndex];
        width = nextArena.width;
        height = nextArena.height;

        int startX = width / 2;
        int startY = 0; 
        player.currentX = startX;
        player.currentY = startY;
        player.transform.position = GetWorldPos(startX, startY);

        yield return StartCoroutine(GenerateGridAnimated());
        yield return StartCoroutine(SpawnEnemies());

        isTransitioning = false;
        player.isTransitioning = false; 
    }

    private IEnumerator GenerateGridAnimated() {
        // --- NEW: Fetch current arena data ---
        ArenaData currentArena = arenas[currentArenaIndex];
        width = currentArena.width;
        height = currentArena.height;

        grid = new GridTile[width, height];
        int entryX = width / 2;
        int entryY = -1; 
        int exitX = width / 2;
        int exitY = height; 

        // 1. Generate Protruding Entrance
        Vector3 entPos = new Vector3(entryX * spacing, 0, entryY * spacing);
        GameObject entObj = Instantiate(tilePrefab, entPos, Quaternion.identity, transform);
        entranceTile = entObj.GetComponent<GridTile>();
        entranceTile.coords = new Vector2Int(entryX, entryY);
        entranceTile.SetupAsEntrance();
        StartCoroutine(DelayedSlideIn(entranceTile, 0f)); 

        // 2. Generate Main Arena Layout
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                // --- NEW: Check if this tile is marked as a "hole" in our layout ---
                if (currentArena.emptyTiles.Contains(new Vector2Int(x, y))) {
                    continue; // Skip building this tile!
                }

                Vector3 pos = new Vector3(x * spacing, 0, y * spacing);
                GameObject newTile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                
                grid[x, y] = newTile.GetComponent<GridTile>();
                grid[x, y].coords = new Vector2Int(x, y);

                float dist = Vector2Int.Distance(new Vector2Int(x, y), new Vector2Int(entryX, entryY));
                StartCoroutine(DelayedSlideIn(grid[x, y], dist * tileStaggerDelay));
            }
        }

        // 3. Generate Protruding Exit
        Vector3 exitPos = new Vector3(exitX * spacing, 0, exitY * spacing);
        GameObject exitObj = Instantiate(tilePrefab, exitPos, Quaternion.identity, transform);
        exitTile = exitObj.GetComponent<GridTile>();
        exitTile.coords = new Vector2Int(exitX, exitY);
        exitTile.SetupAsExit();
        
        float exitDist = Vector2Int.Distance(new Vector2Int(exitX, exitY), new Vector2Int(entryX, entryY));
        StartCoroutine(DelayedSlideIn(exitTile, exitDist * tileStaggerDelay));

        yield return new WaitForSeconds((exitDist * tileStaggerDelay) + transitionDuration);
    }

    private IEnumerator SpawnEnemies() {
        // --- NEW: Loop through the enemy list defined in the ArenaData ---
        ArenaData currentArena = arenas[currentArenaIndex];

        foreach (var enemyInfo in currentArena.enemies) {
            Vector3 spawnPos = GetWorldPos(enemyInfo.spawnCoordinate.x, enemyInfo.spawnCoordinate.y) + new Vector3(0, 10f, 0); 
            
            // Instantiate the specific prefab set in the Inspector
            GameObject enemyObj = Instantiate(enemyInfo.enemyPrefab, spawnPos, Quaternion.identity);
            BossController boss = enemyObj.GetComponent<BossController>();
            
            boss.bossX = enemyInfo.spawnCoordinate.x;
            boss.bossY = enemyInfo.spawnCoordinate.y;
            boss.grid = this;
            activeEnemies.Add(boss);

            Vector3 targetPos = GetWorldPos(boss.bossX, boss.bossY);
            StartCoroutine(DropEnemyRoutine(boss, spawnPos, targetPos));
        }

        yield return new WaitForSeconds(0.5f); // Wait for drops to finish
    }

    // Moved the drop animation to its own coroutine so multiple enemies drop at the same time
    private IEnumerator DropEnemyRoutine(BossController boss, Vector3 start, Vector3 end) {
        float elapsed = 0f;
        while (elapsed < 0.5f) {
            if (boss == null) yield break; // Safety check
            boss.transform.position = Vector3.Lerp(start, end, elapsed / 0.5f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (boss != null) boss.transform.position = end;
    }

    public Vector3 GetWorldPos(int x, int y) { return new Vector3(x * spacing, 1f, y * spacing); }

    public bool IsValidMove(int x, int y) {
        if (x == width / 2 && y == -1) return entranceTile != null && entranceTile.isWalkable;
        if (x == width / 2 && y == height) return exitTile != null && exitTile.isWalkable;

        if (x < 0 || x >= width || y < 0 || y >= height) return false;
        if (grid == null || grid[x, y] == null) return false; // Added safety check for empty layout tiles
        return grid[x, y].isWalkable;
    }

    public void SetTileState(int x, int y, TileState state) {
        if (x == width / 2 && y == -1 && entranceTile != null) { entranceTile.SetState(state); return; }
        if (x == width / 2 && y == height && exitTile != null) { exitTile.SetState(state); return; }
        if (x >= 0 && x < width && y >= 0 && y < height) {
            if (grid != null && grid[x, y] != null) grid[x, y].SetState(state);
        }
    }

    public void SetRowState(int y, TileState state) { for (int x = 0; x < width; x++) SetTileState(x, y, state); }
    public void SetColumnState(int x, TileState state) { for (int y = 0; y < height; y++) SetTileState(x, y, state); }
    public void BreakTile(int x, int y, float duration) { if (x >= 0 && x < width && y >= 0 && y < height && grid[x,y] != null) StartCoroutine(BreakRoutine(x, y, duration)); }
    private IEnumerator BreakRoutine(int x, int y, float duration) { SetTileState(x, y, TileState.Broken); yield return new WaitForSeconds(duration); SetTileState(x, y, TileState.Normal); }
}