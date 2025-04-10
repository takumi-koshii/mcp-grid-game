using UnityEngine;

public static class GridUtils
{
    public static Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * 3f, gridPos.y * 3f, 0f);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x / 3f), Mathf.RoundToInt(worldPos.y / 3f));
    }
}