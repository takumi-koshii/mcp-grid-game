using UnityEngine;

public static class McpMessageDispatcher
{
    /// <summary>
    /// MCP メッセージを解析し、GameManager に指示を出す
    /// </summary>
    public static void Dispatch(string name, string content)
    {
        if (name == "player_move")
        {
            Vector2Int dir = ParseDirection(content);
            if (dir != Vector2Int.zero)
            {
                GameManager.Instance.MovePlayer(dir);
            }
            else
            {
                Debug.LogWarning($"不明な移動指示: {content}");
            }
        }
        else
        {
            Debug.LogWarning($"未対応のメッセージ名: {name}");
        }
    }

    private static Vector2Int ParseDirection(string content)
    {
        switch (content.Trim().ToLower())
        {
            case "move north": return Vector2Int.up;
            case "move south": return Vector2Int.down;
            case "move east": return Vector2Int.right;
            case "move west": return Vector2Int.left;
            default: return Vector2Int.zero;
        }
    }
}