using UnityEngine;
using System.Collections;

[CreateAssetMenu(menuName = "Boss/Attacks/Move Boss")]
public class BossMoveAttack : BossAttack {
public override IEnumerator Execute(BossController boss, GridManager grid) {
    int targetX = Random.Range(0, grid.width);
    int targetY = Random.Range(0, grid.height);

    yield return boss.StartCoroutine(boss.MoveBossAndWait(targetX, targetY));
    
    yield return new WaitForSeconds(actionDuration);
}
}