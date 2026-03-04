using UnityEngine;
using System.Collections.Generic;

// --- NEW: Define our difficulty levels ---
public enum ArenaDifficulty { Easy, Normal, Hard, Boss }

[System.Serializable]
public class EnemySpawnInfo {
    public GameObject enemyPrefab;
    public Vector2Int spawnCoordinate;
}

[CreateAssetMenu(fileName = "NewArena", menuName = "Game/Arena Data")]
public class ArenaData : ScriptableObject {
    
    // --- NEW: Progression Variables ---
    [Header("Progression Stats")]
    [Tooltip("The chapter or floor number this arena belongs to.")]
    public int tier = 1;
    [Tooltip("The difficulty rating of this specific layout/enemy composition.")]
    public ArenaDifficulty difficulty = ArenaDifficulty.Normal;

    [Header("Grid Dimensions")]
    public int width = 5;
    public int height = 5;

    [HideInInspector]
    public List<Vector2Int> emptyTiles = new List<Vector2Int>(); 

    [Header("Enemies")]
    public List<EnemySpawnInfo> enemies;
}