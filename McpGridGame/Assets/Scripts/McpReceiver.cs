using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;

public class MCPReceiver : MonoBehaviour
{
    private HttpListener listener;
    private Thread listenerThread;
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();
    private const string Uri = "http://127.0.0.1:8080/mcp/";

    void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(Uri);
        listener.Start();

        listenerThread = new Thread(HandleIncomingConnections);
        listenerThread.Start();

        Debug.Log($"✅ MCPReceiver started on {Uri}");
    }

    void OnApplicationQuit()
    {
        listener.Stop();
        listenerThread.Abort();
    }

    void Update()
    {
        lock (mainThreadActions)
        {
            while (mainThreadActions.Count > 0)
            {
                mainThreadActions.Dequeue()?.Invoke();
            }
        }
    }

    private void HandleIncomingConnections()
    {
        while (listener.IsListening)
        {
            try
            {
                var context = listener.GetContext();
                var request = context.Request;

                if (request.HttpMethod == "POST")
                {
                    string json;
                    using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                    {
                        json = reader.ReadToEnd();
                    }

                    Debug.Log("📥 受信した MCP メッセージ: " + json);

                    MCPMessage message = JsonUtility.FromJson<MCPMessage>(json);

                    // メインスレッドで GameManager を操作し、応答を生成
                    string responseJson = null;

                    var waitHandle = new AutoResetEvent(false);

                    lock (mainThreadActions)
                    {
                        mainThreadActions.Enqueue(() =>
                        {
                            try
                            {
                                if (message.name == "player_move")
                                {
                                    Vector2Int dir = ParseDirection(message.content);
                                    GameManager.Instance.MovePlayer(dir);
                                }

                                var responseMessage = new MCPMessage
                                {
                                    role = "system",
                                    name = "game_state",
                                    content = GameManager.Instance.GetGameStateString()
                                };

                                responseJson = JsonUtility.ToJson(responseMessage);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogError("❌ GameManager 処理中にエラー: " + ex.Message);
                                responseJson = JsonUtility.ToJson(new MCPMessage
                                {
                                    role = "system",
                                    name = "error",
                                    content = "Internal error"
                                });
                            }
                            finally
                            {
                                waitHandle.Set();
                            }
                        });
                    }

                    // メインスレッドの処理が終わるまで待機
                    waitHandle.WaitOne();

                    // 応答を返す
                    var response = context.Response;
                    byte[] buffer = Encoding.UTF8.GetBytes(responseJson);
                    response.ContentType = "application/json";
                    response.ContentEncoding = Encoding.UTF8;
                    response.ContentLength64 = buffer.Length;
                    response.OutputStream.Write(buffer, 0, buffer.Length);
                    response.OutputStream.Close();

                    Debug.Log("📤 応答を返しました: " + responseJson);
                }
                else
                {
                    context.Response.StatusCode = 405;
                    context.Response.Close();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("❌ MCPReceiver error: " + e.Message);
            }
        }
    }

    private Vector2Int ParseDirection(string content)
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

    [Serializable]
    public class MCPMessage
    {
        public string role;
        public string name;
        public string content;
    }
}