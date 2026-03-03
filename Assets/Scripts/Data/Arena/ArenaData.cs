using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EnemySpawnInfo {
    public GameObject enemyPrefab;
    public Vector2Int spawnCoordinate;
}

[CreateAssetMenu(fileName = "NewArena", menuName = "Game/Arena Data")]
public class ArenaData : ScriptableObject {
    [Header("Grid Dimensions")]
    public int width = 5;
    public int height = 5;

    // --- CHANGED: Hide this from the normal Inspector so we can draw our custom grid instead! ---
    [HideInInspector]
    public List<Vector2Int> emptyTiles = new List<Vector2Int>(); 

    [Header("Enemies")]
    public List<EnemySpawnInfo> enemies;
}