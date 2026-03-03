using UnityEngine;
using UnityEditor; // Required for making custom Inspectors!
using System.Collections.Generic;

// This tells Unity to use this script whenever you click on an ArenaData file
[CustomEditor(typeof(ArenaData))]
public class ArenaDataEditor : Editor {
    
    public override void OnInspectorGUI() {
        // Get a reference to the specific Arena file we are currently looking at
        ArenaData arena = (ArenaData)target;

        // --- NEW: Safety check to prevent Inspector crashes! ---
        if (arena.emptyTiles == null) {
            arena.emptyTiles = new List<Vector2Int>();
        }

        // 1. Draw the default stuff (Width, Height, Enemies list)
        DrawDefaultInspector();

        GUILayout.Space(20); // Add some visual breathing room

        // 2. Add our Custom UI Header
        GUILayout.Label("Visual Arena Layout", EditorStyles.boldLabel);
        GUILayout.Label("Green = Floor | Red = Hole", EditorStyles.helpBox);

        // 3. Draw the interactive grid!
        // We loop the Y axis backwards (from top to bottom) so the visual grid in the Inspector 
        // matches the layout of the physical grid in the game world (where 0,0 is the bottom left).
        for (int y = arena.height - 1; y >= 0; y--) {
            
            GUILayout.BeginHorizontal(); // Start a new row
            
            for (int x = 0; x < arena.width; x++) {
                Vector2Int pos = new Vector2Int(x, y);
                
                // Check if this specific tile is in our "holes" list
                bool isHole = arena.emptyTiles.Contains(pos);

                // Change the button color based on its state
                GUI.backgroundColor = isHole ? Color.red : Color.green;

                // Draw the button and check if the user clicked it THIS frame
                if (GUILayout.Button(isHole ? "Hole" : "Tile", GUILayout.Width(45), GUILayout.Height(45))) {
                    
                    // Tell Unity we are about to make a change (This allows you to use Ctrl+Z to undo!)
                    Undo.RecordObject(arena, "Toggle Arena Tile");
                    
                    if (isHole) {
                        arena.emptyTiles.Remove(pos); // Turn back into floor
                    } else {
                        arena.emptyTiles.Add(pos);    // Turn into a hole
                    }

                    // Tell Unity this file has been modified and needs to be saved
                    EditorUtility.SetDirty(arena);
                }
                
                GUI.backgroundColor = Color.white; // Reset color for the next UI elements
            }
            
            GUILayout.EndHorizontal(); // End the row
        }
    }
}