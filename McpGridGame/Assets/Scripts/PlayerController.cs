using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public void Move(Vector2Int direction)
    {
        GameManager.Instance.MovePlayer(direction);
    }
}