using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Boss/Attacks/Orbit Move")]
public class OrbitMoveAttack : BossAttack {
    public int laps = 1;

    public override IEnumerator Execute(BossController boss, GridManager grid) {
        // 1. Calculate the path around the perimeter
        List<Vector2Int> path = GetPerimeterPath(grid.width, grid.height);

        for (int l = 0; l < laps; l++) {
            foreach (Vector2Int tile in path) {
                // Move boss and wait for the BossVisuals to arrive
                yield return boss.StartCoroutine(boss.MoveBossAndWait(tile.x, tile.y));
                
                // Optional: Short pause at each corner or tile
                // yield return new WaitForSeconds(0.1f); 
            }
        }
    }

    private List<Vector2Int> GetPerimeterPath(int w, int h) {
        List<Vector2Int> path = new List<Vector2Int>();

        // Bottom edge (Left to Right)
        for (int x = 0; x < w; x++) path.Add(new Vector2Int(x, 0));
        // Right edge (Bottom to Top)
        for (int y = 1; y < h; y++) path.Add(new Vector2Int(w - 1, y));
        // Top edge (Right to Left)
        for (int x = w - 2; x >= 0; x--) path.Add(new Vector2Int(x, h - 1));
        // Left edge (Top to Bottom)
        for (int y = h - 2; y >= 1; y--) path.Add(new Vector2Int(0, y));

        return path;
    }
}