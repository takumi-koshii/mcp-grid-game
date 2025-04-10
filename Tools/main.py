import requests
import json

# Unity の MCP サーバーの URL
MCP_URL = "http://127.0.0.1:8080/mcp"

# 送信する MCP メッセージ
mcp_message = {
    "role": "user",
    "name": "player_move",
    "content": "move east"
}

# POST リクエストを送信
try:
    response = requests.post(MCP_URL, json=mcp_message, timeout=5)
    response.raise_for_status()  # HTTP エラーがあれば例外を出す

    # 応答を表示
    print("✅ Unity からの応答:")
    print(response.text)
    # 応答を JSON として解析
    response_json = response.json()
    print("✅ 解析した JSON:")
    print(json.dumps(response_json, indent=2))

except requests.exceptions.RequestException as e:
    print("❌ リクエストに失敗しました:")
    print(e)