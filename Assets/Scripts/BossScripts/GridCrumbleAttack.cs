using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Boss/Attacks/Grid Crumble")]
public class GridCrumbleAttack : BossAttack {
    public float breakDuration = 3.0f;

    public override IEnumerator Execute(BossController boss, GridManager grid) {
        int centerX = Random.Range(1, grid.width - 1);
        int centerY = Random.Range(1, grid.height - 1);

        // 1. Telegraph: Warn the 3x3 area
    SetAreaState(grid, centerX, centerY, TileState.WarningDestruction);
    yield return new WaitForSeconds(telegraphDuration);

        // 2. Action: Break the tiles
for (int x = centerX - 1; x <= centerX + 1; x++) {
        for (int y = centerY - 1; y <= centerY + 1; y++) {
            grid.BreakTile(x, y, breakDuration);
            }
        }

        yield return new WaitForSeconds(actionDuration);
    }

    private void SetAreaState(GridManager grid, int cx, int cy, TileState state) {
        for (int x = cx - 1; x <= cx + 1; x++) {
            for (int y = cy - 1; y <= cy + 1; y++) {
                grid.SetTileState(x, y, state);
            }
        }
    }
}