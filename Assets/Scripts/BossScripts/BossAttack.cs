using UnityEngine;
using System.Collections;

public abstract class BossAttack : ScriptableObject {
    public string attackName;
    public float telegraphDuration = 1.0f;
    public float actionDuration = 0.5f;

    // This is the logic that each specific attack will override
    public abstract IEnumerator Execute(BossController boss, GridManager grid);
}