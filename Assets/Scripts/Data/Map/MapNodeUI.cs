using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public enum RoomType { Battle, Elite, Boss, Shop, Event }
public enum NodeState { Locked, Selectable, Cleared }

public class MapNodeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public RoomType roomType;
    public ArenaData arenaData; 
    public Button button;
    public Image iconImage;

    [Header("Graph Data")]
    public int layer;
    public NodeState state = NodeState.Locked;
    public List<MapNodeUI> nextNodes = new List<MapNodeUI>(); // The nodes this one draws lines to

    [Header("Room Icons (Sprites)")]
    public Sprite battleSprite;
    public Sprite eliteSprite;
    public Sprite bossSprite;
    public Sprite shopSprite;
    public Sprite eventSprite;

    private MapManager mapManager;

    public void Setup(ArenaData data, RoomType type, int nodeLayer, MapManager manager) {
        arenaData = data;
        roomType = type;
        layer = nodeLayer;
        mapManager = manager;
        if (button == null) button = GetComponent<Button>();

        switch (type) {
            case RoomType.Battle: if (battleSprite != null) iconImage.sprite = battleSprite; break;
            case RoomType.Elite: if (eliteSprite != null) iconImage.sprite = eliteSprite; break;
            case RoomType.Boss: if (bossSprite != null) iconImage.sprite = bossSprite; break;
            case RoomType.Shop: if (shopSprite != null) iconImage.sprite = shopSprite; break;
            case RoomType.Event: if (eventSprite != null) iconImage.sprite = eventSprite; break;
        }

        button.onClick.AddListener(OnNodeClicked);
    }

    public void UpdateState(NodeState newState) {
        state = newState;
        button.interactable = (state == NodeState.Selectable);

        // Visual feedback based on state
        if (state == NodeState.Locked) {
            iconImage.color = new Color(0.3f, 0.3f, 0.3f, 0.5f); // Dim and semi-transparent
        } else if (state == NodeState.Cleared) {
            iconImage.color = new Color(0.1f, 0.1f, 0.1f, 0.3f); // Very dark (already visited)
        } else {
            iconImage.color = Color.white; // Bright, fully visible, and clickable
        }
    }

    private void OnNodeClicked() {
        if (state == NodeState.Selectable) {
            mapManager.SelectNode(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (arenaData != null) {
            string enemyInfo = "Enemies:\n";
            foreach (var spawn in arenaData.enemies) {
                if (spawn.enemyPrefab != null) enemyInfo += "- " + spawn.enemyPrefab.name + "\n";
            }
            mapManager.ShowTooltip(enemyInfo, transform.position);
        } else {
            mapManager.ShowTooltip(roomType.ToString() + " Room", transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData) {
        mapManager.HideTooltip();
    }
}