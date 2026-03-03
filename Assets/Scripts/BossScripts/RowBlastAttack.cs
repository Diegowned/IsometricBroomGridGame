using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Boss/Attacks/Row Blast")]
public class RowBlastAttack : BossAttack {
    public override IEnumerator Execute(BossController boss, GridManager grid) {
        int targetRow = Random.Range(0, grid.height);

        // Telegraph
        grid.SetRowState(targetRow, TileState.Warning);
        yield return new WaitForSeconds(telegraphDuration);

        // Action
        grid.SetRowState(targetRow, TileState.Attacking);

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null && player.currentY == targetRow) {
            player.TakeDamage(1);
        }

        yield return new WaitForSeconds(actionDuration);

        // Cleanup
        grid.SetRowState(targetRow, TileState.Normal);
    }
}