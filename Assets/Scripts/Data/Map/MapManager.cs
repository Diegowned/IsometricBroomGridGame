using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MapManager : MonoBehaviour {
    [Header("References")]
    public GridManager gridManager;
    public GameObject mapUI; 
    public Transform choicesContainer; 
    public GameObject nodePrefab; 

    [Header("Scroll View")]
    public ScrollRect mapScrollRect;

    [Header("Map Generation Settings")]
    public int totalLayers = 15; 
    public int minNodesPerLayer = 2;
    public int maxNodesPerLayer = 4;
    
    [Header("Visual Paths")]
    public GameObject linePrefab; 
    public RectTransform linesContainer; 
    private List<GameObject> activeLines = new List<GameObject>();

    [Header("Tooltip")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI tooltipText;

    [Header("Database")]
    public List<ArenaData> allAvailableArenas; 

    private int currentTier = 1;
    private bool isMapGenerated = false;
    private MapNodeUI currentNode = null; 
    
    private List<List<MapNodeUI>> mapGraph = new List<List<MapNodeUI>>();

    void Start() {
        mapUI.SetActive(false);
        HideTooltip();
    }

    public void OpenMap() {
        mapUI.SetActive(true);

        if (gridManager.currentArenaData != null && gridManager.currentArenaData.difficulty == ArenaDifficulty.Boss) {
            currentTier++;
            isMapGenerated = false; 
            currentNode = null;
            Debug.Log("Boss defeated! Advancing to Tier " + currentTier);
        }

        if (!isMapGenerated) {
            GenerateFullMap();
        } else {
            UpdateNodeStates();
            if (mapScrollRect != null) StartCoroutine(SnapToCurrentNodeRoutine());
        }
    }

private void GenerateFullMap() {
        foreach (Transform child in choicesContainer) {
            if (child != linesContainer) Destroy(child.gameObject);
        }
        foreach (GameObject line in activeLines) Destroy(line);
        activeLines.Clear();
        mapGraph.Clear();

        List<ArenaData> tierArenas = allAvailableArenas.FindAll(a => a.tier == currentTier);
        List<ArenaData> normalArenas = tierArenas.FindAll(a => a.difficulty == ArenaDifficulty.Normal || a.difficulty == ArenaDifficulty.Easy);
        List<ArenaData> eliteArenas = tierArenas.FindAll(a => a.difficulty == ArenaDifficulty.Hard);
        List<ArenaData> bossArenas = tierArenas.FindAll(a => a.difficulty == ArenaDifficulty.Boss);

        if (normalArenas.Count == 0 || bossArenas.Count == 0) {
            Debug.LogError("Missing Normal or Boss arenas for Tier " + currentTier);
            return;
        }

        // 1. GENERATE NODES (With Slay the Spire Weighting)
        for (int i = 0; i < totalLayers; i++) {
            List<MapNodeUI> currentLayerNodes = new List<MapNodeUI>();
            
            GameObject rowObj = new GameObject("Layer_" + i);
            rowObj.transform.SetParent(choicesContainer, false);
            HorizontalLayoutGroup hg = rowObj.AddComponent<HorizontalLayoutGroup>();
            hg.childAlignment = TextAnchor.MiddleCenter;
            hg.spacing = 120f; 
            hg.childForceExpandWidth = false;
            hg.childForceExpandHeight = false;
            hg.childControlWidth = false;
            hg.childControlHeight = false;

            rowObj.transform.SetSiblingIndex(1); 

            int nodesInThisLayer = (i == totalLayers - 1) ? 1 : Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);

            for (int j = 0; j < nodesInThisLayer; j++) {
                GameObject btnObj = Instantiate(nodePrefab, rowObj.transform);
                MapNodeUI nodeUI = btnObj.GetComponent<MapNodeUI>();

                // Default to a random normal arena so Shops/Events have a fallback
                ArenaData selectedArena = normalArenas[Random.Range(0, normalArenas.Count)];
                RoomType type;

                if (i == 0) {
                    // Layer 0 is ALWAYS a basic battle
                    type = RoomType.Battle;
                } else if (i == totalLayers - 1) {
                    // Top layer is ALWAYS the Boss
                    selectedArena = bossArenas[Random.Range(0, bossArenas.Count)];
                    type = RoomType.Boss;
                } else {
                    // Slay the Spire random weighting for the middle floors
                    float rng = Random.value;
                    
                    // Elites only spawn after the first 3rd of the map
                    if (i > totalLayers / 3 && rng < 0.15f && eliteArenas.Count > 0) { 
                        selectedArena = eliteArenas[Random.Range(0, eliteArenas.Count)];
                        type = RoomType.Elite;
                    } else if (rng < 0.25f) { 
                        type = RoomType.Shop; // 10% Shop
                    } else if (rng < 0.45f) { 
                        type = RoomType.Event; // 20% Event
                    } else {
                        type = RoomType.Battle; // 55% Basic Battle
                    }
                }

                nodeUI.Setup(selectedArena, type, i, this);
                currentLayerNodes.Add(nodeUI);
            }
            mapGraph.Add(currentLayerNodes);
        }

        // 2. GENERATE CONNECTIONS (The Web Algorithm)
        for (int i = 0; i < totalLayers - 1; i++) {
            List<MapNodeUI> currentLyr = mapGraph[i];
            List<MapNodeUI> nextLyr = mapGraph[i + 1];

            // A. Connect current nodes to the logical node directly above them
            for (int j = 0; j < currentLyr.Count; j++) {
                MapNodeUI currNode = currentLyr[j];
                
                // Find the closest index in the next row to prevent wild, crossing lines
                float ratio = (float)j / Mathf.Max(1, currentLyr.Count - 1);
                int targetIndex = Mathf.RoundToInt(ratio * (nextLyr.Count - 1));
                
                MapNodeUI targetNode = nextLyr[targetIndex];
                if (!currNode.nextNodes.Contains(targetNode)) currNode.nextNodes.Add(targetNode);

                // BRANCHING CHANCE: 60% chance to branch to an adjacent node!
                if (Random.value > 0.4f) { 
                    int branchOffset = (Random.value > 0.5f) ? 1 : -1; // Branch left or right
                    int branchIndex = Mathf.Clamp(targetIndex + branchOffset, 0, nextLyr.Count - 1);
                    MapNodeUI branchTarget = nextLyr[branchIndex];
                    
                    if (!currNode.nextNodes.Contains(branchTarget)) {
                        currNode.nextNodes.Add(branchTarget);
                    }
                }
            }

            // B. Sweep the next layer to ensure NO node is orphaned (unreachable)
            for (int j = 0; j < nextLyr.Count; j++) {
                MapNodeUI nextNode = nextLyr[j];
                bool hasParent = false;
                
                foreach (var parent in currentLyr) {
                    if (parent.nextNodes.Contains(nextNode)) { hasParent = true; break; }
                }
                
                if (!hasParent) {
                    // Connect it to the closest parent
                    float ratio = (float)j / Mathf.Max(1, nextLyr.Count - 1);
                    int parentIndex = Mathf.RoundToInt(ratio * (currentLyr.Count - 1));
                    currentLyr[parentIndex].nextNodes.Add(nextNode);
                }
            }
        }

        isMapGenerated = true;
        currentNode = null; 
        
        StartCoroutine(DrawPathsRoutine());
        UpdateNodeStates();
    }

    private void UpdateNodeStates() {
        foreach (var layer in mapGraph) {
            foreach (var node in layer) {
                if (currentNode == null) {
                    if (node.layer == 0) node.UpdateState(NodeState.Selectable);
                    else node.UpdateState(NodeState.Locked);
                } else {
                    if (node == currentNode || node.layer < currentNode.layer) {
                        node.UpdateState(NodeState.Cleared);
                    } else if (currentNode.nextNodes.Contains(node)) {
                        node.UpdateState(NodeState.Selectable);
                    } else {
                        node.UpdateState(NodeState.Locked);
                    }
                }
            }
        }
    }

    private IEnumerator DrawPathsRoutine() {
        yield return new WaitForEndOfFrame(); 

        foreach (var layer in mapGraph) {
            foreach (var node in layer) {
                foreach (var nextNode in node.nextNodes) {
                    DrawLineBetween(node.GetComponent<RectTransform>(), nextNode.GetComponent<RectTransform>());
                }
            }
        }

        if (mapScrollRect != null) {
            Canvas.ForceUpdateCanvases();
            mapScrollRect.verticalNormalizedPosition = 0f; 
        }
    }

    private IEnumerator SnapToCurrentNodeRoutine() {
        yield return new WaitForEndOfFrame();
        if (currentNode != null) {
            Canvas.ForceUpdateCanvases();
            float progress = (float)currentNode.layer / (totalLayers - 1);
            mapScrollRect.verticalNormalizedPosition = progress;
        }
    }

    private void DrawLineBetween(RectTransform startNode, RectTransform endNode) {
        GameObject lineObj = Instantiate(linePrefab, linesContainer);
        activeLines.Add(lineObj);
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();

        Vector3 startPos = linesContainer.InverseTransformPoint(startNode.position);
        Vector3 endPos = linesContainer.InverseTransformPoint(endNode.position);

        Vector3 dir = endPos - startPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        lineRect.localPosition = startPos;
        lineRect.localRotation = Quaternion.Euler(0, 0, angle);
        lineRect.sizeDelta = new Vector2(distance, lineRect.sizeDelta.y); 
    }

    public void SelectNode(MapNodeUI node) {
        currentNode = node; 
        mapUI.SetActive(false);
        HideTooltip();
        gridManager.LoadNextArenaFromMap(node.arenaData);
    }

    public void ShowTooltip(string text, Vector3 position) {
        tooltipPanel.SetActive(true);
        tooltipText.text = text;
        tooltipPanel.transform.position = position + new Vector3(0, 50f, 0); 
    }
    public void HideTooltip() { tooltipPanel.SetActive(false); }
}