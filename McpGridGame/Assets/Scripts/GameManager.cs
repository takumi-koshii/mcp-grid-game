using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject keyPrefab;
    public GameObject doorPrefab;

    [Header("Game State")]
    public Vector2Int playerGridPos = new Vector2Int(0, 0);
    public Vector2Int keyGridPos;
    public Vector2Int doorGridPos;
    public bool hasKey = false;
    public string lastInput = ""; // ✅ 直前の入力内容を保持

    private GameObject playerObj;
    private GameObject keyObj;
    private GameObject doorObj;

    private List<Vector2Int> availablePositions = new List<Vector2Int>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitializeGrid();
        SpawnObjects();
    }

    void InitializeGrid()
    {
        availablePositions.Clear();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (pos != playerGridPos)
                {
                    availablePositions.Add(pos);
                }
            }
        }

        keyGridPos = GetRandomPosition();
        doorGridPos = GetRandomPosition(exclude: new List<Vector2Int> { keyGridPos });
    }

    Vector2Int GetRandomPosition(List<Vector2Int> exclude = null)
    {
        List<Vector2Int> candidates = new List<Vector2Int>(availablePositions);
        if (exclude != null)
        {
            candidates.RemoveAll(p => exclude.Contains(p));
        }

        return candidates[Random.Range(0, candidates.Count)];
    }

    void SpawnObjects()
    {
        playerObj = Instantiate(playerPrefab, GridUtils.GridToWorld(playerGridPos), Quaternion.identity);
        keyObj = Instantiate(keyPrefab, GridUtils.GridToWorld(keyGridPos), Quaternion.identity);
        doorObj = Instantiate(doorPrefab, GridUtils.GridToWorld(doorGridPos), Quaternion.identity);
    }

    public void MovePlayer(Vector2Int direction)
    {
        string inputStr = DirectionToString(direction);
        lastInput = $"move {inputStr}"; // ✅ 入力内容を記録

        Vector2Int newPos = playerGridPos + direction;
        if (newPos.x < -1 || newPos.x > 1 || newPos.y < -1 || newPos.y > 1)
        {
            Debug.Log("移動不可: 範囲外");
            return;
        }

        playerGridPos = newPos;
        playerObj.transform.position = GridUtils.GridToWorld(playerGridPos);

        if (!hasKey && playerGridPos == keyGridPos)
        {
            hasKey = true;
            Destroy(keyObj);
            Debug.Log("鍵を取得しました！");
        }

        if (playerGridPos == doorGridPos && hasKey)
        {
            Debug.Log("ゲームクリア！");

            var clearText = GameObject.Find("ClearText")?.GetComponent<TextMeshProUGUI>();
            if (clearText != null)
            {
                clearText.text = "Game Clear!!";
            }

            return;
        }
    }

    private string DirectionToString(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return "north";
        if (dir == Vector2Int.down) return "south";
        if (dir == Vector2Int.left) return "west";
        if (dir == Vector2Int.right) return "east";
        return "unknown";
    }

    // ✅ MCP 応答用のゲーム状態を文字列で返す
    public string GetGameStateString()
    {
        return $"Player: ({playerGridPos.x},{playerGridPos.y})\n" +
               $"HasKey: {hasKey.ToString().ToLower()}\n" +
               $"Key: ({keyGridPos.x},{keyGridPos.y})\n" +
               $"Door: ({doorGridPos.x},{doorGridPos.y})\n" +
               $"LastInput: {lastInput}";
    }
}