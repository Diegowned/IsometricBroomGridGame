using UnityEngine;
using System.Collections.Generic; // Needed for List<>

[CreateAssetMenu(menuName = "Boss/Phase")]
public class BossPhase : ScriptableObject {
public string phaseName;
    public float healthThreshold;
    public List<BossAttack> attackSequence;
    public bool continuousOrbit; // The toggle you requested
}