using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Boss/Attacks/Spiral Blast")]
public class SpiralAttack : BossAttack {
    public float delayBetweenTiles = 0.1f; // Speed of the spiral growth

    public override IEnumerator Execute(BossController boss, GridManager grid) {
        List<Vector2Int> spiralPath = CalculateSpiralPath(grid.width, grid.height);

        // We process the spiral in sequence
        foreach (Vector2Int coord in spiralPath) {
            // Start a separate coroutine for each tile so the "Spiral" 
            // keeps moving while individual tiles are still in telegraph/attack phase
            boss.StartCoroutine(ProcessTile(grid, coord));
            
            yield return new WaitForSeconds(delayBetweenTiles);
        }

        // Wait for the final tile in the sequence to finish its action duration
        yield return new WaitForSeconds(telegraphDuration + actionDuration);
    }

    private IEnumerator ProcessTile(GridManager grid, Vector2Int coord) {
        // Telegraph
        grid.SetTileState(coord.x, coord.y, TileState.Warning);
        yield return new WaitForSeconds(telegraphDuration);

        // Action
        grid.SetTileState(coord.x, coord.y, TileState.Attacking);
        
        // Damage Check
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.currentX == coord.x && player.currentY == coord.y) {
            player.TakeDamage(1);
        }

        yield return new WaitForSeconds(actionDuration);

        // Cleanup
        grid.SetTileState(coord.x, coord.y, TileState.Normal);
    }

    private List<Vector2Int> CalculateSpiralPath(int w, int h) {
        List<Vector2Int> path = new List<Vector2Int>();
        int top = h - 1, bottom = 0, left = 0, right = w - 1;

        while (left <= right && bottom <= top) {
            // Right
            for (int i = left; i <= right; i++) path.Add(new Vector2Int(i, bottom));
            bottom++;

            // Up
            for (int i = bottom; i <= top; i++) path.Add(new Vector2Int(right, i));
            right--;

            // Left
            if (bottom <= top) {
                for (int i = right; i >= left; i--) path.Add(new Vector2Int(i, top));
                top--;
            }

            // Down
            if (left <= right) {
                for (int i = top; i >= bottom; i--) path.Add(new Vector2Int(left, i));
                left++;
            }
        }
        return path;
    }
}