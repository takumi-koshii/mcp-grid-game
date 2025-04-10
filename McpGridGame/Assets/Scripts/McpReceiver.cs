using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

public class McpReceiver : MonoBehaviour
{
    private HttpListener listener;
    private const string Address = "127.0.0.1";
    private const int Port = 8080;
    private readonly string _uri = $"https://{Address}:{Port}/mcp/";

    void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add(_uri);
        listener.Start();
        listener.BeginGetContext(OnRequest, null);

        Debug.Log($"✅ MCP サーバーが起動しました: {_uri}");
    }

    void OnApplicationQuit()
    {
        if (listener != null && listener.IsListening)
        {
            listener.Stop();
            listener.Close();
        }
    }

    private void OnRequest(IAsyncResult result)
    {
        if (listener == null || !listener.IsListening) return;

        HttpListenerContext context = listener.EndGetContext(result);
        listener.BeginGetContext(OnRequest, null); // 次のリクエストを待機

        // リクエスト処理は Unity のメインスレッドで行うため、Invoke を使う
        // Unity 2020 以降では UnityMainThreadDispatcher を使わずとも Invoke で代用可能
        UnityMainThreadInvoker.Invoke(() => ProcessRequest(context));
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        if (context.Request.HttpMethod != "POST")
        {
            context.Response.StatusCode = 405;
            context.Response.Close();
            return;
        }

        try
        {
            string body;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }

            Debug.Log("📥 受信した MCP メッセージ: " + body);

            McpMessage mcpMessage = JsonUtility.FromJson<McpMessage>(body);
            McpMessageDispatcher.Dispatch(mcpMessage.name, mcpMessage.content);

            McpMessage responseMessage = new McpMessage
            {
                role = "system",
                name = "game_state",
                content = GameManager.Instance.GetGameStateString()
            };

            string responseJson = JsonUtility.ToJson(responseMessage);
            byte[] buffer = Encoding.UTF8.GetBytes(responseJson);

            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = Encoding.UTF8;
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();

            Debug.Log("📤 応答を返しました: " + responseJson);
        }
        catch (Exception ex)
        {
            Debug.LogError("MCP リクエスト処理中にエラー: " + ex.Message);
            context.Response.StatusCode = 500;
            context.Response.Close();
        }
    }

    [Serializable]
    public class McpMessage
    {
        public string role;
        public string name;
        public string content;
    }
}